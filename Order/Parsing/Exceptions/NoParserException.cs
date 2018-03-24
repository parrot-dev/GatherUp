using System;
namespace GatherUp.Order.Parsing.Exceptions
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
