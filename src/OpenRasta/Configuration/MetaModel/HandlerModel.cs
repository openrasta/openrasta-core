JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
﻿using OpenRasta.TypeSystem;

namespace OpenRasta.Configuration.MetaModel
{
    public class HandlerModel : ConfigurationModel
    {
        public HandlerModel(IType type)
        {
            Type = type;
        }

        public IType Type { get; set; }
    }
}