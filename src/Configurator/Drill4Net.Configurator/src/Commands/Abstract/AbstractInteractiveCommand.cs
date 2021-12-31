using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Drill4Net.Common;
using Drill4Net.Configuration;
using Drill4Net.Repository;

namespace Drill4Net.Configurator
{
    /// <summary>
    /// Abstract command to configure some entity in interactive mode
    /// </summary>
    public abstract class AbstractInteractiveCommand : AbstractConfiguratorCommand
    {
        private readonly BaseOptionsHelper _optHelper;

        /**************************************************************************************/

        protected AbstractInteractiveCommand(ConfiguratorRepository rep) : base(rep)
        {
            _optHelper = new(_rep.Subsystem);
        }

        /**************************************************************************************/

        #region Saving config
        internal bool SaveConfig<T>(string appName, T cfg, string dir) where T : AbstractOptions, new()
        {
            string cfgPath;
            var needSave = true;
            while (true)
            {
                if (!AskQuestion($"Name of the {appName}'s config", out var name, CoreConstants.CONFIG_NAME_DEFAULT))
                    return false;
                if (!CheckFileNameAnswer(ref name, "Wrong file name", false))
                    continue;
                if (!Path.HasExtension(name))
                    name += ".yml";
                cfgPath = Path.Combine(dir, name);

                if (File.Exists(cfgPath))
                {
                    if (!AskQuestion("Such name already exists. Replace?", out var answer, "n"))
                        return false;
                    needSave = IsYes(answer);
                }
                break;
            }
            //
            if (needSave)
            {
                try
                {
                    _rep.WriteOptions<T>(cfg, cfgPath);
                }
                catch (Exception ex)
                {
                    var er = $"Config for {appName} is not saved:\n{ex}";
                    _logger.Error(er);
                    RaiseError(er);
                    return false;
                }
                _logger.Info($"Config for {appName} saved to [{cfgPath}]");
                RaiseMessage($"You can check the {appName}'s settings: {cfgPath}", Cli.CliMessageType.Message);

                // activating the config
                (var needActivate, var redirectCfgPath) = IsNeedAcivateConfigFor(dir, cfgPath);
                if (needActivate)
                    return SaveRedirectFile(appName, cfgPath, redirectCfgPath);
            }
            return true;
        }

        internal (bool, string) IsNeedAcivateConfigFor(string appDir, string curCfgPath)
        {
            var redirectCfgPath = _optHelper.CalcRedirectConfigPath(appDir);
            var name = Path.GetFileName(curCfgPath);
            var isDefName = name.Equals(CoreConstants.CONFIG_NAME_DEFAULT, StringComparison.InvariantCultureIgnoreCase);
            bool needActivate;
            if (File.Exists(redirectCfgPath))
            {
                var redirData = _optHelper.ReadRedirectData(redirectCfgPath);
                if (redirData == null)
                {
                    needActivate = true;
                }
                else
                {
                    var actualPath = redirData.Path;
                    needActivate = string.IsNullOrWhiteSpace(actualPath) ||
                        !actualPath.Equals(curCfgPath, StringComparison.InvariantCultureIgnoreCase);
                }
            }
            else //no redirect-file
            {
                needActivate = !isDefName;
            }
            return (needActivate, redirectCfgPath);
        }

        internal bool SaveRedirectFile(string appName, string actualPath, string redirectCfgPath)
        {
            try
            {
                _optHelper.WriteRedirectData(new RedirectData { Path = actualPath }, redirectCfgPath);
                _logger.Info($"Redirect config for {appName} saved to [{redirectCfgPath}]");
                return true;
            }
            catch (Exception ex)
            {
                var er = $"Redirect config for {appName} is not saved:\n{ex}";
                _logger.Error(er);
                RaiseError(er);
                return false;
            }
        }
        #endregion

