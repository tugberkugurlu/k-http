using System;
using System.Collections.Generic;
using Microsoft.Framework.ConfigurationModel;

namespace KHttp
{
    internal static class ConfigurationExtensions
    {
        public static IConfigurationSourceRoot AddInMemoryConfig(this IConfigurationSourceRoot configuration, IEnumerable<KeyValuePair<string, string>> initialData)
        {
            if (initialData == null)
            {
                throw new ArgumentNullException(nameof(initialData));
            }

            configuration.Add(new MemoryConfigurationSource(initialData));
            return configuration;
        }
    }
}
