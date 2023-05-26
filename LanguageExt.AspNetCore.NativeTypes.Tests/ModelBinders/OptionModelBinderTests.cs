using System.Text.Json;
using FluentAssertions;
using Flurl.Http;
using LanguageExt.AspNetCore.NativeTypes.Tests.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using static LanguageExt.AspNetCore.NativeTypes.Tests.TestPrelude;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.ModelBinders;

[TestFixtureSource(typeof(TestPrelude), nameof(EndpointBindingFixtureSource))]
public class OptionModelBinderTests
{
	private WebApplication _app;
	private LanguageExtAspNetCoreOptions _opts;
	private readonly string _basePath;
	private const string Controller = "option";

	public OptionModelBinderTests(string basePath)
	{
		_basePath = basePath;
	}

	[SetUp]
	public async Task Setup()
	{
		_opts = new LanguageExtAspNetCoreOptions
		{
			OptionSerializationStrategy = OptionSerializationStrategy.AsNullable,
		};
		_app = CreateWebHost(_opts);
		SetupMinimalApis(_app);
		await _app.StartAsync();
	}

	private void SetupMinimalApis(WebApplication app)
	{
		IResult Json(object value) => Results.Json(value, new JsonSerializerOptions().AddLanguageExtSupport(_opts));

		app.MapPost($"/{MINIMAL_API_SOURCE}/{Controller}/body", ([FromBody] Option<int> num) => Json(Increment(num)));
		app.MapPost($"/{MINIMAL_API_SOURCE}/{Controller}/body/complex", ([FromBody] RecordWithOptions value) =>
			Json(new RecordWithOptions(Num1: Increment(value.Num1), Num2: Increment(value.Num2))));

		/*
		 * TODO: find way to support more minimal API sources
		 * Everything except 'body' sources require a TryParse(string, out Option<int>) method
		 * to be defined on the type. There doesn't seem to be an injectable option for serialization.
		 * Until this changes, we won't be able to support minimal APIs very well.
		 */

		//app.MapGet($"/{MINIMAL_API_SOURCE}/option/query", ([FromQuery] Option<int> num) => Increment(num));
		//app.MapGet($"/{MINIMAL_API_SOURCE}/option/route/{{num?}}", ([FromRoute] Option<int> num) => Increment(num));
		//app.MapGet($"/{MINIMAL_API_SOURCE}/option/header", ([FromHeader(Name = "X-OPT-NUM")] Option<int> num) => Increment(num));
		//app.MapGet($"/{MINIMAL_API_SOURCE}/option/form", ([FromForm] Option<int> num) => Increment(num));
	}

	[TearDown]
	public async Task Teardown()
	{
		await _app.StopAsync();
	}

	[Test]
	public async Task OptionAsNullable_FromBody_Some_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "body")
			.PostJsonAsync(Some(3));
		(await response.GetJsonAsync<int>()).Should().Be(4);
	}

	[Test]
	public async Task OptionAsNullable_FromBody_None_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "body")
			.PostJsonAsync(None);
		(await response.GetJsonAsync<int>()).Should().Be(0);
	}

	[Test]
	public async Task OptionAsNullable_FromBodyComplex_Some_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "body", "complex")
			.PostJsonAsync(new RecordWithOptions(Some(3), Some(-2)));

		(await response.GetJsonAsync<RecordWithOptions>())
			.Should().Be(new RecordWithOptions(4, -1));
	}

	[Test]
	public async Task OptionAsNullable_FromBodyComplex_None_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "body", "complex")
			.PostJsonAsync(new RecordWithOptions(None, None));

		(await response.GetJsonAsync<RecordWithOptions>())
			.Should().Be(new RecordWithOptions(0, 0));
	}

	[Test]
	public async Task OptionAsNullable_FromQuery_Some_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "query")
			.SetQueryParam("num", Some(3))
			.GetJsonAsync<int>();

		response.Should().Be(4);
	}

	[Test]
	public async Task OptionAsNullable_FromQuery_None_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "query")
			.GetJsonAsync<int>();

		response.Should().Be(0);
	}

	[Test]
	public async Task OptionAsNullable_FromRoute_Some_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "route", 3)
			.GetJsonAsync<int>();

		response.Should().Be(4);
	}

	[Test]
	public async Task OptionAsNullable_FromRoute_None_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "route")
			.GetJsonAsync<int>();

		response.Should().Be(0);
	}

	[Test]
	public async Task OptionAsNullable_FromHeader_Some_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "header")
			.WithHeader("X-OPT-NUM", 3)
			.GetJsonAsync<int>();

		response.Should().Be(4);
	}

	[Test]
	public async Task OptionAsNullable_FromHeader_None_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "header")
			.WithHeader("X-OPT-NUM", None)
			.GetJsonAsync<int>();

		response.Should().Be(0);
	}

	[Test]
	public async Task OptionAsNullable_FromForm_Some_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "form")
			.PostMultipartAsync(mp => mp.AddJson("num", 3));

		(await response.GetJsonAsync<int>()).Should().Be(4);
	}

	[Test]
	public async Task OptionAsNullable_FromForm_None_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "form")
			.PostMultipartAsync(mp => mp.AddJson("num", null));

		(await response.GetJsonAsync<int>()).Should().Be(0);
	}
}