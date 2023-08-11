using HarmonyLib;
using BepInEx.Logging;

namespace MRASubnautica
{
    public static class Test
    {
        [HarmonyPatch(typeof(PlayerTool))]
        public static class PlayerTool_Patch
        {
            [HarmonyPatch(nameof(PlayerTool.Awake))]
            [HarmonyPostfix]
            public static void Awake_Postfix(PlayerTool __instance) {
                if (__instance.GetType() == typeof(Knife)) {
                    Knife knife = __instance as Knife;
                    float knifeDamage = knife.damage;
                    float newKnifeDamage = knifeDamage * 2.0f;
                    knife.damage = newKnifeDamage;

                    MRA_SN.logger.Log(LogLevel.Info, $"Knife damage was: {knifeDamage}," +
                        $" is now: {newKnifeDamage}");
                }
            }
        }
    }
}
