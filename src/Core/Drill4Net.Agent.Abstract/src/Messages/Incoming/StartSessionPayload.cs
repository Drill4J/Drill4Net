﻿using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public class StartSessionPayload
    {
        public string SessionId { get; set; }
        
        /// <summary>
        /// MANUAL or AUTO
        /// </summary>
        public string TestType { get; set; }
        public string TestName { get; set; }
        public bool IsRealtime { get; set; }
        public bool IsGlobal { get; set; }
    }
}