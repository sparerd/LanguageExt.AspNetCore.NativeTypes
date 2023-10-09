using Newtonsoft.Json;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.NewtonsoftJson;

internal static class Extensions
{
	internal static Unit WriteEmptyArray(this JsonWriter writer)
	{
		writer.WriteStartArray();
		writer.WriteEndArray();
		return unit;
	}

	internal static Option<Type> GetInnerType(this Type t) => t.GetGenericArguments().HeadOrNone();
}