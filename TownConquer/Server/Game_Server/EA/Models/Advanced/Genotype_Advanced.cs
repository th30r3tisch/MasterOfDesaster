using System;
using System.Collections.Generic;

namespace Game_Server.EA.Models.Advanced {
    class Genotype_Advanced : Gene {

        public Dictionary<string, int> supportProperties;
        public Dictionary<string, int> attackProperties;
        public Dictionary<string, int> defensiveProperties;
        public Dictionary<string, int> generalProperties;
        
        public Genotype_Advanced(Dictionary<string, int> supportProperties, Dictionary<string, int> attackProperties, Dictionary<string, int> defensiveProperties, Dictionary<string, int> generalProperties) {
            this.supportProperties = supportProperties;
            this.attackProperties = attackProperties;
            this.defensiveProperties = defensiveProperties;
            this.generalProperties = generalProperties;
        }

        /// <summary>
        /// little trick from https://www.codeproject.com/Tips/244647/Passing-Enum-type-as-a-parameter to pass enum type
        /// </summary>
        /// <typeparam name="T">type of the enum</typeparam>
        /// <param name="propertyValues">list of values for the properties</param>
        /// <returns>dict combining the enum names and values</returns>
        public static Dictionary<string, int> CreateProperties<T>(List<int> propertyValues) {
            string[] propertyNames = Enum.GetNames(typeof(T));
            Dictionary<string, int> properties = new Dictionary<string, int>();
            for (int i = 0; i < propertyNames.Length; i++) {
                properties.Add(propertyNames[i], propertyValues[i]);
            }
            return properties;
        }
    }
}
