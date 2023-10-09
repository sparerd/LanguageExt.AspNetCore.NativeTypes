using LanguageExt.AspNetCore.NativeTypes.JsonConversion;
using LanguageExt.AspNetCore.NativeTypes.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes;

public static class ServiceExtensions
{
	/// <summary>
	/// Configure integration options for using LanguageExt in ASP.Net Core.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="configure">
	/// Fluent builder for configuring integration options
	/// </param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	public static IMvcBuilder AddLanguageExtTypeSupport(
		this IMvcBuilder builder,
		Func<LanguageExtMvcBuilder, LanguageExtMvcBuilder> configure)
	{
		_ = configure ?? throw new ArgumentNullException(nameof(configure));

		var config = configure(LanguageExtMvcBuilder.Empty);
		config.Transform(builder.Services);
		return builder;
	}

	/// <summary>
	/// Configure integration options for using LanguageExt in ASP.Net Core.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="configure">
	/// Fluent builder for configuring integration options
	/// </param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	public static IMvcCoreBuilder AddLanguageExtTypeSupport(
		this IMvcCoreBuilder builder,
		Func<LanguageExtMvcBuilder, LanguageExtMvcBuilder> configure)
	{
		_ = configure ?? throw new ArgumentNullException(nameof(configure));

		var config = configure(LanguageExtMvcBuilder.Empty);
		config.Transform(builder.Services);
		return builder;
	}

	/// <summary>
	/// Configures default integration options for using LanguageExt in ASP.Net Core.
	/// </summary>
	/// <remarks>
	/// The same as calling this method with a custom configuration of:<br/>
	/// <code>
	/// builder => builder
	///   .AddModelBindingSupport(default)
	///   .AddSystemTextJsonSupport(default)
	/// </code>
	/// </remarks>
	/// <param name="builder"></param>
	/// <returns></returns>
	public static IMvcBuilder AddLanguageExtTypeSupport(this IMvcBuilder builder) =>
		builder.AddLanguageExtTypeSupport(_ => LanguageExtMvcBuilder.Default);

	/// <summary>
	/// Configures default integration options for using LanguageExt in ASP.Net Core.
	/// </summary>
	/// <remarks>
	/// The same as calling this method with a custom configuration of:<br/>
	/// <code>
	/// builder => builder
	///   .AddModelBindingSupport(default)
	///   .AddSystemTextJsonSupport(default)
	/// </code>
	/// </remarks>
	/// <param name="builder"></param>
	/// <returns></returns>
	public static IMvcCoreBuilder AddLanguageExtTypeSupport(this IMvcCoreBuilder builder) =>
		builder.AddLanguageExtTypeSupport(_ => LanguageExtMvcBuilder.Default);

	[Obsolete("Use the overloads which take a Func<LanguageExtMvcBuilder, LanguageExtMvcBuilder>")]
	public static IMvcBuilder AddLanguageExtTypeSupport(
		this IMvcBuilder builder,
		LanguageExtAspNetCoreOptions options
	) =>
		AddLanguageExtTypeSupport(
			builder,
			options,
			new LanguageExtJsonOptions());

	[Obsolete("Use the overloads which take a Func<LanguageExtMvcBuilder, LanguageExtMvcBuilder>")]
	public static IMvcBuilder AddLanguageExtTypeSupport(
		this IMvcBuilder builder,
		LanguageExtJsonOptions jsonOptions
	) =>
		AddLanguageExtTypeSupport(
			builder,
			new LanguageExtAspNetCoreOptions(),
			jsonOptions);

	[Obsolete("Use the overloads which take a Func<LanguageExtMvcBuilder, LanguageExtMvcBuilder>")]
	public static IMvcBuilder AddLanguageExtTypeSupport(
		this IMvcBuilder builder,
		LanguageExtAspNetCoreOptions langExtOptions,
		LanguageExtJsonOptions jsonOptions
	) =>
		AddTypeSupport(
			builder.Services,
			cfg => ignore(builder.AddMvcOptions(cfg)),
			opts => ignore(builder.AddJsonOptions(opts)),
			langExtOptions,
			jsonOptions
		).Return(builder);

	[Obsolete("Use the overloads which take a Func<LanguageExtMvcBuilder, LanguageExtMvcBuilder>")]
	public static IMvcCoreBuilder AddLanguageExtTypeSupport(
		this IMvcCoreBuilder builder,
		LanguageExtAspNetCoreOptions langExtOptions
	) =>
		AddLanguageExtTypeSupport(
			builder,
			langExtOptions,
			new LanguageExtJsonOptions());

	[Obsolete("Use the overloads which take a Func<LanguageExtMvcBuilder, LanguageExtMvcBuilder>")]
	public static IMvcCoreBuilder AddLanguageExtTypeSupport(
		this IMvcCoreBuilder builder,
		LanguageExtJsonOptions jsonOptions
	) =>
		AddLanguageExtTypeSupport(
			builder,
			new LanguageExtAspNetCoreOptions(),
			jsonOptions);

	/// <summary>
	/// Adds support for serializing/deserializing LanguageExt types as well as
	/// ASP.Net model binders.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="langExtOptions"></param>
	/// <param name="jsonOptions"></param>
	/// <returns></returns>
	[Obsolete("Use the overloads which take a Func<LanguageExtMvcBuilder, LanguageExtMvcBuilder>")]
	public static IMvcCoreBuilder AddLanguageExtTypeSupport(
		this IMvcCoreBuilder builder,
		LanguageExtAspNetCoreOptions langExtOptions,
		LanguageExtJsonOptions jsonOptions
	) =>
		AddTypeSupport(
			builder.Services,
			cfg => ignore(builder.AddMvcOptions(cfg)),
			opts => ignore(builder.AddJsonOptions(opts)),
			langExtOptions, 
			jsonOptions
		).Return(builder);

	private static Unit AddTypeSupport(
		IServiceCollection services,
		Func<Action<MvcOptions>, Unit> mvcOptsConfig,
		Func<Action<Microsoft.AspNetCore.Mvc.JsonOptions>, Unit> jsonCfg,
		LanguageExtAspNetCoreOptions langExtOptions,
		LanguageExtJsonOptions jsonOptions)
	{
		services.AddSingleton(langExtOptions);
		services.AddSingleton(jsonOptions);
		JsonServiceExtensions.ConfigureJson(jsonCfg, jsonOptions)(services);
		ConfigureModelBinders(langExtOptions, mvcOptsConfig);
		return unit;
	}

	public static Unit ConfigureModelBinders(
		LanguageExtAspNetCoreOptions options,
		Func<Action<MvcOptions>, Unit> mvcOptsConfig
	) =>
		mvcOptsConfig(o => ModelBinding.ConfigureModelBinders(options.AdditionalModelBinderProviders, o.ModelBinderProviders));
}