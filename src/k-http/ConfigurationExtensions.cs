using System;
using System.Collections.Generic;
using Microsoft.Framework.Configuration;

namespace KHttp
{
    internal static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddInMemoryConfig(this IConfigurationBuilder builder, IEnumerable<KeyValuePair<string, string>> initialData)
        {
            if (initialData == null)
            {
                throw new ArgumentNullException(nameof(initialData));
            }

            builder.Add(new MemoryConfigurationSource(initialData));
            return builder;
        }
    }
}
