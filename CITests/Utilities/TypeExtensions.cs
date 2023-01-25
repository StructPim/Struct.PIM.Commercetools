using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions.Types;

namespace CITests.Utilities;

internal static class TypeExtensions
{
    public static MethodInfoSelectorAssertions Should(this IEnumerable<MethodInfo> methodInfos)
    {
        return new MethodInfoSelectorAssertions(methodInfos.ToArray());
    }

    public static void ThrowOnNullProperty(this object obj, IEnumerable<string> excludedProperties)
    {
        ThrowOnNullProperty(obj, new HashSet<object>(), excludedProperties);
    }

    private static void ThrowOnNullProperty(object obj, HashSet<object> visitedObjects, IEnumerable<string> excludedProperties)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        var type = obj.GetType();

        if (type.IsPrimitive || type == typeof(string)) return;

        if (!visitedObjects.Add(obj)) return;

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var nullPropertyNames = properties
            .Where(p => !excludedProperties.Contains(p.Name) && p.GetValue(obj) == null)
            .Select(p => p.Name);

        var propertyNames = nullPropertyNames as string[] ?? nullPropertyNames.ToArray();
        if (propertyNames.Any()) throw new ArgumentException($"Null properties: {string.Join(",", propertyNames)}");

        var notNullPropertyValues = properties
            .Select(p => p.GetValue(obj))
            .Where(v => v != null);

        foreach (var item in notNullPropertyValues)
            // ReSharper disable once PossibleMultipleEnumeration
            ThrowOnNullProperty(item!, visitedObjects, excludedProperties);
    }

    public static HashSet<string> GetPropertyNames(this object obj)
    {
        var properties = new HashSet<string>();
        GetPropertyNames(obj, properties, "", new HashSet<object>());
        return properties;
    }

    private static void GetPropertyNames(object obj, HashSet<string> propertyNames, string parent, HashSet<object> visitedObjects)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        var type = obj.GetType();

        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || !visitedObjects.Add(obj))
        {
            propertyNames.Add(parent);
            return;
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var notNullPropertyValues = properties
            .Select(v => (property: v, value: v.GetValue(obj)));

        foreach (var item in notNullPropertyValues)
        {
            var p = string.IsNullOrEmpty(parent) ? item.property.Name : $"{parent}.{item.property.Name}";
            if (item.value is null)
                propertyNames.Add(p);
            else
                GetPropertyNames(item.value, propertyNames, p, visitedObjects);
        }
    }
}