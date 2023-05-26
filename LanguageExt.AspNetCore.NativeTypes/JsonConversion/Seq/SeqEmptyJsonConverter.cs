using System.Text.Json;
using System.Text.Json.Serialization;

namespace LanguageExt.AspNetCore.NativeTypes.JsonConversion.Seq;

public class SeqEmptyJsonConverter : JsonConverter<SeqEmpty>
{
	public override SeqEmpty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException($"Expected {JsonTokenType.StartArray} but found {reader.TokenType}");

		reader.Read();

		if (reader.TokenType != JsonTokenType.EndArray)
			throw new JsonException($"Expected {JsonTokenType.EndArray} but found {reader.TokenType}");

		return Prelude.Empty;
	}

	public override void Write(Utf8JsonWriter writer, SeqEmpty value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		writer.WriteEndArray();
	}
}