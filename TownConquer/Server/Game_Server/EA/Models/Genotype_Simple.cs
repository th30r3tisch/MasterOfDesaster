using System.Collections.Generic;

namespace Game_Server.EA.Models {
    class Genotype_Simple: Gene {

        public Dictionary<string, int> properties;

        public Genotype_Simple(int initialConquerRadius, int maxConquerRadius, int radiusExpansionStep, int attackMinLife, int supportRadius, int supportMaxCap, int supportMinCap, int supportTownRatio) {
            properties = new Dictionary<string, int>() {
                    { "initialConquerRadius", initialConquerRadius },
                    { "maxConquerRadius", maxConquerRadius },
                    { "radiusExpansionStep", radiusExpansionStep },
                    { "attackMinLife", attackMinLife },
                    { "supportRadius", supportRadius },
                    { "supportMaxCap", supportMaxCap },
                    { "supportMinCap", supportMinCap },
                    { "supportTownRatio", supportTownRatio }
            };
        }
    }
}