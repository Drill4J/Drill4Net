﻿using System;

namespace Drill4Net.Agent.Messaging
{
    public interface IMessageSenderRepository
    {
        MessageSenderOptions SenderOptions { get; set; }
        Guid TargetSession { get; }
        string Target { get; set; }
        string Subsystem { get; }
    }
}