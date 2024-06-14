using Echoin.Models.InfoModels;
using System.Collections.Generic;

namespace Echoin.Models.ChartModels
{
    public sealed class ChartSlice : INoteCollection
    {
        public ChartSlice(Chart chart, ChartSliceInfoModel info)
        {
            var notes = new List<Note>(info.EndIndex - info.StartIndex);
            for (int i = info.StartIndex; i < info.EndIndex; i++) {
                notes.Add(chart.Notes[i]);
            }
            Notes = notes;
        }

        public IReadOnlyList<Note> Notes { get; private set; }
    }
}