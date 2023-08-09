using System.Reflection;
using LanguageExt.Common;
using LanguageExt.Effects.Traits;
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

	public static Aff<Unit> ExecuteResult(IActionResult result, HttpContext context) =>
		Aff(() => result.ExecuteResultAsync(BuildActionContext(context)).ToUnit().ToValue());

	public static Eff<IActionResult> GetEffResult(
		Func<Fin<IActionResult>, IActionResult> unwrapper, 
		OutputFormatterWriteContext context
	) =>
		Eff(() => (IActionResult)typeof(OutputFormat)
			.GetMethod(nameof(RunEff), BindingFlags.Static | BindingFlags.NonPublic)!
			.MakeGenericMethod(context.ObjectType!.GetGenericArguments()[0])
			.Invoke(null, new object[] { Optional(context.Object), unwrapper })!);

	public static Eff<IActionResult> GetEffRuntimeResult<RT>(
		Func<Fin<IActionResult>, IActionResult> unwrapper,
		Func<IServiceProvider, RT> runtimeProvider,
		OutputFormatterWriteContext context
	) where RT : struct =>
		Eff(() =>
		{
			var genericArgs = context.ObjectType!.GetGenericArguments();
			return (IActionResult) typeof(OutputFormat)
				.GetMethod(nameof(RunEffRt), BindingFlags.Static | BindingFlags.NonPublic)!
				.MakeGenericMethod(genericArgs[0], genericArgs[1])
				.Invoke(null, new object[] {Optional(context.Object), unwrapper, runtimeProvider, context.HttpContext.RequestServices})!;
		});

	public static Aff<IActionResult> GetAffResult(
		Func<Fin<IActionResult>, IActionResult> unwrapper, 
		OutputFormatterWriteContext context
	) =>
		Aff(() => (ValueTask<IActionResult>)typeof(OutputFormat)
			.GetMethod(nameof(RunAff), BindingFlags.Static | BindingFlags.NonPublic)!
			.MakeGenericMethod(context.ObjectType!.GetGenericArguments()[0])
			.Invoke(null, new object[] { Optional(context.Object), unwrapper })!);

	public static Aff<IActionResult> GetAffRuntimeResult<RT>(
		Func<Fin<IActionResult>, IActionResult> unwrapper,
		Func<IServiceProvider, RT> runtimeProvider,
		OutputFormatterWriteContext context
	) where RT : struct, HasCancel<RT> =>
		Aff(() =>
		{
			var genericArgs = context.ObjectType!.GetGenericArguments();
			return (ValueTask<IActionResult>) typeof(OutputFormat)
				.GetMethod(nameof(RunAffRt), BindingFlags.Static | BindingFlags.NonPublic)!
				.MakeGenericMethod(genericArgs[0], genericArgs[1])
				.Invoke(null, new object[] {Optional(context.Object), unwrapper, runtimeProvider, context.HttpContext.RequestServices})!;
		});

	private static IActionResult RunEff<T>(Option<object> optEffect, Func<Fin<IActionResult>, IActionResult> unwrapper)
	{
		var eff =
			from effect in optEffect.ToEff(new BottomError())
			from e in EnsureEffReturnsActionResult<T>(effect)
			from _ in guardnot(e == null, Errors.Bottom)
			select e;
		var fin = eff.Run();
		return unwrapper(fin);
	}

	private static IActionResult RunEffRt<RT, T>(
		Option<object> optEffect, 
		Func<Fin<IActionResult>, IActionResult> unwrapper,
		Func<IServiceProvider, RT> runtimeProvider,
		IServiceProvider serviceProvider)
		where RT : struct
	{
		var eff =
			from effect in optEffect.ToEff(new BottomError()).WithRuntime<RT>()
			from e in EnsureEffReturnsActionResult<RT, T>(effect)
			from _ in guardnot(e == null, Errors.Bottom)
			select e;
		var fin = eff.Run(runtimeProvider(serviceProvider));
		return unwrapper(fin);
	}

	private static Eff<IActionResult> EnsureEffReturnsActionResult<T>(object effect) =>
		effect switch
		{
			Eff<IActionResult> eff => eff,
			Eff<T> t => t.Map(ToAction),
			_ => FailEff<IActionResult>(new BottomError()),
		};

	private static Eff<RT, IActionResult> EnsureEffReturnsActionResult<RT, T>(object effect) where RT : struct =>
		effect switch
		{
			Eff<RT, IActionResult> eff => eff,
			Eff<RT, T> t => t.Map(ToAction),
			_ => FailEff<IActionResult>(new BottomError()),
		};

	private static async ValueTask<IActionResult> RunAff<T>(Option<object> optEffect, Func<Fin<IActionResult>, IActionResult> unwrapper)
	{
		var aff =
			from effect in optEffect.ToEff(new BottomError())
			from e in EnsureAffReturnsActionResult<T>(effect)
			from _ in guardnot(e == null, Errors.Bottom)
			select e;
		var fin = await aff.Run();
		return unwrapper(fin);
	}

	private static async ValueTask<IActionResult> RunAffRt<RT, T>(
		Option<object> optEffect, 
		Func<Fin<IActionResult>, IActionResult> unwrapper,
		Func<IServiceProvider, RT> runtimeProvider,
		IServiceProvider serviceProvider
	) where RT : struct, HasCancel<RT>
	{
		var aff =
			from effect in optEffect.ToEff(new BottomError()).ToAffWithRuntime<RT>()
			from e in EnsureAffReturnsActionResult<RT, T>(effect)
			from _ in guardnot(e == null, Errors.Bottom)
			select e;
		var fin = await aff.Run(runtimeProvider(serviceProvider));
		return unwrapper(fin);
	}

	private static Aff<IActionResult> EnsureAffReturnsActionResult<T>(object effect) =>
		effect switch
		{
			Aff<IActionResult> aff => aff,
			Aff<T> t => t.Map(ToAction),
			_ => FailEff<IActionResult>(new BottomError()),
		};

	private static Aff<RT, IActionResult> EnsureAffReturnsActionResult<RT, T>(object effect)
		where RT : struct, HasCancel<RT> =>
		effect switch
		{
			Aff<RT, IActionResult> aff => aff,
			Aff<RT, T> t => t.Map(ToAction),
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
			new ControllerActionDescriptor()
		);
}