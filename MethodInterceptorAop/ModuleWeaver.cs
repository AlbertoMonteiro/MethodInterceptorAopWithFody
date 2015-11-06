using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace MethodInterceptorAop.Fody
{
    public sealed class ModuleWeaver
    {
        // Will log an informational message to MSBuild
        public Action<string> LogInfo { get; set; }

        // An instance of Mono.Cecil.ModuleDefinition for processing
        public ModuleDefinition ModuleDefinition { get; set; }

        TypeSystem _typeSystem;
        private static readonly Type Type = typeof(IMethodInterception);
        private TypeReference _importReference;
        private TypeReference _methodBaseReference;
        private MethodReference _getCurrentMethod;

        // Init logging delegates to make testing easier
        public ModuleWeaver()
        {
            LogInfo = m => { };
        }

        public void Execute()
        {
            _importReference = ModuleDefinition.ImportReference(Type);
            _methodBaseReference = ModuleDefinition.ImportReference(typeof(MethodBase));
            _getCurrentMethod = ModuleDefinition.ImportReference(_methodBaseReference.Resolve().Methods.First(m => m.Name == "GetCurrentMethod"));

            var methodsToIntercept = ModuleDefinition.Types.SelectMany(t => t.Methods)
                                                     .Where(MethodShouldBeIntercepted)
                                                     .ToList();

            foreach (var methodDefinition in methodsToIntercept)
                WeaveInterception(methodDefinition);

            LogInfo("Added type 'Hello' with method 'World'.");
        }

        private void WeaveInterception(MethodDefinition methodDefinition)
        {
            var customAttribute = methodDefinition.CustomAttributes.First(ContainsMethodInterceptAtrribute);
            methodDefinition.Body.InitLocals = true;
            var ilProcessor = methodDefinition.Body.GetILProcessor();

            WeaveBeginInterception(ilProcessor, customAttribute.AttributeType);

            WeaveEndInterception(ilProcessor, customAttribute.AttributeType);
        }

        private void WeaveBeginInterception(ILProcessor ilProcessor, TypeReference onBegin) => InterceptMethod(ilProcessor, onBegin, ilProcessor.Body.Instructions.First(), "OnBegin");

        private void WeaveEndInterception(ILProcessor ilProcessor, TypeReference onEnd) => InterceptMethod(ilProcessor, onEnd, ilProcessor.Body.Instructions.Last(), "OnEnd");

        private void InterceptMethod(ILProcessor processor, TypeReference typeReference, Instruction instruction, string name)
        {
            var typeDefinition = typeReference.Resolve();
            var attributeConstructor = typeDefinition.Methods.First(x => x.Name == ".ctor");
            var attributeMethod = typeDefinition.Methods.First(x => x.Name == name);
            processor.InsertBefore(instruction, processor.Create(OpCodes.Newobj, attributeConstructor));
            processor.InsertBefore(instruction, processor.Create(OpCodes.Call, _getCurrentMethod));
            processor.InsertBefore(instruction, processor.Create(OpCodes.Call, attributeMethod));
        }

        private bool MethodShouldBeIntercepted(MethodDefinition m) => m.CustomAttributes.Any(ContainsMethodInterceptAtrribute);

        private bool ContainsMethodInterceptAtrribute(CustomAttribute at) => at.AttributeType.Resolve().Interfaces.Any(i => i.FullName == _importReference.FullName);
    }
}
