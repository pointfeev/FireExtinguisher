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
            _ = harmony.Patch(AccessTools.Method(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.EndCurrentJob)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(EndCurrentJob)));
            _ = harmony.Patch(AccessTools.Method(typeof(ShieldBelt), nameof(ShieldBelt.AllowVerbCast)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(AllowVerbCast)));
        }

        internal static bool EndCurrentJob(Pawn_JobTracker __instance, Pawn ___pawn)
        {
            if (__instance?.curJob?.def != JobDefOf_ExtinguishFire.ExtinguishFire || ___pawn is null)
                return true;
            _ = InventoryUtils.UnEquipFireExtinguisher(___pawn);
            _ = CastUtils.LastThing.Remove(___pawn.thingIDNumber);
            return true;
        }

        internal static void AllowVerbCast(ref bool __result, Verb verb)
        {
            if (InventoryUtils.CanWeaponExtinguish(verb?.EquipmentSource))
                __result = true;
        }
    }
}