using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Echoin.Models.ChartModels
{
    [JsonObject()]
    public sealed class Chart : INoteCollection
    {
        public List<Note> Notes;

        IReadOnlyList<Note> INoteCollection.Notes => Notes;

        public static bool TryParseFromJson(string json, [NotNullWhen(true)] out Chart chart)
        {
            try {
                chart = JsonConvert.DeserializeObject<Chart>(json);
            } catch { chart = null; }

            return chart != null;
        }

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}
