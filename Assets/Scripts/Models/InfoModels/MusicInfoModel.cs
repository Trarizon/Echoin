using Echoin.Scene;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Echoin.Models.InfoModels
{
    [JsonObject]
    public sealed class MusicInfoModel
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("composer")]
        public string Composer;
        [JsonProperty("metaName")]
        public string MetaName;

        [JsonProperty("charts")]
        public List<ChartInfoModel> ChartInfos;

        [JsonIgnore]
        private AudioClip _musicClip;
        public AudioClip MusicClip
        {
            get {
                if (_musicClip == null) {
                    _musicClip = GlobalSettings.LoadMusic(this);
                }
                return _musicClip;
            }
        }
    }
}