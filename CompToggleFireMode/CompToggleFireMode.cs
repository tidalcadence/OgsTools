﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CompToggleFireMode
{
    public class CompProperties_ToggleFireMode : CompProperties
    {
        public CompProperties_ToggleFireMode()
        {
            this.compClass = typeof(CompToggleFireMode);
        }
        public ResearchProjectDef requiredResearch;
        public bool canSwitchWhileBusy = false;
        public bool switchStartsCooldown = false;
    }

    public class CompToggleFireMode : ThingComp
    {
        public CompProperties_ToggleFireMode Props => (CompProperties_ToggleFireMode)props;

        protected virtual Pawn GetUser
        {
            get
            {
                if (ParentHolder != null && ParentHolder is Pawn_EquipmentTracker)
                {
                    return (Pawn)ParentHolder.ParentHolder;
                }
                else
                {
                    return null;
                }
            }
        }

        protected virtual bool IsHeld => (GetUser != null);
        public CompEquippable equippable;
        public CompEquippable Equippable => equippable ??= parent.TryGetComp<CompEquippable>();
        public Pawn lastUser;
        public bool GizmosOnEquip = true;
        public int fireMode = 0;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.fireMode, "fireMode", 0);
        }
        public void SwitchFireMode(int x)
        {
            fireMode = x;
            if (Props.switchStartsCooldown)
            {
                this.GetUser.stances.SetStance(new Stance_Cooldown(this.Active.AdjustedCooldownTicks(this.Equippable.PrimaryVerb, this.GetUser), null, this.Equippable.PrimaryVerb));
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (GetUser != lastUser)
            {
                lastUser = GetUser;
            }
        }
        public VerbProperties Active
        {
            get
            {
                if (parent != null && parent is ThingWithComps)
                {
                    return parent.def.Verbs[fireMode];
                }
                else
                {
                    return null;
                }
            }
        }

        public FloatMenu MakeModeMenu()
        {
            List<FloatMenuOption> floatMenu = new List<FloatMenuOption>();
            foreach (VerbProperties item in parent.def.Verbs)
            {
                if (fireMode != parent.def.Verbs.IndexOf(item))
                {
                    floatMenu.Add(new FloatMenuOption(item.label, delegate ()
                    {
                        this.SwitchFireMode(parent.def.Verbs.IndexOf(item));
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
            }

            return new FloatMenu(floatMenu);
        }
        public virtual IEnumerable<Gizmo> EquippedGizmos()
        {
            ThingWithComps owner = IsHeld ? GetUser : parent;
            bool flag = Find.Selector.SingleSelectedThing == GetUser;
            if (flag && GetUser.Drafted)
            {
                int num = 700000101;
                Command_Action command_Action = new Command_Action()
                {
                    icon = Active.defaultProjectile.uiIcon,
                    defaultLabel = "Firemode: " + Active.label,
                    defaultDesc = "Switch mode.",
                    hotKey = KeyBindingDefOf.Misc10,
                    activateSound = SoundDefOf.Click,
                    action = delegate ()
                    {
                        Find.WindowStack.Add(MakeModeMenu());
                    },
                    groupKey = num + 1
                };
                if (GetUser.Faction != Faction.OfPlayer)
                {
                    command_Action.Disable("CannotOrderNonControlled".Translate());
                }
                else if (GetUser.stances.curStance.StanceBusy && !Props.canSwitchWhileBusy)
                {
                    command_Action.Disable(GetUser.Name + " can't switch now (Busy)");
                }
                yield return command_Action;
            }
            yield break;
        }

    }
}
