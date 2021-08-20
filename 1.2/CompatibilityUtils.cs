using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using Verse;

namespace FireExtinguisher
{
    public static class CompatibilityUtils
    {
        public static bool CombatExtendedIsActive;
        public static MethodInfo combatExtendedHasAmmo_Method;

        public static bool IsMethodCorrect(string modName, MethodInfo methodInfo, Type[] correctTypes)
        {
            Type[] currentTypes = methodInfo.GetParameters().Select(pi => pi.ParameterType.IsByRef ? pi.ParameterType.GetElementType() : pi.ParameterType).ToArray();
            for (int i = 0; i < currentTypes.Length; i++)
            {
                if (currentTypes[i] != correctTypes[i])
                {
                    Log.Error($"[FireExtinguisher] Failed to support {modName}: Incorrect parameter {i + 1} for method '{methodInfo.ReflectedType.FullName + "." + methodInfo.Name}'!  Please report this error!" +
                        "\n    " + currentTypes[i] + " != " + correctTypes[i]);
                    return false;
                }
            }
            return true;
        }

        static CompatibilityUtils()
        {
            CombatExtendedIsActive = ModLister.AllInstalledMods.Any(x => x.Active && x.PackageId.ToLower() == "ceteam.combatextended");
            if (CombatExtendedIsActive)
            {
                combatExtendedHasAmmo_Method = AccessTools.Method(AccessTools.TypeByName("CombatExtended.CE_Utility"), "HasAmmo");
                if (combatExtendedHasAmmo_Method is null)
                {
                    Log.Error("[FireExtinguisher] Failed to support Combat Extended! Please report this error!");
                    CombatExtendedIsActive = false;
                }
                else
                {
                    CombatExtendedIsActive = IsMethodCorrect("Combat Extended", combatExtendedHasAmmo_Method, new Type[] { typeof(ThingWithComps) });
                }
            }
        }

        public static bool HasAmmo(ThingWithComps thingWithComps)
        {
            if (CombatExtendedIsActive) return (bool)combatExtendedHasAmmo_Method.Invoke(null, new object[] { thingWithComps });
            return true;
        }
    }
}
