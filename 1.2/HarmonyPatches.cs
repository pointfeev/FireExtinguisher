using HarmonyLib;
using Verse;
using Verse.AI;

namespace FireExtinguisher
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("pointfeev.fireextinguisher");

            harmony.Patch(
                original: AccessTools.Method(typeof(Pawn_JobTracker), "EndCurrentJob"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), "EndCurrentJob")
            );

            if (CompatibilityUtils.SimpleSidearmsInstalled)
            {
                harmony.Patch(
                    original: AccessTools.Method(AccessTools.TypeByName("SimpleSidearms.utilities.StatCalculator"), "RangedDPS"),
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), "SimpleSidearmsStatCalculatorPatch")
                );
                harmony.Patch(
                    original: AccessTools.Method(AccessTools.TypeByName("SimpleSidearms.utilities.StatCalculator"), "RangedDPSAverage"),
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), "SimpleSidearmsStatCalculatorPatch")
                );
                harmony.Patch(
                    original: AccessTools.Method(AccessTools.TypeByName("SimpleSidearms.rimworld.CompSidearmMemory"), "InformOfAddedPrimary"),
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), "SimpleSidearmsDefaultWeaponPatch")
                );
            }
        }

        public static bool EndCurrentJob(Pawn_JobTracker __instance, Pawn ___pawn)
        {
            if (__instance != null && __instance.curJob != null && __instance.curJob.def != null && ___pawn != null)
            {
                if (__instance.curJob.def.defName == "ExtinguishFire" && ___pawn.IsFreeColonist)
                {
                    InventoryUtils.UnequipFireExtinguisher(___pawn);
                    //Log.Warning("[FireExtinguisher] ExtinguishFire ended with condition: " + condition.ToString());
                }
            }
            return true;
        }

        public static void SimpleSidearmsStatCalculatorPatch(ThingWithComps weapon, ref float __result)
        {
            if (weapon != null)
            {
                if (InventoryUtils.CheckDefName(weapon.def.defName))
                {
                    __result = 0;
                }
            }
        }

        public static void SimpleSidearmsDefaultWeaponPatch(Thing weapon)
        {
            if (weapon != null)
            {
                CompatibilityUtils.SimpleSidearmsDefaultCheck(weapon.ParentHolder as Pawn);
            }
        }
    }
}
