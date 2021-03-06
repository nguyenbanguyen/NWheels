﻿using System;
using FluentAssertions;
using NWheels.Injection;
using NWheels.Microservices;
using Xunit;
using System.Reflection;

namespace NWheels.Implementation.UnitTests.Microservices
{
    public class MicroserviceHostBuilderTests
    {
        [Fact]
        public void UseApplicationFeature_DefaultFeature_ModuleMissing_OnlyModuleAdded()
        {
            //-- arrange

            var microservice = new MicroserviceHostBuilder("test");

            //-- act

            microservice.UseApplicationFeature<DefaultFeatureLoader>();

            //-- assert

            microservice.BootConfig.MicroserviceConfig.ApplicationModules.Length.Should().Be(1);

            var module = microservice.BootConfig.MicroserviceConfig.ApplicationModules[0];

            module.Assembly.Should().Be(this.GetType().GetTypeInfo().Assembly.GetName().Name);
            module.Features.Length.Should().Be(0);                
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void UseApplicationFeature_NamedFeature_ModuleMissing_ModuleAndFeatureAdded()
        {
            //-- arrange

            var microservice = new MicroserviceHostBuilder("test");

            //-- act

            microservice.UseApplicationFeature<NamedFeatureLoader>();

            //-- assert

            microservice.BootConfig.MicroserviceConfig.ApplicationModules.Length.Should().Be(1);

            var module = microservice.BootConfig.MicroserviceConfig.ApplicationModules[0];

            module.Assembly.Should().Be(this.GetType().GetTypeInfo().Assembly.GetName().Name);
            module.Features.Length.Should().Be(1);
            module.Features[0].Name.Should().Be("Named");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class DefaultFeatureLoader : FeatureLoaderBase
        {
        }

        [FeatureLoader(Name = "Named")]
        private class NamedFeatureLoader : FeatureLoaderBase
        {
        }
    }
}
