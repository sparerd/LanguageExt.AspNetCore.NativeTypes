using System.Text.Json;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion.Option;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion.Seq;
using Microsoft.Extensions.DependencyInjection;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes;

public delegate Unit JsonConfigurationFn(IServiceCollection services);

public static class JsonServiceExtensions
{
	/// <summary>
	/// Creates a new <see cref="JsonSerializerOptions"/> configured with default
	/// <see cref="LanguageExtJsonOptions"/>.
	/// </summary>
	/// <returns></returns>
	public static JsonSerializerOptions LanguageExtSerializer() =>
		LanguageExtSerializer(new LanguageExtJsonOptions());

	/// <summary>
	/// Creates a new <see cref="JsonSerializerOptions"/> configured with the given
	/// <see cref="LanguageExtJsonOptions"/>.
	/// </summary>
	/// <param name="options"></param>
	/// <returns></returns>
	public static JsonSerializerOptions LanguageExtSerializer(LanguageExtJsonOptions options) =>
		new JsonSerializerOptions().AddLanguageExtSupport(options);

	/// <summary>
	/// Configures support for serializing LanguageExt types on this <see cref="JsonSerializerOptions"/>.
	/// Uses default <see cref="LanguageExtJsonOptions"/>.
	/// </summary>
	/// <param name="jsonOptions"></param>
	/// <returns></returns>
	public static JsonSerializerOptions AddLanguageExtSupport(this JsonSerializerOptions jsonOptions) =>
		AddLanguageExtSupport(jsonOptions, new LanguageExtJsonOptions());

	/// <summary>
	/// Configures support for serializing LanguageExt types on this <see cref="JsonSerializerOptions"/>
	/// using the given <see cref="LanguageExtJsonOptions"/>.
	/// </summary>
	/// <param name="jsonOptions"></param>
	/// <param name="options">
	/// Options to control how LanguageExt types are serialized.
	/// </param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public static JsonSerializerOptions AddLanguageExtSupport(this JsonSerializerOptions jsonOptions, LanguageExtJsonOptions options)
	{
		_ = options ?? throw new ArgumentNullException(nameof(options));

		jsonOptions.Converters.Add(
			options.OptionSerializationStrategy switch
			{
				OptionSerializationStrategy.AsNullable => new OptionAsNullableJsonConverterFactory(),
				OptionSerializationStrategy.AsArray => new OptionAsArrayJsonConverterFactory(),
				_ => throw new ArgumentOutOfRangeException(nameof(LanguageExtJsonOptions.OptionSerializationStrategy)),
			});

		jsonOptions.Converters.Add(new LangExtCollectionJsonConverterFactory());
		return jsonOptions;
	}

	/// <summary>
	/// Adds support for serializing LanguageExt using the given <see cref="LanguageExtJsonOptions"/>.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="jsonOptions"></param>
	/// <returns></returns>
	public static IMvcCoreBuilder AddLanguageExtJsonSupport(
		this IMvcCoreBuilder builder,
		LanguageExtJsonOptions jsonOptions
	) =>
		AddTypeSupport(
			builder.Services,
			opts => ignore(builder.AddJsonOptions(opts)),
			jsonOptions
		).Return(builder);

	private static Unit AddTypeSupport(
		IServiceCollection services,
		Func<Action<Microsoft.AspNetCore.Mvc.JsonOptions>, Unit> jsonCfg,
		LanguageExtJsonOptions jsonOptions)
	{
		ConfigureJson(jsonCfg, jsonOptions)(services);
		return unit;
	}

	internal static JsonConfigurationFn ConfigureJson(
		Func<Action<Microsoft.AspNetCore.Mvc.JsonOptions>, Unit> jsonCfg,
		LanguageExtJsonOptions langExtOptions
	) =>
		services =>
		{
			void Conf(JsonSerializerOptions options) => options.AddLanguageExtSupport(langExtOptions);

			// Used by minimal api
			services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o => Conf(o.SerializerOptions));

			// Used by controllers
			jsonCfg(o => Conf(o.JsonSerializerOptions));
			return unit;
		};
}