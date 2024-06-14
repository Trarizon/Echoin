using Newtonsoft.Json;
using System.Collections.Generic;

namespace Echoin.Models.InfoModels
{
    [JsonObject]
    public sealed class ChartInfoModel
    {
        [JsonProperty("diff")]
        public string Difficulty;
        [JsonProperty("level")]
        public int Level;

        [JsonIgnore]
        public ScoreInfoModel ScoreInfo;

        [JsonIgnore]
        public List<ChartSliceInfoModel> SliceInfos;
    }
}