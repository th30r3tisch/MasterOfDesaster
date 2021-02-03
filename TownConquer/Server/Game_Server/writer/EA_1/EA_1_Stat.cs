using System.Collections.Generic;
using System.Numerics;

namespace Game_Server.writer.EA_1 {
    class EA_1_Stat : StatEntry {

        public bool won { get; set; }
        public Vector3 startPos { get; set; }
        public double fitness { get; set; }
        public int score { get; set; }
        public int townLifeSum { get; set; }
        public int gameTime { get; set; }

        public List<int> timeStamps { get; set; }
        public List<int> townDevelopment { get; set; }
        public Dictionary<string, int> properties { get; set; }

        public EA_1_Stat(string name) : base(name) {
            timeStamps = new List<int>();
            townDevelopment = new List<int>();
        }

    }
}
