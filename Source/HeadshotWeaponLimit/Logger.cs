using System;
using System.IO;

namespace HeadshotWeaponLimit
{
    public class Logger
    {
        static string filePath = $"{HeadshotWeaponLimit.ModDirectory}/HeadshotWeaponLimit.log";
        public static void LogError(Exception ex)
        {
            if (HeadshotWeaponLimit.DebugLevel >= 1)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    var prefix = "[HeadshotWeaponLimit @ " + DateTime.Now.ToString() + "]";
                    writer.WriteLine("Message: " + ex.Message + "<br/>" + Environment.NewLine + "StackTrace: " + ex.StackTrace + "" + Environment.NewLine);
                    writer.WriteLine("----------------------------------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }

        public static void LogLine(String line, bool showPrefix = true)
        {
            if (HeadshotWeaponLimit.DebugLevel >= 2)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    string prefix = "";
                    if (showPrefix)
                    {
                        prefix = "[HeadshotWeaponLimit @ " + DateTime.Now.ToString() + "]";
                    }
                    writer.WriteLine(prefix + line);
                }
            }
        }
    }
}
