﻿using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace FireExtinguisher
{
    public class JobDriver_ExtinguishFire : JobDriver
    {
        private const TargetIndex FireIndex = TargetIndex.A;
        private float maxRangeFactor = CastUtils.DefaultMaxRangeFactor;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        private Toil CheckDestroyedToil(TargetIndex targetIndex)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Fire fire = (Fire)pawn.CurJob.GetTarget(targetIndex);
                if (fire != null && !fire.Destroyed)
                {
                    maxRangeFactor -= 0.15f;
                    return;
                }
                pawn.records.Increment(RecordDefOf.FiresExtinguished);
                pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
            };
            return toil;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil equip = InventoryUtils.EquipFireExtinguisherToil();
            _ = equip.EndOnDespawnedOrNull(FireIndex);
            yield return Toils_Combat.TrySetJobToUseAttackVerb(FireIndex);
            Toil checkDestroyed = CheckDestroyedToil(FireIndex);
            Toil approach = CastUtils.GotoCastPosition(FireIndex, maxRangeFactor);
            _ = approach.JumpIfDespawnedOrNull(FireIndex, checkDestroyed);
            yield return approach;
            Toil castVerb = Toils_Combat.CastVerb(FireIndex);
            _ = castVerb.JumpIfDespawnedOrNull(FireIndex, checkDestroyed);
            yield return castVerb;
            yield return checkDestroyed;
            yield return Toils_Jump.Jump(equip);
        }
    }
}