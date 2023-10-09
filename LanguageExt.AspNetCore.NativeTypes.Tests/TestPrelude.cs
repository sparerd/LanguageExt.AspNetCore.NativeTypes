using System.Text.Json;
using Flurl.Http;
using Flurl.Http.Configuration;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests;

public static class TestPrelude
{
	public const string CONTROLLER_SOURCE = "controller";
	public const string MINIMAL_API_SOURCE = "minimal";

	public static IEnumerable<string> EndpointBindingFixtureSource() => Seq1(CONTROLLER_SOURCE);

	public static WebApplication CreateWebHost() => 
		CreateWebHost(new LanguageExtAspNetCoreOptions(), new LanguageExtJsonOptions());

	public static WebApplication CreateWebHost(LanguageExtAspNetCoreOptions o) => 
		CreateWebHost(o, new LanguageExtJsonOptions());

	public static WebApplication CreateWebHost(LanguageExtJsonOptions o) => 
		CreateWebHost(new LanguageExtAspNetCoreOptions(), o);

	public static WebApplication CreateWebHost(
		LanguageExtAspNetCoreOptions options, 
		LanguageExtJsonOptions jsonOptions
	) =>
		CreateWebHost(options, jsonOptions, identity);

	public static WebApplication CreateWebHost(
		LanguageExtAspNetCoreOptions options,
		LanguageExtJsonOptions jsonOptions,
		Func<IMvcBuilder, IMvcBuilder> cfg)
	{
		var builder = WebApplication.CreateBuilder();
		builder.Logging.SetMinimumLevel(LogLevel.Warning);
		builder.Services.AddControllers();
		builder.WebHost
			.ConfigureServices(services =>
			{
				cfg(
					services
						.AddMvc()
						.AddApplicationPart(typeof(TestPrelude).Assembly)
						.AddLanguageExtTypeSupport(options, jsonOptions)
						.AddEffAffEndpointSupport()
						.AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
					);
			}); 
		var app = builder.Build();
		app.MapControllers();
		return app;
	}

	public static IFlurlRequest Request(this WebApplication app)
	{
		var client = new FlurlClient(app.Urls.First());
		var options = app.Services.GetRequiredService<LanguageExtJsonOptions>();
		client.Settings.JsonSerializer = SetupFlurlSerializer(options);
		client.Settings.AllowedHttpStatusRange = "*";
		return client.Request();
	}

	private static ISerializer SetupFlurlSerializer(LanguageExtJsonOptions options)
	{
		var sysTextJsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		}.AddLanguageExtSupport(options);
		return new FlurlSystemTextJsonSerializerAdapter(sysTextJsonOptions);
	}

	public static int Increment(Option<int> num) => num.Match(i => i + 1, () => 0);

	public static JsonResult JsonResult(LanguageExtJsonOptions options, object value) =>
		new(value, JsonServiceExtensions.LanguageExtSerializer(options));
}