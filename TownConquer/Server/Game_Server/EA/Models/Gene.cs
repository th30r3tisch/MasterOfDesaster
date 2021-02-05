using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Server.EA.Models {
    abstract class Gene {

        public Gene ShallowCopy() {
            return (Gene)MemberwiseClone();
        }
    }
}
