using System.Reflection;
using CompatUtils;
using Verse;

namespace FireExtinguisher;

[StaticConstructorOnStartup]
internal static class ModCompatibility
{
    private static readonly MethodInfo CombatExtendedHasAmmoMethod;

    static ModCompatibility()
        => CombatExtendedHasAmmoMethod = Compatibility.GetConsistentMethod("ceteam.combatextended", "CombatExtended.CE_Utility", "HasAmmo",
            new[] { typeof(ThingWithComps) }, true);

    private static bool HasAmmo(ThingWithComps thingWithComps)
        => CombatExtendedHasAmmoMethod is null || (bool)CombatExtendedHasAmmoMethod.Invoke(null, new object[] { thingWithComps });

    internal static bool CheckWeapon(ThingWithComps thingWithComps) => thingWithComps is not null && HasAmmo(thingWithComps);
}