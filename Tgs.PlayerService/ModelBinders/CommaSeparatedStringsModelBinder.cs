using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Tgs.PlayerService.ModelBinders;

public class CommaSeparatedStringsModelBinder<T> : IModelBinder
    where T : IParsable<T>
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var modelName = bindingContext.ModelName;

        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            bindingContext.Result = ModelBindingResult.Success(Array.Empty<T>());
            return Task.CompletedTask;
        }

        var csv = valueProviderResult.Values.ToString();
        if (typeof(T) == typeof(string))
        {
            bindingContext.Result = ModelBindingResult.Success(
                csv.Split(',').Select(v => v.Trim()).ToArray());

            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(
            csv.Split(',').Select(v => T.Parse(v.Trim(), null)).ToArray());

        return Task.CompletedTask;
    }
}
