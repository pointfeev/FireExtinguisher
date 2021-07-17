using System.Linq;
using Verse;

namespace FireExtinguisher
{
    public static class CompatibilityUtils
    {
        public static bool SimpleSidearmsInstalled = (from mod in ModLister.AllInstalledMods
                                                      where mod.Active && mod.PackageId.ToLower() == "petetimessix.simplesidearms"
                                                      select mod).Any<ModMetaData>();

        public static void SimpleSidearmsDefaultCheck(Pawn pawn)
        {
            if (!SimpleSidearmsInstalled) { return; }
            try { DoSimpleSidearmsDefaultCheck(pawn); } catch { }
        }

        private static void DoSimpleSidearmsDefaultCheck(Pawn pawn)
        {
            if (!SimpleSidearmsInstalled) { return; }
            try
            {
                SimpleSidearms.rimworld.CompSidearmMemory memoryCompForPawn = SimpleSidearms.rimworld.CompSidearmMemory.GetMemoryCompForPawn(pawn);
                if (memoryCompForPawn != null)
                {
                    SimpleSidearms.rimworld.ThingDefStuffDefPair? DefaultRangedWeapon = memoryCompForPawn.DefaultRangedWeapon;
                    if (DefaultRangedWeapon != null && DefaultRangedWeapon.Value != null && InventoryUtils.CheckDefName(DefaultRangedWeapon.Value.thing.defName))
                    {
                        memoryCompForPawn.defaultRangedWeaponEx = null;
                    }
                }
            }
            catch { }
        }
    }
}
