using System;
using System.Collections.Generic;
using Drill4Net.Repository;

namespace Drill4Net.Agent.Messaging.Transport
{
    public class TargetedSenderRepository : ConfiguredRepository<MessagerOptions, BaseOptionsHelper<MessagerOptions>>, IMessagerRepository
    {
        public Guid TargetSession { get; private set; }

        public string TargetName { get; private set; }

        public string TargetVersion { get; private set; }

        public MessagerOptions MessagerOptions { get; private set; }

        /***************************************************************************************/

        //public TargetedSenderRepository(string subsystem, Guid targetSession, string targetName, string targetVersion, string cfgPath):
        //    base(subsystem, cfgPath)
        //{
        //    Init(targetSession, targetName, targetVersion);
        //}

        public TargetedSenderRepository(string subsystem, Guid targetSession, string targetName, string targetVersion, MessagerOptions opts):
            base(subsystem, opts)
        {
            Init(targetSession, targetName, targetVersion);
        }

        /***************************************************************************************/

        private void Init(Guid targetSession, string targetName, string targetVersion)
        {
            TargetSession = targetSession;
            TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
            TargetVersion = targetVersion ?? throw new ArgumentNullException(nameof(targetVersion));

            if (MessagingRepository<MessagerOptions>.GetServersFromEnv(out var servers))
                Options.Servers = servers;

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
