using RayTracing.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace RayTracing.Web.Helpers
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description ?? enumValue.ToString();
        }


        public static NameIdModel ToNameIdModel(this Enum enumValue)
        {
            return new NameIdModel
            {
                Id = Convert.ToInt32(enumValue),
                Name = enumValue.GetDescription()
            };
        }
    }
}
