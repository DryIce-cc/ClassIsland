using System.Text.Json;
using System.Text.Json.Serialization;
using ClassIsland.Shared.Models.Profile;
namespace ClassIsland.Shared.JsonConverters;

/// <summary>
/// 用于 <see cref="TimeRule"/> 的 JsonConverter。
/// </summary>
/// <param name="WeekDays">兼容旧版读取；<br/>若长度为 1，写入 (int)WeekDay，否则写入 (List)WeekDays。</param>
/// <param name="WeekCountDivs">兼容旧版读取；<br/>若长度为 1，写入 (int)WeekCountDiv，否则写入 (List)WeekCountDivs。</param>
/// <param name="WeekCountDivTotal">正常读写。</param>
public class TimeRuleJsonConverter : JsonConverter<TimeRule>
{
    public override TimeRule Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var root = JsonDocument.ParseValue(ref reader).RootElement;
        return new TimeRule
        {
            WeekDays = ReadIntList("WeekDays", "WeekDay"),
            WeekCountDivs = ReadIntList("WeekCountDivs", "WeekCountDiv"),
            WeekCountDivTotal = root.GetProperty("WeekCountDivTotal").GetInt32(),
        };

        List<int> ReadIntList(string newName, string oldName)
        {
            // 尝试读取 List
            if (root.TryGetProperty(newName, out var prop))
                return JsonSerializer.Deserialize<List<int>>(prop.GetRawText(), options) ?? [];

            // 尝试读取 int
            if (root.TryGetProperty(oldName, out var oldProp))
                return oldProp.ValueKind switch
                {
                    JsonValueKind.Number => [oldProp.GetInt32()],
                    JsonValueKind.String => int.TryParse(oldProp.GetString(), out var num)
                        ? new List<int> { num }
                        : new List<int>(),
                    _ => []
                };

            return [];
        }
    }

    public override void Write(Utf8JsonWriter writer, TimeRule value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        WriteIntList(value.WeekDays, "WeekDays", "WeekDay");
        WriteIntList(value.WeekCountDivs, "WeekCountDivs", "WeekCountDiv");
        writer.WriteNumber("WeekCountDivTotal", value.WeekCountDivTotal);
        writer.WriteEndObject();
        return;

        void WriteIntList(List<int> list, string newName, string oldName)
        {
            switch (list.Count)
            {
                case 1: // 写入 int
                    writer.WriteNumber(oldName, list[0]);
                    break;

                case > 1: // 写入 List
                    writer.WritePropertyName(newName);
                    JsonSerializer.Serialize(writer, list, options);
                    break;
            }
        }
    }
}