using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace FPMPlasmids
{
    public class HediffComp_PlasmidSeverityPerDay : HediffComp_SeverityPerDay
    {
        public new float severityPerDay
        { get; set; }

        public float severityFactor;
        public Plasmid Plasmid { get; set; }
    }

    public class HediffCompProperties_PlasmidSeverityPerDay : HediffCompProperties_SeverityPerDay
    {
        public float severityFactor;

        public HediffCompProperties_PlasmidSeverityPerDay()
        {
            this.compClass = typeof(HediffComp_PlasmidSeverityPerDay);
        }
    }
}