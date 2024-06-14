using Echoin.Utility;
using UnityEngine;

namespace Echoin.Scene.GamePlay
{
    public sealed class NoteNodeController : MonoBehaviour
    {
        private NoteController _noteController;
        // public SpriteRenderer noteHeadRenderer;
        [SerializeField]
        //private SpriteRenderer _noteBodyRenderer;
        private MeshRenderer _meshRenderer;

        public float StartPosition;
        public float EndPosition;
        public float StartTime;
        public float Duration;

        public float EndTime => StartTime + Duration;

        public void Initialize(NoteController noteController, float headPosition, float endPosition, float startTime, float duration)
        {
            var scene = Scenes.GamePlay;

            _noteController = noteController;

            StartPosition = headPosition;
            EndPosition = endPosition;
            StartTime = startTime;
            Duration = duration;

            _meshRenderer.material = new Material(scene.HoldBodyMaterialPrefab);

            gameObject.transform.position = new Vector3(
                Constants.NotePositionToX(StartPosition),
                gameObject.transform.position.y,
                0);
            // 绘制body
            //_noteBodyRenderer.sprite = scene.HoldBodySpritePrefab;
            //_noteBodyRenderer.color = _noteBodyRenderer.color.WithAlpha(1f);
            UpdateNotePosition();
            //RenderNoteBody();
            //Skew(1);

            // 选择head sprite
            // noteHeadRenderer.sprite = scene.HoldNodeNoteSpritePrefab;

            // 重设color alpha
            //var color = noteHeadRenderer.color;
            //color.a = 1f;
            //noteHeadRenderer.color = color;

        }

        void UpdateNotePosition()
        {
            var scene = Scenes.GamePlay;

            float renderStartTime;
            float renderStartPos;
            if (StartTime > scene.CurrentMusicTime) {
                renderStartPos = StartPosition;
                renderStartTime = StartTime;
            }
            else {
                renderStartTime = scene.CurrentMusicTime;
                // (rsp - sp) / (ep - sp) = (rst - st) / (et - st)
                // rsp = (rst - st) / (et - st) * (ep - sp) + sp
                renderStartPos = (renderStartTime - StartTime) / Duration * (EndPosition - StartPosition) + StartPosition;
            }
            renderStartTime -= _noteController.NoteTime;
            renderStartPos -= _noteController.Note.Position;
            var renderEndTime = EndTime - _noteController.NoteTime;
            var renderEndPos = EndPosition - _noteController.Note.Position;
            var renderTime = (renderStartTime + renderEndTime) / 2;
            var renderTimeForScaleZ = (renderTime - renderStartTime) / 5f;

            var x = Constants.NotePositionToX((renderStartPos + renderEndPos) / 2);
            var z = Constants.TimeToZ((renderStartTime + renderEndTime) / 2);
            var skewShear = (renderEndPos - renderStartPos) * 2.5f;

            _meshRenderer.material.SetFloat("_Shear", skewShear);

            gameObject.transform.localPosition = new Vector3(
                x,
                gameObject.transform.localPosition.y,
                z);

            gameObject.transform.localScale = gameObject.transform.localScale.WithZ(Constants.TimeToZ(renderTimeForScaleZ));
        }

        /// <summary>
        /// 当hold到达判定线时，线下部分应当不可见
        /// </summary>
        public void UpdateOnLineNode()
        {
            var scene = Scenes.GamePlay;
            Debug.Assert(scene.CurrentMusicTime >= StartTime);
            if (scene.CurrentMusicTime > EndTime) {
                OnRelease();
                return;
            }
            UpdateNotePosition();
        }

        private void RenderNoteBody()
        {
            var scene = Scenes.GamePlay;

            // start = startTime - noteTime
            // end   = EndTime - noteTime
            // mid   = (start + end) / 2
            //       = (startTime - noteTime + EndTime - noteTime) / 2
            //       = (startTime + EndTime) / 2 - noteTime
            var start = Mathf.Max(StartTime, scene.CurrentMusicTime) - _noteController.NoteTime;
            var mid = (start + EndTime - _noteController.NoteTime) / 2;
            gameObject.transform.localPosition = gameObject.transform.localPosition.WithZ(Constants.TimeToZ(mid));

            // 当startTime == noteTime时，这个值是 mid / 1.9 * 5
            // 也就是 (mid - start) / 1.9 * 5
            gameObject.transform.localScale = gameObject.transform.localScale.WithY(Constants.TimeToZ(mid - start) / 1.9f * 5f);
        }

        public void OnRelease()
        {
            gameObject.SetActive(false);
            //_noteBodyRenderer.sprite = null;
        }

        /// <summary>
        /// 将alpha调整为0.5，表示已miss
        /// </summary>
        public void OnHoldReleased()
        {
            //TODO:将alpha调整为0.5，表示已miss
            //_meshRenderer.material.color = _meshRenderer.material.color.WithAlpha(0.5f);
            //_noteBodyRenderer.color = _noteBodyRenderer.color.WithAlpha(0.5f);
        }
    }
}
