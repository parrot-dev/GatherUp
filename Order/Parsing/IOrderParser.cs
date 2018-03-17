using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatherUp.Order.Parsing
{
    interface IOrderParser
    {
        Profile ToProfile();
        bool IsValidVersion { get; }
    }
}
