// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Nava.Core.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class NavigationTarget
{
    public required string Url { get; set; }
    public string? Name { get; set; }

    public override string ToString()
    {
        return !string.IsNullOrWhiteSpace(Name) ? Name! : Url;
    }
}