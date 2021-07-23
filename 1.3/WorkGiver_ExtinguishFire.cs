using RimWorld;
using Verse;
using Verse.AI;

namespace FireExtinguisher
{
    internal class WorkGiver_ExtinguishFire : WorkGiver_Scanner
    {
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(ThingDefOf.Fire);
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Fire fire = t as Fire;
			if (fire == null)
			{
				return false;
			}
			Pawn pawn2 = fire.parent as Pawn;
			if (pawn2 != null)
			{
				if (pawn2 == pawn)
				{
					return false;
				}
				if ((pawn2.Faction == pawn.Faction || pawn2.HostFaction == pawn.Faction || pawn2.HostFaction == pawn.HostFaction) /*&& !pawn.Map.areaManager.Home[fire.Position]*/ && IntVec3Utility.ManhattanDistanceFlat(pawn.Position, pawn2.Position) > 15)
				{
					return false;
				}
			}
			else
			{
				if (!pawn.Map.areaManager.Home[fire.Position])
				{
					JobFailReason.Is(WorkGiver_FixBrokenDownBuilding.NotInHomeAreaTrans, null);
					return false;
				}
			}
			if (pawn.WorkTagIsDisabled(WorkTags.Firefighting) || pawn.WorkTagIsDisabled(WorkTags.Violent))
			{
				return false;
			}
			if ((pawn.Position - fire.Position).LengthHorizontalSquared > 225 && !pawn.CanReserve(fire, 1, -1, null, forced) || FireIsBeingHandled(fire, pawn))
            {
				return false;
            }
            if (!CastUtils.CanGotoCastPosition(pawn, fire, out _, 0.95f, true) || !InventoryUtils.EquipFireExtinguisher(pawn, true))
            {
				return false;
            }
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			//Log.Warning("[FireExtinguisher] (WorkGiver) Started a job for pawn: " + pawn.Name);
			return JobMaker.MakeJob(JobDefOf_ExtinguishFire.ExtinguishFire, t);
		}

		public static bool FireIsBeingHandled(Fire f, Pawn potentialHandler)
		{
			if (!f.Spawned)
			{
				return false;
			}
			Pawn pawn = f.Map.reservationManager.FirstRespectedReserver(f, potentialHandler);
			return pawn != null;
		}
	}
}
