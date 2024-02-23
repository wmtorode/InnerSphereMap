using BattleTech.UI;
using System;

namespace InnerSphereMap.Patches;

[HarmonyPatch(typeof(MainMenu), "Init")]
public static class MainMenu_Init_Patch
{

    static void Prefix(ref bool __runOriginal, MainMenu __instance)
    {

        if (!__runOriginal)
        {
            return;
        }

        try
        {
            __instance.topLevelMenu.RadioButtons.Find((HBSButton x) => x.GetText() == "Campaign").gameObject
                .SetActive(false);
        }
        catch (Exception e)
        {
            Logger.LogError(e);

        }
    }
}