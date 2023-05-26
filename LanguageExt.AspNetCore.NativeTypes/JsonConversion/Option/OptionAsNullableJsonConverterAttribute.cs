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
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class OptionAsNullableJsonConverterAttribute : JsonConverterAttribute
{
	public override JsonConverter CreateConverter(Type typeToConvert) =>
		OptionAsNullableJsonConverterFactory
			.New(typeToConvert)
			.IfNone(() => throw new JsonException($"Unable to create json serializer for {typeToConvert}"));
}