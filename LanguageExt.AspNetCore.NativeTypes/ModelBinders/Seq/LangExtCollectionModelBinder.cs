using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static LanguageExt.AspNetCore.NativeTypes.PreludeReflection;

namespace LanguageExt.AspNetCore.NativeTypes.ModelBinders.Seq;

public class LangExtCollectionModelBinder : IModelBinder
{
	private readonly Type _elementType;
	private readonly IModelBinder _collectionBinder;
	private readonly NewUntypedCollection _collectionCtor;

	public LangExtCollectionModelBinder(
		Type elementType,
		NewUntypedCollection collectionCtor,
		IModelBinder elementTypeBinder,
		ILoggerFactory loggerFactory,
		IOptions<MvcOptions> mvcOptions)
	{
		_elementType = elementType;
		_collectionCtor = collectionCtor;

		_collectionBinder = (IModelBinder)Activator.CreateInstance(
			typeof(CollectionModelBinder<>).MakeGenericType(elementType),
			elementTypeBinder,
			loggerFactory,
			true,
			mvcOptions.Value)!;
	}

	public async Task BindModelAsync(ModelBindingContext bindingContext)
	{
		var result = await BindWrappedValue(bindingContext);
		bindingContext.Result = result
			.Filter(r => r.IsModelSet)
			.Map(bindingResult => _collectionCtor(bindingResult.Model!))
			.Match(
				Some: ModelBindingResult.Success,
				None: ModelBindingResult.Failed
			);
	}

	private async Task<Option<ModelBindingResult>> BindWrappedValue(ModelBindingContext context)
	{
		var intermediateCollectionType = typeof(IEnumerable<>).MakeGenericType(_elementType);
		using var nestedScope = context.EnterNestedScope(
			modelMetadata: context.ModelMetadata.GetMetadataForType(intermediateCollectionType),
			fieldName: context.FieldName,
			modelName: context.ModelName,
			model: null);

		await _collectionBinder.BindModelAsync(context);
		return context.Result;
	}
}