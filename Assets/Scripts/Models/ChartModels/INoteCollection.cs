using System.Collections.Generic;

namespace Echoin.Models.ChartModels
{
    public interface INoteCollection
    {
        public IReadOnlyList<Note> Notes { get; }
    }
}