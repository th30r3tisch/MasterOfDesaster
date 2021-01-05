using System.Collections.Generic;

namespace Game_Server.EA.Models {
    class Result {
        public List<int> townNumberDevelopment;
        public List<int> timestamp;

        public Result() {
            townNumberDevelopment = new List<int>();
            timestamp = new List<int>();
        }
    }
}
