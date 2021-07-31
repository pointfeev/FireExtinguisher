using System.Linq;
using Verse;

namespace FireExtinguisher
{
    public static class CompatibilityUtils
    {
        public static bool SimpleSidearmsInstalled = (from mod in ModLister.AllInstalledMods
                                                      where mod.Active && mod.PackageId.ToLower() == "petetimessix.simplesidearms"
                                                      select mod).Any<ModMetaData>();

        public static bool CombatExtendedInstalled = (from mod in ModLister.AllInstalledMods
                                                      where mod.Active && mod.PackageId.ToLower() == "ceteam.combatextended"
                                                      select mod).Any<ModMetaData>();

        public static bool CombatExtendedAmmoCheck(ThingWithComps thingWithComps)
        {
            if (CombatExtendedInstalled)
            {
                CombatExtended.CompAmmoUser comp = thingWithComps.TryGetComp<CombatExtended.CompAmmoUser>();
                if (!(comp is null))
                    return !comp.UseAmmo || comp.CurMagCount > 0 || comp.HasAmmo;
            }
            return true;
        }
    }
}
