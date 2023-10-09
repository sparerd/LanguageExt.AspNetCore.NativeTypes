using FluentAssertions;
using FluentAssertions.Execution;
using Flurl.Http;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion;
using LanguageExt.AspNetCore.NativeTypes.Tests.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using static LanguageExt.AspNetCore.NativeTypes.Tests.TestPrelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.OutputFormatters;

[TestFixtureSource(typeof(TestPrelude), nameof(EndpointBindingFixtureSource))]
public class OptionControllerOutputTests
{
	private WebApplication _app;
	private LanguageExtJsonOptions _jsonOpts;
	private readonly string _basePath;
	private const string Controller = "OptionOutput";

	public OptionControllerOutputTests(string basePath)
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
	public async Task Some_Returns200OKWithValue()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, nameof(OptionOutputController.Some))
			.GetAsync();

		using var _ = new AssertionScope();
		response.StatusCode.Should().Be(StatusCodes.Status200OK);
		(await response.GetJsonAsync<int>()).Should().Be(7);
	}

	[Test]
	public async Task None_Returns404NotFound()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, nameof(OptionOutputController.OptionTNone))
			.GetAsync();

		response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
	}

	[Test]
	public async Task OptionNone_Returns404NotFound()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, Controller, nameof(OptionOutputController.OptionNone))
			.GetAsync();
		
		response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
	}
}