﻿using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;
using Verse.AI;
using System.Text;
using System.Linq;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;

namespace CompTurret.HarmonyInstance 
{
    
    [HarmonyPatch(typeof(Apparel), "CheckPreAbsorbDamage")]
    public static class Apparel_CheckPreAbsorbDamage_CompTurret_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Apparel __instance, DamageInfo dinfo, ref bool __result)
        {
            if (__instance == null)
            {
                return;
            }
            CompTurret apparel_Turret = __instance.TryGetComp<CompTurret>();
            if (apparel_Turret!=null)
            {
                if (dinfo.Def == DamageDefOf.EMP)
                {
                    foreach (ThingComp item in __instance.AllComps)
                    {
                        CompTurretGun Turret = item as CompTurretGun;
                        if (Turret != null && Turret.AffectedByEMP)
                        {
                            Turret.stunTicksLeft += Mathf.RoundToInt(dinfo.Amount * 30f);
                            Turret.stunFromEMP = true;
                        //    Log.Message(Turret.gun.Label + " turret hit by EMP, disabling for " + Mathf.RoundToInt(dinfo.Amount * 30f) + " ticks, Total: "+ Turret.stunTicksLeft);
                        }
                    }
                }
            }
        }
    }
    
}