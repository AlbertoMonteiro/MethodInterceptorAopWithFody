using System;
using System.IO;
using System.Reflection;
using MethodInterceptorAop.Fody;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using NCrunch.Framework;
using TestAssembly;

namespace MethodInterceptorAop.Tests
{
    [TestClass]
    public class UnitTest1
    {
        Assembly _assembly;
        string _newAssemblyPath;
        string _assemblyPath;

        [TestInitialize]
        public void TestMethod1()
        {
            var directoryName = Path.GetDirectoryName(NCrunchEnvironment.GetOriginalSolutionPath());
            var projectPath = Path.GetFullPath(Path.Combine(directoryName, @"TestAssembly\TestAssembly.csproj"));
            _assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\TestAssembly.dll");

            _newAssemblyPath = _assemblyPath.Replace(".dll", "2.dll");
            File.Copy(_assemblyPath, _newAssemblyPath, true);

            var moduleDefinition = ModuleDefinition.ReadModule(_newAssemblyPath);
            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition
            };

            weavingTask.Execute();
            moduleDefinition.Write(_newAssemblyPath);

            _assembly = Assembly.LoadFile(_newAssemblyPath);
        }

        [TestMethod]
        public void ValidateHelloWorldIsInjected()
        {
            var type = _assembly.GetType("TestAssembly.TestClass");
            var instance = (TestClass)Activator.CreateInstance(type);

            instance.MyMethod();
            CollectionAssert.AreEquivalent(TestClass.Strings, new[] { "OnBegin: MyMethod", "Running MyMethod", "OnEnd: MyMethod" });
        }
    }
}
