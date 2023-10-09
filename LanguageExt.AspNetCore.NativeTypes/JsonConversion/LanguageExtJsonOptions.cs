namespace LanguageExt.AspNetCore.NativeTypes.JsonConversion;

public record LanguageExtJsonOptions
{
	/// <summary>
	/// Controls how <see cref="Option{A}"/> types are serialized to JSON.
	/// </summary>
	public OptionSerializationStrategy OptionSerializationStrategy { get; init; }
}