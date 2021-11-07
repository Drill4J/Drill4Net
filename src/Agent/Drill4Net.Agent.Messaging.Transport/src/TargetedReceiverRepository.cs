using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Messaging.Transport
{
    public class TargetedReceiverRepository : MessageReceiverRepository<MessagerOptions>
    {
        public Guid TargetSession { get; private set; }
        public string ConfigPath { get; private set; }

        /***************************************************************************************************/

        public TargetedReceiverRepository(): base(null) { }

        public TargetedReceiverRepository(string subsystem, string targetSession, string cfgPath = null): base(subsystem, cfgPath)
        {
            Init(targetSession, cfgPath);
        }

        public TargetedReceiverRepository(string subsystem, string targetSession, MessagerOptions opts, string cfgPath = null) : base(subsystem, opts)
        {
            Init(targetSession, cfgPath);
        }

        /***************************************************************************************************/

        private void Init(string targetSession, string configPath)
        {
            TargetSession = Guid.Parse(targetSession);
            ConfigPath = configPath;
            if (Options.Receiver == null)
                throw new Exception("No receiver section in config");
            if (Options.Receiver.Topics == null)
                Options.Receiver.Topics = new();
            PrepareLogger();
        }

        public void AddTopics(IEnumerable<string> topics)
        {
            foreach (var topic in topics)
                AddTopic(topic);
        }

        public void AddTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentNullException("Topic's name can't be empty");
            Options.Receiver.Topics.Add(topic);
        }
    }
}
