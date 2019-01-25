using Newtonsoft.Json;

namespace HighLoadCupV3.Model.Dto
{
    public class LikeDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("ts")]
        public int TimeStamp { get; set; }
    }

    public class LikeUpdateDto
    {
        [JsonProperty("likee")]
        public int Likee { get; set; }

        [JsonProperty("ts")]
        public int TimeStamp { get; set; }

        [JsonProperty("liker")]
        public int Liker { get; set; }
    }

    public class LikesUpdateDto
    {
        [JsonProperty("likes")]
        public LikeUpdateDto[] Likes { get; set; }
    }
}
