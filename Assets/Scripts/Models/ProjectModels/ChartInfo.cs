using Echoin.Models.ChartModels;

namespace Echoin.ProjectModels
{
    public class ChartInfo
    {
        public Chart Chart { get; }
        public int Level { get; set; }
        public ChartDifficulty Difficulty { get; set; }

        public ChartInfo(Chart chart, int level, ChartDifficulty difficulty)
        {
            Chart = chart;
            Level = level;
            Difficulty = difficulty;
        }
    }
}
