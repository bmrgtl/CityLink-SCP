using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CityLink_SCP.Common;

public class RestrictedQueryModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (typeof(QueryParameters).IsAssignableFrom(context.Metadata.ModelType))
            return new RestrictedQueryModelBinder();

        return null;
    }
}
