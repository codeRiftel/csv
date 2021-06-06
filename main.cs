using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

class Init {
    private static int Main(string[] args) {
        List<string> tests = new List<string>();

        string generic = "one,two,three,four,five\r\n";
        generic += "six,seven,eight,nine";

        string finishCrlf = "one,two,three,four\r\n";
        string quoted = "one,\"two\",three";
        string commaInsideValue = "one,\"t,wo\",three";
        string quoteInsideValue = "one,\"t\"\"wo\",three";
        string crlfInsideValue = "one,\"t\r\nwo\",three";
        string hangingValue = "one,\"two,three";
        string quoteInUnquoted = "one,t\"wo,three";
        string emptyValues = "one,two,,,five";
        string empty = ",,,,,";
        string startEmpty = ",one,two,three";
        string endEmpty = "one,two,three,";

        tests.Add(generic);
        tests.Add(finishCrlf);
        tests.Add(quoted);
        tests.Add(commaInsideValue);
        tests.Add(quoteInsideValue);
        tests.Add(crlfInsideValue);
        tests.Add(hangingValue);
        tests.Add(quoteInUnquoted);
        tests.Add(emptyValues);
        tests.Add(empty);
        tests.Add(startEmpty);
        tests.Add(endEmpty);

        foreach (var test in tests) {
            Console.WriteLine("TEST: ");
            Console.WriteLine(test);

            Console.WriteLine("RESULT: ");
            CSV.ParseRes res = CSV.Parse(test);
            if (res.error != CSV.ErrorType.None) {
                Console.WriteLine("ERROR: " + res.error);
            } else {
                string restored = CSV.Generate(res.rows);
                Console.Write(restored);
            }
        }

        return 0;
    }
}
