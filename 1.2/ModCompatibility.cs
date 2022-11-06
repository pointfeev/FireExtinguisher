using CompatUtils;

using System;
using System.Reflection;

using Verse;

namespace FireExtinguisher
{
    [StaticConstructorOnStartup]
    internal static class ModCompatibility
    {
        internal static MethodInfo combatExtendedHasAmmoMethod;

        static ModCompatibility() => combatExtendedHasAmmoMethod = Compatibility.GetConsistentMethod("ceteam.combatextended", "CombatExtended.CE_Utility", "HasAmmo", new Type[] {
                typeof(ThingWithComps)
            }, logError: true);

        private static bool HasAmmo(ThingWithComps thingWithComps) => combatExtendedHasAmmoMethod is null
            || (bool)combatExtendedHasAmmoMethod.Invoke(null, new object[] { thingWithComps });

        internal static bool CheckWeapon(ThingWithComps thingWithComps) => !(thingWithComps is null) && HasAmmo(thingWithComps);
    }
}
