using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using System.Reflection.Emit;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Verse.AI;
using RimWorld.Planet;



namespace FPMPlasmids
{

    public class FPMPlasmids : Mod
    {
        public FPMPlasmids(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("com.fpmplasmids");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

}