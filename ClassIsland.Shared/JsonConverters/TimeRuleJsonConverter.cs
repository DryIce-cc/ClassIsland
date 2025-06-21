using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClassIsland.Shared.Models.Profile;
namespace ClassIsland.Shared.JsonConverters;

/// <summary>
/// 用于 <see cref="TimeRule"/> 的 JsonConverter。
/// </summary>
/// <param name="WeekDays">兼容旧版读取；<br/>若长度为 1，写入 (int)WeekDay，否则写入 (ObservableCollection)WeekDays。</param>
/// <param name="WeekCountDivs">兼容旧版读取；<br/>若长度为 1，写入 (int)WeekCountDiv，否则写入 (ObservableCollection)WeekCountDivs。</param>
/// <param name="WeekCountDivTotal">正常读写。</param>
public class TimeRuleJsonConverter : JsonConverter<TimeRule>
{
    public override TimeRule Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var root = JsonDocument.ParseValue(ref reader).RootElement;
        return new TimeRule
        {
            WeekDays = ReadIntObservableCollection("WeekDays", "WeekDay"),
            WeekCountDivs = ReadIntObservableCollection("WeekCountDivs", "WeekCountDiv"),
            WeekCountDivTotal = root.GetProperty("WeekCountDivTotal").GetInt32(),
        };

        ObservableCollection<int> ReadIntObservableCollection(string newName, string oldName)
        {
            // 尝试读取 ObservableCollection
            if (root.TryGetProperty(newName, out var prop))
                return JsonSerializer.Deserialize<ObservableCollection<int>>(prop.GetRawText(), options) ?? [];

            // 尝试读取 int
            if (root.TryGetProperty(oldName, out var oldProp))
                return oldProp.ValueKind switch
                {
                    JsonValueKind.Number => [oldProp.GetInt32()],
                    JsonValueKind.String => int.TryParse(oldProp.GetString(), out var num)
                        ? new ObservableCollection<int> { num }
                        : new ObservableCollection<int>(),
                    _ => []
                };

            return [];
        }
    }

    public override void Write(Utf8JsonWriter writer, TimeRule value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        WriteIntObservableCollection(value.WeekDays, "WeekDays", "WeekDay");
        WriteIntObservableCollection(value.WeekCountDivs, "WeekCountDivs", "WeekCountDiv");
        writer.WriteNumber("WeekCountDivTotal", value.WeekCountDivTotal);
        writer.WriteEndObject();
        return;

        void WriteIntObservableCollection(ObservableCollection<int> ObservableCollection, string newName, string oldName)
        {
            switch (ObservableCollection.Count)
            {
                case 1: // 写入 int
                    writer.WriteNumber(oldName, ObservableCollection[0]);
                    break;

                case > 1: // 写入 ObservableCollection
                    writer.WritePropertyName(newName);
                    JsonSerializer.Serialize(writer, ObservableCollection, options);
                    break;
            }
        }
    }
}