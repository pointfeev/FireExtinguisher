using HarmonyLib;
using RimWorld;
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

            harmony.Patch(
                original: AccessTools.Method(typeof(ShieldBelt), "AllowVerbCast"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), "ShieldBeltAllowVerbCast")
            );

            if (CompatibilityUtils.SimpleSidearmsInstalled)
            {
                harmony.Patch(
                    original: AccessTools.Method(AccessTools.TypeByName("SimpleSidearms.rimworld.CompSidearmMemory"), "set_DefaultRangedWeapon"),
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), "SimpleSidearmsDefaultWeaponPatch")
                );
            }
        }

        public static bool EndCurrentJob(Pawn_JobTracker __instance, Pawn ___pawn)
        {
            if (__instance?.curJob?.def?.defName == "ExtinguishFire" && !(___pawn is null) && ___pawn.IsFreeColonist)
            {
                InventoryUtils.UnequipFireExtinguisher(___pawn);
                //Log.Warning("[FireExtinguisher] ExtinguishFire ended with condition: " + condition.ToString());
            }
            return true;
        }

        public static void ShieldBeltAllowVerbCast(ref bool __result, Verb verb)
        {
            if (InventoryUtils.CheckDefName(verb?.EquipmentSource?.def?.defName))
            {
                __result = true;
            }
        }

        public static void SimpleSidearmsStatCalculatorPatch(ThingWithComps weapon, ref float __result)
        {
            if (InventoryUtils.CheckDefName(weapon?.def?.defName))
            {
                __result = 0;
            }
        }

        public static bool SimpleSidearmsDefaultWeaponPatch(SimpleSidearms.rimworld.ThingDefStuffDefPair? value)
        {
            return !InventoryUtils.CheckDefName(value.HasValue ? value.Value.thing?.defName : null);
        }
    }
}
