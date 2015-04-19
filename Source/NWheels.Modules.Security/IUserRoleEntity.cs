﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Modules.Security
{
    [EntityContract]
    [MustHaveMixin(typeof(IEntityPartId<>))]
    public interface IUserRoleEntity<TRole>
    {
        [PropertyContract.Required, PropertyContract.Unique]
        string Name { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [PropertyContract.Required]
        TRole Role { get; set; }
    }
}