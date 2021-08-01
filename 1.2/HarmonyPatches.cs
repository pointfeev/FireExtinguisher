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
        }

        // makes sure fire extinguishers are unequipped after the ExtinguishFire job ends
        public static bool EndCurrentJob(Pawn_JobTracker __instance, Pawn ___pawn)
        {
            if (__instance?.curJob?.def?.defName == "ExtinguishFire" && !(___pawn is null) && ___pawn.IsFreeColonist)
            {
                InventoryUtils.UnequipFireExtinguisher(___pawn);
                //Log.Warning("[FireExtinguisher] ExtinguishFire ended with condition: " + condition.ToString());
            }
            return true;
        }

        // overrides the AllowVerbCast for shield belts if the verb is from a fire extinguisher
        public static void ShieldBeltAllowVerbCast(ref bool __result, Verb verb)
        {
            if (InventoryUtils.CheckDefName(verb?.EquipmentSource?.def?.defName))
            {
                __result = true;
            }
        }
    }
}
