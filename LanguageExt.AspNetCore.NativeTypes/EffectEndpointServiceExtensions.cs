using LanguageExt.AspNetCore.NativeTypes.OutputFormatters;
using LanguageExt.Effects.Traits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using static LanguageExt.AspNetCore.NativeTypes.OutputFormatters.OutputFormat;

namespace LanguageExt.AspNetCore.NativeTypes;

public static class EffectEndpointServiceExtensions
{
	#region No Runtime
	/// <summary>
	/// Provides support for returning <see cref="Eff{A}"/> or <see cref="Aff{A}"/> types from
	/// controller endpoint methods directly.
	/// Uses the default exception handling logic to convert failures into <see cref="IActionResult"/>.
	/// </summary>
	/// <param name="builder"></param>
	/// <returns></returns>
	public static IMvcBuilder AddEffAffEndpointSupport(this IMvcBuilder builder) =>
		AddEffAffEndpointSupport(builder, DefaultErrorHandler);

	/// <summary>
	/// Provides support for returning <see cref="Eff{A}"/> or <see cref="Aff{A}"/> types from
	/// controller endpoint methods directly.
	/// Uses the default exception handling logic to convert failures into <see cref="IActionResult"/>.
	/// </summary>
	/// <param name="builder"></param>
	/// <returns></returns>
	public static IMvcCoreBuilder AddEffAffEndpointSupport(this IMvcCoreBuilder builder) =>
		AddEffAffEndpointSupport(builder, DefaultErrorHandler);

	/// <summary>
	/// Provides support for returning <see cref="Eff{A}"/> or <see cref="Aff{A}"/> types from
	/// controller endpoint methods directly.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="unwrapper">
	/// A custom unwrapping function to convert <see cref="Fin{A}"/> to <see cref="IActionResult"/>.
	/// Use this to customize how failures are converted into <see cref="IActionResult"/>.
	/// </param>
	/// <returns></returns>
	public static IMvcCoreBuilder AddEffAffEndpointSupport(
		this IMvcCoreBuilder builder,
		Func<Fin<IActionResult>, IActionResult> unwrapper
	) =>
		builder.AddMvcOptions(options => options.AddEffAffEndpointSupport(unwrapper));

	/// <summary>
	/// Provides support for returning <see cref="Eff{A}"/> or <see cref="Aff{A}"/> types from
	/// controller endpoint methods directly.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="unwrapper">
	/// A custom unwrapping function to convert <see cref="Fin{A}"/> to <see cref="IActionResult"/>.
	/// Use this to customize how failures are converted into <see cref="IActionResult"/>.
	/// </param>
	/// <returns></returns>
	[Obsolete("Use the AddLanguageExtTypeSupport() overloads which take a Func<LanguageExtMvcBuilder, LanguageExtMvcBuilder>")]
	public static IMvcBuilder AddEffAffEndpointSupport(
		this IMvcBuilder builder,
		Func<Fin<IActionResult>, IActionResult> unwrapper
	) =>
		builder.AddMvcOptions(options => options.AddEffAffEndpointSupport(unwrapper));

	/// <summary>
	/// Provides support for returning <see cref="Eff{A}"/> or <see cref="Aff{A}"/> types from
	/// controller endpoint methods directly.
	/// </summary>
	/// <param name="options"></param>
	/// <param name="unwrapper"></param>
	/// <returns></returns>
	[Obsolete("Use the AddLanguageExtTypeSupport() overloads which take a Func<LanguageExtMvcBuilder, LanguageExtMvcBuilder>")]
	public static MvcOptions AddEffAffEndpointSupport(
		this MvcOptions options,
		Func<Fin<IActionResult>, IActionResult> unwrapper)
	{
		// Eff<A>
		options.OutputFormatters.Insert(0, new EffectOutputFormatter(
			ctx => GetEffResult(unwrapper, ctx),
			type => type.IsGenericType(typeof(Eff<>))));

		// Aff<A>
		options.OutputFormatters.Insert(0, new EffectOutputFormatter(
			ctx => GetAffResult(unwrapper, ctx),
			type => type.IsGenericType(typeof(Aff<>))));

		options.OutputFormatters.Insert(0, new OptionOutputFormatter());
		return options;
	}
	#endregion

	#region With Runtime
	/// <summary>
	/// Provides support for returning <see cref="Eff{RT,A}"/> or <see cref="Aff{RT,A}"/> types from
	/// controller endpoint methods directly.
	/// Uses the default exception handling logic to convert failures into <see cref="IActionResult"/>.
	/// 
	/// <br/><br/>This may be called multiple times with a different concrete <see cref="RT"/> implementation
	/// to add support for multiple runtimes.
	/// </summary>
	/// <typeparam name="RT"></typeparam>
	/// <param name="builder"></param>
	/// <param name="runtimeProvider">
	/// Function that will provide a configured runtime when the effect is run.
	/// An <see cref="IServiceProvider"/> instance is provided to allow access to dependencies.
	/// </param>
	/// <returns></returns>
	public static IMvcBuilder AddEffAffEndpointSupport<RT>(
		this IMvcBuilder builder,
		Func<IServiceProvider, RT> runtimeProvider
	) where RT : struct, HasCancel<RT> =>
		builder.AddMvcOptions(options => options.AddEffAffEndpointSupport(DefaultErrorHandler, runtimeProvider));

