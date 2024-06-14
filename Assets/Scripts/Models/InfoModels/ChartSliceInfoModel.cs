using Newtonsoft.Json;

namespace Echoin.Models.InfoModels
{
    [JsonObject]
    public sealed class ChartSliceInfoModel
    {
        public ChartSliceInfoModel(int startIndex, int endIndex, float startTime, float endTime)
        {
            StartIndex = startIndex;
            StartTime = startTime;
            EndIndex = endIndex;
            EndTime = endTime;
        }

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("start")]
        public int StartIndex;
        [JsonProperty("startTime")]
        public float StartTime;

        [JsonProperty("end")]
        public int EndIndex;
        [JsonProperty("endTime")]
        public float EndTime;
    }
}