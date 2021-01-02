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
        public string name;

        public Individual(int _number, string _name, Genotype _gene) {
            result = new Result();
            number = _number;
            gene = _gene;
            name = _name;
        }
    }
}
