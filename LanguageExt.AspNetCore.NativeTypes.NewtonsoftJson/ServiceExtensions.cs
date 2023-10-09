using LanguageExt.AspNetCore.NativeTypes.JsonConversion;
using LanguageExt.AspNetCore.NativeTypes.NewtonsoftJson.Converters;
using LanguageExt.AspNetCore.NativeTypes.NewtonsoftJson.Option;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace LanguageExt.AspNetCore.NativeTypes.NewtonsoftJson;

public static class ServiceExtensions
{
	/// <summary>
	/// Adds JSON type converters for LanguageExt using default options.
	/// </summary>
	/// <param name="settings"></param>
	/// <returns></returns>
	public static JsonSerializerSettings AddLanguageExtConverters(this JsonSerializerSettings settings) => 
		settings.AddLanguageExtConverters(new LanguageExtJsonOptions());

	/// <summary>
	/// Adds JSON type converters for LanguageExt using the provided options.
	/// </summary>
	/// <param name="settings"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public static JsonSerializerSettings AddLanguageExtConverters(
		this JsonSerializerSettings settings,
		LanguageExtJsonOptions options)
	{
		settings.Converters.Insert(0, 
			options.OptionSerializationStrategy == OptionSerializationStrategy.AsNullable
				? new OptionAsNullableJsonConverter()
				: new OptionAsArrayJsonConverter());
		settings.Converters.Insert(0, new SeqJsonConverter());
		return settings;
	}

	/// <summary>
	/// Adds support for serializing certain LanguageExt types to JSON using NewtonsoftJson.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="options">
	/// Options to customize how certain LanguageExt types are serialized.
	/// </param>
	/// <returns></returns>
	public static LanguageExtMvcBuilder AddNewtonsoftJsonSupport(
		this LanguageExtMvcBuilder builder,
		Option<LanguageExtJsonOptions> options
	) => 
		new(p => builder.Transform(p)
			.Configure<MvcNewtonsoftJsonOptions>(o => o
				.SerializerSettings
				.AddLanguageExtConverters(options.IfNone(() => new LanguageExtJsonOptions())))
		);
}