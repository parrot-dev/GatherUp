using System;
using System.Collections.Generic;
using System.Linq;
using GatherUp.Order.Parsing.Exceptions;

namespace GatherUp.Order.Parsing
{
    class ProfileParser
    {
        private readonly IOrderParser _parser;

        ///<exception cref = "NoParserException" />
        public ProfileParser(string profilePath)
        {
            _parser = GetParser(profilePath);
        }

        private static IOrderParser GetParser(string profilePath)
        {
            try
            {
                var parsers = new List<IOrderParser>
                {
                    new OrderParserTwo(profilePath), 
                    new OrderParserOne(profilePath) 
                };
                return parsers.First(parser => parser.IsValidVersion);
            }
            catch (Exception)
            {
                throw new NoParserException();
            }
        }
        
        ///<exception cref = "ParsingException" />
        public Profile ToProfile()
        {
            return _parser.ToProfile();
        }

        public IOrderParser GetParser()
        {
            return _parser;
        }


    }
}