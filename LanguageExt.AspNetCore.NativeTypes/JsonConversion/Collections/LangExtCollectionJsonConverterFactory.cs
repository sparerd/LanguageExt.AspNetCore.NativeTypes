using System.Text.Json;
using System.Text.Json.Serialization;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.JsonConversion.Seq
{
	public class LangExtCollectionJsonConverterFactory : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert) =>
			typeToConvert.IsGenericType(typeof(Seq<>)) ||
			typeToConvert.IsGenericType(typeof(Lst<>)) ||
			typeToConvert == typeof(SeqEmpty);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
			New(typeToConvert, options)
				.IfNone(() => throw new JsonException($"Unable to create json serializer for {typeToConvert}"));

		public static Option<JsonConverter> New(Type typeToConvert, JsonSerializerOptions options) =>
			NewTypedConverter(typeToConvert, options, typeof(Seq<>), typeof(SeqJsonConverter<>)) | 
			NewTypedConverter(typeToConvert, options, typeof(Lst<>), typeof(LstJsonConverter<>)) | 
			NewEmptySeqConverter(typeToConvert);

		private static Option<JsonConverter> NewTypedConverter(
			Type typeToConvert, 
			JsonSerializerOptions options, 
			Type collectionGenericType, 
			Type converterGenericType
		) =>
			from optionType in Some(typeToConvert).Filter(type => type.IsGenericType(collectionGenericType))
			from innerType in optionType.GetGenericArguments().HeadOrNone()
			from converter in Optional(Activator.CreateInstance(
				converterGenericType.MakeGenericType(innerType),
				options) as JsonConverter)
			select converter;

		private static Option<JsonConverter> NewEmptySeqConverter(Type typeToConvert) =>
			from noneType in Some(typeToConvert).Filter(type => type == typeof(SeqEmpty))
			select new SeqEmptyJsonConverter() as JsonConverter;
	}
}
