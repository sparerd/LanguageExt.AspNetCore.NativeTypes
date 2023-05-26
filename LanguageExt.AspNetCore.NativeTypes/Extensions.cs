using System.Text.Json;
using System.Text.Json.Serialization;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes;

public static class Extensions
{
	internal static bool IsGenericType(this Type t, Type genericTypeDefinition) => 
		t.IsGenericType && 
		t.GetGenericTypeDefinition() == genericTypeDefinition;

	public static Option<JsonConverter<A>> GetConverter<A>(this JsonSerializerOptions options) =>
		Optional(options.GetConverter(typeof(A)) as JsonConverter<A>);
}