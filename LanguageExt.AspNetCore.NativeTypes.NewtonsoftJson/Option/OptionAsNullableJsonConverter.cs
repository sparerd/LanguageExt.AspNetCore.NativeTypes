using System.Reflection;
using Newtonsoft.Json;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.NewtonsoftJson.Option;

public class OptionAsNullableJsonConverter : JsonConverter
{
	private static readonly MethodInfo WriteMethod = 
		typeof(OptionAsNullableJsonConverter).GetMethod(nameof(WriteJson), BindingFlags.Public | BindingFlags.Static)!;
	private static readonly MethodInfo ReadMethod = 
		typeof(OptionAsNullableJsonConverter).GetMethod(nameof(ReadJson), BindingFlags.Public | BindingFlags.Static)!;

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
			.IfNone(writer.WriteNull);

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
			Some: v => serializer.Serialize(writer, v, typeof(T)),
			None: writer.WriteNull);

	public static Option<T> ReadJson<T>(JsonReader reader, JsonSerializer serializer) =>
		reader.TokenType == JsonToken.Null
			? None
			: serializer.Deserialize<T>(reader);
}