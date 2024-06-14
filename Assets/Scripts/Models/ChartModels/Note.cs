using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Echoin.Models.ChartModels
{
    [JsonObject]
    [Serializable]
    public sealed class Note
    {
        public float Size;
        public float Time;
        public float Position;
        public NoteKind Kind;
        public List<NoteNode> Nodes;

        public bool IsHold => Nodes?.Count > 0;

        [JsonIgnore]
        private float _duration = float.NaN;
        [JsonIgnore]
        public float Duration
        {
            get {
                if (float.IsNaN(_duration)) {
                    if (!IsHold) {
                        _duration = 0f;
                    }
                    else {
                        float time = 0f;
                        foreach (var node in Nodes) {
                            time += node.Duration;
                        }
                        _duration = time;
                    }
                }
                return _duration;
            }
        }
        [JsonIgnore]
        public float EndTime => Time + Duration;

        public enum NoteKind
        {
            Click,
            Slide,
        }

        [JsonObject]
        public sealed class NoteNode
        {
            public float Duration;
            public float EndPosition;
        }
    }
}
