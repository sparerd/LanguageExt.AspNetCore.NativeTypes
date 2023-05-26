using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using FluentAssertions.LanguageExt;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion.Option;
using NUnit.Framework;
using static System.Text.Json.JsonSerializer;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.JsonConversion;

public class OptionAsArrayJsonConverterTests
{
	private readonly JsonSerializerOptions _serializerOptions;

	public OptionAsArrayJsonConverterTests()
	{
		_serializerOptions = new JsonSerializerOptions();
		_serializerOptions.Converters.Add(new OptionAsArrayJsonConverterFactory());
	}

	[TestCase(3, ExpectedResult = @"[3]")]
	[TestCase(0, ExpectedResult = @"[0]")]
	[TestCase(-47, ExpectedResult = @"[-47]")]
	public string Serialize_Some_Correctly(int value) => Serialize(Some(value), _serializerOptions);

	[Test]
	public void Serialize_None_Correctly() => Serialize(None, _serializerOptions).Should().Be("[]");

	[Test]
	public void Deserialize_Some_Correctly() =>
		Deserialize<Option<int>>("[3]", _serializerOptions)
			.Should().BeSome().Which.Should().Be(3);

	[TestCase("[]")]
	[TestCase("null")]
	public void Deserialize_None_Correctly(string json) => Deserialize<Option<int>>(json, _serializerOptions).Should().BeNone();
}