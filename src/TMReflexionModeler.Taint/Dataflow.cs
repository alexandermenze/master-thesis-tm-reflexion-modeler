// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global
namespace TMReflexionModeler.Taint;

public static class Dataflow
{
    public static T Push<T>(string name, Func<T> func) => func();

    public static void Push(string name, Action action) => action();

    public static Task Push(string name, Func<Task> taskFunc) => taskFunc();

    public static Task<T> Push<T>(string name, Func<Task<T>> taskFunc) => taskFunc();

    public static T Pull<T>(string name, Func<T> func) => func();

    public static Task<T> Pull<T>(string name, Func<Task<T>> taskFunc) => taskFunc();
}
