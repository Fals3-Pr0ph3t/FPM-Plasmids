using EBSGFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FPMPlasmids
{
    public class HediffCompProperties_PlasmidRegrowth : HediffCompProperties_Disappears
    {
        public int GeneMultiplier;
        public PlasmidDef plasmidDef;
        public HediffCompProperties_PlasmidRegrowth()
        {
            this.compClass = typeof(HediffCompProperties_PlasmidRegrowth);
        }

    }
    public class HediffComp_PlasmidRegrowth : HediffComp_Disappears
    {
        public new HediffCompProperties_PlasmidRegrowth Props
        {
            get
            {
                return (HediffCompProperties_PlasmidRegrowth)this.props;
            }
        }
        public override void CompPostMake()
        {
            base.CompPostMake();
            HediffComp_Disappears disappearsComp = this.parent.TryGetComp<HediffComp_Disappears>();
            if (disappearsComp != null)
            {


                int geneCount = Props.plasmidDef.geneSet.Count;

                int newTicks = (int)(geneCount * Props.GeneMultiplier);
                disappearsComp.disappearsAfterTicks = newTicks;


            }
        }
    }

}
