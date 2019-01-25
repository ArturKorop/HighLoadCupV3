using System.Collections.Generic;
using HighLoadCupV3.Model.Dto;

namespace HighLoadCupV3.Model.InMemory
{
    public class DescLikeDtoComparer : IComparer<LikeDto>
    {
        public int Compare(LikeDto x, LikeDto y)
        {
            return y.Id - x.Id;
        }
    }
}