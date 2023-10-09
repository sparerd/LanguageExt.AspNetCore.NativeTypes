using LanguageExt.AspNetCore.NativeTypes.ModelBinders.Option;
using LanguageExt.AspNetCore.NativeTypes.ModelBinders.Seq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.ModelBinders;

internal static class ModelBinding
{
	/// <summary>
	/// Inserts custom model binders in the correct list position to ensure LanguageExt
	/// types can be used without breaking normal type binding.
	/// </summary>
	/// <param name="additionalModelBinderProviders"></param>
	/// <param name="providers"></param>
	/// <returns></returns>
	public static Unit ConfigureModelBinders(
		Seq<IModelBinderProvider> additionalModelBinderProviders,
		IList<IModelBinderProvider> providers
	)
	{
		var insertionIndex = GetTargetInsertionProviderIndex(providers);
		return Seq<IModelBinderProvider>(
				new OptionModelBinderProvider(),
				new SeqModelBinderProvider()
			)
			.Append(additionalModelBinderProviders)
			.Iter(p => providers.Insert(insertionIndex, p));
	}

	/// <summary>
	/// Insert new providers just after the last known generic provider (Headers)
	/// This is a bit hacky and might be fragile, but the alternative is reimplementing
	/// source provider logic.
	/// https://github.com/dotnet/aspnetcore/blob/775b001508b2678426319b8cd27453fe90b0f250/src/Mvc/Mvc.Core/src/Infrastructure/MvcCoreMvcOptionsSetup.cs#L51
	/// </summary>
	/// <param name="providers"></param>
	/// <returns></returns>
	private static int GetTargetInsertionProviderIndex(IEnumerable<IModelBinderProvider> providers) =>
		providers
			.Select(p => p.GetType().Name)
			.TakeWhile(binderName => binderName != nameof(HeaderModelBinderProvider))
			.Count() + 1;

	private static Func<IModelBinderProvider, Unit> InsertProvider(IList<IModelBinderProvider> providers, int index) =>
		provider => fun(() => providers.Insert(index, provider))();
}