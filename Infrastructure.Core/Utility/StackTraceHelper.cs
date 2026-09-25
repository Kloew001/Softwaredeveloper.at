using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class StackTraceHelper
{
    public static string GetExecutingAssemblyCallStack()
    {
        var prefix = Assembly.GetExecutingAssembly()
            .GetName().Name!
            .Split('.')[0];

        return GetApplicationStack(prefix);
    }

    public static string GetApplicationStack(params string[] assemblyPrefixes)
    {
        var frames = new StackTrace(fNeedFileInfo: true).GetFrames();

        var lines = frames
            .Select(frame => new
            {
                Frame = frame,
                Method = ResolveMethod(frame.GetMethod())
            })
            .Where(x => x.Method?.DeclaringType != typeof(StackTraceHelper))
            .Where(x =>
            {
                var name = x.Method?.DeclaringType?.Assembly.GetName().Name;

                return name is not null && assemblyPrefixes.Any(prefix =>
                    !string.IsNullOrWhiteSpace(prefix) &&
                    (string.Equals(name, prefix.TrimEnd('.'), StringComparison.Ordinal) ||
                     name.StartsWith(prefix.TrimEnd('.') + ".", StringComparison.Ordinal)));
            })
            .Take(8)
            .Select(x =>
            {
                var method = x.Method!;
                var line = x.Frame.GetFileLineNumber();
                var file = x.Frame.GetFileName();
                var location = line > 0 && file is not null
                    ? $" ({Path.GetFileName(file)}:{line})"
                    : string.Empty;

                return $"{method.DeclaringType!.FullName}.{method.Name}{location}";
            });

        return string.Join(Environment.NewLine, lines);
    }

    private static MethodBase? ResolveMethod(MethodBase? method)
    {
        var stateMachineType = method?.DeclaringType;

        if (method?.Name != "MoveNext" ||
            stateMachineType is null ||
            !typeof(IAsyncStateMachine).IsAssignableFrom(stateMachineType))
        {
            return method;
        }

        var declaringType = stateMachineType.DeclaringType;
        if (declaringType is null)
            return method;

        var definition = stateMachineType.IsGenericType
            ? stateMachineType.GetGenericTypeDefinition()
            : stateMachineType;

        return declaringType.GetMethods(
                BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(candidate =>
                candidate.GetCustomAttribute<AsyncStateMachineAttribute>()
                    ?.StateMachineType == definition)
            ?? method;
    }
}
