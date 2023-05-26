using FluentAssertions;
using Flurl.Http;
using LanguageExt.AspNetCore.NativeTypes.Tests.Models;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using static LanguageExt.AspNetCore.NativeTypes.Tests.TestPrelude;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.ModelBinders;

[TestFixtureSource(typeof(TestPrelude), nameof(EndpointBindingFixtureSource))]
public class SeqModelBinderTests
{
    private WebApplication _app;
    private LanguageExtAspNetCoreOptions _opts;
    private readonly string _basePath;
    private const string Controller = "seq";

    public SeqModelBinderTests(string basePath)
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
        await _app.StartAsync();
    }

    [TearDown]
    public async Task Teardown()
    {
        await _app.StopAsync();
    }

    [Test]
    public async Task Seq_FromBody_Single_BindsCorrectly()
    {
        var response = await _app.Request()
            .AppendPathSegments(_basePath, Controller, "body")
            .PostJsonAsync(Seq1(4));
        (await response.GetJsonAsync<int>()).Should().Be(4);
    }

    [Test]
    public async Task Seq_FromBody_Multiple_BindsCorrectly()
    {
        var response = await _app.Request()
            .AppendPathSegments(_basePath, Controller, "body")
            .PostJsonAsync(Seq(4, 3, 6));
        (await response.GetJsonAsync<int>()).Should().Be(13);
    }

    [Test]
    public async Task Seq_FromBody_Empty_BindsCorrectly()
    {
        var response = await _app.Request()
            .AppendPathSegments(_basePath, Controller, "body")
            .PostJsonAsync(Empty);
        (await response.GetJsonAsync<int>()).Should().Be(0);
    }

    [Test]
    public async Task Seq_FromBodyComplex_Some_BindsCorrectly()
    {
        var response = await _app.Request()
            .AppendPathSegments(_basePath, Controller, "body", "complex")
            .PostJsonAsync(new RecordWithSeqs(Seq(3, 4), Seq1(-2)));

        (await response.GetJsonAsync<RecordWithSeqs>())
            .Should().Be(new RecordWithSeqs(Seq(4, 5), Seq1(-1)));
    }

    [Test]
    public async Task Seq_FromBodyComplex_Empty_BindsCorrectly()
    {
        var response = await _app.Request()
            .AppendPathSegments(_basePath, Controller, "body", "complex")
            .PostJsonAsync(new RecordWithSeqs(Empty, Empty));

        (await response.GetJsonAsync<RecordWithSeqs>())
            .Should().Be(new RecordWithSeqs(Empty, Empty));
    }

    [Test]
    public async Task Seq_FromQuery_Multiple_BindsCorrectly()
    {
        var response = await _app.Request()
            .AppendPathSegments(_basePath, Controller, "query")
            .SetQueryParam("num", Seq(3, 4, 5))
            .GetJsonAsync<int>();

        response.Should().Be(12);
    }

    [Test]
    public async Task Seq_FromQuery_Empty_BindsCorrectly()
    {
        var response = await _app.Request()
            .AppendPathSegments(_basePath, Controller, "query")
            .GetJsonAsync<int>();

        response.Should().Be(0);
    }

    //[Test]
    //public async Task Seq_FromHeader_Some_BindsCorrectly()
    //{
    //	var response = await _app.Request()
    //		.AppendPathSegments(_basePath, Controller, "header")
    //		.WithHeader("X-OPT-NUM", 3)
    //		.GetJsonAsync<int>();

    //	response.Should().Be(4);
    //}

    //[Test]
    //public async Task Seq_FromHeader_None_BindsCorrectly()
    //{
    //	var response = await _app.Request()
    //		.AppendPathSegments(_basePath, Controller, "header")
    //		.WithHeader("X-OPT-NUM", None)
    //		.GetJsonAsync<int>();

    //	response.Should().Be(0);
    //}

    //[Test]
    //public async Task Seq_FromForm_Some_BindsCorrectly()
    //{
    //	var response = await _app.Request()
    //		.AppendPathSegments(_basePath, Controller, "form")
    //		.PostMultipartAsync(mp => mp.AddJson("num", 3));

    //	(await response.GetJsonAsync<int>()).Should().Be(4);
    //}

    //[Test]
    //public async Task Seq_FromForm_None_BindsCorrectly()
    //{
    //	var response = await _app.Request()
    //		.AppendPathSegments(_basePath, Controller, "form")
    //		.PostMultipartAsync(mp => mp.AddJson("num", null));

    //	(await response.GetJsonAsync<int>()).Should().Be(0);
    //}
}