using Echoin.Models.InfoModels;
using Echoin.Scene.GamePlay;
using Echoin.Scene.MusicSelect;
using Echoin.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Echoin.Scene.Result
{
    public sealed class ResultScene : MonoBehaviour
    {
        public static void LoadScene(
            float score, int combo, int totalCombo,
            int missCount, int badCount, int greatCount, int perfectCount)
        {
            ResultDatas.Score = score;
            ResultDatas.Combo = combo;
            ResultDatas.TotalCombo = totalCombo;
            ResultDatas.MissCount = missCount;
            ResultDatas.BadCount = badCount;
            ResultDatas.GreatCount = greatCount;
            ResultDatas.PerfectCount = perfectCount;

            SceneManager.LoadScene(2);
        }

        [SerializeField] TMP_Text _musicNameText;
        [SerializeField] TMP_Text _chartInfoText;
        [SerializeField] TMP_Text _perfectCountText;
        [SerializeField] TMP_Text _greatCountText;
        [SerializeField] TMP_Text _badCountText;
        [SerializeField] TMP_Text _missCountText;
        [SerializeField] TMP_Text _totalCountText;
        [SerializeField] TMP_Text _comboText;
        [SerializeField] TMP_Text _scoreText;

        // Datas
        private MusicInfoModel _musicInfoModel;
        private ChartInfoModel _chartInfoModel;
        private ChartSliceInfoModel _sliceInfoModel;
        private float _score;
        private int _combo;
        private int _totalCombo;
        private int _missCount;
        private int _badCount;
        private int _greatCount;
        private int _perfectCount;

        private void Awake()
        {
            _musicInfoModel = GamePlayDatas.SelectedMusic;
            _chartInfoModel = GamePlayDatas.SelectedChart;
            _sliceInfoModel = GamePlayDatas.SelectedSliceOrNull;
            _score = ResultDatas.Score;
            _combo = ResultDatas.Combo;
            _totalCombo = ResultDatas.TotalCombo;
            _missCount = ResultDatas.MissCount;
            _badCount = ResultDatas.BadCount;
            _greatCount = ResultDatas.GreatCount;
            _perfectCount = ResultDatas.PerfectCount;

            _musicNameText.text = _musicInfoModel.Name;
            _chartInfoText.text = _sliceInfoModel is null
                ? $"{_chartInfoModel.Difficulty} {_chartInfoModel.Level}"
                : $"{_chartInfoModel.Difficulty} {_chartInfoModel.Level} ({_sliceInfoModel.StartTime:F2} - {_sliceInfoModel.EndTime:F2})";
            _perfectCountText.text = _perfectCount.ToString();
            _greatCountText.text = _greatCount.ToString();
            _badCountText.text = _badCount.ToString();
            _missCountText.text = _missCount.ToString();
            _totalCountText.text = _totalCombo.ToString();
            _comboText.text = $"Max Combo {_combo} / {_totalCombo}";
            _scoreText.text = $"{_score:F2} %";
        }

        public void BackToSelect()
        {
            gameObject.SetActive(false);
            if (_sliceInfoModel is null)
                MusicSelectScene.Load(GamePlayDatas.SelectedMusicIndex);
            else
                MusicSelectScene.Load(GamePlayDatas.SelectedMusicIndex, GamePlayDatas.SelectedChartIndex);
        }

        public void Replay()
        {
            gameObject.SetActive(false);
            GamePlayScene.LoadScene(
                GamePlayDatas.SelectedMusicIndex,
                GamePlayDatas.SelectedChartIndex,
                GamePlayDatas.SelectedSliceIndex);
        }
    }
}