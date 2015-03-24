﻿using Autofac;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace NWheels.Puzzle.EntityFramework
{
    public class ModuleLoader : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(SqlClientFactory.Instance).As<DbProviderFactory>();
        }
    }
}
