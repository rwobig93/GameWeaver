﻿using System.ComponentModel.DataAnnotations;

namespace Application.Settings;

public class AuthConfiguration : IAppSettingsSection
{
    public const string SectionName = "Auth";
    
    [Url]
    public string RegisterUrl { get; init; } = "https://localhost:9500/";

    public string Host { get; init; } = "";

    public string Key { get; init; } = "";
}