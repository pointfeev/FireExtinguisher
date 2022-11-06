using RimWorld;

using System.Collections.Generic;
using System.Linq;

using Verse;
using Verse.Sound;

namespace FireExtinguisher
{
    internal static class InventoryUtils
    {
        private static readonly List<string> ExtinguishDamageDefNames = new List<string>() { "VWE_Extinguish", "FExtExtinguish" };
        internal static bool CanWeaponExtinguish(ThingWithComps weapon) => GetPrimaryVerb(weapon)?.GetDamageDef() is DamageDef damageDef
            && (damageDef == DamageDefOf.Extinguish || ExtinguishDamageDefNames.Contains(damageDef.defName));

        internal static Verb GetPrimaryVerb(ThingWithComps weapon) => weapon?.GetComp<CompEquippable>()?.PrimaryVerb;

        internal static ThingWithComps GetFireExtinguisherFromEquipment(Pawn pawn) => pawn?.equipment?.Primary is ThingWithComps primary
            && CanWeaponExtinguish(primary) && ModCompatibility.CheckWeapon(primary) ? primary : null;

        internal static ThingWithComps GetFireExtinguisherFromInventory(Pawn pawn) => (from thing in pawn.inventory.innerContainer
                                                                                       where CanWeaponExtinguish(thing as ThingWithComps) &&
                                                                                           ModCompatibility.CheckWeapon(thing as ThingWithComps)
                                                                                       orderby thing.MarketValue descending
                                                                                       select thing).FirstOrFallback() as ThingWithComps;

        private static readonly Dictionary<Pawn, ThingWithComps> previousWeapons = new Dictionary<Pawn, ThingWithComps>();
        private static bool UnequipWeapon(Pawn pawn, bool cachePrevious = false)
        {
            if (!(pawn is null))
            {
                ThingWithComps primary = pawn.equipment.Primary;
                if (!(primary is null))
                {
                    bool success = pawn.inventory.innerContainer.TryAddOrTransfer(primary);
                    if (success)
                    {
                        if (cachePrevious)
                            previousWeapons.SetOrAdd(pawn, primary);
                    }
                    else
                    {
                        Log.Error($"[FireExtinguisher] Failed to unequip weapon for {pawn.LabelShort}: {primary.Label}");
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private static bool EquipWeapon(Pawn pawn, ThingWithComps weapon, bool cachePrevious = false)
        {
            if (!(pawn is null) && !(weapon is null) && UnequipWeapon(pawn, cachePrevious))
            {
                if (weapon.stackCount > 1)
                    weapon = weapon.SplitOff(1) as ThingWithComps;
                if (!(weapon.holdingOwner is null))
                    _ = weapon.holdingOwner.Remove(weapon);
                pawn.equipment.AddEquipment(weapon);
                weapon.def.soundInteract?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                return true;
            }
            return false;
        }

        private static bool CanEquipFireExtinguisher(Pawn pawn, ThingWithComps extinguisher) => !(pawn is null) && !(extinguisher is null) && !(extinguisher.def is null)
            && !pawn.WorkTagIsDisabled(WorkTags.Firefighting) && (!pawn.WorkTagIsDisabled(WorkTags.Violent) || !extinguisher.def.IsRangedWeapon);
        internal static bool CanEquipFireExtinguisher(Pawn pawn) => !(GetFireExtinguisherFromEquipment(pawn) is null)
            || CanEquipFireExtinguisher(pawn, GetFireExtinguisherFromInventory(pawn));

        internal static bool EquipFireExtinguisher(Pawn pawn) => !(GetFireExtinguisherFromEquipment(pawn) is null)
            || EquipWeapon(pawn, GetFireExtinguisherFromInventory(pawn), true);

        internal static bool UnequipFireExtinguisher(Pawn pawn)
        {
            ThingWithComps fireExtinguisher = GetFireExtinguisherFromEquipment(pawn);
            if (!(fireExtinguisher is null))
            {
                ThingWithComps previousWeapon = previousWeapons.TryGetValue(pawn);
                if (!(previousWeapon is null))
                {
                    if (previousWeapon == pawn.equipment.Primary)
                        _ = previousWeapons.Remove(pawn);
                    else
                        return UnequipWeapon(pawn) && EquipWeapon(pawn, previousWeapon) & previousWeapons.Remove(pawn);
                }
            }
            return true;
        }
    }
}
