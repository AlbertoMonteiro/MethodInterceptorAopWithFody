using System.Reflection;

namespace MethodInterceptorAop.Fody
{
    public interface IMethodInterception
    {
        void OnBegin(MethodBase methodBase);
        void OnEnd(MethodBase methodBase);
    }
}