using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gogo.DynamicProxy
{
    public class MyClassProxy
    {
        private MyClass _myClass;
        private IInterceptor _interceptor;
        private IInvocation _invocation;
        public MyClassProxy(MyClass myclass, IInterceptor interceptor)
        {
            _myClass = myclass;
            _interceptor = interceptor;
        }

        public string Hello(string name)
        {
            _invocation = new Invocation();
            _invocation.Target = _myClass;
            _invocation.TargetMethod = typeof(MyClass).GetMethod("Hello");
            _invocation.TargetArgs = new object[] { name };
            _interceptor.Intercept(_invocation);

            return _invocation.ReturnValue as string;
        }
    }

    public class Interceptor:IInterceptor
    {

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
        }
    }

    public class Invocation : IInvocation
    {

        public object ReturnValue
        {
            get;
            set;
        }

        public void Proceed()
        {
            this.ReturnValue = TargetMethod.Invoke(Target, TargetArgs);
        }

        public System.Reflection.MethodInfo TargetMethod
        {
            get;
            set;
        }


        public object[] TargetArgs
        {
            get;
            set;
        }

        public object Target
        {
            get;
            set;
        }
    }
}