	/// <summary>
	/// Provides support for returning <see cref="Eff{RT,A}"/> or <see cref="Aff{RT,A}"/> types from
	/// controller endpoint methods directly.
	/// Uses the default exception handling logic to convert failures into <see cref="IActionResult"/>.
	/// 
	/// <br/><br/>This may be called multiple times with a different concrete <see cref="RT"/> implementation
	/// to add support for multiple runtimes.
	/// </summary>
	/// <typeparam name="RT"></typeparam>
	/// <param name="builder"></param>
	/// <param name="runtimeProvider">
	/// Function that will provide a configured runtime when the effect is run.
	/// An <see cref="IServiceProvider"/> instance is provided to allow access to dependencies.
	/// </param>
	/// <returns></returns>
	public static IMvcCoreBuilder AddEffAffEndpointSupport<RT>(
		this IMvcCoreBuilder builder,
		Func<IServiceProvider, RT> runtimeProvider
	) where RT : struct, HasCancel<RT> =>
		builder.AddMvcOptions(options => options.AddEffAffEndpointSupport(DefaultErrorHandler, runtimeProvider));

	/// <summary>
	/// Provides support for returning <see cref="Eff{RT,A}"/> or <see cref="Aff{RT,A}"/> types from
	/// controller endpoint methods directly.
	/// 
	/// <br/><br/>This may be called multiple times with a different concrete <see cref="RT"/> implementation
	/// to add support for multiple runtimes.
	/// </summary>
	/// <typeparam name="RT"></typeparam>
	/// <param name="builder"></param>
	/// <param name="unwrapper">
	/// A custom unwrapping function to convert <see cref="Fin{A}"/> to <see cref="IActionResult"/>.
	/// Use this to customize how failures are converted into <see cref="IActionResult"/>.
	/// </param>
	/// <param name="runtimeProvider">
	/// Function that will provide a configured runtime when the effect is run.
	/// An <see cref="IServiceProvider"/> instance is provided to allow access to dependencies.
	/// </param>
	/// <returns></returns>
	public static IMvcBuilder AddEffAffEndpointSupport<RT>(
		this IMvcBuilder builder,
		Func<Fin<IActionResult>, IActionResult> unwrapper,
		Func<IServiceProvider, RT> runtimeProvider
	) where RT : struct, HasCancel<RT> =>
		builder.AddMvcOptions(options => options.AddEffAffEndpointSupport(unwrapper, runtimeProvider));

	/// <summary>
	/// Provides support for returning <see cref="Eff{RT,A}"/> or <see cref="Aff{RT,A}"/> types from
	/// controller endpoint methods directly.
	/// 
	/// <br/><br/>This may be called multiple times with a different concrete <see cref="RT"/> implementation
	/// to add support for multiple runtimes.
	/// </summary>
	/// <typeparam name="RT"></typeparam>
	/// <param name="builder"></param>
	/// <param name="unwrapper">
	/// A custom unwrapping function to convert <see cref="Fin{A}"/> to <see cref="IActionResult"/>.
	/// Use this to customize how failures are converted into <see cref="IActionResult"/>.
	/// </param>
	/// <param name="runtimeProvider">
	/// Function that will provide a configured runtime when the effect is run.
	/// An <see cref="IServiceProvider"/> instance is provided to allow access to dependencies.
	/// </param>
	/// <returns></returns>
	public static IMvcCoreBuilder AddEffAffEndpointSupport<RT>(
		this IMvcCoreBuilder builder,
		Func<Fin<IActionResult>, IActionResult> unwrapper,
		Func<IServiceProvider, RT> runtimeProvider
	) where RT : struct, HasCancel<RT> =>
		builder.AddMvcOptions(options => options.AddEffAffEndpointSupport(unwrapper, runtimeProvider));

	/// <summary>
	/// Provides support for returning <see cref="Eff{RT,A}"/> or <see cref="Aff{RT,A}"/> types from
	/// controller endpoint methods directly.
	/// 
	/// <br/><br/>This may be called multiple times with a different concrete <see cref="RT"/> implementation
	/// to add support for multiple runtimes.
	/// </summary>
	/// <typeparam name="RT"></typeparam>
	/// <param name="options"></param>
	/// <param name="unwrapper">
	/// A custom unwrapping function to convert <see cref="Fin{A}"/> to <see cref="IActionResult"/>.
	/// Use this to customize how failures are converted into <see cref="IActionResult"/>.
	/// </param>
	/// <param name="runtimeProvider">
	/// Function that will provide a configured runtime when the effect is run.
	/// An <see cref="IServiceProvider"/> instance is provided to allow access to dependencies.
	/// </param>
	/// <returns></returns>
	public static MvcOptions AddEffAffEndpointSupport<RT>(
		this MvcOptions options,
		Func<Fin<IActionResult>, IActionResult> unwrapper,
		Func<IServiceProvider, RT> runtimeProvider
	) where RT : struct, HasCancel<RT>
	{
		// Eff<RT, A>
		options.OutputFormatters.Insert(0, new EffectOutputFormatter(
			ctx => GetEffRuntimeResult(unwrapper, runtimeProvider, ctx),
			type => type.IsGenericType(typeof(Eff<,>))));

		// Aff<RT, A>
		options.OutputFormatters.Insert(0, new EffectOutputFormatter(
			ctx => GetAffRuntimeResult(unwrapper, runtimeProvider, ctx),
			type => type.IsGenericType(typeof(Aff<,>))));

		return options;
	}
	#endregion
}