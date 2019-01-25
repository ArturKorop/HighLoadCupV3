using HighLoadCupV3.Model;
using HighLoadCupV3.Model.Filters.Filter;
using HighLoadCupV3.Model.Filters.Group;
using HighLoadCupV3.Model.Filters.Recommend;
using HighLoadCupV3.Model.Filters.Suggest;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3
{
    public class Holder
    {
        public static Holder Instance { get; } = new Holder();

        public InMemoryRepository InMemory { get; set; }
        public RepositoryUpdater Updater { get; set; }

        public Filter Filter { get; set; }

        public Group Group { get; set; }
        public GroupQueryParser GroupQueryParser { get; set; }

        public Suggest Suggest { get; set; }
        public Recommend Recommend { get; set; }

        public int CurrentTimeStamp { get; set; }

        private Holder()
        {
        }
    }
}
