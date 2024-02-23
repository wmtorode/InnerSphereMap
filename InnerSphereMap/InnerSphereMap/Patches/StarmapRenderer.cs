using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using BattleTech;
using UnityEngine;

namespace InnerSphereMap.Patches;

[HarmonyPatch(typeof(StarmapRenderer), "RefreshSystems")]
public static class StarmapRenderer_RefreshSystems
{

    static void Prefix(ref bool __runOriginal, StarmapRenderer __instance)
    {

        if (!__runOriginal)
        {
            return;
        }

        __instance.starmapCamera.gameObject.SetActive(true);
        foreach (StarmapSystemRenderer starmapSystemRenderer in __instance.systemDictionary.Values)
        {
            __instance.InitializeSysRenderer(starmapSystemRenderer.system, starmapSystemRenderer);
            if (__instance.starmap.CurSelected != null && __instance.starmap.CurSelected.System.ID ==
                starmapSystemRenderer.system.System.ID)
            {
                starmapSystemRenderer.Selected();
            }
            else
            {
                starmapSystemRenderer.Deselected();
            }
        }

        __runOriginal = false;

    }

    static void Postfix(StarmapRenderer __instance)
    {
        try
        {
            var davionLogo = GameObject.Find("davionLogo");
            var marikLogo = GameObject.Find("marikLogo");
            var directorateLogo = GameObject.Find("directorateLogo");
            directorateLogo?.SetActive(false);
            davionLogo?.SetActive(false);
            marikLogo?.SetActive(false);
            var liaoLogo = GameObject.Find("liaoLogo");
            liaoLogo?.SetActive(false);
            var taurianLogo = GameObject.Find("taurianLogo");
            taurianLogo?.SetActive(false);
            var magistracyLogo = GameObject.Find("magistracyLogo");
            magistracyLogo?.SetActive(false);
            var restorationLogo = GameObject.Find("restorationLogo");
            restorationLogo?.SetActive(false);

            GameObject go;
            if (Fields.originalTransform == null)
            {
                Fields.originalTransform = UnityEngine.Object.Instantiate(__instance.restorationLogo).transform;
            }

            Texture2D texture2D2;
            byte[] data;

            foreach (LogoItem logoItem in InnerSphereMap.SETTINGS.logos)
            {
                FactionValue factionValue = FactionEnumeration.GetFactionByName(logoItem.factionName);
                if (factionValue.IsClan && InnerSphereMap.SETTINGS.reducedClanLogos)
                {
                    continue;
                }

                string mapGoObject = logoItem.factionName + "Map";
                if (GameObject.Find(mapGoObject) == null)
                {
                    texture2D2 = new Texture2D(2, 2);
                    data = File.ReadAllBytes($"{InnerSphereMap.ModDirectory}/Logos/{logoItem.logoImage}.png");
                    texture2D2.LoadImage(data);
                    go = UnityEngine.Object.Instantiate(__instance.restorationLogo);
                    go.GetComponent<Renderer>().material.mainTexture = texture2D2;
                    go.name = mapGoObject;
                }
                else
                {
                    go = GameObject.Find(mapGoObject);

                }

                __instance.PlaceLogo(FactionEnumeration.GetFactionByName(logoItem.factionName), go);
            }

            if (InnerSphereMap.SETTINGS.reducedClanLogos)
            {
                SimGameState sim =
                    (SimGameState)AccessTools.Field(typeof(Starmap), "sim").GetValue(__instance.starmap);
                List<FactionValue> contestingFactions = new List<FactionValue>();
                foreach (FactionValue faction in FactionEnumeration.FactionList)
                {
                    if (faction.IsClan && faction.CanAlly)
                    {
                        contestingFactions.Add(faction);
                    }
                }

                Dictionary<FactionValue, int> ranking = new Dictionary<FactionValue, int>();
                foreach (StarSystem system in sim.StarSystems)
                {
                    if (contestingFactions.Contains(system.OwnerValue))
                    {
                        if (!ranking.ContainsKey(system.OwnerValue))
                        {
                            ranking.Add(system.OwnerValue, 0);
                        }

                        ranking[system.OwnerValue]++;
                    }
                }

                FactionValue invaderclan = FactionEnumeration.GetInvalidUnsetFactionValue();
                if (ranking.Count > 0)
                {
                    invaderclan = ranking.OrderByDescending(x => x.Value).First().Key;
                }

                if (invaderclan != FactionEnumeration.GetInvalidUnsetFactionValue())
                {
                    if (GameObject.Find("ClansInvaderLogoMap") == null)
                    {
                        texture2D2 = new Texture2D(2, 2);
                        data = File.ReadAllBytes($"{InnerSphereMap.ModDirectory}/Logos/" + invaderclan.Name +
                                                 "Logo.png");
                        texture2D2.LoadImage(data);
                        go = UnityEngine.Object.Instantiate(__instance.restorationLogo);
                        go.GetComponent<Renderer>().material.mainTexture = texture2D2;
                        go.name = "ClansInvaderLogoMap";
                    }
                    else
                    {
                        go = GameObject.Find("ClansInvaderLogoMap");
                        data = File.ReadAllBytes($"{InnerSphereMap.ModDirectory}/Logos/" + invaderclan.Name +
                                                 "Logo.png");
                        texture2D2 = new Texture2D(2, 2);
                        texture2D2.LoadImage(data);
                        go.GetComponent<Renderer>().material.mainTexture = texture2D2;
                    }

                    __instance.PlaceLogo(invaderclan, go);
                }
            }

        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }
}

// The original method had a rectangular normalization here -- it did 56% of the y axis
[HarmonyPatch(typeof(StarmapRenderer), "NormalizeToMapSpace")]
public static class StarmapRenderer_NormalizeToMapSpace_Patch
{

