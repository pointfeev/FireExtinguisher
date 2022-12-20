using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace FireExtinguisher
{
    public class JobDriver_ExtinguishFire : JobDriver
    {
        private Fire TargetFire => (Fire)job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
            => Map.reservationManager.CanReserve(pawn, TargetFire) && pawn.Reserve(TargetFire, job);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
            Toil checkDestroyed = new Toil
            {
                initAction = delegate
                {
                    if (TargetFire != null && !TargetFire.Destroyed)
                        return;
                    pawn.records.Increment(RecordDefOf.FiresExtinguished);
                    pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
            };
            Toil approach = CastUtils.GotoCastPosition(TargetIndex.A);
            _ = approach.JumpIfDespawnedOrNull(TargetIndex.A, checkDestroyed);
            yield return approach;
            Toil castVerb = Toils_Combat.CastVerb(TargetIndex.A);
            _ = castVerb.JumpIfDespawnedOrNull(TargetIndex.A, checkDestroyed);
            yield return castVerb;
            yield return checkDestroyed;
            yield return Toils_Jump.Jump(approach);
        }
    }
}