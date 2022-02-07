using System;
using System.Reflection;

using CompatUtils;

using Verse;

namespace FireExtinguisher
{
    [StaticConstructorOnStartup]
    public static class ModCompatibility
    {
        public static MethodInfo combatExtendedHasAmmoMethod;

        static ModCompatibility() => combatExtendedHasAmmoMethod = Compatibility.GetConsistentMethod("ceteam.combatextended", "CombatExtended.CE_Utility", "HasAmmo", new Type[] {
                typeof(ThingWithComps)
            }, logError: true);

        private static bool HasAmmo(ThingWithComps thingWithComps) => combatExtendedHasAmmoMethod is null || (bool)combatExtendedHasAmmoMethod.Invoke(null, new object[] { thingWithComps });

        public static bool CheckWeapon(ThingWithComps thingWithComps) => !(thingWithComps is null) && HasAmmo(thingWithComps);
    }
}
