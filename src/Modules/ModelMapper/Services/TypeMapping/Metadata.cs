﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevExpress.ExpressApp.Model;
using Fasterflect;
using Xpand.Source.Extensions.Linq;
using Xpand.Source.Extensions.System.Refelction;

namespace Xpand.XAF.Modules.ModelMapper.Services.TypeMapping{
    public static partial class TypeMappingService{
        private static Version _modelMapperModuleVersion;
        private static string[] AllAssemblies(this Type type, PropertyInfo[] propertyInfos){
            return propertyInfos.Select(_ => _.PropertyType)
                .Concat(propertyInfos.SelectMany(_ => _.GetCustomAttributesData())
                    .SelectMany(_ => _.ConstructorArguments)
                    .Select(_ => _.Value == null ? _.ArgumentType : _.Value.GetType()))
                .Concat(new []{type})
                .Concat(AdditionalReferences)
                .Select(_ => _.Assembly.Location)
                .Distinct()
                .ToArray();
        }

        private static Type GetRealType(this Type type){
            if (type != typeof(string)){
                if (typeof(IEnumerable).IsAssignableFrom(type)){
                    if (type.IsGenericType){
                        return type.GenericTypeArguments.First();
                    }
                    if (type.IsArray){
                        return type.GetElementType();
                    }

                    if (typeof(ICollection).IsAssignableFrom(type)){
                        var result = GetParameterType(type, "Add");
                        if (result == null){
                            result = GetParameterType(type, "CopyTo");
                            if (result != null)
                                return result;
                        }
                    }
                    var interfaceType = type.GetInterfaces().FirstOrDefault(_ => _.IsGenericType&&typeof(IEnumerable).IsAssignableFrom(_));
                    if (interfaceType!=null){
                        return interfaceType.GenericTypeArguments.First();
                    }

                    return typeof(object);
                }
            }
            return type;
        }

        private static Type GetParameterType(Type type,string name){
            var methodInfo = type.Methods(name).FirstOrDefault(info => info.Parameters().Any(parameterInfo => parameterInfo.ParameterType!=typeof(object)));
            if (methodInfo != null){
                var parameterType = methodInfo.Parameters().First().ParameterType;
                if (parameterType!=type&&parameterType!=typeof(Array)){
                    return parameterType.GetRealType();
                }

                return parameterType;
            }

            return null;
        }

        private static Type[] AdditionalTypes(this PropertyInfo[] propertyInfos,Type type){
            return propertyInfos
                .Where(_ => !_.PropertyType.IsValueType && typeof(string) != _.PropertyType && _.PropertyType != type)
                .Select(_ => _.PropertyType)
                .DistinctBy(_ => (_,type).ModelName())
                .Where(_ => _.GetRealType()!=typeof(object))
                .ToArray();
        }

        private static PropertyInfo[] PropertyInfos(this Type type){
            return type.PublicProperties(true)
                .GetItems<PropertyInfo>(_ => _.PropertyType.GetRealType().PublicProperties(true), info => info.PropertyType)
                .ToArray();
        }
        
        private static IEnumerable<PropertyInfo> PublicProperties(this Type type,bool includeCollections=false){
            return type.Properties(Flags.AllMembers)
                .Where(info => !info.PropertyType.IsValueType && info.PropertyType != typeof(string) ||info.CanRead && info.CanWrite)
                .Where(info => info.IsValid(includeCollections))
                .Where(info => {
                    if (info.PropertyType == typeof(string) || info.PropertyType.IsNullableType()) return true;
                    var propertyTypeIsReserved = ReservedPropertyTypes.Any(_ => info.PropertyType!=_);
                    if (includeCollections && typeof(IEnumerable).IsAssignableFrom(info.PropertyType))
                        return true;
                    return !info.PropertyType.IsGenericType && info.PropertyType != type &&info.PropertyType != typeof(object) && propertyTypeIsReserved;
                })
                .DistinctBy(info => info.Name);
        }

        private static bool IsValid(this PropertyInfo info,bool includeCollections){
            var isValid = info.AccessModifier() == AccessModifier.Public && !ReservedPropertyNames.Contains(info.Name);
            if (!isValid) return false;
            if (includeCollections&&typeof(IEnumerable).IsAssignableFrom(info.PropertyType)){
                return true;
            }
            return !(typeof(IEnumerable).IsAssignableFrom(info.PropertyType) && info.PropertyType != typeof(string));
        }


        private static object GetEnums(this Type enumType, object value){
            if (EnumsNET.NonGeneric.NonGenericFlagEnums.IsFlagEnum(enumType)&&EnumsNET.NonGeneric.NonGenericFlagEnums.HasAnyFlags(enumType,value)){
                return string.Join("|", EnumsNET.NonGeneric.NonGenericFlagEnums.GetFlagMembers(enumType, value)
                    .Select(member => $"{enumType.FullName}.{member.Name}"));
            }

            var name = Enum.GetName(enumType, value);
            return $"{enumType.FullName}.{name}";
        }


    }
}