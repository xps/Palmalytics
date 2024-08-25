#if !NET7_OR_GREATER

// This is necessary to use the `required` keyword in .NET < 7.0

namespace System.Runtime.CompilerServices
{
    internal class RequiredMemberAttribute : Attribute
    {
    }

    internal class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string name) { }
    }
}

#endif