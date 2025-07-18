using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Avalonia.Threading;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Enums;
using ClassIsland.Shared.Abstraction.Services;
using ClassIsland.Shared.Models.Management;
using ClassIsland.Shared.Protobuf.Client;
using ClassIsland.Shared.Protobuf.Enum;
using ClassIsland.Shared.Protobuf.Server;
using ClassIsland.Shared.Protobuf.Service;
using ClassIsland.Helpers;
using ClassIsland.Services.Logging;
using ClassIsland.Shared;
using ClassIsland.Shared.Protobuf.Command;
using CsesSharp.Models;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;

using Microsoft.Extensions.Logging;

using Timer = System.Timers.Timer;

namespace ClassIsland.Services.Management;

public class ManagementServerConnection : IManagementServerConnection
{
    private ILogger<ManagementServerConnection> Logger { get; } = App.GetService<ILogger<ManagementServerConnection>>();

    private Guid ClientGuid { get; }

    private string Id { get; }
    
    private string ManifestUrl { get; }
    
    private string Host { get; }
    
    internal GrpcChannel Channel { get; }

    private DispatcherTimer CommandConnectionAliveTimer { get; } = new()
    {
        Interval = TimeSpan.FromSeconds(10)
    };
    
    private AsyncDuplexStreamingCall<ClientCommandDeliverScReq, ClientCommandDeliverScRsp>? CommandListeningCall { get;
        set;
    }

    private CancellationTokenSource CommandListeningCallCancellationTokenSource { get; set; } = new();
    
    private ManagementSettings ManagementSettings { get; }
    
    public ManagementServerConnection(ManagementSettings settings, Guid clientUid, bool lightConnect)
    {
        ClientGuid = clientUid;
        Id = settings.ClassIdentity ?? "";
        Host = settings.ManagementServer;
        ManagementSettings = settings;
        ManifestUrl = $"{Host}/api/v1/client/{clientUid}/manifest";
        CommandConnectionAliveTimer.Tick += CommandConnectionAliveTimerOnTick;
        
        Channel = GrpcChannel.ForAddress(settings.ManagementServerGrpc);
        
        Logger.LogInformation("初始化管理服务器连接。");
        if (lightConnect) 
            return;
        AppBase.Current.AppStarted += (sender, args) => InstallAuditHooks();
        // 接受命令
        CommandReceived += OnCommandReceived;
        Task.Run(ListenCommands);
    }

    private void OnCommandReceived(object? sender, ClientCommandEventArgs e)
    {
        if (e.Type != CommandTypes.GetClientConfig)
        {
            return;
        }
        var payload = GetClientConfig.Parser.ParseFrom(e.Payload);
        if (payload == null)
        {
            return;
        }
        
        Logger.LogInformation("集控请求上传配置：{} {}", payload.RequestGuid, payload.ConfigType);
        var uploadPayload = payload.ConfigType switch
        {
            ConfigTypes.AppSettings => JsonSerializer.Serialize(IAppHost.GetService<SettingsService>().Settings),
            ConfigTypes.Profile => JsonSerializer.Serialize(IAppHost.GetService<IProfileService>().Profile),
            ConfigTypes.CurrentComponent => JsonSerializer.Serialize(IAppHost.GetService<IComponentsService>()
                .CurrentComponents),
            ConfigTypes.CurrentAutomation => JsonSerializer.Serialize(IAppHost.GetService<IAutomationService>()
                .Workflows),
            ConfigTypes.Logs => JsonSerializer.Serialize(IAppHost.GetService<AppLogService>().Logs),
            ConfigTypes.PluginList => JsonSerializer.Serialize(IPluginService.LoadedPlugins
                .Where(x => x.LoadStatus == PluginLoadStatus.Loaded)
                .Select(x => x.Manifest.Id)
                .ToList()),
            _ => throw new ArgumentOutOfRangeException()
        };
        var client = new ConfigUpload.ConfigUploadClient(Channel);
        client.UploadConfig(new ConfigUploadScReq()
        {
            RequestGuidId = payload.RequestGuid,
            Payload = uploadPayload
        });
    }


    public async Task<ManagementManifest> RegisterAsync()
    {
        Logger.LogInformation("正在注册实例");
        var client = new ClientRegister.ClientRegisterClient(Channel);
        var r = await client.RegisterAsync(new ClientRegisterCsReq()
        {
            ClientUid = ClientGuid.ToString(),
            ClientId = Id
        });
        Logger.LogTrace("ClientRegisterClient.RegisterAsync: {} {}", r.Retcode, r.Message);
        if (r.Retcode != Retcode.Registered && r.Retcode != Retcode.Success)
            throw new Exception($"无法注册实例：{r.Message}");
        return await GetManifest();
    }

