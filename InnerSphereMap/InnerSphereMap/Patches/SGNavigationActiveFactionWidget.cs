using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using BattleTech.UI.Tooltips;

namespace InnerSphereMap.Patches;

[HarmonyPatch(typeof(SGNavigationActiveFactionWidget), "ActivateFactions")]
public static class SGSystemViewPopulator_UpdateRoutedSystem_Patch {
    static void  Prefix(ref bool __runOriginal, SGNavigationActiveFactionWidget __instance, List<string> activeFactions, string OwnerFaction) {
                
        if (!__runOriginal)
        {
            return;
        }
                
        try {
            __instance.FactionButtons.ForEach(delegate (HBSButton btn) {
                btn.gameObject.SetActive(false);
            });
            int index = 0;
            foreach (string faction in activeFactions) {
                FactionDef factionDef = FactionEnumeration.GetFactionByName(faction).FactionDef;
                __instance.FactionIcons[index].sprite = factionDef.GetSprite();
                HBSTooltip component = __instance.FactionIcons[index].GetComponent<HBSTooltip>();
                if (component != null) {
                    component.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(factionDef));
                }
                __instance.FactionButtons[index].SetState(ButtonState.Enabled, false);
                __instance.FactionButtons[index].gameObject.SetActive(true);
                index++;
            }
            __runOriginal = false;
        }
        catch (Exception e) {
            Logger.LogError(e);
            __runOriginal = true;
        }
    }
}