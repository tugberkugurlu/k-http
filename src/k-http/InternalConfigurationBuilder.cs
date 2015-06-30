using System;
using System.Collections.Generic;
using Microsoft.Framework.Configuration;

namespace KHttp
{
    public class InternalConfigurationBuilder
    {
        private readonly string[] _args;
        private readonly IEnumerable<KeyValuePair<string, string>> _defaultConfigData;

        public InternalConfigurationBuilder(string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            _args = args;
            _defaultConfigData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("server.urls", "http://localhost:5000")
            };
        }

        public IConfiguration Build()
        {
            var builder = new ConfigurationBuilder();
            builder.AddInMemoryConfig(_defaultConfigData);
            builder.AddCommandLine(_args);

            return builder.Build();
        }
    }
}
