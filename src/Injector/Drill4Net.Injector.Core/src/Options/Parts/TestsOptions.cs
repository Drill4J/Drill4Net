﻿using System.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    public class TestsOptions
    {
        public Dictionary<string, List<string>> Assemblies { get; set; }

        /******************************************************/

        public TestsOptions()
        {
            Assemblies = new Dictionary<string, List<string>>();
        }
    }
}
