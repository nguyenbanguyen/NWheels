﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public abstract class DataObjectContractAttribute : Attribute
    {
        public static bool IsDataObjectContract(Type type)
        {
            return (type.IsInterface && type.GetCustomAttribute<DataObjectContractAttribute>() != null);
        }
    }
}