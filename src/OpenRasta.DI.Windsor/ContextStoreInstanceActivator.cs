using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.ComponentActivator;
using Castle.MicroKernel.Context;
using OpenRasta.Pipeline;
using System.Diagnostics;

namespace OpenRasta.DI.Windsor
{
    public class ContextStoreInstanceActivator : AbstractComponentActivator
    {
        readonly string storeKey;

        public ContextStoreInstanceActivator(ComponentModel model, IKernelInternal kernelInternal, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
            : base(model, kernelInternal, onCreation, onDestruction)
        {
            storeKey = model.Name;
        }

        protected override object InternalCreate(CreationContext context)
        {
            var store = (IContextStore) Kernel.Resolve(typeof (IContextStore));
            if (store[storeKey] == null)
            {
                Debug.WriteLine("The instance is not present in the context store");
                return null;
            }

            return store[storeKey];
        }

        protected override void InternalDestroy(object instance)
        {
            var store = (IContextStore) Kernel.Resolve(typeof (IContextStore));

            store[storeKey] = null;
        }
    }
}