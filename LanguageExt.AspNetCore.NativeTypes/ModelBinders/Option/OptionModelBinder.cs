using LanguageExt;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static LanguageExt.AspNetCore.NativeTypes.PreludeReflection;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.ModelBinders.Option;

public class OptionModelBinder : IModelBinder
{
    private readonly Type _wrappedType;
    private readonly ModelMetadata _wrappedTypeMetadata;
    private readonly IModelBinder _wrappedTypeBinder;

    public OptionModelBinder(Type wrappedType, IModelBinder wrappedTypeBinder, ModelMetadata wrappedTypeMetadata)
    {
        _wrappedTypeBinder = wrappedTypeBinder;
        _wrappedTypeMetadata = wrappedTypeMetadata;
        _wrappedType = wrappedType;
    }

    public Task BindModelAsync(ModelBindingContext context)
    {
        if (context.BindingSource == BindingSource.Header && HeaderIsNone(context))
            return Task.CompletedTask;

        if (context.BindingSource == BindingSource.Query && QueryIsNone(context))
            return Task.CompletedTask;

        if (context.BindingSource == BindingSource.Path && RouteIsNone(context))
            return Task.CompletedTask;

        if (context.BindingSource == BindingSource.Form && FormIsNone(context))
            return Task.CompletedTask;

        return BindCoreAsync(context);
    }

    private bool FormIsNone(ModelBindingContext context) =>
        !context.HttpContext.Request.Form.TryGetValue(context.FieldName, out var formValue) ||
        formValue.All(s => s == None.ToString() || s == "null");

    private bool HeaderIsNone(ModelBindingContext context) =>
        !context.HttpContext.Request.Headers.TryGetValue(context.FieldName, out var head) ||
        head.All(s => s == None.ToString());

    private bool QueryIsNone(ModelBindingContext context) =>
        !context.HttpContext.Request.Query.TryGetValue(context.FieldName, out var query) ||
        query.All(s => s == None.ToString());

    private bool RouteIsNone(ModelBindingContext context) =>
        !context.HttpContext.Request.RouteValues.ContainsKey(context.FieldName);

    private async Task BindCoreAsync(ModelBindingContext context)
    {
        var result = await BindWrappedValue(context);
        result
            .Bind<object>(bindingResult => Optional(_wrappedType, bindingResult.Model))
            .IfSome(o => context.Result = ModelBindingResult.Success(o));
    }

    private async Task<Option<ModelBindingResult>> BindWrappedValue(ModelBindingContext context)
    {
        using var nestedScope = context.EnterNestedScope(
            modelMetadata: _wrappedTypeMetadata,
            fieldName: context.FieldName,
            modelName: context.ModelName,
            model: null);

        await _wrappedTypeBinder.BindModelAsync(context);
        return Optional(context.Result)
            .Filter(r => r.IsModelSet);
    }
}