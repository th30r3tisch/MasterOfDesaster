
namespace Game_Server.EA.Models {
    abstract class Gene {

        public Gene ShallowCopy() {
            return (Gene)MemberwiseClone();
        }
    }
}
