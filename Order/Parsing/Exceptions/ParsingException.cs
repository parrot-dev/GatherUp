using System;

namespace GatherUp.Order.Parsing.Exceptions
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
