using System.Collections.Generic;
using Newtonsoft.Json;

namespace HighLoadCupV3.Model.Dto
{
    public class RecommendAccountsResponseDto
    {
        [JsonProperty("accounts")]
        public IEnumerable<RecommendResponseDto> Accounts { get; set; }
    }
}