using Newtonsoft.Json;

namespace HighLoadCupV3.Model.Dto
{
    public class GroupResponseDto
    {
        [JsonProperty(Names.Count)]
        public int Count { get; set; }

        [JsonProperty(Names.City)]
        public string City { get; set; }

        [JsonProperty(Names.Country)]
        public string Country { get; set; }

        [JsonProperty(Names.Sex)]
        public string Sex { get; set; }

        [JsonProperty(Names.Status)]
        public string Status { get; set; }

        [JsonProperty(Names.Interests)]
        public string Interests { get; set; }
    }
}