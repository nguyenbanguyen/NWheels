﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.Configuration;
using NWheels.Entities;

namespace NWheels.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsExceptionType(this Type type)
        {
            return typeof(Exception).IsAssignableFrom(type);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string AssemblyQualifiedNameNonVersioned(this Type type)
        {
            //TODO: recurse to handle generic type parameters as well!
            return string.Format("{0}, {1}", type.FullName, type.Assembly.GetName().Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsConstructedGenericTypeOf(this Type type, Type genericTypeDefinition)
        {
            return (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsAnonymousType(this Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            var nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            return (hasCompilerGeneratedAttribute && nameContainsAnonymousType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type Replace(this Type type, Type findWhat, Type replaceWith)
        {
            if ( type == findWhat )
            {
                return replaceWith;
            }
            
            if ( type.IsGenericType )
            {
                var genericArguments = type.GetGenericArguments();
                var replacedAny = false;

                for ( int i = 0 ; i < genericArguments.Length ; i++ )
                {
                    var replacedGenericArgument = genericArguments[i].Replace(findWhat, replaceWith);
                    
                    if ( replacedGenericArgument != genericArguments[i] )
                    {
                        genericArguments[i] = replacedGenericArgument;
                        replacedAny = true;
                    }
                }

                if ( replacedAny )
                {
                    return type.GetGenericTypeDefinition().MakeGenericType(genericArguments);
                }
            }

            return type;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type Replace(this Type type, Func<Type, bool> findWhat, Func<Type, Type> replaceWith)
        {
            if ( findWhat(type) )
            {
                return replaceWith(type);
            }

            if ( type.IsGenericType )
            {
                var genericArguments = type.GetGenericArguments();
                var replacedAny = false;

                for ( int i = 0; i < genericArguments.Length; i++ )
                {
                    var replacedGenericArgument = genericArguments[i].Replace(findWhat, replaceWith);

                    if ( replacedGenericArgument != genericArguments[i] )
                    {
                        genericArguments[i] = replacedGenericArgument;
                        replacedAny = true;
                    }
                }

                if ( replacedAny )
                {
                    return type.GetGenericTypeDefinition().MakeGenericType(genericArguments);
                }
            }

            return type;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsConfigurationContract(this Type contract)
        {
            return (contract.IsInterface && ConfigurationSectionAttribute.IsConfigSection(contract));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsEntityContract(this Type contract)
        {
            return (contract.IsInterface && EntityContractAttribute.IsEntityContract(contract));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsEntityPartContract(this Type contract)
        {
            return (contract.IsInterface && EntityPartContractAttribute.IsEntityPartContract(contract));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string SimpleQualifiedName(this Type type)
        {
            if ( type.Namespace != null )
            {
                var significantNamespaceParts = type.Namespace.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(IsSignificantNamespacePart)
                    .ToArray();

                if ( significantNamespaceParts.Length > 0 )
                {
                    return significantNamespaceParts[significantNamespaceParts.Length - 1] + "." + type.FriendlyName();
                }
            }

            return type.FriendlyName();
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool IsSignificantNamespacePart(string part)
        {
            var comparer = StringComparer.InvariantCultureIgnoreCase;

            return (
                comparer.Compare(part, "Domain") != 0 && 
                comparer.Compare(part, "Core") != 0 && 
                comparer.Compare(part, "Impl") != 0 && 
                comparer.Compare(part, "UI") != 0 && 
                comparer.Compare(part, "Entities") != 0);
        }
    }
}
