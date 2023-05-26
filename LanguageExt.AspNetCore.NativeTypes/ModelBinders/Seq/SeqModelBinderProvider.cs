using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static LanguageExt.AspNetCore.NativeTypes.PreludeReflection;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.ModelBinders.Seq;

public class SeqModelBinderProvider : IModelBinderProvider
{
	public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
		(
			from collectionType in Optional(context.Metadata.ModelType)
			where collectionType.IsGenericType && collectionType.GenericTypeArguments.Length == 1
			let genericCollectionType = collectionType.GetGenericTypeDefinition()
			from elementType in collectionType.GenericTypeArguments.HeadOrNone()
			from collectionBuilder in NewSeqBuilder(elementType, genericCollectionType) | 
			                          NewLstBuilder(elementType, genericCollectionType)
			select new LangExtCollectionModelBinder(
				elementType,
				collectionBuilder,
				context.CreateBinder(context.Metadata.GetMetadataForType(elementType)),
				context.Services.GetRequiredService<ILoggerFactory>(),
				context.Services.GetRequiredService<IOptions<MvcOptions>>()
			)
		).IfNoneUnsafe(() => null);

	private static Option<NewUntypedCollection> NewSeqBuilder(Type elementType, Type collectionType) =>
		collectionType == typeof(Seq<>)
			? SeqNew(elementType)
			: None;

	private static Option<NewUntypedCollection> NewLstBuilder(Type elementType, Type collectionType) =>
		collectionType == typeof(Lst<>)
			? LstNew(elementType)
			: None;
}