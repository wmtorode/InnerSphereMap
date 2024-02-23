using System;
using System.Collections.Generic;
using BattleTech;
using Localize;

namespace InnerSphereMap.Patches;

[HarmonyPatch(typeof(Starmap), "CancelTravelAndMoveToCurrentSystem")]
public static class Starmap_CancelTravelAndMoveToCurrentSystem
{
    static void Prefix(ref bool __runOriginal, Starmap __instance) {
                
        if (!__runOriginal)
        {
            return;
        }
                
        try {
            __instance.PotentialPath = new List<StarSystemNode>();
            __instance.PotentialPath.Add(__instance.CurPlanet);
            __instance.PendingTravelOrder = new WorkOrderEntry_TravelGeneric("Travel", Strings.T("Travel to {0}", __instance.CurPlanet.System.Def.Description.Name), 0);
            int jumpDistance = __instance.sim.CurSystem.JumpDistance;
            __instance.ProjectedTravelTime += jumpDistance;
            WorkOrderEntry_TravelToSystem entryTravelToSystem = new WorkOrderEntry_TravelToSystem("Travel", Strings.T("Traveling to {0} System", __instance.CurPlanet.System.Def.Description.Name), jumpDistance, __instance.PendingTravelOrder);
            __instance.CreateActivePath(false);
            // This fixes a bug where travel time becomes the jumpship recharge time instead of the actual time to planet
            __instance.sim.SetTravelTime(jumpDistance);
            __instance.sim.TravelManager.SetTravelStateFromInterrupt(SimGameTravelStatus.TRANSIT_FROM_JUMP);
            __runOriginal = false;
        }
        catch (Exception e) {
            Logger.LogError(e);
        }
        
    }
}