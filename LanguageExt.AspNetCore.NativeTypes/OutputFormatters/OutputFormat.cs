using System.Reflection;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Routing;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.OutputFormatters;

public static class OutputFormat
{
	/// <summary>
	/// 
	/// </summary>
	public static readonly Func<Fin<IActionResult>, IActionResult> DefaultErrorHandler =
		fin => fin.IfFail(err => new JsonResult(err)
		{
			StatusCode = StatusCodes.Status500InternalServerError,
		});
	
	public static Eff<IActionResult> GetEffResult(
		Func<Fin<IActionResult>, IActionResult> unwrapper, 
		OutputFormatterWriteContext context
	) =>
		Eff(() => (IActionResult)typeof(OutputFormat)
			.GetMethod(nameof(RunEff), BindingFlags.Static | BindingFlags.NonPublic)!
			.MakeGenericMethod(context.ObjectType!.GetGenericArguments()[0])
			.Invoke(null, new[] { context.Object, unwrapper })!);

	public static Aff<IActionResult> GetAffResult(
		Func<Fin<IActionResult>, IActionResult> unwrapper, 
		OutputFormatterWriteContext context
	) =>
		Aff(() => (ValueTask<IActionResult>)typeof(OutputFormat)
			.GetMethod(nameof(RunAff), BindingFlags.Static | BindingFlags.NonPublic)!
			.MakeGenericMethod(context.ObjectType!.GetGenericArguments()[0])
			.Invoke(null, new[] { context.Object, unwrapper })!);

	private static IActionResult RunEff<T>(object effect, Func<Fin<IActionResult>, IActionResult> unwrapper)
	{
		var eff =
			from e in EnsureEffReturnsActionResult<T>(effect)
			from _ in guardnot(e == null, Errors.Bottom)
			select e;
		var fin = eff.Run();
		return unwrapper(fin);
	}

	private static Eff<IActionResult> EnsureEffReturnsActionResult<T>(object effect) =>
		effect switch
		{
			Eff<IActionResult> eff => eff,
			Eff<T> t => t.Map(ToAction),
			_ => FailEff<IActionResult>(new BottomError()),
		};

	private static async ValueTask<IActionResult> RunAff<T>(object effect, Func<Fin<IActionResult>, IActionResult> unwrapper)
	{
		var aff =
			from e in EnsureAffReturnsActionResult<T>(effect)
			from _ in guardnot(e == null, Errors.Bottom)
			select e;
		var fin = await aff.Run();
		return unwrapper(fin);
	}

	private static Aff<IActionResult> EnsureAffReturnsActionResult<T>(object effect) =>
		effect switch
		{
			Aff<IActionResult> aff => aff,
			Aff<T> t => t.Map(ToAction),
			_ => FailEff<IActionResult>(new BottomError()),
		};

	private static IActionResult ToAction<T>(T value) =>
		typeof(T) switch
		{
			{ } type when IsSpecificActionResult(type) => (IActionResult)value,
			_ => new JsonResult(value),
		};

	/// <summary>
	/// True when the given type implements <see cref="IActionResult"/>.
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	private static bool IsSpecificActionResult(Type t) => t.IsAssignableTo(typeof(IActionResult));

	public static ActionContext BuildActionContext(HttpContext context) =>
		new(
			context,
			context.GetRouteData(),
			context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>()!
		);
}