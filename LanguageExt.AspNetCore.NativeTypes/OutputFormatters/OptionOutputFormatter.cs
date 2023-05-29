using Microsoft.AspNetCore.Mvc.Formatters;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.OutputFormatters
{
	public class OptionOutputFormatter : IOutputFormatter
	{
		public bool CanWriteResult(OutputFormatterCanWriteContext context) => 
			Optional(context.ObjectType)
				.Map(TypeSupported)
				.IfNone(false);

		private static bool TypeSupported(Type objectType) =>
			objectType.IsGenericType(typeof(Option<>)) ||
			objectType == typeof(OptionNone);

		public Task WriteAsync(OutputFormatterWriteContext context)
		{
			throw new NotImplementedException();
		}
	}
}
