using LanguageExt.AspNetCore.NativeTypes.JsonConversion;
using LanguageExt.AspNetCore.NativeTypes.OutputFormatters;
using LanguageExt.Effects.Traits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using static LanguageExt.AspNetCore.NativeTypes.ModelBinders.ModelBinding;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes;

public record LanguageExtMvcBuilder(Func<IServiceCollection, IServiceCollection> Transform)
{
	/// <summary>
	/// Empty builder which performs no actions.
	/// </summary>
	public static readonly LanguageExtMvcBuilder Empty = new(identity);

	/// <summary>
	/// Default builder which includes basic support for model binding and
	/// System.Text.Json deserialization.
	/// </summary>
	public static readonly LanguageExtMvcBuilder Default = Empty.AddDefaultSupport();

	/// <summary>
	/// Adds all the default support options to this builder which includes
	/// basic support for model binding and System.Text.Json deserialization.
	/// </summary>
	/// <remarks>
	/// The same as calling these methods:<br/>
	/// <code>
	/// builder
	///   .AddModelBindingSupport(default)
	///   .AddSystemTextJsonSupport(default)
	/// </code>
	/// </remarks>
	/// <returns></returns>
	public LanguageExtMvcBuilder AddDefaultSupport() =>
		AddModelBindingSupport(default)
			.AddSystemTextJsonSupport(default);

	/// <summary>
	/// Adds support for model binding LanguageExt types when resolving
	/// controller endpoints. This allows using LanguageExt types directly
	/// in controller method signatures for incoming requests.
	/// </summary>
	/// <param name="additionalModelBinderProviders">
	/// Optional additional <see cref="IModelBinderProvider"/> instances to
	/// inject along with those provided by this library. Providers are added
	/// in stack order, meaning the last provider in this list will be the first
	/// one evaluated by the model binder pipeline. The providers specified here
	/// will always be evaluated before providers from this library, allowing you
	/// to override any provider if necessary.
	/// </param>
	/// <returns>
	/// A new builder with this configuration action appended.
	/// </returns>
	public LanguageExtMvcBuilder AddModelBindingSupport(
		Seq<IModelBinderProvider> additionalModelBinderProviders
	) =>
		new(services => Transform(services)
			.Configure<MvcOptions>(o => ConfigureModelBinders(additionalModelBinderProviders, o.ModelBinderProviders))
		);

	/// <summary>
	/// Provides support for returning <see cref="Eff{A}"/> or <see cref="Aff{A}"/> types from
	/// controller endpoint methods directly.
	/// </summary>
	/// <param name="unwrapper">
	/// A custom unwrapping function to convert <see cref="Fin{A}"/> to <see cref="IActionResult"/>.
	/// Use this to customize how failures are converted into <see cref="IActionResult"/>. Providing
	/// None/default will use the default unwrapping logic.
	/// </param>
	/// <returns>
	/// A new builder with this configuration action appended.
	/// </returns>
	public LanguageExtMvcBuilder AddEffAffEndpointSupport(
		Option<Func<Fin<IActionResult>, IActionResult>> unwrapper
	) =>
		new(services => Transform(services)
			.Configure<MvcOptions>(o => o
				.AddEffAffEndpointSupport(unwrapper.IfNone(OutputFormat.DefaultErrorHandler)))
		);

	/// <summary>
	/// Provides support for returning <see cref="Eff{RT,A}"/> or <see cref="Aff{RT,A}"/> types from
	/// controller endpoint methods directly.
	/// 
	/// <br/><br/>This may be called multiple times with a different concrete <see cref="RT"/> implementation
	/// to add support for multiple runtimes.
	/// </summary>
	/// <typeparam name="RT"></typeparam>
	/// <param name="runtimeProvider">
	/// Function that will provide a configured runtime when the effect is run.
	/// An <see cref="IServiceProvider"/> instance is provided to allow access to dependencies.
	/// </param>
	/// <param name="unwrapper">
	/// A custom unwrapping function to convert <see cref="Fin{A}"/> to <see cref="IActionResult"/>.
	/// Use this to customize how failures are converted into <see cref="IActionResult"/>. Providing
	/// None/default will use the default unwrapping logic.
	/// </param>
	/// <returns>
	/// A new builder with this configuration action appended.
	/// </returns>
	public LanguageExtMvcBuilder AddEffAffEndpointSupport<RT>(
		Func<IServiceProvider, RT> runtimeProvider,
		Option<Func<Fin<IActionResult>, IActionResult>> unwrapper
	) where RT : struct, HasCancel<RT> =>
		new(services => Transform(services)
			.Configure<MvcOptions>(o => o
				.AddEffAffEndpointSupport(
					unwrapper.IfNone(OutputFormat.DefaultErrorHandler),
					runtimeProvider
				))
		);

	/// <summary>
	/// Adds support for serializing certain LanguageExt types to JSON using System.Text.Json.
	/// </summary>
	/// <param name="options">
	/// Options to customize how certain LanguageExt types are serialized.
	/// </param>
	/// <returns>
	/// A new builder with this configuration action appended.
	/// </returns>
	public LanguageExtMvcBuilder AddSystemTextJsonSupport(
		Option<LanguageExtJsonOptions> options
	) =>
		new(services => Transform(services)
			.Configure<JsonOptions>(o => o
				.JsonSerializerOptions
				.AddLanguageExtSupport(options.IfNone(() => new LanguageExtJsonOptions())))
		);
}