using System.Collections.Generic;

namespace HighLoadCupV3.Model.Filters.Group
{
    public class GroupQuery
    {
        public GroupKey Key { get; set; }
        public Dictionary<string, string> Filter { get; set; }
        public int Limit { get; set; }
        public int Order { get; set; }
    }
}