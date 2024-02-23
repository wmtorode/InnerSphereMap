using System;
using System.Collections.Generic;
using BattleTech;


namespace InnerSphereMap.Patches;

[HarmonyPatch(typeof(SimGameState), "InitializeDataFromDefs")]
public static class SimGameState_InitializeDataFromDefs_Patch
{

    static void Prefix(ref bool __runOriginal, SimGameState __instance)
    {

        if (!__runOriginal)
        {
            return;
        }

        StarSystemDef starSystemDef = null;
        try
        {
            Dictionary<string, StarSystem> test = new Dictionary<string, StarSystem>();
            foreach (string id in __instance.DataManager.SystemDefs.Keys)
            {
                starSystemDef = __instance.DataManager.SystemDefs.Get(id);
                if (starSystemDef.StartingSystemModes.Contains(__instance.SimGameMode))
                {
                    StarSystem starSystem = new StarSystem(starSystemDef, __instance);
                    test.Add(starSystemDef.CoreSystemID, starSystem);
                }
            }
            //TODO: SOMESOMETHINGSOMETHING FACTION STORE
        }
        catch (Exception e)
        {
            Logger.LogLine("STARSYSTEM BROKEN: " + starSystemDef.CoreSystemID);
            Logger.LogError(e);
        }
    }
}