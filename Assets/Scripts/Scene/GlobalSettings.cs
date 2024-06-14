using Echoin.Models.ChartModels;
using Echoin.Models.InfoModels;
using Echoin.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;

namespace Echoin.Scene
{
    public static class GlobalSettings
    {
        private class Values
        {
            public float ChartOffset;
            public int Speed;
        }

        private static Values _values;

        public static float ChartOffset
        {
            get => (_values ??= LoadSettings()).ChartOffset;
            set => (_values ??= LoadSettings()).ChartOffset = value;
        }

        public static int Speed
        {
            get {
                _values ??= LoadSettings();

                if (_values.Speed < 1)
                    _values.Speed = 4;

                return _values.Speed;
            }

            set => (_values ??= LoadSettings()).Speed = value;
        }

        public static float OffsetTime(float time) => time + ChartOffset;

        public static void SaveSettings()
        {
            var path = Path.Combine(Application.persistentDataPath, $"globalsettings.json");
            var json = JsonConvert.SerializeObject(_values);
            File.WriteAllText(path, json);
        }

        private static Values LoadSettings()
        {
            var path = Path.Combine(Application.persistentDataPath, $"globalsettings.json");
            if (!File.Exists(path))
                return new();

            return JsonConvert.DeserializeObject<Values>(File.ReadAllText(path));
        }

        public static MusicInfoModel[] LoadMusicInfos()
        {
            // Resources.Load不带扩展名
            var jsonText = Resources.Load<TextAsset>("musicList");
            if (jsonText == null) {
                Debug.Log("MusicListFile Not found");
                return Array.Empty<MusicInfoModel>();
            }

            return JsonConvert.DeserializeObject<MusicInfoModel[]>(jsonText.text);
        }

        public static void LoadSliceInfos(MusicInfoModel musicInfo, int chartIndex)
        {
            var path = Path.Combine(Application.persistentDataPath, $"chartslices/{musicInfo.MetaName}.{chartIndex}.json");
            if (!File.Exists(path)) {
                return;
            }
            musicInfo.ChartInfos[chartIndex].SliceInfos = JsonConvert.DeserializeObject<List<ChartSliceInfoModel>>(File.ReadAllText(path));
        }

        public static void SaveSliceInfos(MusicInfoModel musicInfo, int chartIndex)
        {
            var dir = Path.Combine(Application.persistentDataPath, $"chartslices");
            var path = Path.Combine(dir, $"{musicInfo.MetaName}.{chartIndex}.json");
            var json = JsonConvert.SerializeObject(musicInfo.ChartInfos[chartIndex].SliceInfos);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, json);
        }

        public static void SaveScore(MusicInfoModel musicInfo, int chartIndex, ScoreInfoModel scoreInfo)
        {
            var dir = Path.Combine(Application.persistentDataPath, $"scores");
            var path = Path.Combine(dir, $"{musicInfo.MetaName}.{chartIndex}.json");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, JsonConvert.SerializeObject(scoreInfo));
        }

        public static ScoreInfoModel LoadScore(MusicInfoModel musicInfo, int chartIndex)
        {
            var path = Path.Combine(Application.persistentDataPath, $"scores/{musicInfo.MetaName}.{chartIndex}.json");
            if (!File.Exists(path))
                return null;
            return JsonConvert.DeserializeObject<ScoreInfoModel>(File.ReadAllText(path));
        }

        public static AudioClip LoadMusic(MusicInfoModel music)
        {
            return Resources.Load<AudioClip>($"ChartItems/{music.MetaName}");
        }

        public static Chart LoadChart(MusicInfoModel music, int chartIndex)
        {
            var text = Resources.Load<TextAsset>($"ChartItems/{music.MetaName}.{chartIndex}");
            if (text == null) {
                Debug.Log($"Chart {music.MetaName}.{chartIndex} NotFound.");
                return null;
            }

            if (Chart.TryParseFromJson(text.text, out var cht)) {
                return cht;
            }
            Debug.Log($"Parse Chart {music.MetaName}.{chartIndex} failed");
            return null;
        }

        public static ChartSlice LoadSlice(MusicInfoModel music, int chartIndex, int sliceIndex)
        {
            var chart = LoadChart(music, chartIndex);
            var info = music.ChartInfos[chartIndex].SliceInfos[sliceIndex];
            return new(chart, info);
        }
    }
}