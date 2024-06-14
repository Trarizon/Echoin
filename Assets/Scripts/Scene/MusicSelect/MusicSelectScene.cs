using Echoin.Models.InfoModels;
using Echoin.Scene.GamePlay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Echoin.Scene.MusicSelect
{
    public sealed class MusicSelectScene : MonoBehaviour
    {
        public static void Load()
        {
            SceneManager.LoadScene(0);
        }

        public static void Load(int initMusicIndex, int initChartIndex = -1)
        {
            GamePlayDatas.SelectedMusicIndex = initMusicIndex;
            GamePlayDatas.SelectedChartIndex = initChartIndex;

            SceneManager.LoadScene(0);
        }

        [Header("Controllers")]
        [SerializeField] SettingsController _settingsController;
        [SerializeField] MusicListController _musicListController;
        [SerializeField] InfoPanelController _infoPanelController;
        [SerializeField] ChartListController _chartListController;

        [Header("UI")]
        [SerializeField] GraphicRaycaster _mainGraphicRaycaster;
        [SerializeField] AudioSource _audioSource;

        public MusicListController MusicListController => _musicListController;
        public InfoPanelController InfoPanelController => _infoPanelController;
        public ChartListController ChartListController => _chartListController;

        private void Awake()
        {
            Scenes.MusicSelect = this;

            _musicListController.OnCreate();
            _chartListController.OnCreate();

            _musicListController.Activate();
        }

        private void Start()
        {
            MusicListController.Select(GamePlayDatas.SelectedMusicIndex);
        }

        public void ShowSettings()
        {
            _settingsController.Activate();
            _mainGraphicRaycaster.enabled = false;
        }

        public void HideSettings()
        {
            _settingsController.Deactivate();
            _mainGraphicRaycaster.enabled = true;
        }

        public void PlayMusic(MusicInfoModel model)
        {
            var newClip = model.MusicClip;
            if (ReferenceEquals(newClip, _audioSource.clip))
                return;
            _audioSource.Pause();
            _audioSource.clip = newClip;
            _audioSource.Play();
        }

        public void LoadGamePlay(int chartIndex, GamePlayMode mode)
            => LoadGamePlay(chartIndex, -1, mode);

        public void LoadGamePlay(int chartIndex, int sliceIndex, GamePlayMode mode)
        {
            gameObject.SetActive(false);
            if (sliceIndex == -1) {
                GamePlayScene.LoadScene(
                    _musicListController.SelectedIndex,
                    chartIndex, mode);
            }
            else {
                GamePlayScene.LoadScene(
                    _musicListController.SelectedIndex,
                    chartIndex, sliceIndex, mode);
            }
        }
    }
}