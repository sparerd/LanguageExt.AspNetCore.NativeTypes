using FluentAssertions;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.JsonConversion;

[TestFixtureSource(typeof(JsonRuntimes), nameof(JsonRuntimes.GetRuntimes))]
public class SeqJsonConverterTests
{
	private readonly Func<object, string> _serialize;
	private readonly Func<string, Seq<int>> _deserialize;

	public SeqJsonConverterTests(JsonRuntime json)
	{
		var opts = new JsonOpts(true);
		_serialize = json.Serialize(opts);
		_deserialize = s => (Seq<int>)json.Deserialize(opts)(typeof(Seq<int>), s);
	}

	[Test]
	public void Serialize_SeqEmpty()
	{
		_serialize(Empty).Should().Be("[]");
	}

	[Test]
	public void Serialize_Empty()
	{
		_serialize(Seq<int>.Empty)
			.Should().Be("[]");
	}

	[Test]
	public void Serialize_Single()
	{
		_serialize(Seq1(6))
			.Should().Be("[6]");
	}

	[Test]
	public void Serialize_Multiple()
	{
		_serialize(Seq(1, 2, 3))
			.Should().Be("[1,2,3]");
	}

	[Test]
	public void Deserialize_Empty()
	{
		_deserialize("[]")
			.Should().BeOfType<Seq<int>>()
			.Which.Should().BeEmpty();
	}

	[Test]
	public void Deserialize_Single()
	{
		_deserialize("[5]")
			.Should().BeOfType<Seq<int>>()
			.Which.Should().BeEquivalentTo(Seq1(5));
	}

	[Test]
	public void Deserialize_Multiple()
	{
		_deserialize("[1,2,3]")
			.Should().BeOfType<Seq<int>>()
			.Which.Should().BeEquivalentTo(Seq(1, 2, 3));
	}
}