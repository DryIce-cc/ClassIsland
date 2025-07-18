using Avalonia.Data.Converters;

namespace ClassIsland.Converters;

public static class WeatherConverters
{
    public static readonly FuncValueConverter<string, string> WeatherTypeToIconGlyphConverter = new(x => x switch
    {
        "0" => "\uf465", // 晴
        "1" => "\uf44f", // 多云
        "2" => "\uf43d", // 阴
        "3" => "\uf455", // 阵雨
        "4" => "\uf46b", // 雷阵雨
        "5" => "\uf46b", // 雷阵雨并伴有冰雹
        "6" => "\uf459", // 雨夹雪
        "7" => "\uf43f", // 小雨
        "8" => "\uf453", // 中雨
        "9" => "\uf453", // 大雨
        "10" => "\uf453", // 暴雨
        "11" => "\uf453", // 大暴雨
        "12" => "\uf453", // 特大暴雨
        "13" => "\uf45d", // 阵雪
        "14" => "\uf45b", // 小雪
        "15" => "\uf45b", // 中雪
        "16" => "\uf45b", // 大雪
        "17" => "\uf45b", // 暴雪
        "18" => "\uf443", // 雾
        "19" => "\uf461", // 冻雨
        "20" => "\uf441", // 沙尘暴
        "21" => "\uf453", // 小雨-中雨
        "22" => "\uf453", // 中雨-大雨
        "23" => "\uf453", // 大雨-暴雨 
        "24" => "\uf453", // 暴雨-大暴雨
        "25" => "\uf453", // 大暴雨-特大暴雨
        "26" => "\uf45b", // 小雪-中雪
        "27" => "\uf45b", // 中雪-大雪
        "28" => "\uf45b", // 大雪-暴雪
        "29" => "\uf449", // 浮尘
        "30" => "\uf441", // 扬沙
        "31" => "\uf441", // 强沙尘暴
        "32" => "\uf463", // 飑
        "33" => "\uf463", // 龙卷风
        "34" => "\uf45b", // 弱高吹雪
        "35" => "\uf443", // 轻雾
        "53" => "\uf449", // 霾
        "301" => "\uf453", // 雨
        "302" => "\uf45b", // 雪
        "99" or _ => "\uee31", // 未知
    });
}