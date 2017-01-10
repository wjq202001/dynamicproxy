using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gogo.DynamicProxy
{
    public interface IInterceptor
    {
        void Intercept(IInvocation invocation);
    }
}
