﻿namespace Drill4Net.Agent.Messaging
{
    public interface ICommandSender
    {
        void SendCommand(int type, string data);
    }
}