    static void Prefix(ref bool __runOriginal, StarmapRenderer __instance, Vector2 normalizedPos,
        ref Vector3 __result)
    {

        if (!__runOriginal)
        {
            return;
        }

        // Reminder -- normalizedPos is normalized between [0,1]
        // This normalizes it between [-100,100]
        Vector3 newResult = normalizedPos;
        newResult.x = (newResult.x * 2f - 1f) * InnerSphereMap.SETTINGS.MapWidth;
        newResult.y = (newResult.y * 2f - 1f) * InnerSphereMap.SETTINGS.MapHeight;
        newResult.z = 0f;

        __result = newResult;
        __runOriginal = false;
    }
}


[HarmonyPatch(typeof(StarmapRenderer), "Update")]
public static class StarmapRenderer_Update_Patch
{

    // This transpiler aims to remove two method calls at the end of the Update loop
    // The this.needsPan = false; in the if statement
    // and the final this.starmapCamera.transform.position = position3;
    // These are converted to NOOPs and then properly handled with a PostFix
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {

        List<CodeInstruction> instructionList = instructions.ToList();
        try
        {
            // Targetting the last instance of: this.needsPan = false; 
            FieldInfo panInfo = AccessTools.Field(typeof(StarmapRenderer), "needsPan");
            int setPanIndex = instructionList.FindLastIndex(instruction =>
            {
                return instruction.opcode == OpCodes.Stfld && panInfo.Equals(instruction.operand);
            });
            instructionList[setPanIndex - 2].opcode = OpCodes.Nop; // remove loading "this"
            instructionList[setPanIndex - 1].opcode = OpCodes.Nop; // remove loading "false"
            instructionList[setPanIndex].opcode = OpCodes.Nop; // remove the set

            // Targetting the last instance of: this.starmapCamera.transform.position = position3
            // We don't want to simply remove this code, since there is some branching jumps that land on it earlier
            // So replace these with NOPs too
            MethodInfo setPosInfo =
                AccessTools.Property(typeof(Transform), nameof(Transform.position)).GetSetMethod();
            int setPositionIndex = instructionList.FindLastIndex(instruction =>
            {
                return instruction.opcode == OpCodes.Callvirt && setPosInfo.Equals(instruction.operand);
            });
            instructionList[setPositionIndex - 4].opcode = OpCodes.Nop; // remove loading "this"
            instructionList[setPositionIndex - 3].opcode = OpCodes.Nop; // remove load starmapCamera
            instructionList[setPositionIndex - 2].opcode = OpCodes.Nop; // remove get_transform
            instructionList[setPositionIndex - 1].opcode = OpCodes.Nop; // remove loading position3
            instructionList[setPositionIndex].opcode = OpCodes.Nop; // remove the set_position

            return instructionList;
        }
        catch (Exception e)
        {
            Logger.LogError(e);
            return instructions;
        }
    }

