using System.Text.Json;
using System.Text.Json.Serialization;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.JsonConversion.Option;

public class OptionAsNullableJsonConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => 
		typeToConvert.IsGenericType(typeof(Option<>)) ||
		typeToConvert == typeof(OptionNone);

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
		New(typeToConvert)
			.IfNone(() => throw new JsonException($"Unable to create json serializer for {typeToConvert}"));

	public static Option<JsonConverter> New(Type typeToConvert) =>
		NewTypedConverter(typeToConvert) | 
		NewNoneConverter(typeToConvert);

	private static Option<JsonConverter> NewTypedConverter(Type typeToConvert) =>
		from optionType in Some(typeToConvert).Filter(type => type.IsGenericType(typeof(Option<>)))
		from innerType in optionType.GetGenericArguments().HeadOrNone()
		from converter in Optional(Activator.CreateInstance(typeof(OptionAsNullableJsonConverter<>).MakeGenericType(innerType)) as JsonConverter)
		select converter;

	private static Option<JsonConverter> NewNoneConverter(Type typeToConvert) =>
		from noneType in Some(typeToConvert).Filter(type => type == typeof(OptionNone))
		select new OptionAsNullableJsonConverter() as JsonConverter;
}