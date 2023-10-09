using FluentAssertions;
using Flurl.Http;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion;
using LanguageExt.AspNetCore.NativeTypes.Tests.Models;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using static LanguageExt.AspNetCore.NativeTypes.Tests.TestPrelude;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.ModelBinders;

[TestFixtureSource(typeof(TestPrelude), nameof(EndpointBindingFixtureSource))]
public class LstModelBinderTests
{
	private WebApplication _app;
	private LanguageExtJsonOptions _jsonOpts;
	private readonly string _basePath;
	private const string Controller = "lst";

	public LstModelBinderTests(string basePath)
	{
		_basePath = basePath;
	}

	[SetUp]
	public async Task Setup()
	{
		_jsonOpts = new LanguageExtJsonOptions
		{
			OptionSerializationStrategy = OptionSerializationStrategy.AsNullable,
		};
		_app = CreateWebHost(_jsonOpts);
		await _app.StartAsync();
	}

	[TearDown]
	public async Task Teardown()
	{
		await _app.StopAsync();
	}

	[Test]
	public async Task FromBody_Single_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "body")
			.PostJsonAsync(List(4));
		(await response.GetJsonAsync<int>()).Should().Be(4);
	}

	[Test]
	public async Task FromBody_Multiple_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "body")
			.PostJsonAsync(List(4, 3, 6));
		(await response.GetJsonAsync<int>()).Should().Be(13);
	}

	[Test]
	public async Task FromBody_Empty_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "body")
			.PostJsonAsync(Lst<int>.Empty);
		(await response.GetJsonAsync<int>()).Should().Be(0);
	}

	[Test]
	public async Task FromBodyComplex_Multiple_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "body", "complex")
			.PostJsonAsync(new RecordWithLsts(List(3, 4), List(-2)));

		(await response.GetJsonAsync<RecordWithLsts>())
			.Should().Be(new RecordWithLsts(List(4, 5), List(-1)));
	}

	[Test]
	public async Task FromBodyComplex_Empty_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "body", "complex")
			.PostJsonAsync(new RecordWithLsts(Empty, Empty));

		(await response.GetJsonAsync<RecordWithLsts>())
			.Should().Be(new RecordWithLsts(Empty, Empty));
	}

	[Test]
	public async Task FromQuery_Multiple_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "query")
			.SetQueryParam("num", List(3, 4, 5))
			.GetJsonAsync<int>();

		response.Should().Be(12);
	}

	[Test]
	public async Task FromQuery_Empty_BindsCorrectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, "query")
			.GetJsonAsync<int>();

		response.Should().Be(0);
	}
}