    static void Postfix(StarmapRenderer __instance)
    {
        try
        {

            // starMapCamera is public
            Camera starMapCamera = __instance.starmapCamera;

            // We want to readjust the field of view given our own min and max
            float newFov = Mathf.Lerp(InnerSphereMap.SETTINGS.MinFov, InnerSphereMap.SETTINGS.MaxFov,
                __instance.zoomLevel);
            starMapCamera.fieldOfView = newFov;
            __instance.fakeCamera.fieldOfView = newFov;

            // Now we need to clamp the bounadries
            float verticalViewSize =
                CameraHelper.GetViewSize(Mathf.Abs(starMapCamera.transform.position.z), newFov);
            float horizontalViewSize = CameraHelper.GetViewSize(Mathf.Abs(starMapCamera.transform.position.z),
                CameraHelper.GetHorizontalFov(newFov));

            Vector3 currentPosition = starMapCamera.transform.position;
            Vector3 clampedPosition = currentPosition;

            // The clamping boundaries are the map width / height + the buffers + the viewing distance created with the FOVs
            float leftBoundary = -InnerSphereMap.SETTINGS.MapWidth - InnerSphereMap.SETTINGS.MapLeftViewBuffer +
                                 horizontalViewSize;
            float rightBoundary = InnerSphereMap.SETTINGS.MapWidth + InnerSphereMap.SETTINGS.MapRightViewBuffer -
                                  horizontalViewSize;
            float bottomBoundary = -InnerSphereMap.SETTINGS.MapHeight -
                                   InnerSphereMap.SETTINGS.MapBottomViewBuffer +
                                   verticalViewSize;
            float topBoundary = InnerSphereMap.SETTINGS.MapHeight + InnerSphereMap.SETTINGS.MapTopViewBuffer -
                                verticalViewSize;

            float totalWidth = InnerSphereMap.SETTINGS.MapWidth * 2f + InnerSphereMap.SETTINGS.MapLeftViewBuffer +
                               InnerSphereMap.SETTINGS.MapRightViewBuffer;
            float totalHeight = InnerSphereMap.SETTINGS.MapHeight * 2f + InnerSphereMap.SETTINGS.MapTopViewBuffer +
                                InnerSphereMap.SETTINGS.MapBottomViewBuffer;

            // We have to check for the FOV being larger than the whole map -- or it'll bounce around
            if (horizontalViewSize * 2f >= totalWidth)
            {
                clampedPosition.x = 0f;
                __instance.needsPan = false;
            }
            else
            {
                clampedPosition.x = Mathf.Clamp(currentPosition.x, leftBoundary, rightBoundary);
            }

            if (verticalViewSize * 2f >= totalHeight)
            {
                clampedPosition.y = 0f;
                __instance.needsPan = false;
            }
            else
            {
                clampedPosition.y = Mathf.Clamp(currentPosition.y, bottomBoundary, topBoundary);
            }

            // Check for boundaries conditions -- continue the previous HBS behavior of not panning
            if (clampedPosition.x == leftBoundary || clampedPosition.x == rightBoundary ||
                clampedPosition.y == topBoundary || clampedPosition.y == bottomBoundary)
            {
                __instance.needsPan = false;
            }

            starMapCamera.transform.position = clampedPosition;
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }

    //rezising all the map logos
    [HarmonyPatch(typeof(StarmapRenderer), "PlaceLogo")]
    public static class StarmapRenderer_PlaceLogo_Patch
    {

        static void Postfix(StarmapRenderer __instance, FactionValue faction, GameObject logo)
        {
            try
            {
                if (logo.transform.localScale == Fields.originalTransform.localScale)
                {
                    logo.transform.localScale += new Vector3(4f, 4f, 4f);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }
    
    [HarmonyPatch(typeof(StarmapSystemRenderer), "SetBlackMarket")]
    public static class StarmapSystemRenderer_SetBlackMarket_Patch {
        static void Prefix(ref bool __runOriginal, StarmapSystemRenderer __instance, ref bool state) {
                
            if (!__runOriginal)
            {
                return;
            }
                
            try {
                state = false;
            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }
    }
}