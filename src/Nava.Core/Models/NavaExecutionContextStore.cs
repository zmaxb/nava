// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

using Newtonsoft.Json.Linq;

namespace Nava.Core.Models;

public class NavaExecutionContextStore
{
    public Dictionary<string, JToken> Script { get; set; } = new();
    public Dictionary<string, JToken> Flow { get; set; } = new();
    public Dictionary<string, JToken> Page { get; set; } = new();
}