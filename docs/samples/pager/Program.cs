using System;
using McMaster.Extensions.CommandLineUtils;

class Program
{
    static void Main(string[] args)
    {
        // Instead of writing to stdout directly with Console.WriteLine,
        // this will create a console pager and display output in a
        // searchable, scrollable view.

        using (var pager = new Pager())
        {
            for (var i = 1; i <= 1000; i++)
            {
                pager.Writer.WriteLine($"This is sentence {i} of 1,000");
            }
        }
    }
}
