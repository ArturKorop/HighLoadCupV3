using Newtonsoft.Json;

namespace HighLoadCupV3.Model.Dto
{
    public class AccountsDto
    {
        [JsonProperty("accounts")]
        public AccountDto[] Accounts { get; set; }
    }
}
