using System;
using System.Collections.Generic;
using Microsoft.Framework.ConfigurationModel;

namespace KHttp
{
    public class ConfigurationBuilder
    {
        private readonly string[] _args;
        private readonly IEnumerable<KeyValuePair<string, string>> _defaultConfigData;

        public ConfigurationBuilder(string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            _args = args;
            _defaultConfigData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("server.urls", "http://localhost:5000")
            };
        }

        public Configuration Build()
        {
            var config = new Configuration();
            config.AddInMemoryConfig(_defaultConfigData);
            config.AddCommandLine(_args);

            return config;
        }
    }
}