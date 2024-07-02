﻿using System.Linq;
using System.Text;
using static System.String;

namespace GatherUp.Helpers
{
    static class CodeIndentor
    {
        public static string IndentCode(string[] code, int initialSpacing = 0)
        {
            if (code == null) return Empty;
            var curTabs = initialSpacing;
            var sb = new StringBuilder();
            foreach (var line in code)
            {
                sb.AppendLine($"{GetSpaces(curTabs)}{line}");
                curTabs += line.Count(c => c.Equals('{')) - line.Count(c => c.Equals('}'));
            }
            return sb.ToString().TrimEnd();
        }

        private static string GetSpaces(int num)
        {
            var arr =  new string[num];
            for (int i = 0; i < num; i++)
            {
                arr[i] = "  ";
            }
            return Join(Empty, arr);
        }
    }
}
