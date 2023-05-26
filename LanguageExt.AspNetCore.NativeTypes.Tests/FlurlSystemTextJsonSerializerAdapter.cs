using System.Text.Json;
using Flurl.Http.Configuration;

namespace LanguageExt.AspNetCore.NativeTypes.Tests;

public class FlurlSystemTextJsonSerializerAdapter : ISerializer
{
	private readonly JsonSerializerOptions _serializerOptions;

	public FlurlSystemTextJsonSerializerAdapter(JsonSerializerOptions serializerOptions)
	{
		_serializerOptions = serializerOptions;
	}

	public string Serialize(object obj) => JsonSerializer.Serialize(obj, _serializerOptions);
	public T Deserialize<T>(string s) => JsonSerializer.Deserialize<T>(s, _serializerOptions)!;
	public T Deserialize<T>(Stream stream) => JsonSerializer.Deserialize<T>(stream, _serializerOptions)!;
}