using System.Collections.Generic;

namespace Game_Server.EA.Models {
    class Genotype {
        public Dictionary<string, int> properties;

        public Genotype ShallowCopy() {
            return (Genotype)MemberwiseClone();
        }
    }
}