//using System;

//namespace Drill4Net.Agent.Kafka.Transmitter
//{
//    public class DataTransmitter
//    {
//        private readonly IProbeSender _sender;

//        /*************************************************************************/

//        public DataTransmitter(IProbeSender sender)
//        {
//            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
//        }

//        /*************************************************************************/

//        public int Send(string str)
//        {
//            return _sender.Send(str);
//        }
//    }
//}
