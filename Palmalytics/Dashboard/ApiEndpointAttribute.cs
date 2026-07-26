using System;

namespace Palmalytics.Dashboard
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class ApiEndpointAttribute : Attribute
    {
    }
}
