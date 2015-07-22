﻿using System;
using Autofac;
using NUnit.Framework;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Testing
{
    [TestFixture, Category(TestCategory.Unit)]
    public abstract class UnitTestBase : TestFixtureWithoutNodeHosts
    {
    }
}
