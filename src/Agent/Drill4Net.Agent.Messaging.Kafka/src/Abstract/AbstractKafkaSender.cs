using System;
using Confluent.Kafka;
using Drill4Net.Common;

namespace Drill4Net.Agent.Messaging.Kafka
{
    public abstract class AbstractKafkaSender : IDataSender
    {
        //TODO: incapsulate Error props
        public bool IsError { get; private set; }

        public string LastError { get; private set; }

        public bool IsFatalError { get; private set; }

        protected readonly int _packetMaxSize;

        protected readonly ProducerConfig _cfg;
        protected readonly IMessageSenderRepository _rep;
        protected Headers _headers;

        private bool _disposed;

        /***************************************************************************************/

        protected AbstractKafkaSender(IMessageSenderRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _cfg = CreateBaseProducerConfig(rep.SenderOptions);

            //https://stackoverflow.com/questions/21020347/how-can-i-send-large-messages-with-kafka-over-15mb
            _packetMaxSize = (_cfg.MessageMaxBytes ?? MessagingConstants.MaxMessageSize) - 512; //less because also service info included!

            CreateHeaders();
            CreateProducers();
        }

        ~AbstractKafkaSender()
        {
            Dispose(false);
        }

        /***************************************************************************************/

        #region Headers
        private void CreateHeaders()
        {
            _headers = GetCommonHeaders();
            AddSpecificHeaders();
            SetMessageType();
        }

        private void SetMessageType()
        {
            _headers.Add(new Header(MessagingConstants.HEADER_MESSAGE_TYPE, Serializer.StringToArray(GetMessageType())));
        }

        protected abstract string GetMessageType();
        protected virtual void AddSpecificHeaders() { }

        internal virtual Headers GetCommonHeaders()
        {
            return new Headers
            {
                new Header(MessagingConstants.HEADER_SUBSYSTEM, Serializer.StringToArray(_rep.Subsystem)),
                new Header(MessagingConstants.HEADER_TARGET, Serializer.StringToArray(_rep.TargetName)),
            };
        }

        protected void SetHeaderValue<T>(string key, T val)
        {
            _headers.Remove(key);
            var header = new Header(key, Serializer.ToArray<T>(val));
            _headers.Add(header);
        }
        #endregion
        #region Producer
        protected abstract void CreateProducers();
        private ProducerConfig CreateBaseProducerConfig(MessageSenderOptions opts)
        {
            return new ProducerConfig
            {
                BootstrapServers = string.Join(",", opts.Servers),
                MessageMaxBytes = MessagingConstants.MaxMessageSize,
            };
        }

        protected void Handle(Error err)
        {
            IsError = err.IsError;
            IsFatalError = err.IsFatal;
            LastError = err.Reason;
        }
        #endregion
        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    ConcreteDisposing();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //.....

                // Note disposing has been done.
                _disposed = true;
            }
        }

        protected abstract void ConcreteDisposing();
        #endregion
    }
}
