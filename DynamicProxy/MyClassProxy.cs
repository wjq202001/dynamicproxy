using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Gogo.DynamicProxy
{
    public class MyClassProxy
    {
        private MyClass _myClass;
        private IInterceptor _interceptor;
        //private IInvocation _invocation;
        public MyClassProxy(MyClass myclass, IInterceptor interceptor)
        {
            _myClass = myclass;
            _interceptor = interceptor;
        }

        public string Hello()
        {
            var target = new MyClass("n");
            var hello = target.GetType().GetMethod("Hello");
            var _invocation = new Invocation(new object());
            _invocation.Target = target;
            _invocation.TargetMethod = hello;
            _invocation.TargetArgs = hello.GetParameters();
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

        public Invocation(Object obj)
        {
            this.Target = obj;
        }

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
