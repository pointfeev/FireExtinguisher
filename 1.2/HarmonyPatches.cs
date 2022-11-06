using HarmonyLib;

using RimWorld;

using Verse;
using Verse.AI;

namespace FireExtinguisher
{
    [StaticConstructorOnStartup]
    internal static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("pointfeev.fireextinguisher");
            _ = harmony.Patch(
                original: AccessTools.Method(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.EndCurrentJob)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(EndCurrentJob))
            );
            _ = harmony.Patch(
                original: AccessTools.Method(typeof(ShieldBelt), nameof(ShieldBelt.AllowVerbCast)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompAllowVerbCast))
            );
        }

        internal static bool EndCurrentJob(Pawn_JobTracker __instance, Pawn ___pawn)
        {
            if (__instance?.curJob?.def == JobDefOf_ExtinguishFire.ExtinguishFire && !(___pawn is null))
                _ = InventoryUtils.UnequipFireExtinguisher(___pawn);
            return true;
        }

        internal static void CompAllowVerbCast(ref bool __result, Verb verb)
        {
            if (InventoryUtils.CanWeaponExtinguish(verb?.EquipmentSource))
                __result = true;
        }
    }
}
