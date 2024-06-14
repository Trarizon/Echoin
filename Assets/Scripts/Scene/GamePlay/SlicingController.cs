using Echoin.Models.ChartModels;
using Echoin.Models.InfoModels;
using Echoin.Scene.MusicSelect;
using Echoin.Utility;
using Lean.Touch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Echoin.Scene.GamePlay
{
    public sealed class SlicingController : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _sliceStartLineRender;
        [SerializeField] SpriteRenderer _sliceEndLineRender;

        [Header("UI")]
        [SerializeField] Button _asSliceStartButton;
        [SerializeField] Button _asSliceEndButton;
        [SerializeField] Button _saveButton;
        [SerializeField] Button _backButton;
        [SerializeField] TMP_Text _asStartText;
        [SerializeField] TMP_Text _asEndText;

        /// <summary>
        /// -1表示未指定
        /// notes.Count表示结尾
        /// </summary>
        [Header("Values")]
        [SerializeField]
        private float _startTime;
        [SerializeField]
        private float _endTime;

        [SerializeField]
        private float _draggingSensitivity;

        public void Initialize(float initStart, float initEnd)
        {
            _startTime = initStart;
            _endTime = initEnd;
            _draggingSensitivity = 0.02f;
        }

        public void Activate()
        {
            if (gameObject.activeSelf)
                return;

            gameObject.SetActive(true);
            LeanTouch.OnFingerUpdate += OnFingerUpdate;
            _sliceStartLineRender.gameObject.SetActive(true);
            _sliceEndLineRender.gameObject.SetActive(true);
            _asSliceStartButton.gameObject.SetActive(true);
            _asSliceEndButton.gameObject.SetActive(true);
            _saveButton.gameObject.SetActive(true);
            _backButton.gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            if (!gameObject.activeSelf)
                return;

            gameObject.SetActive(false);
            LeanTouch.OnFingerUpdate -= OnFingerUpdate;
            _asSliceStartButton.gameObject.SetActive(false);
            _asSliceEndButton.gameObject.SetActive(false);
            _sliceStartLineRender.gameObject.SetActive(false);
            _sliceEndLineRender.gameObject.SetActive(false);
            _saveButton.gameObject.SetActive(false);
            _backButton.gameObject.SetActive(false);
        }

        private void Update()
        {
            UpdateSliceLineRender(_startTime, _sliceStartLineRender);
            UpdateSliceLineRender(_endTime, _sliceEndLineRender);
        }

        private void UpdateSliceLineRender(float time, SpriteRenderer renderer)
        {
            var scene = Scenes.GamePlay;
            var currentTime = scene.CurrentTime;

            if (time <= currentTime || time >= currentTime + Constants.VisibleNotesTimeRange) {
                renderer.gameObject.SetActive(false);
            }
            else {
                renderer.gameObject.SetActive(true);
                var transform = renderer.gameObject.transform;
                transform.position = transform.position.WithZ(Constants.TimeToZ(time - currentTime));
            }
        }

        private void OnFingerUpdate(LeanFinger finger)
        {
            if (!IsFingerOnStage(finger))
                return;

            var delta = finger.ScreenDelta.y;
            var scene = Scenes.GamePlay;
            scene.SetTime(scene.CurrentTime - delta * _draggingSensitivity);
            scene.UpdateNotes();
        }

        private bool IsFingerOnStage(LeanFinger finger)
        {
            if (finger.IsOverGui)
                return false;

            return true;
        }

        public void MarkCurrentTimeAsSliceStart()
        {
            var scene = Scenes.GamePlay;
            _startTime = scene.CurrentTime;
            if (_startTime > _endTime) {
                (_startTime, _endTime) = (_endTime, _startTime);
            }
            scene.UpdateNotes();
        }

        public void MarkCurrentTimeAsSliceEnd()
        {
            var scene = Scenes.GamePlay;
            _endTime = scene.CurrentTime;
            if (_startTime > _endTime) {
                (_startTime, _endTime) = (_endTime, _startTime);
            }
            scene.UpdateNotes();
        }

        public bool IsNoteSelected(float noteTime)
        {
            return noteTime >= _startTime && noteTime <= _endTime;
        }

        public ChartSliceInfoModel SaveSlice()
        {
            var scene = Scenes.GamePlay;
            int i;
            Note note = null;
            for (i = 0; i < scene.Chart.Notes.Count; i++) {
                note = scene.Chart.Notes[i];
                if (note.Time >= _startTime)
                    break;
            }
            if (note is null) {
                return null;
            }

            int sliceStartIndex = i;
            float sliceStartTime = note.Time;
            float sliceEndTime = sliceStartTime;
            for (; i < scene.Chart.Notes.Count; i++) {
                note = scene.Chart.Notes[i];
                if (note.Time > _endTime)
                    break;
                sliceEndTime = Mathf.Max(sliceEndTime, note.EndTime);
            }
            int sliceEndIndex = i;

            if (sliceStartIndex == sliceEndIndex) {
                return null; // no note selected
            }

            return new(sliceStartIndex, sliceEndIndex, sliceStartTime, sliceEndTime);
        }

        public void OnCurrentTimeChanged(float time)
        {
            if (time < _startTime) {
                _asEndText.text = "Reverse &\n mark start";
                var transf = _asSliceEndButton.transform as RectTransform;
                transf.sizeDelta = new(transf.sizeDelta.x, 120);
            }
            else {
                _asEndText.text = "Mark end";
                var transf = _asSliceEndButton.transform as RectTransform;
                transf.sizeDelta = new(transf.sizeDelta.x, 50);
            }

            if (time > _endTime) {
                _asStartText.text = "Reverse &\n mark end";
                var transf = _asSliceStartButton.transform as RectTransform;
                transf.sizeDelta = new(transf.sizeDelta.x, 120);
            }
            else {
                _asStartText.text = "Mark start";
                var transf = _asSliceStartButton.transform as RectTransform;
                transf.sizeDelta = new(transf.sizeDelta.x, 50);
            }
        }
    }
}