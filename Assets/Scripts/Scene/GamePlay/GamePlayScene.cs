using Echoin.Models.ChartModels;
using Echoin.Models.InfoModels;
using Echoin.Scene.MusicSelect;
using Echoin.Scene.Result;
using Echoin.Utility;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Echoin.Scene.GamePlay
{
    public sealed class GamePlayScene : MonoBehaviour
    {
        public static void LoadScene(int musicIndex, int chartIndex, GamePlayMode mode = GamePlayMode.Play)
            => LoadScene(musicIndex, chartIndex, -1, mode);

        // TODO
        public static void LoadScene(int musicIndex, int chartIndex, int sliceIndex, GamePlayMode mode = GamePlayMode.Play)
        {
            GamePlayDatas.SelectedMusicIndex = musicIndex;
            GamePlayDatas.SelectedChartIndex = chartIndex;
            GamePlayDatas.SelectedSliceIndex = sliceIndex;
            GamePlayDatas.GamePlayMode = mode;

            SceneManager.LoadScene(1);
        }

        [Header("GameObject refs")]
        [SerializeField] ScoreController _scoreController;
        [SerializeField] SlicingController _slicingController;
        //[SerializeField] AudioSource _musicSource;
        [SerializeField] GamePlayMusicSource _musicSource;
        [SerializeField] Transform _chartPanelTransform;
        [SerializeField] GraphicRaycaster _uiGraphicRaycaster;

        [Header("UI")]
        [SerializeField] Slider _musicSlider;
        [SerializeField] TMP_Text _musicNameText;
        [SerializeField] Canvas _pauseCanvas;

        [Header("UI ReadyLine")]
        [SerializeField] GameObject _readyPanel;
        [SerializeField] Image _readyLineImage;
        [SerializeField] HorizontalLayoutGroup _readyTextsGroup;
        [SerializeField] TMP_Text[] _readyTexts;

        public ScoreController ScoreController => _scoreController;
        public SlicingController SlicingController => _slicingController;

        // Static datas
        private ObjectPool<NoteController> _notePool;
        private ObjectPool<NoteNodeController> _noteNodePool;

        public GamePlayMode Mode;
        private int _noteLoadIndex;
        [HideInInspector]
        public INoteCollection Chart;
        [HideInInspector]
        public List<NoteController> OnStageNotes;

        [Header("Prefabs")]
        [SerializeField] NoteController _notePrefab;
        [SerializeField] NoteNodeController _noteNodePrefab;
        public Material HoldBodyMaterialPrefab;
        public Sprite ClickNoteSpritePrefab;
        public Sprite SlideNoteSpritePrefab;
        public Sprite HoldBodySpritePrefab;
        public Sprite HoldingNoteSpritePrefab;
        public Sprite[] HitEffectFrameSpritesPrefab;
        // TODO: unassigned
        public Sprite[] ReadyBanner;


        public MusicInfoModel MusicInfo;
        public ChartInfoModel ChartInfo;
        public ChartSliceInfoModel SliceInfo;

        public float CurrentTime => _musicSource.CurrentTime;
        public float CurrentMusicTime => _musicSource.CurrentMusicTime;

        #region Unity

        private void Awake()
        {
            Debug.Log("Awake");

            Scenes.GamePlay = this;

            _notePool = new ObjectPool<NoteController>(
                () => Instantiate(_notePrefab, _chartPanelTransform),
                actionOnGet: n => n.gameObject.SetActive(true),
                actionOnRelease: n =>
                {
                    n.OnReleased();
                    n.gameObject.SetActive(false);
                });
            _noteNodePool = new ObjectPool<NoteNodeController>(
                () => Instantiate(_noteNodePrefab),
                n => n.gameObject.SetActive(true),
                n =>
                {
                    n.OnRelease();
                    n.gameObject.SetActive(false);
                });

            MusicInfo = GamePlayDatas.SelectedMusic;
            ChartInfo = GamePlayDatas.SelectedChart;
            SliceInfo = GamePlayDatas.SelectedSliceOrNull;
            Mode = GamePlayDatas.GamePlayMode;
            OnStageNotes = new();

            _musicSource.Clip = MusicInfo.MusicClip;
            Chart = GamePlayDatas.LoadChart();

            if (SliceInfo is not null) {
                _musicSource.StartTime = Mathf.Max(SliceInfo.StartTime - 1f, 0f);
                _musicSource.EndTime = Mathf.Min(_musicSource.Clip.length, SliceInfo.EndTime + 1f);
                _musicSource.Fade = true;
            }
            else {
                _musicSource.StartTime = 0f;
                _musicSource.EndTime = _musicSource.Clip.length;
                _musicSource.Fade = false;
            }
            _musicSlider.maxValue = _musicSource.PlayTotalTime;
            _musicNameText.text = MusicInfo.Name;

            _noteLoadIndex = 0;
            if (Mode is GamePlayMode.Play) {
                _musicSlider.interactable = false;
                _scoreController.Initialize();
                _scoreController.Activate(enableInput: false);
            }
            else {
                _musicSlider.interactable = true;
                _slicingController.Initialize(0, _musicSource.Clip.length);
                _slicingController.Activate();

                _musicSlider.onValueChanged.AddListener(time =>
                {
                    _musicSource.CurrentTime = time;
                    UpdateNotes();
                });
                _musicSlider.onValueChanged.AddListener(_slicingController.OnCurrentTimeChanged);
            }

            _musicSource.CurrentTime = 0f;
            _musicSlider.value = 0f;

            // UpdateNotes();
        }

        private void Start()
        {
            if (Mode is GamePlayMode.Play) {
                _uiGraphicRaycaster.enabled = false;
                StartCoroutine(ReadyAsync(true));
            }
        }

        /*
        protected override void OnAwake(bool onCreating)
        {
            Debug.Log("OnGamePlayAwake");

            if (onCreating) {
                _notePool = new ObjectPool<NoteController>(
                    () => Instantiate(_notePrefab, _chartPanelTransform),
                    actionOnGet: n => n.gameObject.SetActive(true),
                    actionOnRelease: n =>
                    {
                        n.OnReleased();
                        n.gameObject.SetActive(false);
                    });
                _noteNodePool = new(
                    () => Instantiate(_noteNodePrefab),
                    n => n.gameObject.SetActive(true),
                    n =>
                    {
                        n.OnRelease();
                        n.gameObject.SetActive(false);
                    });

                //if (Mode is GamePlayMode.Play)
                //    _slicingController.Deactivate();
                //else
                //    _scoreController.Deactivate();

                _musicSlider.onValueChanged.AddListener(OnManuallySetTime);
                OnStageNotes = new();
            }


            MusicInfo = GamePlayDatas.SelectedMusic;
            ChartInfo = GamePlayDatas.SelectedChart;
            SliceInfo = GamePlayDatas.SelectedSliceOrNull;
            Mode = GamePlayDatas.GamePlayMode;
            _musicSource.clip = GlobalSettings.LoadMusic(MusicInfo);
            Chart = GamePlayDatas.LoadChart();

            if (SliceInfo is not null) {
                _playStartTime = Mathf.Max(SliceInfo.StartTime - 1f, 0f);
                _playEndTime = Mathf.Min(_musicSource.clip.length, SliceInfo.EndTime + 1f);
            }
            else {
                _playStartTime = 0f;
                _playEndTime = _musicSource.clip.length;
            }

            //// fake data
            //MusicInfo = Constants.FakeMusics()[0];
            //ChartInfo = MusicInfo.ChartInfos[0];
            //Chart = Constants.FakeChart(); // GamePlayDatas.Chart;

            foreach (var note in OnStageNotes) {
                ReleasePooledNote(note);
            }
            OnStageNotes.Clear();
            _isPlayable = false;


            _noteLoadIndex = 0;

            _musicSlider.maxValue = _playEndTime - _playStartTime;
            _musicNameText.text = MusicInfo.Name;

            if (Mode is GamePlayMode.Play) {
                _musicSlider.interactable = false;
                _scoreController.Initialize();
                _scoreController.Activate();
            }
            else {
                _musicSlider.interactable = true;
                _slicingController.Initialize();
                _slicingController.Activate();
            }

            SetGamePlayTime(0f);
        }
        */
        private void Update()
        {
            if (Mode == GamePlayMode.Play) {
                if (!_musicSource.IsPaused) {
                    // Sync Time
                    _musicSource.SyncMusicTime();
                    _musicSlider.value = _musicSource.CurrentTime;
                    UpdateNotes();
                }

                if (_musicSource.IsEnd) {
                    LoadResult();
                }
            }
        }

        public void UpdateNotes()
        {
            if (Mode is GamePlayMode.Play) {
                UpdateNotesVisibility();
                foreach (var note in OnStageNotes) {
                    note.UpdateNoteRender();
                }
                ReturnReleasedNotes();
            }
            else {
                //该方法内部会进行Release
                UpdateNotesVisibilityRandomly();
                foreach (var note in OnStageNotes) {
                    note.UpdateNoteRender();
                }
            }

            // optimize for 正常下落
            void UpdateNotesVisibility()
            {
                // Enqueue new notes
                for (; _noteLoadIndex < Chart.Notes.Count; _noteLoadIndex++) {
                    var note = Chart.Notes[_noteLoadIndex];

                    // Reach last on-stage note
                    if (GlobalSettings.OffsetTime(note.Time) - Constants.VisibleNotesTimeRange > CurrentMusicTime)
                        break;

                    // Load note to stage
                    OnStageNotes.Add(GetPooledNote(note));
                }
                // Remove miss notes
            }

            void UpdateNotesVisibilityRandomly()
            {
                foreach (var note in OnStageNotes) {
                    _notePool.Release(note);
                }
                OnStageNotes.Clear();

                // 不显示线下？
                int i;
                for (i = 0; i < Chart.Notes.Count; i++) {
                    var note = Chart.Notes[i];
                    if (note.Time > CurrentMusicTime) {
                        break;
                    }
                }

                for (; i < Chart.Notes.Count; i++) {
                    var note = Chart.Notes[i];
                    if (note.Time > Constants.VisibleNotesTimeRange + CurrentMusicTime)
                        break;

                    OnStageNotes.Add(GetPooledNote(note));
                }
            }

            void ReturnReleasedNotes()
            {
                OnStageNotes.RemoveAll(n =>
                {
                    if (n.IsReadyToRelease) {
                        _notePool.Release(n);
                        return true;
                    }
                    return false;
                });
            }
        }

        #endregion

        #region Time

        public void SetTime(float time)
        {
            time = Mathf.Clamp(time, _musicSource.StartTime, _musicSource.EndTime);
            _musicSource.CurrentTime = time;
            _musicSlider.value = time;
        }

        /*
        public void SetGamePlayTime(float time)
        {
            _musicSource.time = time + _playStartTime;
            _musicSlider.value = time;
            UpdateNotes();
        }
        */
        #endregion

        private IEnumerator ReadyAsync(bool fromStart)
        {
            Debug.Log("Start Ready");
            const float FrozeTime = 0.5f;
            const float FadeTime = 1f;
            const float ReadyLineWidth = 500f;
            const float ReadyTextWidth = 300f;

            var imgTransf = _readyLineImage.transform as RectTransform;
            imgTransf.sizeDelta = new(ReadyLineWidth, imgTransf.sizeDelta.y);
            var txtTransf = _readyTextsGroup.transform as RectTransform;
            txtTransf.sizeDelta = new(ReadyTextWidth, txtTransf.sizeDelta.y);
            _readyLineImage.color = _readyLineImage.color.WithAlpha(1f);
            foreach (var txt in _readyTexts) {
                txt.color = txt.color.WithAlpha(1f);
            }
            _readyPanel.SetActive(true);
            yield return new WaitForSeconds(FrozeTime);

            float time = 0;
            while (time < FadeTime) {
                var t = time / FadeTime;
                imgTransf.sizeDelta = new(Mathf.Lerp(ReadyLineWidth, 0f, t), imgTransf.sizeDelta.y);
                txtTransf.sizeDelta = new(Mathf.Lerp(ReadyTextWidth, 0f, t), txtTransf.sizeDelta.y);

                var alpha = Mathf.Lerp(1f, 0f, t);
                _readyLineImage.color = _readyLineImage.color.WithAlpha(alpha);
                foreach (var txt in _readyTexts) {
                    txt.color = txt.color.WithAlpha(alpha);
                }
                yield return null;
                time += Time.deltaTime;
            }

            _readyPanel.SetActive(false);

            Debug.Log("End Ready");
            if (fromStart)
                _musicSource.Replay();
            else
                _musicSource.Play();
            _scoreController.EnableInput(true);
            _uiGraphicRaycaster.enabled = true;
        }

        public NoteController GetPooledNote(Note noteModel)
        {
            var nc = _notePool.Get();
            nc.Initialize(noteModel);
            return nc;
        }

        public void ReleasePooledNote(NoteController noteController)
        {
            _notePool.Release(noteController);
        }

        public NoteNodeController GetPooledNoteNode(NoteController note, float headPosition, float endPosition, float startTime, float duration)
        {
            var node = _noteNodePool.Get();
            node.transform.parent = note.transform;
            node.Initialize(note, headPosition, endPosition, startTime, duration);
            return node;
        }

        public void ReleasePooledNoteNode(NoteNodeController node)
        {
            _noteNodePool.Release(node);
        }

        #region Button Actions

        public void OnPause()
        {
            _musicSource.Pause();
            _scoreController.EnableInput(false);
            _uiGraphicRaycaster.enabled = false;
            _pauseCanvas.gameObject.SetActive(true);
        }

        public void OnResume()
        {
            _pauseCanvas.gameObject.SetActive(false);
            // _uiGraphicRaycaster.enabled = true;
            StartCoroutine(ReadyAsync(false));
        }

        public void Replay()
        {
            if (OnStageNotes.Count > 0) {
                foreach (var nc in OnStageNotes) {
                    _notePool.Release(nc);
                }
                OnStageNotes.Clear();
            }
            _noteLoadIndex = 0;
            _scoreController.ResetScore();

            _pauseCanvas.gameObject.SetActive(false);
            _uiGraphicRaycaster.enabled = true;

            StartCoroutine(ReadyAsync(true));
        }

        #endregion

        //public void TogglePause()
        //{
        //    // Resume
        //    if (IsPaused) {
        //        StartCoroutine(ReadyAsync());
        //        //_musicSource.Play();
        //        //IsPlayable = true;
        //    }
        //    // Pause
        //    else {
        //        _musicSource.Pause();
        //        _scoreController.EnableInput(false);
        //    }
        //}

        public void BackToMusicSelectScene()
        {
            Deactivate();
            if (SliceInfo is null)
                MusicSelectScene.Load(GamePlayDatas.SelectedMusicIndex);
            else
                MusicSelectScene.Load(GamePlayDatas.SelectedMusicIndex, GamePlayDatas.SelectedChartIndex);
        }

        public void SaveSliceInfo()
        {
            var slice = _slicingController.SaveSlice();
            if (slice is not null) {
                ChartInfo.SliceInfos.Add(slice);
                GlobalSettings.SaveSliceInfos(MusicInfo, GamePlayDatas.SelectedChartIndex);
            }
            BackToMusicSelectScene();
        }

        private void LoadResult()
        {
            Deactivate();

            var score = _scoreController.Score;
            if (ChartInfo.ScoreInfo is null) {
                ChartInfo.ScoreInfo = new() { Score = score };
            }
            else if (score > ChartInfo.ScoreInfo.Score) {
                ChartInfo.ScoreInfo.Score = score;
            }
            GlobalSettings.SaveScore(MusicInfo, GamePlayDatas.SelectedChartIndex, ChartInfo.ScoreInfo);
            ResultScene.LoadScene(
                _scoreController.Score,
                _scoreController.MaxCombo,
                Chart.Notes.Count,
                _scoreController.MissCount,
                _scoreController.BadCount,
                _scoreController.GreatCount,
                _scoreController.PerfectCount);
        }



        private void Deactivate()
        {
            gameObject.SetActive(false);

            if (Mode is GamePlayMode.Play)
                _scoreController.Deactivate();
            else
                _slicingController.Deactivate();

            foreach (var item in OnStageNotes) {
                _notePool.Release(item);
            }
            OnStageNotes.Clear();
        }
    }
}