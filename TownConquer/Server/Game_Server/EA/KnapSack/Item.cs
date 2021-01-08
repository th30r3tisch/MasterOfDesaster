using System;

namespace Game_Server.EA.KnapSack {
    class Item {
        public int value;
        public int weight;
        private readonly int _maxWeight = 10;
        private readonly int _maxValue = 20;

        public Item(Random r) {
            Create(r);
        }

        /// <summary>
        /// Initializes the weight and value of a new item with random values
        /// </summary>
        /// <param name="r">Random number generator</param>
        private void Create(Random r) {
            value = r.Next(1, _maxValue);
            weight = r.Next(1, _maxWeight);
        }
    }
}
