using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace FireExtinguisher;

internal static class InventoryUtils
{
    private static readonly List<string> ExtinguishDamageDefNames = new() { "VWE_Extinguish", "FExtExtinguish" };

    private static readonly Dictionary<int, ThingWithComps> PreviousWeapons = new();

    internal static bool CanWeaponExtinguish(ThingWithComps weapon)
        => GetPrimaryVerb(weapon)?.GetDamageDef() is { } damageDef
        && (damageDef == DamageDefOf.Extinguish || ExtinguishDamageDefNames.Contains(damageDef.defName));

    internal static Verb GetPrimaryVerb(ThingWithComps weapon) => weapon?.GetComp<CompEquippable>()?.PrimaryVerb;

    internal static ThingWithComps GetFireExtinguisherFromEquipment(Pawn pawn)
        => pawn?.equipment?.Primary is { } primary && CanWeaponExtinguish(primary) && ModCompatibility.CheckWeapon(primary) ? primary : null;

    internal static ThingWithComps GetFireExtinguisherFromInventory(Pawn pawn)
        => pawn?.inventory?.innerContainer?.FirstOrFallback(thing
            => thing is ThingWithComps withComps && CanWeaponExtinguish(withComps) && ModCompatibility.CheckWeapon(withComps)) as ThingWithComps;

    private static bool UnEquipWeapon(Pawn pawn, bool cachePrevious = false)
    {
        if (pawn is null)
            return false;
        ThingWithComps primary = pawn.equipment.Primary;
        if (primary is null)
            return true;
        bool success = pawn.inventory.innerContainer.TryAddOrTransfer(primary);
        if (success)
        {
            if (cachePrevious)
                PreviousWeapons.SetOrAdd(pawn.thingIDNumber, primary);
        }
        else
        {
            Log.Error($"[FireExtinguisher] Failed to un-equip weapon for {pawn.LabelShort}: {primary.Label}");
            return false;
        }
        return true;
    }

    private static bool EquipWeapon(Pawn pawn, ThingWithComps weapon, bool cachePrevious = false)
    {
        if (pawn is null || weapon is null || !UnEquipWeapon(pawn, cachePrevious))
            return false;
        if (weapon.stackCount > 1)
            weapon = weapon.SplitOff(1) as ThingWithComps;
        if (weapon?.holdingOwner != null)
            _ = weapon.holdingOwner.Remove(weapon);
        pawn.equipment.AddEquipment(weapon);
        weapon?.def?.soundInteract?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
        return true;
    }

    private static bool CanEquipFireExtinguisher(Pawn pawn, Thing extinguisher)
        => pawn != null && extinguisher?.def != null && !pawn.WorkTagIsDisabled(WorkTags.Firefighting)
        && (!pawn.WorkTagIsDisabled(WorkTags.Violent) || !extinguisher.def.IsRangedWeapon);

    internal static bool CanEquipFireExtinguisher(Pawn pawn)
        => GetFireExtinguisherFromEquipment(pawn) is not null || CanEquipFireExtinguisher(pawn, GetFireExtinguisherFromInventory(pawn));

    internal static bool EquipFireExtinguisher(Pawn pawn, bool cachePrevious = true)
        => GetFireExtinguisherFromEquipment(pawn) is not null || EquipWeapon(pawn, GetFireExtinguisherFromInventory(pawn), cachePrevious);

    internal static Toil EquipFireExtinguisherToil()
    {
        Toil toil = new();
        toil.initAction = delegate
        {
            Pawn pawn = toil.actor;
            Pawn_JobTracker jobTracker = pawn.jobs;
            if (GetFireExtinguisherFromEquipment(pawn) == null)
                _ = EquipFireExtinguisher(pawn, !PreviousWeapons.ContainsKey(pawn.thingIDNumber));
            if (GetFireExtinguisherFromEquipment(pawn) == null)
                jobTracker.EndCurrentJob(JobCondition.Incompletable);
            jobTracker.curDriver.ReadyForNextToil();
        };
        return toil;
    }

    internal static bool UnEquipFireExtinguisher(Pawn pawn)
    {
        ThingWithComps fireExtinguisher = GetFireExtinguisherFromEquipment(pawn);
        if (fireExtinguisher is null)
            return true;
        if (!PreviousWeapons.TryGetValue(pawn.thingIDNumber, out ThingWithComps previousWeapon))
            return true;
        if (previousWeapon == pawn.equipment?.Primary)
            _ = PreviousWeapons.Remove(pawn.thingIDNumber);
        else
            return UnEquipWeapon(pawn) && EquipWeapon(pawn, previousWeapon) && PreviousWeapons.Remove(pawn.thingIDNumber);
        return true;
    }
}