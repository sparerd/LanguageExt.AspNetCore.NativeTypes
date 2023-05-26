using Microsoft.AspNetCore.Mvc.ModelBinding;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.ModelBinders.Option;

public class OptionModelBinderProvider : IModelBinderProvider
{
	public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
		Optional(context.Metadata.ModelType)
			.Filter(type => type.IsGenericType)
			.Filter(type => type.GetGenericTypeDefinition() == typeof(Option<>))
			.Bind(optionType => optionType.GenericTypeArguments.HeadOrNone())
			.Map(wrappedType =>
			{
				var wrappedTypeMetadata = context.Metadata.GetMetadataForType(wrappedType);
				return new OptionModelBinder(
					wrappedType,
					context.CreateBinder(wrappedTypeMetadata),
					wrappedTypeMetadata);
			})
			.IfNoneUnsafe(() => null);
}