using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatherUp.Order.Parsing
{
    class ParsingException : Exception
    {
        public ParsingException()
        {
        }

        public ParsingException(string message) : base(message)
        {
        }
    }
}
