using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace HighLoadCupV3.Model.Dto
{
    public class PremiumDto
    {
        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("finish")]
        public int Finish { get; set; }
    }
}
