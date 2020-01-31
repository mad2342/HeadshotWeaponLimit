using System;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace HeadshotWeaponLimit.Patches
{
    class GameLogic
    {
        [HarmonyPatch(typeof(CombatHUD), "OnWeaponModified")]
        public static class CombatHUD_OnWeaponModified_Patch
        {
            public static void Postfix(CombatHUD __instance)
            {
                try
                {
                    // Only relevant for Precision Strike
                    if (__instance.SelectionHandler.ActiveState == null || __instance.SelectionHandler.ActiveState.SelectionType != SelectionType.FireMorale)
                    {
                        return;
                    }
                    // No headshots from behind
                    if (__instance.CalledShotPopUp.ShownAttackDirection == AttackDirection.FromBack)
                    {
                        return;
                    }

                    Logger.Debug("[CombatHUD_OnWeaponModified_POSTFIX] Clear confirmed called headshot if enabled weapons change to an invalid amount...");

                    bool isCalledShotPopupVisible = __instance.CalledShotPopUp.Visible;
                    Logger.Debug("[CombatHUD_OnWeaponModified_POSTFIX] isCalledShotPopupVisible: " + isCalledShotPopupVisible);

                    int enabledWeaponCount = Utilities.GetReadiedWeaponCount(__instance.SelectedActor.Weapons, __instance.SelectedTarget, true);
                    Pilot selectedPilot = __instance.SelectedActor.GetPilot();
                    int maxAllowedWeapons = Utilities.GetMaxAllowedWeaponCountForHeadshots(selectedPilot);
                    bool validWeaponCountForPrecisionStrike = enabledWeaponCount <= maxAllowedWeapons ? true : false;
                    Logger.Debug("[CombatHUD_OnWeaponModified_POSTFIX] enabledWeaponCount: " + enabledWeaponCount);
                    Logger.Debug("[CombatHUD_OnWeaponModified_POSTFIX] validWeaponCountForPrecisionStrike: " + validWeaponCountForPrecisionStrike);

                    ArmorLocation baseCalledShotLocation = (ArmorLocation)typeof(SelectionStateFire).GetField("calledShotLocation", AccessTools.all).GetValue(__instance.SelectionHandler.ActiveState);
                    bool headIsTargeted = baseCalledShotLocation == ArmorLocation.Head;
                    Logger.Debug("[CombatHUD_OnWeaponModified_POSTFIX] headIsTargeted: " + headIsTargeted);

                    //bool isMoraleAttack = __instance.SelectionHandler.ActiveState is SelectionStateMoraleAttack;
                    //Logger.Debug("[CombatHUD_OnWeaponModified_POSTFIX] isMoraleAttack: " + isMoraleAttack);

                    bool shouldBackOut = headIsTargeted && !isCalledShotPopupVisible && !validWeaponCountForPrecisionStrike;
                    Logger.Debug("[CombatHUD_OnWeaponModified_POSTFIX] shouldBackOut: " + shouldBackOut);

                    if (shouldBackOut) {
                        Logger.Debug("[CombatHUD_OnWeaponModified_POSTFIX] " + __instance.SelectionHandler.ActiveState.ToString() + ".BackOut()");
                        __instance.SelectionHandler.ActiveState.BackOut();
                    }

                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        [HarmonyPatch(typeof(HUDMechArmorReadout), "SetHoveredArmor", new Type[] { typeof(ArmorLocation) })]
        public static class HUDMechArmorReadout_SetHoveredArmor_Patch
        {
            public static void Postfix(HUDMechArmorReadout __instance, ArmorLocation location, Mech ___displayedMech)
            {
                try
                {
                    if (__instance.UseForCalledShots && location == ArmorLocation.Head)
                    {
                        Logger.Debug("[HUDMechArmorReadout_SetHoveredArmor_POSTFIX] Clear head selection if too many weapons are enabled...");

                        int enabledWeaponCount = Utilities.GetReadiedWeaponCount(__instance.HUD.SelectedActor.Weapons, __instance.HUD.SelectedTarget);
                        Pilot selectedPilot = __instance.HUD.SelectedActor.GetPilot();
                        int maxAllowedWeapons = Utilities.GetMaxAllowedWeaponCountForHeadshots(selectedPilot);
                        bool validWeaponCountForPrecisionStrike = enabledWeaponCount <= maxAllowedWeapons ? true : false;

                        bool headCanBeTargeted = __instance.HUD.SelectedTarget.IsShutDown || __instance.HUD.SelectedTarget.IsProne || validWeaponCountForPrecisionStrike;

                        Logger.Debug("[HUDMechArmorReadout_SetHoveredArmor_POSTFIX] __instance.HUD.SelectedActor: " + __instance.HUD.SelectedActor.DisplayName);
                        Logger.Debug("[HUDMechArmorReadout_SetHoveredArmor_POSTFIX] enabledWeaponCount: " + enabledWeaponCount);
                        Logger.Debug("[HUDMechArmorReadout_SetHoveredArmor_POSTFIX] validWeaponCountForPrecisionStrike: " + validWeaponCountForPrecisionStrike);

                        Logger.Debug("[HUDMechArmorReadout_SetHoveredArmor_POSTFIX] ___displayedMech: " + ___displayedMech.DisplayName);
                        Logger.Debug("[HUDMechArmorReadout_SetHoveredArmor_POSTFIX] ___displayedMech.IsShutDown: " + ___displayedMech.IsShutDown);
                        Logger.Debug("[HUDMechArmorReadout_SetHoveredArmor_POSTFIX] ___displayedMech.IsProne: " + ___displayedMech.IsProne);
                        Logger.Debug("[HUDMechArmorReadout_SetHoveredArmor_POSTFIX] headCanBeTargeted: " + headCanBeTargeted);

                        if (!headCanBeTargeted)
                        {
                            Logger.Debug("[HUDMechArmorReadout_SetHoveredArmor_POSTFIX] Prevent targeting of head...");
                            __instance.ClearHoveredArmor(ArmorLocation.Head);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        [HarmonyPatch(typeof(SelectionStateFire), "SetCalledShot", new Type[] { typeof(ArmorLocation) })]
        public static class SelectionStateFire_SetCalledShot_Patch
        {
            public static void Postfix(SelectionStateFire __instance, ArmorLocation location)
            {
                try
                {
                    if (location == ArmorLocation.Head)
                    {
                        Logger.Debug("[SelectionStateFire_SetCalledShot_POSTFIX] Disable headshot if too many weapons are enabled...");

                        int enabledWeaponCount = Utilities.GetReadiedWeaponCount(__instance.SelectedActor.Weapons, __instance.TargetedCombatant);
                        Pilot selectedPilot = __instance.SelectedActor.GetPilot();
                        int maxAllowedWeapons = Utilities.GetMaxAllowedWeaponCountForHeadshots(selectedPilot);
                        bool validWeaponCountForPrecisionStrike = enabledWeaponCount <= maxAllowedWeapons ? true : false;

                        bool headCanBeTargeted = __instance.TargetedCombatant.IsShutDown || __instance.TargetedCombatant.IsProne || validWeaponCountForPrecisionStrike;

                        Logger.Debug("[SelectionStateFire_SetCalledShot_POSTFIX] __instance.SelectedActor: " + __instance.SelectedActor.DisplayName);
                        Logger.Debug("[SelectionStateFire_SetCalledShot_POSTFIX] enabledWeaponCount: " + enabledWeaponCount);
                        Logger.Debug("[SelectionStateFire_SetCalledShot_POSTFIX] validWeaponCountForPrecisionStrike: " + validWeaponCountForPrecisionStrike);

                        Logger.Debug("[SelectionStateFire_SetCalledShot_POSTFIX] __instance.TargetedCombatant.DisplayName: " + __instance.TargetedCombatant.DisplayName);
                        Logger.Debug("[SelectionStateFire_SetCalledShot_POSTFIX] __instance.TargetedCombatant.IsShutDown: " + __instance.TargetedCombatant.IsShutDown);
                        Logger.Debug("[SelectionStateFire_SetCalledShot_POSTFIX] __instance.TargetedCombatant.IsProne: " + __instance.TargetedCombatant.IsProne);
                        Logger.Debug("[SelectionStateFire_SetCalledShot_POSTFIX] headCanBeTargeted: " + headCanBeTargeted);

                        if (!headCanBeTargeted)
                        {
                            Logger.Debug("[SelectionStateFire_SetCalledShot_POSTFIX] Disabling headshot...");
                            Traverse.Create(__instance).Method("ClearCalledShot").GetValue();
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }
    }
}
