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
            var myClassProxy = DynamicProxy.Create<IMyInterface, MyClass>(myClass);

            Console.WriteLine(myClassProxy.Hello("Lao Ju"));
            Console.WriteLine(myClassProxy.Name);
            Console.Read();

        }
    }
}
