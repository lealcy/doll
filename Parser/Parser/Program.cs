using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Parser
{
    class Program
    {

        static void Main(string[] args)
        {
            Parser p = new Parser();
            string code;
            int result;
            while (true)
            {
                Console.Write("> ");
                code = Console.ReadLine();
                p.ErrorList.Clear();
                p.ParsedCode.Clear();
                result = p.Parse(code);
                if (result > 0) Console.WriteLine("Characters parsed: {0}", result);
                if (p.ErrorList.Count() > 0)
                {
                    foreach (var s in p.ErrorList)
                    {
                        Console.WriteLine(s);
                    }
                }
            }
        }
    }

}
