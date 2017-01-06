using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gogo.DynamicProxy
{
    public class MyClass
    {
        public MyClass(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
        public string Hello(string name)
        {
            return "hello " + name;
        }
    }
}
