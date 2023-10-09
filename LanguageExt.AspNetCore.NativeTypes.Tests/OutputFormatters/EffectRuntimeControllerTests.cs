using FluentAssertions;
using FluentAssertions.Execution;
using Flurl.Http;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion;
using LanguageExt.AspNetCore.NativeTypes.Tests.Controllers;
using LanguageExt.AspNetCore.NativeTypes.Tests.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
namespace LanguageExt.AspNetCore.NativeTypes.Tests.OutputFormatters;
using static LanguageExt.AspNetCore.NativeTypes.Tests.TestPrelude;
using static LanguageExt.Prelude;

[TestFixtureSource(typeof(EffectControllerOutputTests), nameof(TestFixtureSource))]
public class EffectRuntimeControllerTests
{
	private WebApplication _app;
	private LanguageExtAspNetCoreOptions _opts;
	private LanguageExtJsonOptions _jsonOpts;
	private readonly string _basePath;
	private readonly string _controller;
	private const int RuntimeVal1 = 13;
	private const int RuntimeSubVal = 9;

	private static readonly Seq<string> Controllers = Seq("eff", "aff");

	public static IEnumerable<object> TestFixtureSource() =>
		from basePath in EndpointBindingFixtureSource()
		from controller in Controllers
		select new object[] { basePath, controller };

	public EffectRuntimeControllerTests(string basePath, string controller)
	{
		_basePath = basePath;
		_controller = controller;
	}

	[SetUp]
	public async Task Setup()
	{
		_opts = new LanguageExtAspNetCoreOptions();
		_jsonOpts = new LanguageExtJsonOptions
		{
			OptionSerializationStrategy = OptionSerializationStrategy.AsNullable,
		};
		_app = CreateWebHost(_opts, _jsonOpts,
			builder => builder
				.AddEffAffEndpointSupport(
					runtimeProvider: _ => new TestRuntime(RuntimeVal1, RuntimeSubVal)
				)
		);
		await _app.StartAsync();
	}

	[TearDown]
	public async Task Teardown()
	{
		await _app.StopAsync();
	}

	[Test]
	public async Task SupportsEndpoint_RequestingSpecificRuntimeImplementation()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, _controller, nameof(EffController.NeedsSpecificRuntime))
			.GetAsync();

		using var _ = new AssertionScope();
		response.StatusCode.Should().Be(StatusCodes.Status200OK);
		(await response.GetJsonAsync<RecordWithOptions>())
			.Should().Be(new RecordWithOptions(RuntimeVal1, None));
	}

	[Test, Ignore("ASPNET doesnt know how to bind generic methods, so this wont work until then")]
	public async Task SupportsEndpoint_RequestingRuntimeTrait()
	{
		var response = await _app.Request()
			.AppendPathSegments(_basePath, _controller, nameof(EffController.NeedsRuntimeWithTrait))
			.GetAsync();

		using var _ = new AssertionScope();
		response.StatusCode.Should().Be(StatusCodes.Status200OK);
		(await response.GetJsonAsync<RecordWithOptions>())
			.Should().Be(new RecordWithOptions(RuntimeSubVal * 2, None));
	}
}