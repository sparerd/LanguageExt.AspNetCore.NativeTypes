using System.Text.Json;
using System.Text.Json.Serialization;

namespace LanguageExt.AspNetCore.NativeTypes.JsonConversion.Seq;

public class SeqJsonConverter<T> : JsonConverter<Seq<T>>
{
	private readonly JsonConverter<T> _innerConverter;

	public SeqJsonConverter(JsonSerializerOptions options) =>
		_innerConverter = options.GetConverter<T>()
			.IfNone(() => throw new JsonException($"Could not get converter for {typeof(T)}"));

	public override Seq<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException($"Expected {JsonTokenType.StartArray} but found {reader.TokenType}");
		
		var seq = Seq<T>.Empty;
		while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
		{
			var element = _innerConverter.Read(ref reader, typeof(T), options)!;
			seq = seq.Add(element);
		}

		return seq;
	}

	public override void Write(Utf8JsonWriter writer, Seq<T> value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		value.Iter(val => _innerConverter.Write(writer, val, options));
		writer.WriteEndArray();
	}
}