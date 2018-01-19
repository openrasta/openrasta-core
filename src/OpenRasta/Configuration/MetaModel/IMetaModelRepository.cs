using System.Collections;
using System.Collections.Generic;

namespace OpenRasta.Configuration.MetaModel
{
    public interface IMetaModelRepository
    {
        IList<ResourceModel> ResourceRegistrations { get; }
        IList CustomRegistrations { get; }
        void Process();
    }
}