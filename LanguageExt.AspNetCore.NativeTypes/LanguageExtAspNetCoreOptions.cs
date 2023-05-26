using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LanguageExt.AspNetCore.NativeTypes;

/// <summary>
/// Controls various aspects of model binding LanguageExt types in Asp.Net Core.
/// </summary>
public record LanguageExtAspNetCoreOptions
{
	/// <summary>
	/// Controls how <see cref="Option{A}"/> types are serialized to JSON.
	/// </summary>
	public OptionSerializationStrategy OptionSerializationStrategy { get; init; }

	/// <summary>
	/// Optional additional <see cref="IModelBinderProvider"/> instances to
	/// inject along with those provided by this library. Providers are added
	/// in stack order, meaning the last provider in this list will be the first
	/// one evaluated by the model binder pipeline. The providers specified here
	/// will always be evaluated before providers from this library, allowing you
	/// to override any provider if necessary.
	/// </summary>
	public Seq<IModelBinderProvider> AdditionalModelBinderProviders { get; init; }
}