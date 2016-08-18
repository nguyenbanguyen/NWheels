﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    public interface ITypeMetadataCache
    {
        ITypeMetadata GetTypeMetadata(Type contract);
        void EnsureRelationalMapping(ITypeMetadata type);
    }
}