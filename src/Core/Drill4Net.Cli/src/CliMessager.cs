namespace Drill4Net.Cli
{
    public class CliMessager
    {
        public event MessageDeliveredDelegate? MessageDelivered;

        public string Id { get; protected set; }

        /************************************************************************/

        public CliMessager(string id = "")
        {
            Id = id;
        }

        /************************************************************************/

        #region RaiseMessage
        protected void RaiseDelivered(string message, CliMessageType messType = CliMessageType.Annotation,
            MessageState state = MessageState.NewLine)
        {
            MessageDelivered?.Invoke(Id, message, messType, state);
        }

        protected void RaiseMessage(string message, CliMessageType messType = CliMessageType.Annotation)
        {
            MessageDelivered?.Invoke(Id, message, messType);
        }

        protected void RaiseQuestion(string message)
        {
            MessageDelivered?.Invoke(Id, message, CliMessageType.Question);
        }

        protected void RaiseWarning(string message, MessageState state = MessageState.NewLine)
        {
            MessageDelivered?.Invoke(Id, message, CliMessageType.Warning, state);
        }

        protected void RaiseError(string message)
        {
            MessageDelivered?.Invoke(Id, message, CliMessageType.Error);
        }
        #endregion
    }
}