    private async void CommandConnectionAliveTimerOnTick(object? sender, EventArgs e)
    {
        try
        {
            if (CommandListeningCall == null)
            {
                throw new Exception("CommandListeningCall is null!");
            }
            if (CommandListeningCallCancellationTokenSource.IsCancellationRequested)
                return;
            // Logger.LogTrace("向命令流发送心跳包。");
            await CommandListeningCall.RequestStream.WriteAsync(new ClientCommandDeliverScReq()
            {
                Type = CommandTypes.Ping
            });
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "命令流与集控服务器断开。");
            await CommandListeningCallCancellationTokenSource.CancelAsync();
            CommandConnectionAliveTimer.Stop();
            CommandListeningCall = null;
            Logger.LogInformation("尝试重新连接命令流");
            _ = Task.Run(ListenCommands);
        }
    }
    
    private async Task ListenCommands()
    {
        if (CommandListeningCall != null)
        {
            Logger.LogWarning("已连接到命令流，无需重复连接");
            return;
        }
        try
        {
            Logger.LogInformation("正在连接到命令流");
            var client = new ClientCommandDeliver.ClientCommandDeliverClient(GrpcChannel.ForAddress(ManagementSettings.ManagementServerGrpc));
            var call = client.ListenCommand(new Grpc.Core.Metadata()
            {
                { "cuid", ClientGuid.ToString() }
            });
            CommandListeningCallCancellationTokenSource = new CancellationTokenSource();
            CommandListeningCall = call;
            // await call.RequestStream.WriteAsync(new ClientCommandDeliverScReq());
            CommandConnectionAliveTimer.Start();
            while (!CommandListeningCallCancellationTokenSource.IsCancellationRequested)
            {
                await call.ResponseStream.MoveNext(CommandListeningCallCancellationTokenSource.Token);
                var r = call.ResponseStream.Current;
                if (r == null)
                    continue;
                if (r.Type == CommandTypes.Pong)
                {
                    continue;
                }

                if (r.RetCode != Retcode.Success)
                {
                    Logger.LogWarning("接受指令时未返回成功代码：{}", r.RetCode);
                    continue;
                }
                Logger.LogInformation("接受指令：[{}] {}", r.Type, r.Payload);
                try
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        CommandReceived?.Invoke(this, new ClientCommandEventArgs()
                        {
                            Type = r.Type,
                            Payload = r.Payload
                        });
                    });
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "在处理事件时出现异常");
                }
            }
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                return;
            Logger.LogError(ex, "无法连接到集控服务器命令流，将在30秒后重试。");
            CommandConnectionAliveTimer.Stop();
            CommandListeningCall = null;
            var timer = new Timer()
            {
                Interval = 30000,
                AutoReset = false
            };
            timer.Elapsed += (sender, args) => Task.Run(ListenCommands);
            timer.Start();
        }
    }

    private void InstallAuditHooks()
    {
        
    }

    internal void LogAuditEvent(AuditEvents eventType, IBufferMessage payload)
    {
        _ = Task.Run(() =>
        {
            try
            {
                new Audit.AuditClient(Channel).LogEvent(new AuditScReq()
                {
                    Event = eventType,
                    Payload = payload.ToByteString(),
                    TimestampUtc = (long)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "无法上传审计日志 {}", eventType);
            }
        });
    }
    
    public async Task<ManagementManifest> GetManifest()
    {
        return await WebRequestHelper.GetJson<ManagementManifest>(new Uri(ManifestUrl));
    }

    private Uri DecorateUrl(string url)
    {
        var uri = url.Replace("{cuid}", ClientGuid.ToString())
            .Replace("{id}", Id)
            .Replace("{host}", Host);
        Logger.LogTrace("拼接url模板：{} -> {} ", url, uri);
        return new Uri(uri);
    }

    public async Task<T> GetJsonAsync<T>(string url)
    {
        var decorateUrl = DecorateUrl(url);
        Logger.LogInformation("发起json请求：{}", decorateUrl);
        return await WebRequestHelper.GetJson<T>(decorateUrl);
    }

    public async Task<T> SaveJsonAsync<T>(string url, string path)
    {
        var decorateUrl = DecorateUrl(url);
        Logger.LogInformation("保存json请求：{} {}", decorateUrl, path);
        return await WebRequestHelper.SaveJson<T>(decorateUrl, path);
    }

    public event EventHandler<ClientCommandEventArgs>? CommandReceived;
}