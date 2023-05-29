using System.Text.Json;
using System.Text.Json.Serialization;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes;

public static class Extensions
{
	internal static bool IsGenericType(this Type t, Type genericTypeDefinition) => 
		t.IsGenericType && 
		t.GetGenericTypeDefinition() == genericTypeDefinition;

	/// <summary>
	/// Determines if the generic type argument of the given type extends or implements
	/// the given <see cref="THigherType"/>.
	/// </summary>
	/// <typeparam name="THigherType"></typeparam>
	/// <param name="t"></param>
	/// <returns></returns>
	internal static bool GenericTypeAssignableTo<THigherType>(this Type t) =>
		t.IsGenericType &&
		t.GetGenericArguments()
			.HeadOrNone()
			.Map(genericType => genericType.IsAssignableTo(typeof(THigherType)))
			.IfNone(false);

	public static Option<JsonConverter<A>> GetConverter<A>(this JsonSerializerOptions options) =>
		Optional(options.GetConverter(typeof(A)) as JsonConverter<A>);
}