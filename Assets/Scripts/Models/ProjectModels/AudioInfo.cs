using System.Collections.Generic;

namespace Echoin.ProjectModels
{
    public sealed class AudioInfo
    {
        public string Name { get; set; }
        public string Composer { get; set; }
        public string Audio { get; set; } // TODO: you know, audio won't be a string

        public List<(float Bpm, float StartTime)> Bpms { get; }
    }
}
