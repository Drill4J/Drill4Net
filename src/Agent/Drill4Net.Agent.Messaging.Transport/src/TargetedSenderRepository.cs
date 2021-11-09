using System;
using System.Collections.Generic;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Messaging.Transport
{
    public class TargetedSenderRepository : ConfiguredRepository<MessagerOptions, BaseOptionsHelper<MessagerOptions>>, IMessagerRepository
    {
        public Guid TargetSession { get; private set; }

        public string TargetName { get; private set; }

        public MessagerOptions MessagerOptions { get; private set; }

        /***************************************************************************************/

        public TargetedSenderRepository(string subsystem, Guid targetSession, string targetName, string cfgPath) : base(subsystem, cfgPath)
        {
            Init(targetSession, targetName);
        }

        public TargetedSenderRepository(string subsystem, Guid targetSession, string targetName, MessagerOptions opts) : base(subsystem, opts)
        {
            Init(targetSession, targetName);
        }

        /***************************************************************************************/

        private void Init(Guid targetSession, string targetName)
        {
            TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
            TargetSession = targetSession;
            MessagerOptions = Options; //guano
            if (Options.Sender == null)
                Options.Sender = new();
            if (Options.Sender.Topics == null)
                Options.Sender.Topics = new();
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
            Options.Sender.Topics.Add(topic);
        }
    }
}
