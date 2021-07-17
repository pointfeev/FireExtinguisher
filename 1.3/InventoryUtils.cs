using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FireExtinguisher
{
    public static class InventoryUtils
    {
        public static List<string> fireExtinguisherDefNames = new List<string>(new string[] { "VWE_Gun_FireExtinguisher", "Gun_Fire_Ext" });

        public static bool CheckDefName(string defName)
        {
            return fireExtinguisherDefNames.Contains(defName);
        }

        public static ThingWithComps GetEquippedFireExtinguisher(Pawn pawn)
        {
            if (pawn.equipment.Primary != null && CheckDefName(pawn.equipment.Primary.def.defName))
            {
                return pawn.equipment.Primary;
            }
            return null;
        }

        public static ThingWithComps GetFireExtinguisherFromInventory(Pawn pawn)
        {
            ThingWithComps fireExtinguisher = (from thing in pawn.inventory.innerContainer
                                               where thing is ThingWithComps && CheckDefName(thing.def.defName)
                                               orderby thing.MarketValue descending
                                               select thing).FirstOrDefault<Thing>() as ThingWithComps;
            if (fireExtinguisher != null)
            {
                return fireExtinguisher;
            }
            return null;
        }

        private static Dictionary<Pawn, ThingWithComps> previousEquipped = new Dictionary<Pawn, ThingWithComps>();

        public static bool EquipFireExtinguisher(Pawn pawn, bool jobBeginning)
        {
            if (jobBeginning && pawn.equipment.Primary != null && !CheckDefName(pawn.equipment.Primary.def.defName))
            {
                previousEquipped.SetOrAdd(pawn, pawn.equipment.Primary);
            }
            ThingWithComps fireExtinguisher = GetEquippedFireExtinguisher(pawn);
            if (fireExtinguisher == null)
            {
                fireExtinguisher = GetFireExtinguisherFromInventory(pawn);
                if (fireExtinguisher != null)
                {
                    if (pawn.equipment.Primary != null)
                    {
                        ThingWithComps primary = pawn.equipment.Primary;
                        pawn.equipment.Remove(primary);
                        pawn.inventory.innerContainer.TryAdd(primary, true);
                    }
                    pawn.inventory.innerContainer.Remove(fireExtinguisher);
                    pawn.equipment.MakeRoomFor(fireExtinguisher);
                    pawn.equipment.AddEquipment(fireExtinguisher);
                }
            }
            if (fireExtinguisher != null)
            {
                TweakUtils.Apply(fireExtinguisher);

                CompatibilityUtils.SimpleSidearmsDefaultCheck(pawn);
                return true;
            }
            return false;
        }

        public static bool UnequipFireExtinguisher(Pawn pawn)
        {
            ThingWithComps fireExtinguisher = GetEquippedFireExtinguisher(pawn);
            if (fireExtinguisher == null)
            {
                return true;
            }
            else
            {
                TweakUtils.Unapply(fireExtinguisher);

                ThingWithComps previousEq = previousEquipped.TryGetValue(pawn);
                if (previousEq != null)
                {
                    pawn.equipment.Remove(fireExtinguisher);
                    pawn.inventory.innerContainer.TryAdd(fireExtinguisher, true);

                    pawn.inventory.innerContainer.Remove(previousEq);
                    pawn.equipment.MakeRoomFor(previousEq);
                    pawn.equipment.AddEquipment(previousEq);
                }
                return true;
            }
        }
    }
}
