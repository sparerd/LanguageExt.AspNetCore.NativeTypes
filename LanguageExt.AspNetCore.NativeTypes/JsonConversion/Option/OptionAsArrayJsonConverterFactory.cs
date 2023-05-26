using System.Text.Json;
using System.Text.Json.Serialization;

namespace LanguageExt.AspNetCore.NativeTypes.JsonConversion.Option;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// From https://github.com/louthy/language-ext/discussions/1132#discussioncomment-3860993
/// </remarks>
public class OptionAsArrayJsonConverterFactory : JsonConverterFactory
{
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var innerType = typeToConvert.GetGenericArguments().Match(
			() => throw new Exception("no generic type argument"),
			x => x,
			(x, xs) => throw new Exception("more than one generic type argument")
		);
		return Activator.CreateInstance(
			typeof(OptionAsArrayJsonConverter<>).MakeGenericType(innerType), 
			options) as JsonConverter;
	}

	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType(typeof(Option<>));
}