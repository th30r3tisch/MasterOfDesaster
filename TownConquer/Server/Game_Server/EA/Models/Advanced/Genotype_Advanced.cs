using System;
using System.Collections.Generic;

namespace Game_Server.EA.Models.Advanced {
    class Genotype_Advanced : Gene {

        public Dictionary<string, int> supportProperties;
        public Dictionary<string, int> attackProperties;
        public Dictionary<string, int> defensiveProperties;

        public Genotype_Advanced(Dictionary<string, int> supportProperties, Dictionary<string, int> attackProperties, Dictionary<string, int> defensiveProperties) {
            this.supportProperties = supportProperties;
            this.attackProperties = attackProperties;
            this.defensiveProperties = defensiveProperties;
        }

        public static Dictionary<string, int> CreateProperties(List<int> propertyValues) {
            string[] propertyNames = Enum.GetNames(typeof(PropertyNames_Advanced));
            Dictionary<string, int> properties = new Dictionary<string, int>();
            for (int i = 0; i < propertyNames.Length; i++) {
                properties.Add(propertyNames[i], propertyValues[i]);
            }
            return properties;
        }
    }
}
