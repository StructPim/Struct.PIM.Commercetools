using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Struct.PIM.Commercetools.Converters;

public abstract class JsonCreationConverter<T> : JsonConverter
{
    /// <summary>
    ///     this is very important, otherwise serialization breaks!
    /// </summary>
    public override bool CanWrite => false;

    /// <summary>
    ///     Create an instance of objectType, based properties in the JSON object
    /// </summary>
    /// <param name="objectType">type of object expected</param>
    /// <param name="jObject">
    ///     contents of JSON object that will be
    ///     deserialized
    /// </param>
    protected abstract T? Create(Type objectType, JObject jObject);

    public override bool CanConvert(Type objectType)
    {
        return typeof(T).IsAssignableFrom(objectType);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        // Load JObject from stream 
        JObject? jObject;
        if (reader.ValueType != null && reader.ValueType.FullName == typeof(string).FullName)
        {
            var tmp = new { class_type_name = reader.Value };
            jObject = JObject.FromObject(tmp);
        }
        else
        {
            jObject = JObject.Load(reader);
        }

        // Create target object based on JObject 
        var target = Create(objectType, jObject);

        // Populate the object properties 
        if (target == null)
        {
            return null;
        }

        serializer.Populate(jObject.CreateReader(), target);

        return target;

    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}