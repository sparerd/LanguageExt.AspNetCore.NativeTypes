using System.Text.Json;
using System.Text.Json.Serialization;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.JsonConversion.Option;

public class OptionAsArrayJsonConverter<T> : JsonConverter<Option<T>>
{
	private readonly JsonConverter<T> _innerConverter;

	public OptionAsArrayJsonConverter(JsonSerializerOptions options) =>
		_innerConverter = options.GetConverter<T>()
			.IfNone(() => throw new JsonException($"Could not get converter for {typeof(T)}"));

	public override Option<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return None;

		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException($"Expected {JsonTokenType.StartArray} but found {reader.TokenType}");

		if (!reader.Read())
			throw new JsonException($"Could not read element in array or {JsonTokenType.EndArray}");

		if (reader.TokenType == JsonTokenType.EndArray)
			return None;

		var value = _innerConverter.Read(ref reader, typeof(T), options);
		if (!reader.Read())
			throw new JsonException("Could not read next token");

		if (reader.TokenType != JsonTokenType.EndArray)
			throw new JsonException($"Expected token {JsonTokenType.EndArray} but found {reader.TokenType}");

		return Some(value!);
	}

	public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		value.IfSome(some => _innerConverter.Write(writer, some, options));
		writer.WriteEndArray();
	}
}