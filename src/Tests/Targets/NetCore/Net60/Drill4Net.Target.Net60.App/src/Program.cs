﻿// See https://aka.ms/new-console-template for more information
using Drill4Net.Target.Common;

Console.WriteLine("Press any key when you'll have started the session on Admin side (if needed)");
Console.ReadKey(true);

try
{
    await new ModelTarget().RunTests();
    Console.WriteLine("\nDone.");
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
Console.ReadKey(true);
