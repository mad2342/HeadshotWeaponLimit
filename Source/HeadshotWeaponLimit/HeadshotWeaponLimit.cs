using Harmony;
using System.Reflection;
using System;
using Newtonsoft.Json;
using System.IO;

namespace HeadshotWeaponLimit
{
    public static class HeadshotWeaponLimit
    {
        internal static string LogPath;
        internal static string ModDirectory;
        internal static Settings Settings;
        // BEN: Debug (0: nothing, 1: errors, 2:all)
        internal static int DebugLevel = 1;

        public static void Init(string directory, string settings)
        {
            ModDirectory = directory;
            LogPath = Path.Combine(ModDirectory, "HeadshotWeaponLimit.log");

            Logger.Initialize(LogPath, DebugLevel, ModDirectory, nameof(HeadshotWeaponLimit));

            try
            {
                Settings = JsonConvert.DeserializeObject<Settings>(settings);
            }
            catch (Exception e)
            {
                Settings = new Settings();
                Logger.LogError(e);
            }

            // Harmony calls need to go last here because their Prepare() methods directly check Settings...
            HarmonyInstance harmony = HarmonyInstance.Create("de.mad.HeadshotWeaponLimit");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
