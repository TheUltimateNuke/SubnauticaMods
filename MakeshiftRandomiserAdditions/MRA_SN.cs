using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace MRASubnautica
{
    [BepInPlugin(myGUID, pluginName, versionString)]
    public class MRA_SN : BaseUnityPlugin
    {
        private const string myGUID = "com.theultimatenuke.makeshiftrandomiseradditions";
        private const string pluginName = "Makeshift Randomiser Additions";
        private const string versionString = "1.0.0";

        private static readonly Harmony harmony = new Harmony(myGUID);

        public static ManualLogSource logger;

        private void Awake()
        {
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo(pluginName + " v" + versionString + " loaded.");
            logger = Logger;
        }
    }
}
