using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace team.pinewood.utilities
{
    public static class Reflection
    {
        public static T GetField<C, T>(C instance, string fieldName, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance)
        {
            return (T)(typeof(C).GetField(fieldName, flags).GetValue(instance));
        }

        public static void SetField<C>(C instance, string fieldName, object value, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance)
        {
            typeof(C).GetField(fieldName, flags).SetValue(instance, value);
        }
    }
}
