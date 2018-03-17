using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private static IOrderParser GetParser(string profielPath)
        {
            IOrderParser parser = new LegacyOrderParser(profielPath);
            if (parser.IsValidVersion)
            {
                return parser;
            }
            throw new NoParserException();
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