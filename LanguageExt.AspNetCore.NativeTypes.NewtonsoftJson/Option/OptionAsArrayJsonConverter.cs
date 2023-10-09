using System.Reflection;
using Newtonsoft.Json;
using static LanguageExt.Prelude;
using JsonException = Newtonsoft.Json.JsonException;

namespace LanguageExt.AspNetCore.NativeTypes.NewtonsoftJson.Option;

public class OptionAsArrayJsonConverter : JsonConverter
{
	private static readonly MethodInfo WriteMethod = 
		typeof(OptionAsArrayJsonConverter).GetMethod(nameof(WriteJson), BindingFlags.Public | BindingFlags.Static)!;
	private static readonly MethodInfo ReadMethod = 
		typeof(OptionAsArrayJsonConverter).GetMethod(nameof(ReadJson), BindingFlags.Public | BindingFlags.Static)!;

	public override bool CanConvert(Type objectType) => 
		objectType.IsGenericType(typeof(Option<>)) ||
		objectType == typeof(OptionNone);

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
		value!.GetType()
			.GetInnerType()
			.Map(innerType => ignore(WriteMethod
				.MakeGenericMethod(innerType)
				.Invoke(null, new[] { writer, value, serializer }))
			)
			.IfNone(writer.WriteEmptyArray);

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) =>
		objectType
			.GetInnerType()
			.Map(innerType => ReadMethod
				.MakeGenericMethod(innerType)
				.Invoke(null, new object[] { reader, serializer })
			)
			.IfNone(None);

	public static void WriteJson<T>(JsonWriter writer, Option<T> value, JsonSerializer serializer) =>
		value.Match(
			Some: v =>
			{
				writer.WriteStartArray();
				serializer.Serialize(writer, v, typeof(T));
				writer.WriteEndArray();
			},
			None: () => writer.WriteEmptyArray());

	public static Option<T> ReadJson<T>(JsonReader reader, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
			return None;

		if (reader.TokenType != JsonToken.StartArray)
			throw new JsonException($"Expected {JsonToken.StartArray} but found {reader.TokenType}");

		if (!reader.Read())
			throw new JsonException($"Could not read element in array or {JsonToken.EndArray}");

		if (reader.TokenType == JsonToken.EndArray)
			return None;

		var value = serializer.Deserialize<T>(reader);
		if (!reader.Read())
			throw new JsonException("Could not read next token");

		if (reader.TokenType != JsonToken.EndArray)
			throw new JsonException($"Expected token {JsonToken.EndArray} but found {reader.TokenType}");

		return Some(value!);
	}
}