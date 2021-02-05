using System.Collections.Generic;

namespace Game_Server.EA.Models {
    class Genotype_Advanced : Gene {

        public Dictionary<string, int> supportProperties;
        public Dictionary<string, int> attackProperties;
        public Dictionary<string, int> defensiveProperties;

        public Genotype_Advanced(Dictionary<string, int> supportProperties, Dictionary<string, int> attackProperties, Dictionary<string, int> defensiveProperties) {
            this.supportProperties = supportProperties;
            this.attackProperties = attackProperties;
            this.defensiveProperties = defensiveProperties;
        }

        public Dictionary<string, int> CreateProperties(int initialConquerRadius, int maxConquerRadius, int radiusExpansionStep, int attackMinLife, int supportRadius, int supportMaxCap, int supportMinCap, int supportTownRatio) {
            Dictionary<string, int> properties = new Dictionary<string, int>() {
                    { "initialConquerRadius", initialConquerRadius },
                    { "maxConquerRadius", maxConquerRadius },
                    { "radiusExpansionStep", radiusExpansionStep },
                    { "attackMinLife", attackMinLife },
                    { "supportRadius", supportRadius },
                    { "supportMaxCap", supportMaxCap },
                    { "supportMinCap", supportMinCap },
                    { "supportTownRatio", supportTownRatio }
            };
            return properties;
        }
    }
}
