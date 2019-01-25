using System.Collections.Generic;
using Newtonsoft.Json;

namespace HighLoadCupV3.Model.Dto
{
    public class SuggestAccountsResponseDto
    {
        [JsonProperty("accounts")]
        public IEnumerable<SuggestResponseDto> Accounts { get; set; }
    }
}