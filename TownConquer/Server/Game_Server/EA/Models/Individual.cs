using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Server.EA.Models {
    abstract class Individual<T> where T: Gene{
        public T gene;
        public int number;
        public string name;

        public Individual(T gene, int number) {
            this.number = number;
            this.gene = gene;
        }

        public Individual<T> DeepCopy() {
            Individual<T> other = (Individual<T>)MemberwiseClone();
            other.gene = (T)gene.ShallowCopy();
            return other;
        }
    }
}
