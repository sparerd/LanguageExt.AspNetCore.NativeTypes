using Flurl.Http;
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
		builder.Logging.SetMinimumLevel(LogLevel.Trace);
		builder.Services.AddControllers();
		builder.WebHost
			.ConfigureServices(services =>
			{
				services
					.AddMvc()
					.AddApplicationPart(typeof(TestPrelude).Assembly)
					.AddLanguageExtTypeSupport(options);
			}); 
		var app = builder.Build();
		app.MapControllers();
		return app;
	}

	public static IFlurlRequest Request(this WebApplication app)
	{
		var client = new FlurlClient(app.Urls.First());
		client.Settings.JsonSerializer = new FlurlSystemTextJsonSerializerAdapter(ServiceExtensions.LanguageExtSerializer(
			app.Services.GetRequiredService<LanguageExtAspNetCoreOptions>()));
		return client.Request();
	}


	public static int Increment(Option<int> num) => num.Match(i => i + 1, () => 0);

	public static JsonResult JsonResult(LanguageExtAspNetCoreOptions options, object value) =>
		new(value, ServiceExtensions.LanguageExtSerializer(options));
}