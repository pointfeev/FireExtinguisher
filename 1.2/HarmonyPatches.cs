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
            Harmony harmony = new Harmony("pointfeev.fireextinguisher");
            harmony.Patch(
                original: AccessTools.Method(typeof(Pawn_JobTracker), "EndCurrentJob"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), "EndCurrentJob")
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(ShieldBelt), "AllowVerbCast"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), "ShieldBeltAllowVerbCast")
            );
        }

        public static bool EndCurrentJob(Pawn_JobTracker __instance, Pawn ___pawn)
        {
            if (__instance?.curJob?.def?.defName == "ExtinguishFire" && !(___pawn is null))
                InventoryUtils.UnequipFireExtinguisher(___pawn);
            return true;
        }

        public static void ShieldBeltAllowVerbCast(ref bool __result, Verb verb)
        {
            if (InventoryUtils.IsWeaponFireExtinguisher(verb?.EquipmentSource))
                __result = true;
        }
    }
}
