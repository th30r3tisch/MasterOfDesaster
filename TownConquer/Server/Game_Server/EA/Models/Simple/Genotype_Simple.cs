using System;
using System.Collections.Generic;

namespace Game_Server.EA.Models.Simple {
    class Genotype_Simple: Gene {

        public Dictionary<string, int> properties;

        /// <summary>
        /// creates property pairs for the gene consisting of the name and the value of the property
        /// </summary>
        /// <param name="propertyValues">list of values for the properties</param>
        public Genotype_Simple(List<int> propertyValues) {
            string[] propertyNames = Enum.GetNames(typeof(PropertyNames_Simple)); 
            properties = new Dictionary<string, int>();
            for (int i = 0; i < propertyNames.Length; i++) {
                properties.Add(propertyNames[i], propertyValues[i]);
            }
        }
    }
}