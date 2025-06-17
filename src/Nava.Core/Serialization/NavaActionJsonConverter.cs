using Nava.Core.Enums;
using Nava.Core.Models.Actions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nava.Core.Serialization;

public class NavaActionJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(NavaAction).IsAssignableFrom(objectType);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        var typeString = jObject["action"]?.ToString();

        if (string.IsNullOrWhiteSpace(typeString))
            throw new JsonException("No 'type' specified for NavaAction");

        if (!Enum.TryParse<NavaActionType>(typeString, true, out var actionType))
            throw new JsonException($"Unknown action type: {typeString}");

        var concrete = actionType switch
        {
            NavaActionType.Log => typeof(LogAction),
            NavaActionType.Navigate => typeof(NavigateAction),
            NavaActionType.Wait => typeof(WaitAction),
            NavaActionType.Js => typeof(JsAction),
            NavaActionType.ConditionalJs => typeof(ConditionalJsAction),
            NavaActionType.Screenshot => typeof(ScreenshotAction),
            NavaActionType.SaveToFile => typeof(FileResultSaverAction),
            NavaActionType.PostResult => typeof(PostResultSaverAction),
            _ => throw new JsonException($"Unknown action type: {typeString}")
        };

        var safeSerializer = new JsonSerializer
        {
            // ReSharper disable once RedundantEmptyObjectOrCollectionInitializer
            Converters = { }
        };

        using var subReader = jObject.CreateReader();
        return safeSerializer.Deserialize(subReader, concrete);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value, value?.GetType() ?? typeof(object));
    }
}