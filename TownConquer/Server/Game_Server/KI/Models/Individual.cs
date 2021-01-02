using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Server.KI.Models {
    class Individual {
        public Genotype gene;
        public Result result;

        public int number;

        public Individual(int _number, Genotype _gene) {
            result = new Result();
            number = _number;
            gene = _gene;
        }
    }
}
