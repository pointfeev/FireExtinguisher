using System;
using Verse;
using Verse.AI;

namespace FireExtinguisher
{
    public static class CastUtils
    {
        public static bool CanGotoCastPosition(Pawn actor, Thing thing, out IntVec3 intVec, float maxRangeFactor, bool verbFromInventory)
        {
            intVec = new IntVec3();
            Verb verb = null;
            if (InventoryUtils.GetEquippedFireExtinguisher(actor) == null && verbFromInventory)
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
                {
                    verb = curJob.verbToUse;
                }
                if (verb == null)
                {
                    verb = actor.TryGetAttackVerb(thing);
                }
            }
            if (verb == null)
            {
                return false;
            }
            return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = actor,
                target = thing,
                verb = verb,
                maxRangeFromTarget = Math.Max(verb.verbProps.range * maxRangeFactor, 1.42f),
                wantCoverFromTarget = false
            }, out intVec);
        }

        public static Toil GotoCastPosition(TargetIndex targetInd, float maxRangeFactor = 1f)
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
                IntVec3 intVec;
                bool canGoto = CanGotoCastPosition(toil.actor, thing, out intVec, maxRangeFactor, false);
                if (!canGoto)
                {
                    //InventoryUtils.UnequipFireExtinguisher(toil.actor);
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