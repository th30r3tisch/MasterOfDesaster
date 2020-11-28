using System;

namespace Game_Server.KI.KnapSack {
    class Item {
        public int value;
        public int weight;

        int maxWeight = 10;
        int maxValue = 20;

        public Item(Random r) {
            Create(r);
        }

        /// <summary>
        /// Initializes the weight and value of a new item with random values
        /// </summary>
        /// <param name="r">Random number generator</param>
        private void Create(Random r) {
            value = r.Next(1, maxValue);
            weight = r.Next(1, maxWeight);
        }
    }
}
