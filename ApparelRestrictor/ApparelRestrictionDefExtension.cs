﻿using Verse;
using RimWorld;
using System.Collections.Generic;

namespace ApparelRestrictor
{
    // Token: 0x02000020 RID: 32
    public class ApparelRestrictionDefExtension : DefModExtension
    {
        public bool Any = false;
        public List<ThingDef> RaceDefs = new List<ThingDef>();
        public List<ThingDef> ApparelDefs = new List<ThingDef>();
        public List<HediffDef> HediffDefs = new List<HediffDef>();
        public List<TraitDef> TraitDefs = new List<TraitDef>();
        public Gender gender = Gender.None;
        public BodyTypeDef forcedBodytype = null;

    }
}
