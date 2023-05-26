using System.Text.Json;
using FluentAssertions;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion.Seq;
using NUnit.Framework;
using static System.Text.Json.JsonSerializer;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.JsonConversion
{
	public class LstJsonConverterTests
	{
		private readonly JsonSerializerOptions _serializerOptions;

		public LstJsonConverterTests()
		{
			_serializerOptions = new JsonSerializerOptions();
			_serializerOptions.Converters.Add(new LangExtCollectionJsonConverterFactory());
		}
		
		[Test]
		public void Serialize_Empty()
		{
			Serialize(Lst<int>.Empty, _serializerOptions)
				.Should().Be("[]");
		}

		[Test]
		public void Serialize_Single()
		{
			Serialize(List(6), _serializerOptions)
				.Should().Be("[6]");
		}

		[Test]
		public void Serialize_Multiple()
		{
			Serialize(List(1, 2, 3), _serializerOptions)
				.Should().Be("[1,2,3]");
		}

		[Test]
		public void Deserialize_Empty()
		{
			Deserialize<Lst<int>>("[]", _serializerOptions).As<IEnumerable<int>>()
				.Should().BeOfType<Lst<int>>()
				.Which.As<IEnumerable<int>>().Should().BeEmpty();
		}

		[Test]
		public void Deserialize_Single()
		{
			
			Deserialize<Lst<int>>("[5]", _serializerOptions).As<IEnumerable<int>>()
				.Should().BeOfType<Lst<int>>()
				.Which.As<IEnumerable<int>>().Should().BeEquivalentTo(List(5));
		}

		[Test]
		public void Deserialize_Multiple()
		{
			Deserialize<Lst<int>>("[1,2,3]", _serializerOptions).As<IEnumerable<int>>()
				.Should().BeOfType<Lst<int>>()
				.Which.As<IEnumerable<int>>().Should().BeEquivalentTo(List(1, 2, 3));
		}
	}
}
