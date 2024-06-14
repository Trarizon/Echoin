using Newtonsoft.Json;

namespace Echoin.Models.InfoModels
{
    [JsonObject]
    public sealed class ScoreInfoModel
    {
        [JsonProperty("score")]
        public float Score;
        [JsonProperty("rank")]
        public Rank ScoreRank;

        public enum Rank
        {
            None,
            FullCombo,
            AllPerfect,
        }
    }

    public static class ScoreRankExtensions
    {
        public static string ToDisplayString(this ScoreInfoModel.Rank rank)
        {
            return rank switch {
                ScoreInfoModel.Rank.None => "",
                ScoreInfoModel.Rank.FullCombo => "FC",
                ScoreInfoModel.Rank.AllPerfect => "AP",
                _ => null,
            };
        }
    }
}