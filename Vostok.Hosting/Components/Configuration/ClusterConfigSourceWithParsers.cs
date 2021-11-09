using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Sources.ClusterConfig;
using Vostok.Configuration.Sources.Json;
using Vostok.Configuration.Sources.Xml;
using Vostok.Configuration.Sources.Yaml;

namespace Vostok.Hosting.Components.Configuration
{
    [PublicAPI]
    public class ClusterConfigSourceWithParsers : ClusterConfigSource
    {
        public ClusterConfigSourceWithParsers([NotNull] IClusterConfigClient client, [NotNull] string prefix)
            : base(new ClusterConfigSourceSettings(client, prefix)
            {
                ConditionalValuesParsers = new List<(ValueNodeParser, ValueNodeCondition)>
                {
                    (XmlConfigurationParser.Parse, node => node.Name != null && node.Name.EndsWith(".xml")),
                    (JsonConfigurationParser.Parse, node => node.Name != null && node.Name.EndsWith(".json")),
                    (YamlConfigurationParser.Parse, node => node.Name != null && (node.Name.EndsWith(".yaml") || node.Name.EndsWith("yml")))
                }
            })
        {
        }
    }
}