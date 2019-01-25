using Newtonsoft.Json;

namespace HighLoadCupV3.Model.Dto
{
    public class AccountDto
    {
        public AccountDto()
        {
        }

        [JsonRequired]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonRequired]
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("fname")]
        public string FName { get; set; }

        [JsonProperty("sname")]
        public string SName { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonRequired]
        [JsonProperty("sex")]
        public string Sex { get; set; }

        [JsonRequired]
        [JsonProperty("birth")]
        public int Birth { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonRequired]
        [JsonProperty("joined")]
        public int Joined { get; set; }

        [JsonRequired]
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("interests")]
        public string[] Interests { get; set; }

        [JsonProperty("premium")]
        public PremiumDto Premium { get; set; }

        [JsonProperty("likes")]
        public LikeDto[] Likes { get; set; }
    }
}
