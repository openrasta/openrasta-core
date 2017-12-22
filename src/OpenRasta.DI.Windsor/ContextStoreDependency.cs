using System;
using OpenRasta.Pipeline;

namespace OpenRasta.DI.Windsor
{
    public class ContextStoreDependency
    {
        public ContextStoreDependency(string key, object instance, IContextStoreDependencyCleaner cleaner)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            Cleaner = cleaner;
        }

        public IContextStoreDependencyCleaner Cleaner { get; set; }
        public object Instance { get; set; }
        public string Key { get; set; }
    }
}