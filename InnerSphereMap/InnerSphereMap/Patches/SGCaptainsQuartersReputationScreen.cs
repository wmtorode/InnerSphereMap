using System;
using BattleTech;
using BattleTech.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace InnerSphereMap.Patches;

[HarmonyPatch(typeof(SGCaptainsQuartersReputationScreen), "RefreshWidgets")]
public static class SGCaptainsQuartersReputationScreen_RefreshWidgets
{

    static void Prefix(ref bool __runOriginal, ref SGCaptainsQuartersReputationScreen __instance)
    {

        if (!__runOriginal)
        {
            return;
        }

        try
        {
            Settings settings = InnerSphereMap.SETTINGS;
            if (__instance.simState.displayedFactions.Contains(FactionEnumeration.GetFactionByName("Locals").Name))
            {
                __instance.simState.displayedFactions.Remove(FactionEnumeration.GetFactionByName("Locals").Name);
            }

            GameObject parent = GameObject.Find("factionsPanel_V2");
            if (parent != null)
            {
                parent.transform.position = new Vector3(830, 670, parent.transform.position.z);
                Transform factionHeader = parent.transform.FindRecursive("factionHeader");
                factionHeader.localPosition =
                    new Vector3(factionHeader.localPosition.x, 250, factionHeader.localPosition.z);
                GameObject restPanel = GameObject.Find("RestorationRepPanel");
                if (restPanel != null)
                {
                    restPanel.SetActive(false);
                }

                GameObject superParent = GameObject.Find("uixPrfPanl_captainsQuarters_Reputation-Panel_V2(Clone)");
                ScrollRect scroller;
                Scrollbar scrollbar;
                if (superParent != null)
                {
                    GameObject bgfill = superParent.transform.FindRecursive("bgFill").gameObject;
                    if (bgfill != null)
                    {
                        bgfill.SetActive(false);
                    }

                    scroller = superParent.AddComponent<ScrollRect>();
                    scrollbar = scroller.transform.gameObject.AddComponent<Scrollbar>();
                    scroller.verticalScrollbar = scrollbar;
                    scrollbar.size = 1;
                    scrollbar.SetDirection(Scrollbar.Direction.BottomToTop, false);
                    scroller.viewport = parent.GetComponent<RectTransform>();
                    //scroller.content = parent.GetComponent<RectTransform>();
                    scroller.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
                    scroller.vertical = true;
                    scroller.horizontal = false;
                    scroller.scrollSensitivity = 25;
                }
                else
                {
                    scroller = null;
                }

                GameObject MRBRep = GameObject.Find("uixPrfPanl_AA_MercBoardReputationPanel");
                if (MRBRep != null)
                {
                    MRBRep.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                    MRBRep.transform.localPosition = new Vector3(0, 390, MRBRep.transform.localPosition.z);
                }

                GridLayoutGroup grid = parent.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    grid.constraintCount = 5;
                    grid.spacing = new Vector2(0, 0);
                    grid.cellSize = new Vector2(275, grid.cellSize.y);
                    grid.childAlignment = TextAnchor.UpperLeft;
                    scroller.content = grid.GetComponent<RectTransform>();
                }

                GameObject primeWidget = __instance.FactionPanelWidgets[0].gameObject;
                if (__instance.FactionPanelWidgets.Count < __instance.simState.displayedFactions.Count + 1)
                {
                    __instance.FactionPanelWidgets.Clear();
                    for (int i = 0; i < __instance.simState.displayedFactions.Count + 1; i++)
                    {
                        GameObject newwidget = GameObject.Instantiate(primeWidget);
                        newwidget.transform.parent = primeWidget.transform.parent;
                        newwidget.name = "NewWidget";
                        newwidget.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                        newwidget.transform.position = new Vector3(newwidget.transform.position.x, 200,
                            newwidget.transform.position.z);
                        RectTransform repText = newwidget.transform.FindRecursive("classification-text")
                            .GetComponent<RectTransform>();
                        repText.localPosition = new Vector3(0, repText.localPosition.y, repText.localPosition.z);
                        RectTransform bar = newwidget.transform.FindRecursive("factionBar_Layout")
                            .GetComponent<RectTransform>();
                        bar.sizeDelta = new Vector2(125, bar.sizeDelta.y);
                        RectTransform score = newwidget.transform.FindRecursive("RepScore-text")
                            .GetComponent<RectTransform>();
                        score.localPosition = new Vector3(120, score.localPosition.y, score.localPosition.z);
                        RectTransform negative = newwidget.transform
                            .FindRecursive("faction_Negativefill_moveThisNegative").GetComponent<RectTransform>();
                        negative.localPosition = new Vector3(0, 0, 0);
                        negative.sizeDelta = new Vector2(64, 0);
                        RectTransform allianceButton = newwidget.transform.FindRecursive("OBJ_allianceButtons")
                            .GetComponent<RectTransform>();
                        allianceButton.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                        allianceButton.transform.FindRecursive("connectorH").gameObject.SetActive(false);
                        RectTransform positive = newwidget.transform
                            .FindRecursive("faction_Positivefill_moveThisPositive").GetComponent<RectTransform>();
                        positive.localPosition = new Vector3(0, 0, 0);
                        positive.sizeDelta = new Vector2(64, 0);
                        /*RectTransform square = newwidget.transform.FindRecursive("squaresPanel").GetComponent<RectTransform>();
                        square.localPosition = new Vector3(18, square.localPosition.y, square.localPosition.z);*/
                        SGFactionReputationWidget newSGWidget = newwidget.GetComponent<SGFactionReputationWidget>();
                        __instance.FactionPanelWidgets.Add(newSGWidget);
                    }
                }

                foreach (GameObject go in parent.FindAllContains(
                             "uixPrfWidget_factionReputationBidirectionalWidget"))
                {
                    go.SetActive(false);
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e);

        }
    }

    static void Postfix(ref SGCaptainsQuartersReputationScreen __instance)
    {
        try
        {
            FactionDef factionDef = FactionEnumeration.GetAuriganRestorationFactionValue().FactionDef;
            if (factionDef != null)
            {
                __instance.FactionPanelWidgets[__instance.FactionPanelWidgets.Count - 1].gameObject.SetActive(true);
                __instance.FactionPanelWidgets[__instance.FactionPanelWidgets.Count - 1].Init(__instance.simState,
                    FactionEnumeration.GetAuriganRestorationFactionValue(),
                    new UnityAction(__instance.RefreshWidgets), false);

            }
        }
        catch (Exception e)
        {
            Logger.LogError(e);

        }
    }
}