﻿using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FireExtinguisher
{
    internal static class CastUtils
    {
        private const float DefaultMaxRangeFactor = 0.95f;
        internal static readonly Dictionary<int, Thing> LastThing = new Dictionary<int, Thing>();
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
            else if (LastThing.TryGetValue(actor.thingIDNumber, out Thing last) && last == thing)
                maxRangeFactor -= 0.15f;
            LastThing.SetOrAdd(actor.thingIDNumber, (Fire)thing);
            return CastPositionFinder.TryFindCastPosition(
                new CastPositionRequest
                {
                    caster = actor, target = thing, verb = verb, maxRangeFromTarget = Math.Max(verb.verbProps.range * maxRangeFactor, 1.42f),
                    preferredCastPosition = actor.Position, wantCoverFromTarget = false
                }, out intVec);
        }

        internal static Toil GotoCastPosition(TargetIndex targetInd)
        {
            Toil toil = ToilMaker.MakeToil();
            toil.initAction = delegate
            {
                Pawn pawn = toil.actor;
                Pawn_JobTracker jobTracker = pawn.jobs;
                Pawn_PathFollower pather = pawn.pather;
                Thing thing = jobTracker.curJob.GetTarget(targetInd).Thing;
                if (pawn == thing || thing == null)
                {
                    pather.StopDead();
                    jobTracker.curDriver.ReadyForNextToil();
                    return;
                }
                bool canGoto = CanGotoCastPosition(pawn, thing, out IntVec3 intVec, false);
                if (!canGoto)
                {
                    jobTracker.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }
                pather.StartPath(intVec, PathEndMode.OnCell);
                pawn.Map.pawnDestinationReservationManager.Reserve(pawn, jobTracker.curJob, intVec);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }
    }
}