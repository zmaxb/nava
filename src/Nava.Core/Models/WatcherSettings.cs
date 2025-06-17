// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Nava.Core.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class WatcherSettings
{
    public bool Enabled { get; set; } = false;
    public int IntervalMs { get; set; } = 60000;
}