using System;
using System.IO;
using System.Linq;
using Injector.Core;

namespace TestA.Interceptor
{
    class Program
    {
        static void Main(string[] args)
        {
            var injector = new InjectorEngine();
            injector.Process(null);
        }
    }
}
