using System.Reflection;
using Newtonsoft.Json;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.NewtonsoftJson.Converters;

public class SeqJsonConverter : JsonConverter
{
    private static readonly MethodInfo ReadMethod = typeof(SeqJsonConverter)
        .GetMethod(nameof(ReadJson), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo WriteMethod = typeof(SeqJsonConverter)
        .GetMethod(nameof(WriteJson), BindingFlags.Public | BindingFlags.Static)!;

    public override bool CanConvert(Type objectType) =>
        objectType.IsGenericType(typeof(Seq<>)) || objectType == typeof(SeqEmpty);

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
        GetInnerType(value!.GetType())
            .Map(innerType => WriteMethod
                .MakeGenericMethod(innerType)
                .Invoke(null, new[] { writer, value, serializer })
            )
            .IfNone(() => writer.WriteEmptyArray());

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) =>
        GetInnerType(objectType)
            .Map(innerType => ReadMethod
                .MakeGenericMethod(innerType)
                .Invoke(null, new object[] { reader, serializer })
            )
            .IfNone(() => Empty);

    public static Seq<T> ReadJson<T>(JsonReader reader, JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.StartArray)
            throw new JsonException($"Expected {JsonToken.StartArray} but found {reader.TokenType}");

        var seq = Seq<T>.Empty;
        while (reader.Read() && reader.TokenType != JsonToken.EndArray)
        {
            var element = serializer.Deserialize<T>(reader);
            seq = seq.Add(element);
        }

        return seq;
    }

    public static Unit WriteJson<T>(JsonWriter writer, Seq<T> value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        value.Iter(val => serializer.Serialize(writer, val, typeof(T)));
        writer.WriteEndArray();
        return unit;
    }

    private static Option<Type> GetInnerType(Type t) => t.GetGenericArguments().HeadOrNone();
}