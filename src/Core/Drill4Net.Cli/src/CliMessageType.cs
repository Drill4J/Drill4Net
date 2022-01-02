namespace Drill4Net.Cli
{
    public enum CliMessageType
    {
        Default,

        /// <summary>
        /// Question from program to user
        /// </summary>
        Question,

        /// <summary>
        /// Additional text, annotation to the question
        /// </summary>
        Annotation,

        /// <summary>
        /// The user's input is empty (it can be default)
        /// </summary>
        Input_Default,

        /// <summary>
        /// Info from programm to user after action
        /// </summary>
        Info,

        Help,
        Warning,
        Error,
    }
}
