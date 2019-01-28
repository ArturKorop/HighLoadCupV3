using HighLoadCupV3.Model.InMemory;
using Newtonsoft.Json;

namespace HighLoadCupV3.Model.Dto
{
    public class SuggestResponseDto : IResetable
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("fname")]
        public string FName { get; set; }

        [JsonProperty("sname")]
        public string SName { get; set; }

        public void Reset()
        {
        }
    }
}