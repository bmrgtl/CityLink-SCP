using System.Collections;
using System.Xml.Serialization;
namespace CityLink_SCP.Common;
public static class XmlTypeSerializer
{
    public static string ToXml(Type type, bool recursive = false)
    {
        const int ListItemCount = 3;

        object instance = BuildInstance(type, ListItemCount, recursive);

        var serializer = new XmlSerializer(type);
        using var writer = new StringWriter();
        serializer.Serialize(writer, instance);
        return writer.ToString();
    }

    private static object BuildInstance(Type type, int listItemCount, bool recursive)
    {
        object instance = Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"Could not create instance of {type.FullName}.");

        foreach (var property in type.GetProperties())
        {
            if (!property.CanRead || !property.CanWrite)
                continue;

            Type propType = property.PropertyType;

            if (typeof(IList).IsAssignableFrom(propType))
            {
                Type elementType = GetListElementType(propType);

                if (elementType == null)
                    continue;

                IList list = (IList)(Activator.CreateInstance(propType)
                    ?? throw new InvalidOperationException($"Could not create list of type {propType.FullName}."));

                for (int i = 0; i < listItemCount; i++)
                {
                    object element = recursive && !elementType.IsPrimitive && elementType != typeof(string)
                        ? BuildInstance(elementType, listItemCount, recursive)
                        : (Activator.CreateInstance(elementType)
                            ?? throw new InvalidOperationException($"Could not create instance of element type {elementType.FullName}."));

                    list.Add(element);
                }

                property.SetValue(instance, list);
            }
            else if (recursive && !propType.IsPrimitive && propType != typeof(string) && IsUserDefinedClass(propType))
            {
                object nested = BuildInstance(propType, listItemCount, recursive);
                property.SetValue(instance, nested);
            }
        }

        return instance;
    }

    private static Type? GetListElementType(Type listType)
    {
        // Prefer generic type argument (e.g. List<Foo> -> Foo)
        if (listType.IsGenericType)
        {
            Type[] args = listType.GetGenericArguments();
            if (args.Length == 1)
                return args[0];
        }

        // Fall back to checking if the type implements IList<T>
        foreach (Type iface in listType.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IList<>))
                return iface.GetGenericArguments()[0];
        }

        return null;
    }

    private static bool IsUserDefinedClass(Type type)
    {
        return type.IsClass
            && !type.IsAbstract
            && type.Namespace != null
            && !type.Namespace.StartsWith("System", StringComparison.Ordinal);
    }
}