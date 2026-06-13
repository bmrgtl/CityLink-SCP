using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

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
                Type? elementType = GetListElementType(propType);
                if (elementType == null)
                    continue;

                IList list = (IList)(Activator.CreateInstance(propType)
                    ?? throw new InvalidOperationException($"Could not create list of type {propType.FullName}."));

                for (int i = 0; i < listItemCount; i++)
                {
                    object element = CreateDefault(elementType, listItemCount, recursive);
                    list.Add(element);
                }

                property.SetValue(instance, list);
            }
            else if (recursive && IsUserDefinedClass(propType))
            {
                property.SetValue(instance, BuildInstance(propType, listItemCount, recursive));
            }
            else
            {
                object? defaultValue = GetDefaultValue(propType);
                if (defaultValue != null)
                    property.SetValue(instance, defaultValue);
            }
        }

        return instance;
    }

    /// <summary>
    /// Creates a default value for a given type.
    /// User-defined classes always get BuildInstance so their properties are populated.
    /// Primitives and strings get a sensible placeholder.
    /// </summary>
    private static object CreateDefault(Type type, int listItemCount, bool recursive)
    {
        if (IsUserDefinedClass(type))
            return BuildInstance(type, listItemCount, recursive);

        return GetDefaultValue(type)
            ?? Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"Could not create default for {type.FullName}.");
    }

    /// <summary>
    /// Returns a sensible placeholder for primitive-like types so they appear in serialized XML.
    /// </summary>
    private static object? GetDefaultValue(Type type)
    {
        if (type == typeof(string)) return string.Empty;
        if (type == typeof(int)) return 0;
        if (type == typeof(long)) return 0L;
        if (type == typeof(double)) return 0.0;
        if (type == typeof(float)) return 0f;
        if (type == typeof(decimal)) return 0m;
        if (type == typeof(bool)) return false;
        if (type == typeof(DateTime)) return DateTime.MinValue;
        if (type == typeof(Guid)) return Guid.Empty;

        // Nullable<T> — return null so XmlSerializer omits the element
        if (Nullable.GetUnderlyingType(type) != null)
            return null;

        return null;
    }

    private static Type? GetListElementType(Type listType)
    {
        if (listType.IsGenericType)
        {
            Type[] args = listType.GetGenericArguments();
            if (args.Length == 1)
                return args[0];
        }

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
            && type != typeof(string)
            && (type.Namespace == null || !type.Namespace.StartsWith("System", StringComparison.Ordinal));
    }
}