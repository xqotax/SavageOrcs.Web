using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavageOrcs.BusinessObjects
{
    public class Area
    { 
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Region { get; set; }

        public string Community { get; set; }

        public string NameEng { get; set; }

        public string RegionEng { get; set; }

        public string CommunityEng { get; set; }

        public virtual ICollection<Mark> Marks { get; set; } = new HashSet<Mark>();
    }
}
