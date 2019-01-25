using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HighLoadCupV3.Model.Dto;

namespace HighLoadCupV3.Model.InMemory
{
    public interface ILikesStorage
    {
        void AddFromFile(int id, LikeDto[] likes);
        void AddToBuffer(int id, LikeDto[] likes);
        void UpdateBuffer(LikeUpdateDto dto);
        void UpdateBuffer(LikeDto dto, int id);
        bool ContainsLikesFrom(int id);
        bool ContainsLikesTo(int id);
        void FillReverse(Stopwatch sw);
        void Flush();

        List<int> GetFrom(int id);
        Task<List<int>> GetFromAsync(int id);
        List<List<LikeDto>> GetTo(IEnumerable<int> ids);
        List<LikeDto> GetTo(int id);
        Task<List<LikeDto>> GetToAsync(int id);
        Task<List<List<LikeDto>>> GetToAsync(IEnumerable<int> ids);
    }
}