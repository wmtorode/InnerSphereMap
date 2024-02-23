using System.Reflection;

namespace InnerSphereMap
{
    public class InnerSphereMap
    {
        internal static string ModDirectory;

        public static Settings SETTINGS;

        public static void Init(string directory, string settingsJSON) {
            ModDirectory = directory;
            SETTINGS = Helper.LoadSettings();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "de.morphyum.InnerSphereMap");
        }
    }
}
