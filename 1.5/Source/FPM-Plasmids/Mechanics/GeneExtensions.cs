using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace FPMPlasmids
{
    public class FPM_GeneExtension : DefModExtension
    {
        //public bool plasmidRepair = false;
        public bool plasmidImplanter = false;

        //public GeneDef plasmidChoice = null;
        public PlasmidInfo plasmidInfo = null;
    }

    public enum TransferType
    {
        Transfer,
        Copy,
        Apply,
    }

    public class PlasmidInfo
    {
        public bool xenogene = true;
        public List<GeneDef> addedGenes = [];
        public List<GeneDef> suppressedGenes = new List<GeneDef>();
        public bool singleCopy = false;
        public bool selfRepair = false;
        public int IntPerGene = 60000;
        public TransferType transferType = TransferType.Copy;
    }

    public static class GeneExtensionMethods
    {
        public static List<FPM_GeneExtension> GetActiveGeneExtensions(this Pawn_GeneTracker geneTracker)
        {
            var gExtensions = geneTracker?.GenesListForReading?
                .Select(gene => gene.def.GetModExtension<FPM_GeneExtension>())
                .Where(extension => extension != null)
                .ToList();
            return gExtensions ?? new List<FPM_GeneExtension>();
        }
    }

    public class Plasmid : Gene
    {
        private bool contained;

        public bool Contained
        {
            get { return contained; }
            set { contained = value; }
        }

        public override bool Active
        {
            get
            {
                if (Contained)
                { return false; }
                else
                { return base.Active; }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref contained, "Contained", defaultValue: false);
        }
    }
}