using System.Text.Json;
using Flurl.Http;
using Flurl.Http.Configuration;
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

	public static WebApplication CreateWebHost() => CreateWebHost(new LanguageExtAspNetCoreOptions());

	public static WebApplication CreateWebHost(LanguageExtAspNetCoreOptions options)
	{
		var builder = WebApplication.CreateBuilder();
		builder.Logging.SetMinimumLevel(LogLevel.Warning);
		builder.Services.AddControllers();
		builder.WebHost
			.ConfigureServices(services =>
			{
				services
					.AddMvc()
					.AddApplicationPart(typeof(TestPrelude).Assembly)
					.AddLanguageExtTypeSupport(options)
					.AddEffAffEndpointSupport()
					.AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
					;
			}); 
		var app = builder.Build();
		app.MapControllers();
		return app;
	}

	public static IFlurlRequest Request(this WebApplication app)
	{
		var client = new FlurlClient(app.Urls.First());
		var options = app.Services.GetRequiredService<LanguageExtAspNetCoreOptions>();
		client.Settings.JsonSerializer = SetupFlurlSerializer(options);
		client.Settings.AllowedHttpStatusRange = "*";
		return client.Request();
	}

	private static ISerializer SetupFlurlSerializer(LanguageExtAspNetCoreOptions options)
	{
		var sysTextJsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		}.AddLanguageExtSupport(options);
		return new FlurlSystemTextJsonSerializerAdapter(sysTextJsonOptions);
	}

	public static int Increment(Option<int> num) => num.Match(i => i + 1, () => 0);

	public static JsonResult JsonResult(LanguageExtAspNetCoreOptions options, object value) =>
		new(value, ServiceExtensions.LanguageExtSerializer(options));
}