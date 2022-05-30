using System;

using Verse;
using Verse.AI;

namespace FireExtinguisher
{
    public static class CastUtils
    {
        private static Thing lastFire = null;

        private static readonly float defaultMaxRangeFactor = 0.95f;
        private static float maxRangeFactor = defaultMaxRangeFactor;

        public static bool CanGotoCastPosition(Pawn actor, Thing thing, out IntVec3 intVec, bool fromWorkGiver)
        {
            intVec = new IntVec3();
            Verb verb = null;
            if (InventoryUtils.GetFireExtinguisherFromEquipment(actor) == null && fromWorkGiver)
            {
                ThingWithComps fireExtinguisher = InventoryUtils.GetFireExtinguisherFromInventory(actor);
                if (fireExtinguisher != null)
                {
                    CompEquippable compEquippable = fireExtinguisher.GetComp<CompEquippable>();
                    if (compEquippable != null)
                    {
                        verb = compEquippable.verbTracker.PrimaryVerb;
                        verb.caster = actor;
                    }
                }
            }
            else
            {
                Job curJob = actor.jobs.curJob;
                if (curJob != null)
                    verb = curJob.verbToUse;
                if (verb == null)
                    verb = actor.TryGetAttackVerb(thing);
            }
            if (verb == null)
                return false;
            if (fromWorkGiver)
                maxRangeFactor = defaultMaxRangeFactor;
            else if (lastFire == thing)
                maxRangeFactor -= 0.15f;
            lastFire = thing;
            return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = actor,
                target = thing,
                verb = verb,
                maxRangeFromTarget = Math.Max(verb.verbProps.range * maxRangeFactor, 1.42f),
                wantCoverFromTarget = false
            }, out intVec);
        }

        public static Toil GotoCastPosition(TargetIndex targetInd)
        {
            Toil toil = new Toil();
            toil.initAction = delegate ()
            {
                Thing thing = toil.actor.jobs.curJob.GetTarget(targetInd).Thing;
                if (toil.actor == thing)
                {
                    toil.actor.pather.StopDead();
                    toil.actor.jobs.curDriver.ReadyForNextToil();
                    return;
                }
                if (thing == null)
                {
                    toil.actor.pather.StopDead();
                    toil.actor.jobs.curDriver.ReadyForNextToil();
                    return;
                }
                bool canGoto = CanGotoCastPosition(toil.actor, thing, out IntVec3 intVec, false);
                if (!canGoto)
                {
                    toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable, true, true);
                    return;
                }
                toil.actor.pather.StartPath(intVec, PathEndMode.OnCell);
                toil.actor.Map.pawnDestinationReservationManager.Reserve(toil.actor, toil.actor.jobs.curJob, intVec);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }
    }
}