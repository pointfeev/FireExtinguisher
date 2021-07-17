using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace FireExtinguisher
{
    public class JobDriver_ExtinguishFire : JobDriver
    {
		protected Fire TargetFire
		{
			get
			{
				return (Fire)this.job.targetA.Thing;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (this.Map.reservationManager.CanReserve(this.pawn, this.TargetFire))
			{
				return this.pawn.Reserve(this.TargetFire, this.job);
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
        {
			yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
			Toil CheckDestroyed = new Toil
			{
				initAction = delegate()
				{
					if (this.TargetFire == null || this.TargetFire.Destroyed)
					{
						this.pawn.records.Increment(RecordDefOf.FiresExtinguished);
						//InventoryUtils.UnequipFireExtinguisher(pawn);
						this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true, true);
					}
				}
			};
			Toil Approach = CastUtils.GotoCastPosition(TargetIndex.A, 0.95f);
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
