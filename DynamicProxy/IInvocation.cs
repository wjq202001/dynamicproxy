using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Gogo.DynamicProxy
{
    public interface IInvocation
    {
        object Target { get; set; }
        MethodInfo TargetMethod { get; set; }
        object[] TargetArgs { get; set; }
        object ReturnValue { get; set; }
        void Proceed();
    }
}
