using System.Text.Json;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion.Option;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion.Seq;
using LanguageExt.AspNetCore.NativeTypes.ModelBinders.Option;
using LanguageExt.AspNetCore.NativeTypes.ModelBinders.Seq;
using LanguageExt.AspNetCore.NativeTypes.OutputFormatters;
using LanguageExt.Effects.Traits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using static LanguageExt.AspNetCore.NativeTypes.OutputFormatters.OutputFormat;
using static LanguageExt.Prelude;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace LanguageExt.AspNetCore.NativeTypes;

public static class ServiceExtensions
{
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
	public static IMvcBuilder AddEffAffEndpointSupport(
		this IMvcBuilder builder,
		Func<Fin<IActionResult>, IActionResult> unwrapper
	) =>
		builder.AddMvcOptions(options => options.AddEffAffEndpointSupport(unwrapper));

	public static IMvcBuilder AddEffAffEndpointSupport<RT>(
		this IMvcBuilder builder,
		Func<IServiceProvider, RT> runtimeProvider
	) where RT : struct, HasCancel<RT> =>
		builder.AddMvcOptions(options => options.AddEffAffEndpointSupport(DefaultErrorHandler, runtimeProvider));

	public static IMvcCoreBuilder AddEffAffEndpointSupport<RT>(
		this IMvcCoreBuilder builder,
		Func<IServiceProvider, RT> runtimeProvider
	) where RT : struct, HasCancel<RT> =>
		builder.AddMvcOptions(options => options.AddEffAffEndpointSupport(DefaultErrorHandler, runtimeProvider));

	public static IMvcBuilder AddEffAffEndpointSupport<RT>(
		this IMvcBuilder builder,
		Func<Fin<IActionResult>, IActionResult> unwrapper,
		Func<IServiceProvider, RT> runtimeProvider
	) where RT : struct, HasCancel<RT> => 
		builder.AddMvcOptions(options => options.AddEffAffEndpointSupport(unwrapper, runtimeProvider));

	public static IMvcCoreBuilder AddEffAffEndpointSupport<RT>(
		this IMvcCoreBuilder builder,
		Func<Fin<IActionResult>, IActionResult> unwrapper,
		Func<IServiceProvider, RT> runtimeProvider
	) where RT : struct, HasCancel<RT> =>
		builder.AddMvcOptions(options => options.AddEffAffEndpointSupport(unwrapper, runtimeProvider));

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

	/// <summary>
	/// 
	/// </summary>
	/// <param name="builder"></param>
	/// <returns></returns>
	public static IMvcBuilder AddLanguageExtTypeSupport(this IMvcBuilder builder) =>
		AddLanguageExtTypeSupport(builder, new LanguageExtAspNetCoreOptions());

	/// <summary>
	/// 
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="langExtOptions"></param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	/// <returns></returns>
	public static IMvcBuilder AddLanguageExtTypeSupport(
		this IMvcBuilder builder,
		LanguageExtAspNetCoreOptions langExtOptions
	) =>
		AddTypeSupport(
			builder.Services,
			cfg => ignore(builder.AddMvcOptions(cfg)),
			opts => ignore(builder.AddJsonOptions(opts)),
			langExtOptions
		).Return(builder);

	public static IMvcCoreBuilder AddLanguageExtTypeSupport(this IMvcCoreBuilder builder) =>
		AddLanguageExtTypeSupport(builder, new LanguageExtAspNetCoreOptions());

	public static IMvcCoreBuilder AddLanguageExtTypeSupport(
		this IMvcCoreBuilder builder,
		LanguageExtAspNetCoreOptions langExtOptions
	) =>
		AddTypeSupport(
			builder.Services,
			cfg => ignore(builder.AddMvcOptions(cfg)),
			opts => ignore(builder.AddJsonOptions(opts)),
			langExtOptions
		).Return(builder);

	private static Unit AddTypeSupport(
		IServiceCollection services,
		Func<Action<MvcOptions>, Unit> mvcOptsConfig,
		Func<Action<Microsoft.AspNetCore.Mvc.JsonOptions>, Unit> jsonCfg,
		LanguageExtAspNetCoreOptions langExtOptions)
	{
		services.AddSingleton(langExtOptions);
		ConfigureJson(services, jsonCfg, langExtOptions);
		mvcOptsConfig(o => ConfigureModelBinders(langExtOptions, o.ModelBinderProviders));
		return unit;
	}

	private static void ConfigureJson(
		IServiceCollection services,
		Func<Action<Microsoft.AspNetCore.Mvc.JsonOptions>, Unit> jsonCfg,
		LanguageExtAspNetCoreOptions langExtOptions)
	{
		void Conf(JsonSerializerOptions options) => options.AddLanguageExtSupport(langExtOptions);
		
		// Used by minimal api
		services.Configure<JsonOptions>(o => Conf(o.SerializerOptions));

		// Used by controllers
		jsonCfg(o => Conf(o.JsonSerializerOptions));
	}

	private static Unit ConfigureModelBinders(LanguageExtAspNetCoreOptions options, IList<IModelBinderProvider> providers) =>
		Seq<IModelBinderProvider>(
				new OptionModelBinderProvider(),
				new SeqModelBinderProvider()
			)
			.Append(options.AdditionalModelBinderProviders)
			.Map(InsertProvider(providers, GetTargetInsertionProviderIndex(providers)))
			.Consume();

	/// <summary>
	/// Insert new providers just after the last known generic provider (Headers)
	/// This is a bit hacky and might be fragile, but the alternative is reimplementing
	/// source provider logic.
	/// https://github.com/dotnet/aspnetcore/blob/775b001508b2678426319b8cd27453fe90b0f250/src/Mvc/Mvc.Core/src/Infrastructure/MvcCoreMvcOptionsSetup.cs#L51
	/// </summary>
	/// <param name="providers"></param>
	/// <returns></returns>
	private static int GetTargetInsertionProviderIndex(IEnumerable<IModelBinderProvider> providers) =>
		providers
			.Select(p => p.GetType().Name)
			.TakeWhile(binderName => binderName != nameof(HeaderModelBinderProvider))
			.Count() + 1;

	private static Func<IModelBinderProvider, Unit> InsertProvider(IList<IModelBinderProvider> providers, int index) =>
		provider => fun(() => providers.Insert(index, provider))();

	public static JsonSerializerOptions LanguageExtSerializer() =>
		LanguageExtSerializer(new LanguageExtAspNetCoreOptions());

	public static JsonSerializerOptions LanguageExtSerializer(LanguageExtAspNetCoreOptions options) => 
		new JsonSerializerOptions().AddLanguageExtSupport(options);

	public static JsonSerializerOptions AddLanguageExtSupport(this JsonSerializerOptions jsonOptions) =>
		AddLanguageExtSupport(jsonOptions, new LanguageExtAspNetCoreOptions());

	public static JsonSerializerOptions AddLanguageExtSupport(this JsonSerializerOptions jsonOptions, LanguageExtAspNetCoreOptions options)
	{
		jsonOptions.Converters.Add(
			options.OptionSerializationStrategy switch
			{
				OptionSerializationStrategy.AsNullable => new OptionAsNullableJsonConverterFactory(),
				OptionSerializationStrategy.AsArray => new OptionAsArrayJsonConverterFactory(),
				_ => throw new ArgumentOutOfRangeException(nameof(LanguageExtAspNetCoreOptions
					.OptionSerializationStrategy)),
			});

		jsonOptions.Converters.Add(new LangExtCollectionJsonConverterFactory());
		return jsonOptions;
	}
}