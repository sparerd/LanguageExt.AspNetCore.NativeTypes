using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using static LanguageExt.Prelude;
using static LanguageExt.AspNetCore.NativeTypes.OutputFormatters.OutputFormat;

namespace LanguageExt.AspNetCore.NativeTypes.OutputFormatters;

public class OptionOutputFormatter : IOutputFormatter
{
	public bool CanWriteResult(OutputFormatterCanWriteContext context) => 
		Optional(context.ObjectType)
			.Map(TypeSupported)
			.IfNone(false);

	private static bool TypeSupported(Type objectType) =>
		objectType.IsGenericType(typeof(Option<>)) ||
		objectType == typeof(OptionNone);

	public async Task WriteAsync(OutputFormatterWriteContext context) =>
		await Write(context).RunUnit();

	private Aff<Unit> Write(OutputFormatterWriteContext context) =>
		from result in Run(context)
		from _ in ExecuteResult(result, context.HttpContext)
		select unit;

	private Eff<IActionResult> Run(OutputFormatterWriteContext context) =>
		Eff(() => (IActionResult)typeof(OptionOutputFormatter)
			.GetMethod(nameof(Evaluate), BindingFlags.Static | BindingFlags.NonPublic)!
			.MakeGenericMethod(context.ObjectType!.GetGenericArguments()[0])
			.Invoke(null, new[] { context.Object })!);

	private static IActionResult Evaluate<T>(Option<T> option) =>
		option
			.Map<IActionResult>(val => new OkObjectResult(val))
			.IfNone(() => new NotFoundResult());
}