using System.Text.Json;
using FluentAssertions;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion.Seq;
using NUnit.Framework;
using static System.Text.Json.JsonSerializer;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.JsonConversion
{
	public class SeqJsonConverterTests
	{
		private readonly JsonSerializerOptions _serializerOptions;

		public SeqJsonConverterTests()
		{
			_serializerOptions = new JsonSerializerOptions();
			_serializerOptions.Converters.Add(new LangExtCollectionJsonConverterFactory());
		}

		[Test]
		public void Serialize_SeqEmpty()
		{
			Serialize(Empty, _serializerOptions)
				.Should().Be("[]");
		}

		[Test]
		public void Serialize_Empty()
		{
			Serialize(Seq<int>.Empty, _serializerOptions)
				.Should().Be("[]");
		}

		[Test]
		public void Serialize_Single()
		{
			Serialize(Seq1(6), _serializerOptions)
				.Should().Be("[6]");
		}

		[Test]
		public void Serialize_Multiple()
		{
			Serialize(Seq(1, 2, 3), _serializerOptions)
				.Should().Be("[1,2,3]");
		}

		[Test]
		public void Deserialize_Empty()
		{
			Deserialize<Seq<int>>("[]", _serializerOptions)
				.Should().BeOfType<Seq<int>>()
				.Which.Should().BeEmpty();
		}

		[Test]
		public void Deserialize_Single()
		{
			Deserialize<Seq<int>>("[5]", _serializerOptions)
				.Should().BeOfType<Seq<int>>()
				.Which.Should().BeEquivalentTo(Seq1(5));
		}

		[Test]
		public void Deserialize_Multiple()
		{
			Deserialize<Seq<int>>("[1,2,3]", _serializerOptions)
				.Should().BeOfType<Seq<int>>()
				.Which.Should().BeEquivalentTo(Seq(1, 2, 3));
		}
	}
}
