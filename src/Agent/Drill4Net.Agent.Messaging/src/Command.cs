﻿using System;

namespace Drill4Net.Agent.Messaging
{
    [Serializable]
    public class Command
    {
        public int Type { get; set; }
        public string Data { get; set; }

        /***********************************************************/

        public override string ToString()
        {
            return $"Command: {Type} -> {Data}";
        }
    }
}