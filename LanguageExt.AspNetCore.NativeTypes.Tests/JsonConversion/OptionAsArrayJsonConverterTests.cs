using FluentAssertions;
using FluentAssertions.LanguageExt;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.JsonConversion;

[TestFixtureSource(typeof(JsonRuntimes), nameof(JsonRuntimes.GetRuntimes))]
public class OptionAsArrayJsonConverterTests
{
	private readonly Func<object, string> _serialize;
	private readonly Func<string, Option<int>> _deserialize;

	public OptionAsArrayJsonConverterTests(JsonRuntime json)
	{
		var opts = new JsonOpts(false);
		_serialize = json.Serialize(opts);
		_deserialize = s => (Option<int>)json.Deserialize(opts)(typeof(Option<int>), s);
	}

	[TestCase(3, ExpectedResult = @"[3]")]
	[TestCase(0, ExpectedResult = @"[0]")]
	[TestCase(-47, ExpectedResult = @"[-47]")]
	public string Serialize_Some_Correctly(int value) => _serialize(Some(value));

	[Test]
	public void Serialize_None_Correctly() => _serialize(None).Should().Be("[]");

	[Test]
	public void Deserialize_Some_Correctly() =>
		_deserialize("[3]")
			.Should().BeSome().Which.Should().Be(3);

	[TestCase("[]")]
	[TestCase("null")]
	public void Deserialize_None_Correctly(string json) => _deserialize(json).Should().BeNone();
}