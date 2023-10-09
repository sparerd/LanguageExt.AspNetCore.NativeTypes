using FluentAssertions;
using FluentAssertions.Execution;
using Flurl.Http;
using LanguageExt;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion;
using LanguageExt.AspNetCore.NativeTypes.Tests.Controllers;
using LanguageExt.AspNetCore.NativeTypes.Tests.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using static LanguageExt.AspNetCore.NativeTypes.Tests.TestPrelude;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.OutputFormatters;

[TestFixtureSource(typeof(EffectControllerOutputTests), nameof(TestFixtureSource))]
public class EffectControllerOutputTests
{
	private WebApplication _app;
	private LanguageExtJsonOptions _jsonOpts;
	private readonly string _basePath;
	private readonly string _controller;

	private static readonly Seq<string> Controllers = Seq("eff", "aff");

	public static IEnumerable<object> TestFixtureSource() =>
		from basePath in EndpointBindingFixtureSource()
		from controller in Controllers
		select new object[] { basePath, controller };

	public EffectControllerOutputTests(string basePath, string controller)
	{
		_basePath = basePath;
		_controller = controller;
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
	public async Task SupportsEndpoint_ReturningActionResultClass()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, _controller, nameof(EffController.ReturnsActionResultClass))
			.GetAsync();

		using var _ = new AssertionScope();
		response.StatusCode.Should().Be(StatusCodes.Status200OK);
		(await response.GetJsonAsync<int>()).Should().Be(0);
	}

	[Test]
	public async Task SupportsEndpoint_ReturningActionResultInterface()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, _controller, nameof(EffController.ReturnsActionResultInterface))
			.GetAsync();

		using var _ = new AssertionScope();
		response.StatusCode.Should().Be(StatusCodes.Status200OK);
		(await response.GetJsonAsync<int>()).Should().Be(0);
	}

	[Test]
	public async Task SupportsEndpoint_ReturningSimpleTypeDirectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, _controller, nameof(EffController.ReturnsSimpleType))
			.GetAsync();

		using var _ = new AssertionScope();
		response.StatusCode.Should().Be(StatusCodes.Status200OK);
		(await response.GetJsonAsync<int>()).Should().Be(0);
	}

	[Test]
	public async Task SupportsEndpoint_ReturningComplexTypeDirectly()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, _controller, nameof(EffController.ReturnsComplexType))
			.GetAsync();

		using var _ = new AssertionScope();
		response.StatusCode.Should().Be(StatusCodes.Status200OK);
		(await response.GetJsonAsync<RecordWithOptions>())
			.Should().Be(new RecordWithOptions(5, None));
	}

	[Test]
	public async Task ReturnsErrorWhenControllerReturnsDefaultEffect()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, _controller, nameof(EffController.ReturnsDefaultEffect))
			.GetAsync();

		response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
	}
}