namespace Vostok.Hosting.Components.Metrics;

internal sealed class LifecycleAnnotationsAdditionalTags
{
    public (string key, string value)[] Tags { get; }

    public LifecycleAnnotationsAdditionalTags((string key, string value)[] tags)
    {
        Tags = tags;
    }
}