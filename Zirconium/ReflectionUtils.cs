using System.Collections;
using System.Reflection;
using Zirconium.Tools;

namespace Zirconium;

public static class ReflectionUtils
{
    public static IReadOnlyList<string> GetParamNames(Type type, string function)
    {
        ArgumentNullException.ThrowIfNull(type);

        var overloads = type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == function)
            .Where(m => !m.IsSpecialName)
            .DistinctBy(SignatureKey)
            .ToArray();

        return overloads.Length switch
        {
            0 => throw new InvalidOperationException($"No public {function} overloads were found on {type.FullName}"),
            1 => ParameterNames(overloads[0]),
            > 2 => throw new InvalidOperationException($"Too many {function} overloads on {type.FullName}"),
            2 => ParameterNames(ChooseBestOverload(overloads)),
            _ => throw new InvalidOperationException("Unexpected reflection state")
        };
    }

    public static IReadOnlyList<string> GetScanParameterNames(Scanner scanner) => GetParamNames(scanner.GetType(), nameof(Scanner.Scan));

    private static MethodInfo ChooseBestOverload(MethodInfo[] method)
    {
        bool IsSingleObjectScan(MethodInfo m)
        {
            var p = m.GetParameters();
            return p.Length == 1 && p[0].ParameterType == typeof(object);
        }

        var nonObject = method.Where(m => !IsSingleObjectScan(m)).ToArray();

        return nonObject.Length switch
        {
            1 => nonObject[0],
            > 1 => nonObject
                .OrderByDescending(m => m.GetParameters().Length)
                .First(),
            _ => method.Single(IsSingleObjectScan)
        };
    }

    private static IReadOnlyList<string> ParameterNames(MethodInfo method) =>
        method.GetParameters()
              .Select(p => p.Name ?? string.Empty)
              .ToArray();

    private static string SignatureKey(MethodInfo method) => $"{method.Name}({string.Join(",", method.GetParameters().Select(p => p.ParameterType.FullName ?? p.ParameterType.Name))})";


    public static object?[] ConvertTypes(this object?[] values, MethodInfo method)
    {
        ParameterInfo[] parameters = method.GetParameters();

        return values
            .Select((value, i) =>
                ConvertType(value, parameters[i].ParameterType))
            .ToArray();
    }

    private static object? ConvertType(object? value, Type targetType)
    {
        if (value == null)
            return null;

        // already assignable
        if (targetType.IsInstanceOfType(value))
            return value;

        // string -> IEnumerable wrapper
        if (value is string s &&
            targetType != typeof(string) &&
            typeof(IEnumerable).IsAssignableFrom(targetType))
        {
            Type elementType =
                targetType.IsArray
                    ? targetType.GetElementType()!
                    : targetType.GetGenericArguments().FirstOrDefault()
                        ?? typeof(string);

            // array
            if (targetType.IsArray)
            {
                Array array =
                    Array.CreateInstance(elementType, 1);

                array.SetValue(
                    Convert.ChangeType(s, elementType),
                    0);

                return array;
            }

            // List<T> / ICollection<T> / IEnumerable<T>
            object collection =
                Activator.CreateInstance(
                    targetType.IsInterface
                        ? typeof(List<>).MakeGenericType(elementType)
                        : targetType
                )!;

            if (collection is IList list)
            {
                list.Add(
                    Convert.ChangeType(s, elementType));

                return collection;
            }
        }

        return Convert.ChangeType(value, targetType);
    }
}
