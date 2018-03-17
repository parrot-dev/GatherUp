using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatherUp.Order.Parsing
{
    class NoParserException : Exception
    {
        public NoParserException()
        {
        }

        public NoParserException(string message) : base(message)
        {
        }
    }
}
