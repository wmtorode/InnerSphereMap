using BattleTech;
using UnityEngine;

namespace InnerSphereMap.Patches;

[HarmonyPatch(typeof(StarmapBorders), "OnWillRenderObject")]
public static class StarmapBorders_OnWillRenderObject
{

    static void Prefix(ref bool __runOriginal, StarmapBorders __instance)
    {

        if (!__runOriginal)
        {
            return;
        }

        if (InnerSphereMap.SETTINGS.drawBorders && !InnerSphereMap.SETTINGS.rescaleBorders)
        {
            return;
        }

        // Disables the dashed-line border
        GameObject.Find("Edges").SetActive(false);

        if (!InnerSphereMap.SETTINGS.drawBorders)
        {
            // Disables the gray box, and other borders -- also fully disables StarmapBorders itself (since its a MonoBehavior attached to RegionBorders)
            GameObject.Find("RegionBorders").SetActive(false);
            __runOriginal = false;
        }
        else
        {
            var borderTransform = __instance.gameObject.transform;
            borderTransform.localScale = new Vector3(4f * InnerSphereMap.SETTINGS.MapHeight,
                4f * InnerSphereMap.SETTINGS.MapWidth);

            const int textureSize = 64;
            var blackTexture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
            for (var x = 0; x < textureSize; x++)
            {
                for (var y = 0; y < textureSize; y++)
                {
                    blackTexture.SetPixel(x, y, Color.black);
                }
            }

            blackTexture.Apply();
            __instance.plusTex = blackTexture;
        }
    }
}