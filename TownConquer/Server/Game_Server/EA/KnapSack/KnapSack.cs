using System;
using System.Collections.Generic;

namespace Game_Server.EA.KnapSack {
    class KnapSack {
        public List<int> content;
        public int value;
        public int capasity;
        public readonly int maxCapasity = 40;

        /// <summary>
        /// A knapsack, representing one possible solution
        /// </summary>
        public KnapSack() {
            value = 0;
            capasity = 0;
            content = new List<int>();
        }

        /// <summary>
        /// Fills the backpack. The content is represented as a string of bytes. The index of each byte coresponds to one item in the itemlist. 
        /// The value of the byte shows whether the item exists in the knapsack.(1 yes, 0 no)
        /// </summary>
        /// <param name="itemNumber">number of all different items existing</param>
        /// <param name="r">Random number generator</param>
        public void CreateRandomContent(int itemNumber, Random r) {
            int i = 0;
            while (i < itemNumber) {
                content.Add(r.Next(0, 2));
                i++;
            }
        }
    }
}
