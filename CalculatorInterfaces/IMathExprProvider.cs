using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcularorInterfaces
{
    public interface IMathExprProvider
    {
        string Evaluate(string expression);
    }
}
