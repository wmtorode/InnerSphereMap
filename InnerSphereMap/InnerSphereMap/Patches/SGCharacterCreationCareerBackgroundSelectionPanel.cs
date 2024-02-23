using BattleTech;
using BattleTech.UI;

namespace InnerSphereMap.Patches;

[HarmonyPatch(typeof(SGCharacterCreationCareerBackgroundSelectionPanel), "Done")]
public class SGCharacterCreationCareerBackgroundSelectionPanel_Done_Patch {
    // Token: 0x06000004 RID: 4 RVA: 0x000020BC File Offset: 0x000002BC
    public static void Prefix(ref bool __runOriginal, SGCharacterCreationCareerBackgroundSelectionPanel __instance) {
                
        if (!__runOriginal)
        {
            return;
        }
                
        Settings settings = InnerSphereMap.SETTINGS;
        SimGameResultAction simGameResultAction = new SimGameResultAction();
        simGameResultAction.Type = SimGameResultAction.ActionType.System_ShowSummaryOverlay;
        simGameResultAction.value = settings.splashTitle;
        simGameResultAction.additionalValues = new string[1];
        simGameResultAction.additionalValues[0] = settings.splashText;
        SimGameState.ApplyEventAction(simGameResultAction, null);
        __instance.onComplete.Invoke();
        __runOriginal = false;
    }
}