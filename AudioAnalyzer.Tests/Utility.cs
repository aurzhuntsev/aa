using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AudioAnalyzer.Tests
{
    public static class Utility
    {
        public static object InvokePrivateMethod<T>(string methodName, T target, object?[]? args)
        {
            var methodInfo = typeof(T).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return methodInfo.Invoke(target, args);
        }
    }
}
