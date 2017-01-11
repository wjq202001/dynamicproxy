using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gogo.DynamicProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            MyClass myClass = new MyClass("Wang Juqiang");
            
            var myClassProxy = DynamicProxy.Create<IMyInterface, MyClass>(myClass,new Interceptor());
            
            Console.WriteLine(myClassProxy.Hello("Lao Ju"));
            Console.WriteLine(myClassProxy.Name);
            Console.Read();

            //MyClass myClass = new MyClass("Wang Juqiang");
            //IInterceptor interceptor = new Interceptor();
            //MyClassProxy proxy = new MyClassProxy(myClass, interceptor);
            //Console.WriteLine(proxy.Hello("eang"));
            //Console.Read();
        }
    }
}
