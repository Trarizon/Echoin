using Echoin.Models.ChartModels;
using Echoin.Models.InfoModels;
using Echoin.Scene.MusicSelect;

namespace Echoin.Scene
{
    public static class GamePlayDatas
    {
        public static int SelectedMusicIndex;
        public static int SelectedChartIndex;
        public static int SelectedSliceIndex;
        public static GamePlayMode GamePlayMode;

        public static MusicInfoModel SelectedMusic => Scenes.MusicSelect.MusicListController.MusicInfoModels[SelectedMusicIndex];
        public static ChartInfoModel SelectedChart => SelectedMusic.ChartInfos[SelectedChartIndex];

        public static ChartSliceInfoModel SelectedSliceOrNull => SelectedSliceIndex >= 0 ? SelectedChart.SliceInfos[SelectedSliceIndex] : null;

        public static INoteCollection LoadChart()
        {
            if (SelectedSliceIndex >= 0) {
                return GlobalSettings.LoadSlice(SelectedMusic, SelectedChartIndex, SelectedSliceIndex);
            }
            else {
                return GlobalSettings.LoadChart(SelectedMusic, SelectedChartIndex);
            }
        }
    }
}