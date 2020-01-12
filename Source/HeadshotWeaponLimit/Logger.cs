using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace HeadshotWeaponLimit
{
    public class Logger
    {
        private static string _logPath;
        private static int _debugLevel;
        private static string _modDirectory;
        private static string _modName;

        private static string _globalConfigFile = ".madmods.config";
        private static string _globalLogFile = ".madmods.log";

        public static void Initialize(string logPath, int debugLevel, string modDirectory, string modName)
        {
            _logPath = logPath;
            _debugLevel = debugLevel;
            _modDirectory = modDirectory;
            _modName = modName;

            Cleanup();
            LogAlways($"Logger.Initialize({logPath}, {debugLevel}, {modDirectory}, {modName})");

            OverrideDebugLevel(modName);
        }

        public static void Cleanup()
        {
            using (StreamWriter writer = new StreamWriter(_logPath, false))
            {
                writer.WriteLine($"[{_modName} @ {DateTime.Now.ToString()}] CLEANED UP");
            }
        }

        public static void LogError(Exception ex)
        {
            if (_debugLevel >= 1)
            {
                using (StreamWriter writer = new StreamWriter(_logPath, true))
                {
                    writer.WriteLine("----------------------------------------------------------------------------------------------------");
                    writer.WriteLine($"[{_modName} @ {DateTime.Now.ToString()}] EXCEPTION:");
                    writer.WriteLine("Message: " + ex.Message + "<br/>" + Environment.NewLine + "StackTrace: " + ex.StackTrace);
                    writer.WriteLine("----------------------------------------------------------------------------------------------------");
                }
            }
        }

        public static void LogLine(String line, bool showPrefix = true)
        {
            if (_debugLevel >= 2)
            {
                using (StreamWriter writer = new StreamWriter(_logPath, true))
                {
                    string prefix = showPrefix ? $"[{_modName} @ {DateTime.Now.ToString()}] " : "";
                    writer.WriteLine(prefix + line);
                }
            }
        }

        public static void LogAlways(String line, bool showPrefix = true)
        {
            using (StreamWriter writer = new StreamWriter(_logPath, true))
            {
                string prefix = showPrefix ? $"[{_modName} @ {DateTime.Now.ToString()}] " : "";
                writer.WriteLine(prefix + line);
            }
        }

        public static void OverrideDebugLevel(string modName)
        {
            LogAlways($"Logger.OverrideDebugLevel({modName})");
            try
            {
                string filePath = $"{_modDirectory}";
                DirectoryInfo dir = new DirectoryInfo(filePath).Parent;
                FileInfo file = dir.GetFiles(_globalConfigFile).First();

                using (StreamReader r = new StreamReader(file.FullName))
                {
                    string json = r.ReadToEnd();
                    JObject globalConfig = JObject.Parse(json);

                    JToken ModConfigToken;
                    JToken ForceGlobalDebugLevelToken;

                    if (globalConfig.TryGetValue($"{modName}", out ModConfigToken))
                    {
                        JObject modConfig = (JObject)ModConfigToken;
                        JToken ForceModDebugLevelToken;

                        if (modConfig.TryGetValue("ForceModDebugLevel", out ForceModDebugLevelToken))
                        {
                            LogAlways($"{_globalConfigFile}.{modName}.ForceModDebugLevel: {(int)ForceModDebugLevelToken}");
                            _debugLevel = (int)ForceModDebugLevelToken;
                        }
                    }
                    if (globalConfig.TryGetValue("ForceGlobalDebugLevel", out ForceGlobalDebugLevelToken))
                    {
                        LogAlways($"{_globalConfigFile}.ForceGlobalDebugLevel: {(int)ForceGlobalDebugLevelToken}");
                        if ((int)ForceGlobalDebugLevelToken >= 0)
                        {
                            _debugLevel = (int)ForceGlobalDebugLevelToken;
                        }
                    }

                    JToken ForceGlobalLogToken;
                    if (globalConfig.TryGetValue("ForceGlobalLog", out ForceGlobalLogToken))
                    {
                        LogAlways($"{_globalConfigFile}.ForceGlobalLog: {(bool)ForceGlobalLogToken}");
                        if ((bool)ForceGlobalLogToken)
                        {
                            LogAlways($"NEW LOG LOCATION: {Path.Combine(_modDirectory, "..", _globalLogFile)}");
                            _logPath = Path.Combine(_modDirectory, "..", _globalLogFile);
                            LogAlways($"Logger.DebugLevel: {_debugLevel}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }
    }
}
