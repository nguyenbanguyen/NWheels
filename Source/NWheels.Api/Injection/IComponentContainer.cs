﻿using System;
using System.Collections.Generic;

namespace NWheels.Injection
{
    public interface IComponentContainer : IDisposable
    {
        TInterface Resolve<TInterface>();

        IEnumerable<TInterface> ResolveAll<TInterface>();
    }
}