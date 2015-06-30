using System;
using Microsoft.Framework.Caching;

namespace KHttp.Tests.Common
{
    internal class TestFileTrigger : IExpirationTrigger
    {
        public bool ActiveExpirationCallbacks { get; } = false;

        public bool IsExpired { get; set; }

        public IDisposable RegisterExpirationCallback(Action<object> callback, object state)
        {
            throw new NotImplementedException();
        }
    }
}
