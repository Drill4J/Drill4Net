using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.BanderLog;
using Drill4Net.Repository;
using Drill4Net.Configuration;
using Microsoft.Extensions.Logging;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    public class CliInteractor : CliMessager
    {
        private readonly Logger _logger;

        /*************************************************************************/

        public CliInteractor(string subsystem, string id = "") : base(id)
        {
            _logger = new TypedLogger<CliInteractor>(subsystem);
        }

        /*************************************************************************/

        /// <summary>
        /// Ask the question and get the value.
        /// </summary>
        /// <param name="question">The question for user</param>
        /// <param name="answer">The answer with default, if empty input is getted</param>
        /// <param name="defValue">The default value</param>
        /// <param name="showDefVal">Do it need to output the default value</param>
        /// <returns>False, if user want to quit from the current setup</returns>
        public bool AskQuestion(string question, out string answer, string? defValue, bool showDefVal = true)
        {
            if (string.IsNullOrWhiteSpace(question))
                question = "???";
            if (question.EndsWith(":"))
                question = question[1..];
            question = $"\n{question.Trim()}";

            var colon = question.EndsWith(".") || question.EndsWith("!") || question.EndsWith("?") ? "" : ":";
            if (showDefVal)
                question = $"{question}{colon} [{defValue}]";
            else
                question += colon;
            //
            RaiseQuestion(question);
            answer = Console.ReadLine().Trim();
            if (IsQuit(answer))
                return false;
            //
            var empty = answer?.Length == 0;
            if (empty)
            {
                answer = defValue ?? "";
                RaiseMessage(answer, CliMessageType.EmptyInput);
            }
            _logger.Info($"Question: [{question}]; Default: [{defValue}]; Answer: [{answer}]");
            return true;
        }

        public bool AskQuestionBoolean(string question, out bool answer, bool? defValue)
        {
            answer = false;
            var def = defValue == null ? null : (defValue == true ? "y" : "n");
            while (true)
            {
                if (!AskQuestion(question, out var answerS, def, defValue != null))
                    return false;
                if (IsYes(answerS))
                {
                    answer = true;
                    break;
                }
                if (IsNo(answerS))
                    break;
            }
            return true;
        }

        public bool AskDirectory(string question, out string destDir, string? defValue, bool mustExists, bool showDefVal = true)
        {
            destDir = "";
            do
            {
                if (!AskQuestion(question, out destDir, defValue, showDefVal))
                    return false;
            }
            while (!CheckDirectoryAnswer(ref destDir, mustExists));
            return true;
        }

        public bool AskFilePath(string question, out string filePath, string? defValue, bool mustExists, bool showDefVal = true)
        {
            while (true)
            {
                if (!AskQuestion(question, out filePath, defValue, showDefVal))
                    return false;
                string answer = filePath;
                if (!CheckStringAnswer(ref answer, "File path cannot be empty", true))
                    continue;
                var dir = Path.GetDirectoryName(filePath);
                if (!CheckDirectoryAnswer(ref dir, mustExists))
                    continue;
                var fileName = Path.GetFileName(filePath);
                if (!CheckFileNameAnswer(ref fileName, "", !mustExists))
                    continue;
                break;
            }
            return true;
        }

        public bool AskFileNameOrPath(string question, out string fileName, string? defValue, bool showDefVal = true)
        {
            fileName = "";
            while (true)
            {
                if (!AskQuestion(question, out fileName, defValue, showDefVal))
                    return false;
                if (!CheckFileNameAnswer(ref fileName, "", false))
                    continue;
                break;
            }
            return true;
        }

        public bool AskDegreeOfParallelism(string mess, ref int degree)
        {
            degree = 1;
            var defDegree = degree == 0 ? 0 : Environment.ProcessorCount;
            string degreeS;
            do
            {
                if (!AskQuestion(mess, out degreeS, defDegree.ToString()))
                    return false;
            }
            while (!CheckIntegerAnswer(degreeS, $"The max degree of parallelism must be from 2 to {defDegree}", 2, defDegree, out degree));
            return true;
        }

        /// <summary>
        /// Add file log options to the config.
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="programName">Name of program</param>
        /// <returns>If false, it is the need to exit from this setup.</returns>
        public bool AddLogFile(List<LogData> logs, string programName = "program")
        {
            if (!AskQuestionBoolean($"The {programName} logs will be output to the its console and to a file in the its {LoggerHelper.LOG_FOLDER} folder. Add an additional parallel log file?",
                out var answerBool, false))
                return false;
            if (answerBool)
            {
                if (!AskFilePath("File path", out var logPath, "", false, false))
                    return false;
                //
                var logLevel = LogLevel.Debug;
                while (true)
                {
                    //log level
                    if (!AskQuestion("Set the log level", out var logTypeS, logLevel.ToString()))
                        return false;
                    logTypeS = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(logTypeS);
                    if (Enum.TryParse(typeof(LogLevel), logTypeS, out object logType))
                    {
                        logLevel = (LogLevel)logType;
                        break;
                    }
                    RaiseWarning($"Unknown type of log level: {logTypeS}");
                }
                //
                var logData = new LogData()
                {
                    Disabled = false,
                    Type = LogSinkType.File,
                    Path = logPath,
                    Level = logLevel,
                };
                logs.Add(logData);
            }
            return true;
        }

        public bool CheckStringAnswer(ref string answer, string mess, bool canBeEmpty = false)
        {
            if (canBeEmpty || !string.IsNullOrWhiteSpace(answer))
                return true;
            RaiseWarning(mess);
            return false;
        }

        public bool CheckIntegerAnswer(string answer, string mess, int min, int max, out int val)
        {
            if (int.TryParse(answer, out val) && val >= min && val <= max)
                return true;
            RaiseWarning(mess);
            return false;
        }

        public bool CheckDirectoryAnswer(ref string directory, bool mustExist = true)
        {
            if (mustExist && string.IsNullOrWhiteSpace(directory))
            {
                RaiseWarning("Directory cannot be empty and must exist\n", MessageState.PrevLine);
                return false;
            }
            if (directory?.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                RaiseWarning("The directory has invalid characters.");
                return false;
            }
            //
            if (FileUtils.IsSystemDirectory(directory, true, out var reason))
            {
                RaiseWarning($"Please, specify another directory. {reason}");
                return false;
            }
            //
            if (!mustExist || (mustExist && Directory.Exists(directory)))
            {
                //I don't see a way to unambiguously check the correctness of the path that
                //has not yet been created (the check won't work through DirectoryInfo -
                //it will use the current path for some abracadabra as inner folders)
                return true;
            }
            RaiseWarning("Directory does not exists.");
            return false;
        }

        public bool CheckFileNameAnswer(ref string filename, string mess, bool canBeEmpty)
        {
            if (!canBeEmpty && string.IsNullOrWhiteSpace(filename))
            {
                RaiseWarning("Filename cannot be empty\n", MessageState.PrevLine);
                return false;
            }
            //
            if (filename.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                if (string.IsNullOrWhiteSpace(mess))
                    mess = "File path contains invalid symbols";
                RaiseWarning(mess);
                return false;
            }
            if (Path.GetPathRoot(filename) == null) //this is just name
            {
                if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                {
                    if (string.IsNullOrWhiteSpace(mess))
                        mess = "File name contains invalid symbols";
                    RaiseWarning(mess);
                    return false;
                }
            }

            //I don't see a way to unambiguously check the correctness of the path that
            //has not yet been created (the check won't work through FileInfo -
            //it will use the current path for some abracadabra as inner folders)
            return true;
        }

        public bool IsQuit(string? s)
        {
            return string.Equals(s, ConfiguratorConstants.ANSWER_QUIT, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsOk(string? s)
        {
            return s?.Replace("\"", null).Equals(ConfiguratorConstants.ANSWER_OK, StringComparison.InvariantCultureIgnoreCase) == true;
        }

        public bool IsYes(string? s, bool noInputIsYes = true)
        {
            if (s?.Length == 0 && noInputIsYes)
                return true;
            return string.Equals(s, ConfiguratorConstants.ANSWER_YES, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsNo(string? s, bool noInputIsYes = true)
        {
            if (s?.Length == 0 && noInputIsYes)
                return true;
            return string.Equals(s, ConfiguratorConstants.ANSWER_NO, StringComparison.OrdinalIgnoreCase);
        }

        public void DrawShortSeparator()
        {
            RaiseMessage(new string('.', 10));
        }

        public void DrawLine()
        {
            RaiseMessage(new string('-', 70));
        }

        public void DrawDoubleLine()
        {
            RaiseMessage(new string('=', 70));
        }
    }
}
