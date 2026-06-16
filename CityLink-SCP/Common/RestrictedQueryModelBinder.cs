using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Reflection;

namespace CityLink_SCP.Common;
public class RestrictedQueryModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var actionDescriptor = bindingContext.ActionContext.ActionDescriptor
            as ControllerActionDescriptor;

        var controllerName = actionDescriptor?.ControllerName;
        var user = bindingContext.HttpContext.User;

        // Create an instance of whatever QueryParameters subclass was asked for
        var modelType = bindingContext.ModelType;
        var model = Activator.CreateInstance(modelType)!;

        foreach (var property in modelType.GetProperties())
        {
            var controllerAttr = property.GetCustomAttribute<QueryControllerAttribute>();
            var roleAttr = property.GetCustomAttribute<UserRoleAttribute>();

            // Check all restrictions — all must pass
            if (controllerAttr != null)
            {
                if (!string.Equals(controllerName, controllerAttr.ControllerName,
                    StringComparison.OrdinalIgnoreCase))
                    continue; // skip, leave null
            }

            if (roleAttr != null)
            {
                if (!user.IsInRole(roleAttr.Role))
                    continue; // skip, leave null
            }

            // No restrictions or all passed — bind normally
            var valueResult = bindingContext.ValueProvider.GetValue(property.Name);
            if (valueResult == ValueProviderResult.None)
                continue;

            try
            {
                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                object? converted;

                if (targetType.IsEnum)
                    converted = Enum.Parse(targetType, valueResult.FirstValue!, ignoreCase: true);
                else if (targetType == typeof(TimeOnly))
                    converted = TimeOnly.Parse(valueResult.FirstValue!);
                else if (targetType == typeof(DateOnly))
                    converted = DateOnly.Parse(valueResult.FirstValue!);
                else if (targetType == typeof(string))
                    converted = valueResult.FirstValue!.ToLower();
                else
                    converted = Convert.ChangeType(valueResult.FirstValue, targetType);

                property.SetValue(model, converted);
            }
            catch
            {
                // Bad value from query string — leave null, don't filter on this field
            }
        }

        bindingContext.Result = ModelBindingResult.Success(model);
        return Task.CompletedTask;
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class QueryControllerAttribute : Attribute
{
    public string ControllerName { get; }
    public QueryControllerAttribute(string controllerName) => ControllerName = controllerName;
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class UserRoleAttribute : Attribute
{
    public string Role { get; }
    public UserRoleAttribute(string role) => Role = role;
}
