using System.Text.Json;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion.Option;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion.Seq;
using LanguageExt.AspNetCore.NativeTypes.ModelBinders.Option;
using LanguageExt.AspNetCore.NativeTypes.ModelBinders.Seq;
using LanguageExt.AspNetCore.NativeTypes.OutputFormatters;
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
	)
	{
		builder.Services.AddSingleton(langExtOptions);
		ConfigureJson(builder, langExtOptions);

		return builder
			.AddMvcOptions(o =>
			{
				ConfigureModelBinders(langExtOptions, o.ModelBinderProviders);
			});
	}

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
		builder.AddMvcOptions(options =>
		{
			options.OutputFormatters.Insert(0, new EffectOutputFormatter(ctx => GetEffResult(unwrapper, ctx), type => type.IsGenericType(typeof(Eff<>))));
			options.OutputFormatters.Insert(0, new EffectOutputFormatter(ctx => GetAffResult(unwrapper, ctx), type => type.IsGenericType(typeof(Aff<>))));
		});

	private static void ConfigureJson(IMvcBuilder builder, LanguageExtAspNetCoreOptions langExtOptions)
	{
		void Conf(JsonSerializerOptions options) => options.AddLanguageExtSupport(langExtOptions);

		// Used by minimal api
		builder.Services.Configure<JsonOptions>(o => Conf(o.SerializerOptions));

		// Used by controllers
		builder.AddJsonOptions(o => Conf(o.JsonSerializerOptions));
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