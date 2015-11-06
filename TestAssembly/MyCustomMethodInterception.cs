using System;
using System.Reflection;
using MethodInterceptorAop.Fody;

namespace TestAssembly
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MyCustomMethodInterception : Attribute, IMethodInterception
    {
        public void OnBegin(MethodBase methodBase)
        {
            TestClass.Strings.Add($"OnBegin: {methodBase.Name}");
        }

        public void OnEnd(MethodBase methodBase)
        {
            TestClass.Strings.Add($"OnEnd: {methodBase.Name}");
        }
    }
}