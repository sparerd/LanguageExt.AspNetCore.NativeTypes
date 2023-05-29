using System.Text.Json;
using System.Text.Json.Serialization;

namespace LanguageExt.AspNetCore.NativeTypes.JsonConversion.Option;

/// <summary>
/// serialize Option&lt;T&gt; like T? 
/// </summary>
/// <remarks>
/// Add [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] to the property to disable writing 'null' for None into json.
/// From https://github.com/louthy/language-ext/discussions/1132#discussioncomment-3860993
/// </remarks>
public class OptionAsNullableJsonConverter<T> : JsonConverter<Option<T>>
{
	public override Option<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
		reader.TokenType == JsonTokenType.Null
			? Prelude.None
			: options
				.GetConverter<T>()
				.IfNone(() => throw new JsonException($"Could not get converter for {typeof(T)}"))
				.Read(ref reader, typeof(T), options);

	public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options) =>
		value
			.Match(some => options
					.GetConverter<T>()
					.IfNone(() => throw new JsonException($"Could not get converter for {typeof(T)}"))
					.Write(writer, some, options),
				writer.WriteNullValue
			);
}