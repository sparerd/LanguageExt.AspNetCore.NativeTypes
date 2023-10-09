using System.Text.Json;
using LanguageExt.AspNetCore.NativeTypes.JsonConversion;
using LanguageExt.AspNetCore.NativeTypes.NewtonsoftJson;
using Newtonsoft.Json;
using static LanguageExt.AspNetCore.NativeTypes.JsonServiceExtensions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.JsonConversion;

public record JsonRuntime(
	string Name,
	Func<JsonOpts, Func<object, string>> Serialize,
	Func<JsonOpts, Func<Type, string, object>> Deserialize)
{
	public override string ToString() => Name;
}

public record JsonOpts(bool OptionAsNullable);

public static class JsonRuntimes
{
	public static Seq<JsonRuntime> GetRuntimes()
	{
		JsonSerializerOptions GetSysTextJsonOptions(JsonOpts opts) =>
			LanguageExtSerializer(new LanguageExtJsonOptions
			{
				OptionSerializationStrategy = opts.OptionAsNullable
					? OptionSerializationStrategy.AsNullable
					: OptionSerializationStrategy.AsArray,
			});

		JsonSerializerSettings GetNewtonsoftOptions(JsonOpts opts) =>
			new JsonSerializerSettings()
				.AddLanguageExtConverters(new LanguageExtJsonOptions
				{
					OptionSerializationStrategy = opts.OptionAsNullable
						? OptionSerializationStrategy.AsNullable
						: OptionSerializationStrategy.AsArray,
				});

		return Prelude.Seq(
			new JsonRuntime(
				"System.Text.Json",
				opts => o => JsonSerializer.Serialize(o, GetSysTextJsonOptions(opts)),
				opts => (type, json) => JsonSerializer.Deserialize(json, type, GetSysTextJsonOptions(opts))
			),
			new JsonRuntime(
				"NewtonsoftJson",
				opts => o => JsonConvert.SerializeObject(o, GetNewtonsoftOptions(opts)),
				opts => (type, json) => JsonConvert.DeserializeObject(json, type, GetNewtonsoftOptions(opts))
			)
		);
	}
}