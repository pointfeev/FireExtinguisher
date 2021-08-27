using System;
using System.Reflection;
using Verse;
using CompatUtils;

namespace FireExtinguisher
{
    [StaticConstructorOnStartup]
    public static class ModCompatibility
    {
        public static bool combatExtendedActive = Compatibility.IsModActive("ceteam.combatextended");

        public static MethodInfo combatExtendedHasAmmoMethod;

        static ModCompatibility()
        {
            combatExtendedHasAmmoMethod = Compatibility.GetConsistentMethod("ceteam.combatextended", "CombatExtended.CE_Utility", "HasAmmo", new Type[] {
                typeof(ThingWithComps)
            }, logError: true);
        }

        private static bool HasAmmo(ThingWithComps thingWithComps)
        {
            return combatExtendedHasAmmoMethod is null || (bool)combatExtendedHasAmmoMethod.Invoke(null, new object[] { thingWithComps });
        }

        public static bool CheckWeapon(ThingWithComps thingWithComps)
        {
            return !(thingWithComps is null) && HasAmmo(thingWithComps);
        }
    }
}
