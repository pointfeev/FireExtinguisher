using System.Collections.Generic;
using System.Linq;

using Verse;
using Verse.Sound;

namespace FireExtinguisher
{
    public static class InventoryUtils
    {
        public static List<string> fireExtinguisherDefNames = new List<string>(new string[] { "VWE_Gun_FireExtinguisher", "Gun_Fire_Ext" });
        private static bool CheckDefName(string defName) => !(defName is null) && fireExtinguisherDefNames.Contains(defName);

        public static bool IsWeaponFireExtinguisher(ThingWithComps weapon) => !(weapon is null) && CheckDefName(weapon.def?.defName);

        public static ThingWithComps GetFireExtinguisherFromEquipment(Pawn pawn)
        {
            ThingWithComps primary = pawn.equipment.Primary;
            return IsWeaponFireExtinguisher(primary) && ModCompatibility.CheckWeapon(primary) ? primary : null;
        }

        public static ThingWithComps GetFireExtinguisherFromInventory(Pawn pawn) => (from thing in pawn.inventory.innerContainer
                                                                                     where IsWeaponFireExtinguisher(thing as ThingWithComps) &&
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
                    weapon.holdingOwner.Remove(weapon);
                pawn.equipment.AddEquipment(weapon);
                weapon.def.soundInteract?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                return true;
            }
            return false;
        }

        private static bool CanEquipFireExtinguisher(Pawn pawn, ThingWithComps extinguisher) => !(pawn is null) && !(extinguisher is null) && !(extinguisher.def is null)
            && !pawn.WorkTagIsDisabled(WorkTags.Firefighting) && (!pawn.WorkTagIsDisabled(WorkTags.Violent) || !extinguisher.def.IsRangedWeapon);
        public static bool CanEquipFireExtinguisher(Pawn pawn) => CanEquipFireExtinguisher(pawn, GetFireExtinguisherFromInventory(pawn));

        public static bool EquipFireExtinguisher(Pawn pawn) => !(GetFireExtinguisherFromEquipment(pawn) is null)
            || EquipWeapon(pawn, GetFireExtinguisherFromInventory(pawn), true);

        public static bool UnequipFireExtinguisher(Pawn pawn)
        {
            ThingWithComps fireExtinguisher = GetFireExtinguisherFromEquipment(pawn);
            if (!(fireExtinguisher is null))
            {
                ThingWithComps previousWeapon = previousWeapons.TryGetValue(pawn);
                if (!(previousWeapon is null))
                {
                    if (previousWeapon == pawn.equipment.Primary)
                        previousWeapons.Remove(pawn);
                    else
                        return UnequipWeapon(pawn) && EquipWeapon(pawn, previousWeapon) & previousWeapons.Remove(pawn);
                }
            }
            return true;
        }
    }
}
