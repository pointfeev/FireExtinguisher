using System.Collections.Generic;
using Verse;

namespace FireExtinguisher
{
    public static class TweakUtils
    {
        // You can no longer tweak forcedMissRadius, so either VWE will fix their fire extinguisher or I'll have to find a new solution

        static Dictionary<string, float> defaultForcedMissRadius = new Dictionary<string, float>();

        public static void Apply(ThingWithComps fireExtinguisher)
        {/*
            if (fireExtinguisher == null) { return; }
            if (fireExtinguisher.def.defName == "VWE_Gun_FireExtinguisher") // a slight "fix" to the Vanilla Weapons Expanded fire extinguisher to work better with the mod
            {
                CompEquippable compEquippable = fireExtinguisher.GetComp<CompEquippable>();
                if (compEquippable != null)
                {
                    VerbProperties verbProperties = compEquippable.verbTracker.PrimaryVerb.verbProps;
                    if (!defaultForcedMissRadius.ContainsKey(fireExtinguisher.def.defName))
                    {
                        defaultForcedMissRadius[fireExtinguisher.def.defName] = verbProperties.forcedMissRadius;
                    }
                    verbProperties.forcedMissRadius = 0.9f; // otherwise it almost always misses the targeted fire, and using the extinguisher isn't even worth it

                    //Log.Warning("[FireExtinguisher] Applied tweaks.");
                }
            }
        */}

        public static void Unapply(ThingWithComps fireExtinguisher)
        {/*
            if (fireExtinguisher == null) { return; }
            if (!defaultForcedMissRadius.ContainsKey(fireExtinguisher.def.defName)) { return; }
            if (fireExtinguisher.def.defName == "VWE_Gun_FireExtinguisher")
            {
                CompEquippable compEquippable = fireExtinguisher.GetComp<CompEquippable>();
                if (compEquippable != null)
                {
                    VerbProperties verbProperties = compEquippable.verbTracker.PrimaryVerb.verbProps;
                    verbProperties.forcedMissRadius = defaultForcedMissRadius[fireExtinguisher.def.defName];

                    //Log.Warning("[FireExtinguisher] Unapplied tweaks.");
                }
            }
        */}
    }
}
