// ReSharper disable UnusedType.Global
#pragma warning disable CS9113 // Parameter is unread.
namespace TMReflexionModeler.Taint;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class ThreatModelProcessAttribute(string processName) : Attribute;
