using System.Text.Json;
using System.Text.Json.Serialization;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.JsonConversion.Option;

public class OptionNoneAsNullableJsonConverter : JsonConverter<OptionNone>
{
	public override OptionNone Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
		reader.TokenType == JsonTokenType.Null
			? None
			: throw new JsonException("Did not expect any value");

	public override void Write(Utf8JsonWriter writer, OptionNone value, JsonSerializerOptions options) =>
		writer.WriteNullValue();
}