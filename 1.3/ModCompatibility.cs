using System;
using System.Reflection;
using Verse;
using CompatibilityUtils;

namespace FireExtinguisher
{
    [StaticConstructorOnStartup]
    public static class ModCompatibility
    {
        public static MethodInfo combatExtendedHasAmmoMethod;

        static ModCompatibility()
        {
            combatExtendedHasAmmoMethod = Compatibility.GetConsistentMethod("ceteam.combatextended", "CombatExtended.CE_Utility", "HasAmmo", new Type[] {
                typeof(ThingWithComps)
            }, logError: true);
        }

        public static bool HasAmmo(ThingWithComps thingWithComps)
        {
            return combatExtendedHasAmmoMethod is null || (bool)combatExtendedHasAmmoMethod.Invoke(null, new object[] { thingWithComps });
        }
    }
}
