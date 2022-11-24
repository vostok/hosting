using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Metrics;

namespace Vostok.Hosting.Helpers;

internal static class IVostokHostExtensionsExtensions
{
    [CanBeNull]
    public static LifecycleAnnotationsAdditionalTags GetLifecycleAnnotationsTags(this IVostokHostExtensions hostExtensions)
    {
        hostExtensions.TryGet<LifecycleAnnotationsAdditionalTags>(out var additionalTags);
        return additionalTags;
    }
}