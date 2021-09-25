﻿using System;
using System.Collections.Generic;

namespace RayTracing.Web.Helpers
{
    public class EnumUtils
    {
        public static IReadOnlyList<T> GetValues<T>() => (T[])Enum.GetValues(typeof(T));
    }
}
