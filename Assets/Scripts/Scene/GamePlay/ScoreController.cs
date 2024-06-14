using Echoin.Models.InfoModels;
using Echoin.ProjectModels;
using Echoin.Utility;
using Lean.Touch;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Echoin.Scene.GamePlay
{
    public sealed class ScoreController : MonoBehaviour
    {
        [SerializeField] Camera _camera;

        [Header("UI")]
        [SerializeField] TMP_Text _comboText;
        [SerializeField] TMP_Text _scoreText;
        [SerializeField] GameObject _comboObject;
        [SerializeField] Button _pauseButton;
        // [SerializeField] Button _replayButton;

        #region Datas

        public bool InputEnabled;

        // ReadOnly
        private int _totalComboScore;
        private int _maxCombo;

        private float _accScore;
        private int _comboScore;
        /// <summary>
        /// less than or equals to 100
        /// </summary>
        private float _score;

        private int _combo;
        private int _missCount;
        private int _badCount;
        private int _greatCount;
        private int _perfectCount;

        public int MaxCombo => _maxCombo;
        public float Score => _score;
        public int MissCount => _missCount;
        public int BadCount => _badCount;
        public int GreatCount => _greatCount;
        public int PerfectCount => _perfectCount;

        public ScoreInfoModel.Rank ScoreRank => this switch {
            _ when _perfectCount == _maxCombo => ScoreInfoModel.Rank.AllPerfect,
            _ when _combo == _maxCombo => ScoreInfoModel.Rank.FullCombo,
            _ => ScoreInfoModel.Rank.None,
        };

        #endregion

        public void Initialize()
        {
            var scene = Scenes.GamePlay;

            var totalCombo = scene.Chart.Notes.Count;
            _totalComboScore = (1 + totalCombo) * totalCombo / 2;
            ResetScore();
        }

        public void Activate(bool enableInput)
        {
            if (gameObject.activeSelf)
                return;

            gameObject.SetActive(true);
            if (enableInput)
                EnableInput(true);

            _comboObject.SetActive(true);
            _scoreText.gameObject.SetActive(true);
            _pauseButton.gameObject.SetActive(true);
            //_replayButton.gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            if (!gameObject.activeSelf)
                return;

            gameObject.SetActive(false);
            EnableInput(false);

            _comboObject.SetActive(false);
            _scoreText.gameObject.SetActive(false);
            _pauseButton.gameObject.SetActive(false);
            // _replayButton.gameObject.SetActive(false);
        }

        public void EnableInput(bool enable)
        {
            if (enable) {
                if (InputEnabled)
                    return;
                LeanTouch.OnFingerDown += OnFingerDown;
                LeanTouch.OnFingerUpdate += OnFingerUpdate;
                InputEnabled = true;
            }
            else {
                if (!InputEnabled)
                    return;
                LeanTouch.OnFingerDown -= OnFingerDown;
                LeanTouch.OnFingerUpdate -= OnFingerUpdate;
                InputEnabled = false;
            }
        }



        public void ResetScore()
        {
            Debug.Log("Reset score");

            _accScore = 0f;
            _comboScore = 0;
            _score = 0f;

            _combo = 0;
            _missCount = 0;
            _badCount = 0;
            _greatCount = 0;
            _perfectCount = 0;
            UpdateScoreUIText();
        }

        private void OnFingerUpdate(LeanFinger finger)
        {
            if (!IsFingerOnStage(finger, out var pos))
                return;
            var scene = Scenes.GamePlay;
            foreach (var curNote in scene.OnStageNotes) {
                curNote.TryCatch(pos.x);
            }

            //if (!TryCatchNote(pos.x, out var note))
            //    return;

            //note.OnHitted();
        }

        private void OnFingerDown(LeanFinger finger)
        {
            if (!IsFingerOnStage(finger, out var pos))
                return;
            var scene = Scenes.GamePlay;
            bool block = false;
            foreach (var curNote in scene.OnStageNotes) {
                curNote.TryHit(pos.x, ref block);
            }
            //if (!TryHitNote(pos.x, out var note))
            //    return;

            //note.OnHitted();
        }

        //private bool TryCatchNote(float posX, [NotNullWhen(true)] out NoteController note)
        //{
        //    var scene = Scenes.GamePlay;

        //    foreach (var curNote in scene.OnStageNotes) {
        //        if (curNote.Status >= NoteController.NoteStatus.Judged)
        //            continue;

        //        if (curNote.Note.IsHold) {
        //            // 对于c-hold，需要已判定
        //            if (curNote.Note.Kind == Note.NoteKind.Click) {
        //                if (curNote.Status < NoteController.NoteStatus.Holding)
        //                    continue;

        //            }
        //        }
        //        if (curNote.Note.Kind != Note.NoteKind.Slide)
        //            continue;
        //        if (!curNote.IsPositionOverlap(posX, scene.CurrentTime, out var hitTimeOffset))
        //            continue;

        //        if (curNote.TryJudge(hitTimeOffset)) {
        //            note = curNote;
        //            return true;
        //        }
        //    }

        //    note = null;
        //    return false;
        //}

        //private bool TryHitNote(float posX, [NotNullWhen(true)] out NoteController note)
        //{
        //    var scene = Scenes.GamePlay;

        //    foreach (var curNote in scene.OnStageNotes) {
        //        if (curNote.Status != NoteController.NoteStatus.Unhitted)
        //            continue;

        //        if (!curNote.IsPositionOverlap(posX, scene.CurrentTime, out var hitTimeOffset))
        //            continue;

        //        if (curNote.TryJudge(hitTimeOffset)) {
        //            note = curNote;
        //            return true;
        //        }
        //    }

        //    note = null;
        //    return false;
        //}

        private bool IsFingerOnStage(LeanFinger finger, out Vector3 pos)
        {
            pos = default;
            if (finger.IsOverGui)
                return false;

            var scrPos = finger.ScreenPosition;
            pos = _camera.ScreenToWorldPoint(new(scrPos.x, scrPos.y, _camera.nearClipPlane));
            pos.x /= Constants.NotePositionToXScale;

            return pos.y < 0f; // 屏幕上半部分不判定
        }

        // 在NoteController中更新分数
        public void UpdateScore(NoteGrade grade)
        {
            switch (grade) {
                case NoteGrade.Miss:
                    _combo = 0;
                    _missCount++;
                    break;
                case NoteGrade.Bad:
                    _combo = 0;
                    _badCount++;
                    break;
                case NoteGrade.HoldReleased:
                    _badCount++;
                    break;
                case NoteGrade.Great:
                    _accScore += 0.7f;
                    _combo += 1;
                    _comboScore += _combo;
                    _maxCombo = Math.Max(_combo, _maxCombo);
                    _greatCount++;
                    break;
                case NoteGrade.Perfect:
                    _accScore += 1f;
                    _combo += 1;
                    _comboScore += _combo;
                    _maxCombo = Math.Max(_combo, _maxCombo);
                    _perfectCount++;
                    break;
                default:
                    break;
            }

            var scene = Scenes.GamePlay;
            _score = _accScore / scene.Chart.Notes.Count * 90f + (float)_comboScore / _totalComboScore * 10f;
            UpdateScoreUIText();
        }

        private void UpdateScoreUIText()
        {
            _scoreText.text = $"{_score:F2} %";
            _comboText.text = _combo.ToString();
        }
    }
}