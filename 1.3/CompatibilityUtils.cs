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
            if (pawn is null || !SimpleSidearmsInstalled) { return; }
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

        public static bool CombatExtendedInstalled = (from mod in ModLister.AllInstalledMods
                                                      where mod.Active && mod.PackageId.ToLower() == "ceteam.combatextended"
                                                      select mod).Any<ModMetaData>();

        public static bool CombatExtendedAmmoCheck(ThingWithComps thingWithComps)
        {
            if (!CombatExtendedInstalled) { return true; }
            try { return DoCombatExtendedAmmoCheck(thingWithComps); } catch { }
            return true;
        }

        private static bool DoCombatExtendedAmmoCheck(ThingWithComps thingWithComps)
        {
            if (!CombatExtendedInstalled) { return true; }
            try
            {
                CombatExtended.CompAmmoUser comp = thingWithComps.TryGetComp<CombatExtended.CompAmmoUser>();
                if (comp == null) return true;
                return !comp.UseAmmo || comp.CurMagCount > 0 || comp.HasAmmo;
            }
            catch { }
            return true;
        }
    }
}
