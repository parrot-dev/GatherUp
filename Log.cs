using ff14bot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GatherUp
{
    static class Log
    {
        public static class Bot
        {
            public static void print(string input, Color col)
            {
                Logging.Write(col, string.Format("[GatherUp] {0}", input));
            }

            public static void print(string input)
            {
                print(input, Colors.Red);
            }
        }
    }
}
