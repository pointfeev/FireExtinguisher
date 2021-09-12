using RimWorld;
using System.Collections.Generic;
using Verse.AI;

namespace FireExtinguisher
{
    public class JobDriver_ExtinguishFire : JobDriver
    {
        protected Fire TargetFire => (Fire)job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (Map.reservationManager.CanReserve(pawn, TargetFire))
            {
                return pawn.Reserve(TargetFire, job);
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
            Toil CheckDestroyed = new Toil
            {
                initAction = delegate ()
                {
                    if (TargetFire == null || TargetFire.Destroyed)
                    {
                        pawn.records.Increment(RecordDefOf.FiresExtinguished);
                        pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true, true);
                    }
                }
            };
            Toil Approach = CastUtils.GotoCastPosition(TargetIndex.A);
            Approach.JumpIfDespawnedOrNull(TargetIndex.A, CheckDestroyed);
            yield return Approach;
            Toil CastVerb = Toils_Combat.CastVerb(TargetIndex.A);
            CastVerb.JumpIfDespawnedOrNull(TargetIndex.A, CheckDestroyed);
            yield return CastVerb;
            yield return CheckDestroyed;
            yield return Toils_Jump.Jump(Approach);
            yield break;
        }
    }
}
