using System.Collections.Generic;
using HighLoadCupV3.Model.Dto;
using Newtonsoft.Json;

namespace HighLoadCupV3.Model.Filters.Group
{
    public class GroupHolder
    {
        [JsonProperty("groups")]
        public IEnumerable<GroupResponseDto> Groups { get; set; }
    }
}