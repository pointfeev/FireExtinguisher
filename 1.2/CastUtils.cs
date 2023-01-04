﻿using System;
using Verse;
using Verse.AI;

namespace FireExtinguisher
{
    internal static class CastUtils
    {
        private const float DefaultMaxRangeFactor = 0.95f;
        private static Thing lastFire;
        private static float maxRangeFactor = DefaultMaxRangeFactor;

        internal static bool CanGotoCastPosition(Pawn actor, Thing thing, out IntVec3 intVec, bool fromWorkGiver)
        {
            intVec = new IntVec3();
            Verb verb = null;
            if (InventoryUtils.GetFireExtinguisherFromEquipment(actor) == null && fromWorkGiver)
            {
                ThingWithComps fireExtinguisher = InventoryUtils.GetFireExtinguisherFromInventory(actor);
                if (fireExtinguisher != null)
                {
                    verb = InventoryUtils.GetPrimaryVerb(fireExtinguisher);
                    if (verb != null)
                        verb.caster = actor;
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
                maxRangeFactor = DefaultMaxRangeFactor;
            else if (lastFire == thing)
                maxRangeFactor -= 0.15f;
            lastFire = thing;
            return CastPositionFinder.TryFindCastPosition(
                new CastPositionRequest
                {
                    caster = actor, target = thing, verb = verb, maxRangeFromTarget = Math.Max(verb.verbProps.range * maxRangeFactor, 1.42f),
                    wantCoverFromTarget = false
                }, out intVec);
        }

        internal static Toil GotoCastPosition(TargetIndex targetInd)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Thing thing = toil.actor.jobs.curJob.GetTarget(targetInd).Thing;
                if (toil.actor == thing || thing == null)
                {
                    toil.actor.pather.StopDead();
                    toil.actor.jobs.curDriver.ReadyForNextToil();
                    return;
                }
                bool canGoto = CanGotoCastPosition(toil.actor, thing, out IntVec3 intVec, false);
                if (!canGoto)
                {
                    toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
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