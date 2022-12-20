using RimWorld;
using Verse;
using Verse.AI;

namespace FireExtinguisher
{
    public class WorkGiver_ExtinguishFire : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.Fire);

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Fire fire))
                return false;
            if (fire.parent is Pawn attachedPawn)
            {
                if (attachedPawn == pawn)
                    return false;
                if ((attachedPawn.Faction == pawn.Faction || attachedPawn.HostFaction == pawn.Faction
                                                          || attachedPawn.HostFaction == pawn.HostFaction)
                    /*&& !pawn.Map.areaManager.Home[fire.Position]*/
                 && IntVec3Utility.ManhattanDistanceFlat(pawn.Position, attachedPawn.Position) > 15)
                    return false;
            }
            else if (!pawn.Map.areaManager.Home[fire.Position])
            {
                JobFailReason.Is(WorkGiver_FixBrokenDownBuilding.NotInHomeAreaTrans);
                return false;
            }
            return InventoryUtils.CanEquipFireExtinguisher(pawn)
                && ((pawn.Position - fire.Position).LengthHorizontalSquared <= 225
                 || pawn.CanReserve(fire, 1, -1, null, forced))
                && !FireIsBeingHandled(fire, pawn)
                && CastUtils.CanGotoCastPosition(pawn, fire, out _, true)
                && InventoryUtils.EquipFireExtinguisher(pawn);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
            => JobMaker.MakeJob(JobDefOf_ExtinguishFire.ExtinguishFire, t);

        private static bool FireIsBeingHandled(Fire f, Pawn potentialHandler) => f.Spawned
         && f.Map.reservationManager.FirstRespectedReserver(f, potentialHandler) != null;
    }
}