using Nava.Core.Enums;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Nava.Core.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class NavaEnvironment
{
    public NavaBrowserType Browser { get; set; } = NavaBrowserType.Chromium;
    public bool Headless { get; set; } = true;
    public Viewport? Viewport { get; set; }
    public string? UserAgent { get; set; }
    public int SlowMo { get; set; } = 0;

    public string? Proxy { get; set; }
    public string? Locale { get; set; }
    public string? TimezoneId { get; set; }
    public bool IgnoreHttpsErrors { get; set; } = false;
    public bool AcceptDownloads { get; set; } = false;
    public Dictionary<string, string>? ExtraHttpHeaders { get; set; }

    public double? DeviceScaleFactor { get; set; }
    public bool? IsMobile { get; set; }
    public string? ColorScheme { get; set; }

    public bool Stealth { get; set; } = false;
    public ChromiumCdpOptions? ChromiumCdp { get; set; }
}

public class ChromiumCdpOptions
{
    public string? UserDataDir { get; set; }
    public int DebugPort { get; set; } = 9222;
    public string? CustomArgs { get; set; }
    public bool OverrideDefaultArgs { get; set; } = false;
}