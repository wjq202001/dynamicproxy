using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gogo.DynamicProxy
{
    public interface IMyInterface
    {
        string Name { get; set; }
        string Hello(string name);
    }
}
