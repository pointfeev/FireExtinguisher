using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FireExtinguisher
{
    public class JobDriver_ExtinguishFire : JobDriver
    {
        private const TargetIndex FireIndex = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Fire fire = (Fire)job.GetTarget(FireIndex);
            return fire != null && Map.reservationManager.CanReserve(pawn, fire) && pawn.Reserve(fire, job);
        }

        private Toil CheckDestroyedToil(TargetIndex targetIndex)
        {
            Toil toil = ToilMaker.MakeToil();
            toil.initAction = delegate
            {
                Fire fire = (Fire)pawn.CurJob.GetTarget(targetIndex);
                if (fire != null && !fire.Destroyed)
                    return;
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
            Toil approach = CastUtils.GotoCastPosition(FireIndex);
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