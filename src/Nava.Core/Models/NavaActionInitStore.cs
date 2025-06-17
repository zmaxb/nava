// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

using Newtonsoft.Json.Linq;

// ReSharper disable CollectionNeverUpdated.Global

namespace Nava.Core.Models;

public class NavaActionInitStore
{
    public Dictionary<string, JToken> Script { get; set; } = new();
    public Dictionary<string, JToken> Flow { get; set; } = new();
    public Dictionary<string, JToken> Page { get; set; } = new();
}