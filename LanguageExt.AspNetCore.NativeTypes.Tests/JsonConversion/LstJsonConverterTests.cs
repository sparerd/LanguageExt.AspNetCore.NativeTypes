using FluentAssertions;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.JsonConversion;

[TestFixtureSource(typeof(JsonRuntimes), nameof(JsonRuntimes.GetRuntimes))]
public class LstJsonConverterTests
{
	private readonly Func<object, string> _serialize;
	private readonly Func<string, Lst<int>> _deserialize;

	public LstJsonConverterTests(JsonRuntime json)
	{
		var opts = new JsonOpts(true);
		_serialize = json.Serialize(opts);
		_deserialize = s => (Lst<int>)json.Deserialize(opts)(typeof(Lst<int>), s);
	}
		
	[Test]
	public void Serialize_Empty()
	{
		_serialize(Lst<int>.Empty)
			.Should().Be("[]");
	}

	[Test]
	public void Serialize_Single()
	{
		_serialize(List(6))
			.Should().Be("[6]");
	}

	[Test]
	public void Serialize_Multiple()
	{
		_serialize(List(1, 2, 3))
			.Should().Be("[1,2,3]");
	}

	[Test]
	public void Deserialize_Empty()
	{
		_deserialize("[]").As<IEnumerable<int>>()
			.Should().BeOfType<Lst<int>>()
			.Which.As<IEnumerable<int>>().Should().BeEmpty();
	}

	[Test]
	public void Deserialize_Single()
	{

		_deserialize("[5]").As<IEnumerable<int>>()
			.Should().BeOfType<Lst<int>>()
			.Which.As<IEnumerable<int>>().Should().BeEquivalentTo(List(5));
	}

	[Test]
	public void Deserialize_Multiple()
	{
		_deserialize("[1,2,3]").As<IEnumerable<int>>()
			.Should().BeOfType<Lst<int>>()
			.Which.As<IEnumerable<int>>().Should().BeEquivalentTo(List(1, 2, 3));
	}
}