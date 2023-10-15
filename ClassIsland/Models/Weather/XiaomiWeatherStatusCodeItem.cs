﻿using System.Text.Json.Serialization;

namespace ClassIsland.Models.Weather;

public class XiaomiWeatherStatusCodeItem
{
    [JsonPropertyName("code")]
    public int Code { get; set; } = 99;

    [JsonPropertyName("wea")]
    public string Weather { get; set; } = "";
}