        /// <summary>
        /// Ask the question and get the value.
        /// </summary>
        /// <param name="question">The question for user</param>
        /// <param name="answer">The answer with default, if empty input is getted</param>
        /// <param name="defValue">The default value</param>
        /// <param name="showDefVal">Do it need to output the default value</param>
        /// <returns>False, if user want to quit from the current setup</returns>
        protected bool AskQuestion(string question, out string answer, string? defValue, bool showDefVal = true)
        {
            if (string.IsNullOrWhiteSpace(question))
                question = "?";
            if (question.EndsWith(":"))
                question = question[1..];
            question = $"\n{question.Trim()}";
            if (showDefVal)
                question = $"{question}: [{defValue}]";
            else
                question += ":";
            //
            RaiseQuestion(question);
            answer = Console.ReadLine().Trim();
            if (IsQuit(answer))
                return false;
            //
            var empty = answer?.Length == 0;
            if (empty)
            {
                answer = defValue;
                RaiseMessage(answer, Cli.CliMessageType.Input_Default);
            }
            _logger.Info($"Question: [{question}]; Default: [{defValue}]; Answer: [{answer}]");
            return true;
        }

        protected bool AskDirectory(string question, out string destDir, string? defValue, bool mustExists, bool showDefVal = true)
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

        protected bool AskFilePath(string question, out string filePath, string? defValue, bool mustExists, bool showDefVal = true)
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

        protected bool AskFileName(string question, out string fileName, string? defValue, bool showDefVal = true)
        {
            fileName = "";
            while (true)
            {
                if (!AskQuestion(question, out fileName, defValue, showDefVal))
                    return false;
                if (!CheckFileNameAnswer(ref fileName, "File name cannot be empty", false))
                    continue;
                break;
            }
            return true;
        }

        protected bool AskDegreeOfParallelism(string mess, out int degree)
        {
            degree = 1;
            var defDegree = Environment.ProcessorCount;
            string degreeS;
            do
            {
                if (!AskQuestion(mess, out degreeS, defDegree.ToString()))
                    return false;
            }
            while (!CheckIntegerAnswer(degreeS, $"The degree of parallelism must be from 2 to {defDegree}", 2, defDegree, out degree));
            return true;
        }

        /// <summary>
        /// Add file log options to the config.
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="programName">Name of program</param>
        /// <returns>If false, it is the need to exit from this setup.</returns>
        protected bool AddLogFile(List<LogData> logs, string programName = "program")
        {
            if (!AskQuestion($"The {programName} logs will be output to the its console and to a file in the its {LoggerHelper.LOG_FOLDER} folder. Add an additional parallel log file?", out var answer, "n"))
                return false;
            if (IsYes(answer))
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

        protected bool CheckStringAnswer(ref string answer, string mess, bool canBeEmpty = false)
        {
            if (canBeEmpty || !string.IsNullOrWhiteSpace(answer))
                return true;
            RaiseWarning(mess);
            return false;
        }

        protected bool CheckIntegerAnswer(string answer, string mess, int min, int max, out int val)
        {
            if (int.TryParse(answer, out val) && val >= min && val <= max)
                return true;
            RaiseWarning(mess);
            return false;
        }

        protected bool CheckDirectoryAnswer(ref string directory, bool mustExist = true)
        {
            if (mustExist && string.IsNullOrWhiteSpace(directory))
            {
                RaiseWarning("Directory cannot be empty and must exist\n", false);
                return false;
            }
            if (!mustExist || (mustExist && Directory.Exists(directory)))
            {
                if (directory?.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    RaiseWarning("The directory is invalid.");
                    return false;
                }

                //TODO: check for proper dir path itself (cross-platform!)
                return true;
            }
            RaiseWarning("Directory does not exists.");
            return false;
        }

        protected bool CheckFileNameAnswer(ref string filename, string mess, bool canBeEmpty)
        {
            if (!canBeEmpty && string.IsNullOrWhiteSpace(filename))
            {
                RaiseWarning("Filename cannot be empty", false);
                return false;
            }
            if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                if (string.IsNullOrWhiteSpace(mess))
                    mess = "File name contains invalid symbols";
                RaiseWarning(mess);
                return false;
            }
            return true;
        }

        protected bool IsQuit(string? s)
        {
            return string.Equals(s, ConfiguratorConstants.COMMAND_QUIT, StringComparison.OrdinalIgnoreCase);
        }

        protected bool IsOk(string? s)
        {
            return s.Replace("\"", null).Equals(ConfiguratorConstants.COMMAND_OK, StringComparison.InvariantCultureIgnoreCase);
        }

        protected bool IsYes(string? s, bool noInputIsYes = true)
        {
            if (s?.Length == 0 && noInputIsYes)
                return true;
            return string.Equals(s, ConfiguratorConstants.COMMAND_YES, StringComparison.OrdinalIgnoreCase);
        }
    }
}
