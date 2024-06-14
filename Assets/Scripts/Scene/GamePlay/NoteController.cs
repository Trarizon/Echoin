using Echoin.Models.ChartModels;
using Echoin.ProjectModels;
using Echoin.Utility;
using System;
using System.Collections;
using UnityEngine;

namespace Echoin.Scene.GamePlay
{
    public sealed class NoteController : MonoBehaviour
    {
        [SerializeField] Transform _noteHeadTransform;
        [SerializeField] Transform _hitEffectTransform;
        [SerializeField] SpriteRenderer _noteHeadRenderer;
        [SerializeField] SpriteRenderer _hitEffectRenderer;
        //[Obsolete]
        //public Transform noteBodyTransform;
        //[Obsolete]
        //public SpriteRenderer noteBodyRenderer;

        // New design

        private NoteNodeController[] _noteNodes;
        private int _currentNoteNodeIndex;
        private int _noteNodeCount;
        public Span<NoteNodeController> NoteNodes => _noteNodes.AsSpan(0, _noteNodeCount);

        // Datas
        public Note Note;
        public bool IsReadyToRelease => _renderStatus is NoteRenderStatus.ToBeReleased;

        // 当斜hold在判定中时，这个位置会发生变化
        // 对于其他情况，应当和Note.Position一致
        // 在UpdateForHold里顺带更新
        private float _noteRenderPos;

        private NoteJudgementStatus _judgementStatus;
        private NoteGrade _grade;
        // 抬手的空隙时间
        private float _intervalTime;
        private float _hitEffectStartTime;

        public float NoteTime => Scenes.GamePlay.Mode is GamePlayMode.Slicing ? Note.Time : GlobalSettings.OffsetTime(Note.Time);
        public float NoteEndTime => Scenes.GamePlay.Mode is GamePlayMode.Slicing ? Note.EndTime : GlobalSettings.OffsetTime(Note.EndTime);

        private NoteRenderStatus _renderStatus;

        public NoteGrade Grade => _grade;

        public void Initialize(Note note)
        {
            var scene = Scenes.GamePlay;

            Note = note;
            _intervalTime = 0f;
            _renderStatus = NoteRenderStatus.Falling;
            _noteRenderPos = note.Position;

            _judgementStatus = NoteJudgementStatus.Unhitted;
            _grade = NoteGrade.None;
            gameObject.transform.position = new Vector3(Constants.NotePositionToX(note.Position), gameObject.transform.position.y, 0);
            // hold下落时会调整position，因此调回来
            _noteHeadTransform.localPosition = new Vector3();

            // note head的基础scale是1，hiteffect是2，详见editor
            _noteHeadTransform.localScale = new(note.Size, 1, 1);
            _hitEffectTransform.localScale = new(note.Size * 2, 2, 2);

            // 选择sprite
            _noteHeadRenderer.sprite = note.Kind switch {
                Note.NoteKind.Click => scene.ClickNoteSpritePrefab,
                Note.NoteKind.Slide or _ => scene.SlideNoteSpritePrefab,
            };

            if (scene.Mode is GamePlayMode.Play) {
                _noteHeadRenderer.color = new(1, 1, 1, 1);
                StartCoroutine(FadeIn());
            }
            else {
                if (scene.SlicingController.IsNoteSelected(NoteTime)) {
                    _noteHeadRenderer.color = new Color(0.4f, 0.9f, 1f);
                }
                else {
                    _noteHeadRenderer.color = new(1, 1, 1, 1);
                }
            }

            // 绘制hold
            if (note.IsHold) {
                //noteBodyRenderer.sprite = null;
                //noteBodyRenderer.sprite = scene.HoldBodySpritePrefab;
                //defaultColor = noteBodyRenderer.color;
                //defaultColor.a = 0.5f;
                //noteBodyRenderer.color = defaultColor;
                //RenderNoteTailViaTime(0f, note.Nodes[0].Duration);

                // init nodes
                _currentNoteNodeIndex = 0;
                _noteNodeCount = note.Nodes.Count;
                _noteNodes = System.Buffers.ArrayPool<NoteNodeController>.Shared.Rent(_noteNodeCount);

                float startTime = NoteTime;
                float startPos = note.Position;
                for (int i = 0; i < _noteNodeCount; i++) {
                    ref var node = ref _noteNodes[i];
                    var nodeData = note.Nodes[i];
                    var endPos = nodeData.EndPosition;
                    var duration = nodeData.Duration;
                    node = scene.GetPooledNoteNode(this, startPos, endPos, startTime, duration);

                    startTime += duration;
                    startPos = endPos;
                }
            }
            else {
                //noteBodyRenderer.sprite = null;
                _currentNoteNodeIndex = 0;
                _noteNodeCount = 0;
                _noteNodes = null;
            }

            _hitEffectRenderer.sprite = null;

            IEnumerator FadeIn()
            {
                const float TotalTime = 0.5f;

                var time = 0f;
                while (_renderStatus is NoteRenderStatus.Falling && time <= TotalTime) {
                    _noteHeadRenderer.color = _noteHeadRenderer.color.WithAlpha(Mathf.Lerp(0, 1, time / TotalTime));
                    yield return null;
                    time += Time.deltaTime;
                }
                _noteHeadRenderer.color = _noteHeadRenderer.color.WithAlpha(1);
            }
        }

        public void UpdateNoteRender()
        {
            const float ThresholdUnderLine = -0.25f;

            if (_renderStatus is NoteRenderStatus.ToBeReleased)
                return;

            if (Note.IsHold)
                UpdatePositionForHold();
            else if (Note.Kind == Note.NoteKind.Click)
                UpdatePositionForClick();
            else
                UpdatePositionForSlide();


            void UpdateHitEffect()
            {
                Debug.Assert(_renderStatus is NoteRenderStatus.HitEffect);

                var scene = Scenes.GamePlay;

                var timeOffsetHitTime = scene.CurrentMusicTime - _hitEffectStartTime;
                var index = Mathf.FloorToInt(timeOffsetHitTime / Constants.HitEffectFrameSpeed);
                if (index >= scene.HitEffectFrameSpritesPrefab.Length) {
                    SwitchToReleaseRenderStatus();
                    return;
                }
                _hitEffectRenderer.sprite = scene.HitEffectFrameSpritesPrefab[index];
            }

            void UpdatePositionForClick()
            {
                Debug.Assert(NoteTime == NoteEndTime);

                if (_renderStatus is NoteRenderStatus.HitEffect) {
                    UpdateHitEffect();
                    return;
                }

                if (_judgementStatus is NoteJudgementStatus.Judged) {
                    Debug.Log("In UpdPosForClick.Judged");
                    SwitchToHitEffectRenderStatus();
                    UpdateHitEffect();
                    return;
                }

                var scene = Scenes.GamePlay;
                float noteTimeOffset = NoteTime - scene.CurrentMusicTime;

                if (noteTimeOffset > 0f) {
                    gameObject.transform.position = gameObject.transform.position.WithZ(noteTimeOffset * Constants.TimeToZScale);
                    return;
                }

                if (noteTimeOffset > ThresholdUnderLine) {
                    // Under judgeline, we change speed to 0.1x
                    gameObject.transform.position = gameObject.transform.position.WithZ(noteTimeOffset * 0.1f * Constants.TimeToZScale);
                    _noteHeadRenderer.color = _noteHeadRenderer.color.WithAlpha(Mathf.Lerp(1, 0, noteTimeOffset / ThresholdUnderLine));
                    return;
                }

                else {
                    SwitchToJudgedJudgeStatus(NoteGrade.Miss);
                    SwitchToReleaseRenderStatus();
                }
            }

            void UpdatePositionForSlide()
            {
                Debug.Assert(NoteTime == NoteEndTime);

                if (_renderStatus is NoteRenderStatus.HitEffect) {
                    UpdateHitEffect();
                    return;
                }

                var scene = Scenes.GamePlay;
                float noteTimeOffset = NoteTime - scene.CurrentMusicTime;

                if (noteTimeOffset > 0f) {
                    gameObject.transform.position = gameObject.transform.position.WithZ(Constants.TimeToZ(noteTimeOffset));
                    return;
                }

                else if (_judgementStatus is NoteJudgementStatus.Judged) {
                    SwitchToHitEffectRenderStatus();
                    UpdateHitEffect();
                    return;
                }

                else if (noteTimeOffset > ThresholdUnderLine) {
                    // Under judgeline, we change speed to 0.1x
                    gameObject.transform.position = gameObject.transform.position.WithZ(0.1f * Constants.TimeToZ(noteTimeOffset));
                    _noteHeadRenderer.color = _noteHeadRenderer.color.WithAlpha(Mathf.Lerp(1, 0, noteTimeOffset / ThresholdUnderLine));
                }

                else {
                    SwitchToJudgedJudgeStatus(NoteGrade.Miss);
                    SwitchToReleaseRenderStatus();
                }
            }

            void UpdatePositionForHold()
            {
                var scene = Scenes.GamePlay;
                var currentTime = scene.CurrentMusicTime;

                if (_renderStatus is NoteRenderStatus.HitEffect) {
                    UpdateHitEffect();
                    return;
                }
                //if (_judgementStatus is NoteJudgementStatus.ToBeReleased)
                //    return;

                float noteTimeOffset = NoteTime - currentTime;
                float endTimeOffset = NoteEndTime - currentTime;

                // 正常下落
                if (noteTimeOffset > 0) {
                    gameObject.transform.position = gameObject.transform.position.WithZ(Constants.TimeToZ(noteTimeOffset));
                }
                // head已经到线下了，将head锁定在判定线上随note移动，
                // 整体正常下落，并遮住线下部分
                else if (endTimeOffset > 0) {
                    gameObject.transform.position = gameObject.transform.position.WithZ(Constants.TimeToZ(noteTimeOffset));

                    var curNode = _noteNodes[_currentNoteNodeIndex];
                    if (curNode.EndTime < currentTime) {
                        curNode.OnRelease();
                        _currentNoteNodeIndex++;
                        curNode = _noteNodes[_currentNoteNodeIndex];
                    }

                    var headPos = _noteHeadTransform.position;
                    // (curp - hp) / (ep - hp) = (curt - ht) / (et - ht)
                    // curp = hp + (ep - hp) * (curt - ht) / (et - ht)
                    var pos = curNode.StartPosition + (curNode.EndPosition - curNode.StartPosition) * (currentTime - curNode.StartTime) / (curNode.Duration);
                    _noteRenderPos = pos;
                    headPos.x = Constants.NotePositionToX(pos);
                    // 将note固定在判定线上
                    headPos.z = 0f;

                    _noteHeadTransform.position = headPos;

                    curNode.UpdateOnLineNode();

                    // Miss 判定，和click一样，head掉落过久直接miss
                    if (noteTimeOffset < ThresholdUnderLine && _grade is NoteGrade.None) {
                        SwitchToJudgedJudgeStatus(NoteGrade.Miss);
                    }
                }
                // 线下
                else {
                    _noteNodes[_currentNoteNodeIndex].OnRelease();
                    switch (_grade, _judgementStatus) {
                        // 已计分 未结算，此帧之后判定完整
                        case (not NoteGrade.None, < NoteJudgementStatus.Judged):
                            SwitchToJudgedJudgeStatus(_grade);
                            SwitchToHitEffectRenderStatus();
                            break;
                        // 已计分，已结算，该音符任务完成
                        case (not NoteGrade.None, _):
                            SwitchToReleaseRenderStatus();
                            break;
                        // 未计分，hold过短导致来不及线下判定，此时预留一段时间给判定
                        case (NoteGrade.None, _):
                            if (noteTimeOffset < ThresholdUnderLine) {
                                SwitchToJudgedJudgeStatus(NoteGrade.Miss);
                                SwitchToReleaseRenderStatus();
                            }
                            break;
                    }
                    return;
                }
            }
        }

        //private void RenderNoteTailViaTime(float localStartTime, float duration)
        //{
        //    var dz = (localStartTime + duration) * Constants.TimeToZScale / 2f;
        //    noteBodyTransform.localPosition = noteBodyTransform.localPosition.WithZ(dz);
        //    noteBodyTransform.localScale = noteBodyTransform.localScale.WithY(dz / 1.9f * 5f);
        //}

        //private void PrepareForRelease()
        //{
        //    _judgementStatus = NoteJudgementStatus.ToBeReleased;
        //    hitEffectRenderer.sprite = null;
        //}

        public void OnReleased()
        {
            var scene = Scenes.GamePlay;
            if (_noteNodes is not null) {
                foreach (var node in NoteNodes) {
                    scene.ReleasePooledNoteNode(node);
                }
                System.Buffers.ArrayPool<NoteNodeController>.Shared.Return(_noteNodes);
            }
        }

        //public bool TryJudge(float hitTimeOffset)
        //{
        //    _grade = GetGrade(hitTimeOffset);
        //    return _grade != NoteGrade.None;
        //}
        /*
        [Obsolete]
        public void OnHitted()
        {
            switch (Note.Kind) {
                case Note.NoteKind.Click:
                    StartCoroutine(HitClickNoteAsync());
                    break;
                case Note.NoteKind.Slide:
                    StartCoroutine(HitSlideNoteAsync());
                    break;
                default:
                    break;
            }

            IEnumerator HitSlideNoteAsync()
            {
                var scene = Scenes.GamePlay;

                _judgementStatus = NoteJudgementStatus.Holding;

                // wait for note to fall on judge line
                while (scene.CurrentTime - NoteTime < 0f)
                    yield return null;

                // Align z axis of effect
                if (_grade is NoteGrade.Perfect)
                    gameObject.transform.position = gameObject.transform.position.WithZ(0f);

                _noteHeadRenderer.sprite = null;
                scene.ScoreController.UpdateScore(_grade);
                _judgementStatus = NoteJudgementStatus.Judged;

                float timeAfterHit = 0;
                int index = -1;

                while (true) {
                    timeAfterHit += Time.deltaTime;
                    var newIndex = Mathf.FloorToInt(timeAfterHit / Constants.HitEffectFrameSpeed);
                    if (index != newIndex) {
                        if (newIndex >= scene.HitEffectFrameSpritesPrefab.Length)
                            break;
                        index = newIndex;
                        _hitEffectRenderer.sprite = scene.HitEffectFrameSpritesPrefab[index];
                    }
                    yield return null;
                }
                _hitEffectRenderer.sprite = null;
                _judgementStatus = NoteJudgementStatus.ToBeReleased;
            }

            IEnumerator HitClickNoteAsync()
            {
                var scene = Scenes.GamePlay;

                if (_judgementStatus == NoteJudgementStatus.Unhitted) {
                    // Align z axis of effect
                    if (_grade is NoteGrade.Perfect)
                        gameObject.transform.position = gameObject.transform.position.WithZ(0f);

                    _noteHeadRenderer.sprite = null;
                    scene.ScoreController.UpdateScore(_grade);
                    _judgementStatus = NoteJudgementStatus.Judged;
                }
                float timeAfterHit = 0;
                int index = -1;

                while (true) {
                    timeAfterHit += Time.deltaTime;
                    var newIndex = Mathf.FloorToInt(timeAfterHit / Constants.HitEffectFrameSpeed);
                    if (index != newIndex) {
                        if (newIndex >= scene.HitEffectFrameSpritesPrefab.Length)
                            break;
                        index = newIndex;
                        _hitEffectRenderer.sprite = scene.HitEffectFrameSpritesPrefab[index];
                    }
                    yield return null;
                }
                _hitEffectRenderer.sprite = null;
                _judgementStatus = NoteJudgementStatus.ToBeReleased;
            }
        }
        */
        // Judgement

        /// <summary>
        /// 临时标记，用于在下一帧判断hold是否断了
        /// </summary>
        private bool _isHoldTouching;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fingerPosition"></param>
        /// <returns>true表示note判定结束，接下来可以记录分数</returns>
        public void TryCatch(float fingerPosition)
        {
            var scene = Scenes.GamePlay;

            if (_judgementStatus == NoteJudgementStatus.Unhitted) {
                if (Note.Kind != Note.NoteKind.Slide) {
                    return;
                }
                if (!IsPositionOverlap(fingerPosition)) {
                    return;
                }

                var hitTimeOffset = NoteTime - scene.CurrentMusicTime;

                var grade = GetGrade(hitTimeOffset);
                if (grade is NoteGrade.None)
                    return;

                if (Note.IsHold) {
                    SwitchToHoldingJudgeStatus(grade);
                }
                else {
                    SwitchToJudgedJudgeStatus(grade);
                }
                return;
            }

            else if (_judgementStatus == NoteJudgementStatus.Holding) {
                if (!IsPositionOverlap(fingerPosition)) {
                    return;
                }
                _isHoldTouching = true;
                return;
            }

            //else if (_judgementStatus == NoteJudgementStatus.Interval) {
            //    SwitchToHoldingJudgeStatus(_grade);
            //    _intervalTime = 0f;
            //    return;
            //}
            else {
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fingerPosition"></param>
        /// <param name="hitBlocked">当已经有音符被判定时，为true，此时可能阻止后面的音符进行判定</param>
        public void TryHit(float fingerPosition, ref bool hitBlocked)
        {
            var scene = Scenes.GamePlay;

            if (_judgementStatus == NoteJudgementStatus.Unhitted) {
                if (Note.Kind == Note.NoteKind.Click && hitBlocked)
                    return;

                if (!IsPositionOverlap(fingerPosition)) {
                    return;
                }

                var hitTimeOffset = NoteTime - scene.CurrentMusicTime;
                var grade = GetGrade(hitTimeOffset);
                if (grade == NoteGrade.None)
                    return;

                if (Note.IsHold) {
                    if (grade is NoteGrade.Bad) {
                        // 线上bad区间，不判定
                        if (hitTimeOffset > 0)
                            return;
                    }
                    SwitchToHoldingJudgeStatus(grade);
                    hitBlocked = true;
                    return;
                }
                else {
                    SwitchToJudgedJudgeStatus(grade);
                    hitBlocked = true;
                    return;
                }
            }

            else if (_judgementStatus == NoteJudgementStatus.Holding) {
                if (!IsPositionOverlap(fingerPosition))
                    return;

                _isHoldTouching = true;
                return;
            }

            //else if (_judgementStatus == NoteJudgementStatus.Interval) {
            //    SwitchToHoldingJudgeStatus(_grade);
            //    _intervalTime = 0f;
            //    return;
            //}
            else {
                return;
            }
        }

        private void LateUpdate()
        {
            if (_judgementStatus is NoteJudgementStatus.Unhitted)
                return;

            if (_judgementStatus is NoteJudgementStatus.Holding) {
                // 临时标记note为Interval，用于在下一帧的判定
                // 
                // 如果下一frame更新了notestatus，说明有判定行为，
                // 否则，记该note进入断连状态
                if (_isHoldTouching) {
                    _isHoldTouching = false;
                }
                else {
                    // 若提前50ms内松开，则记为判定完全。
                    if (NoteEndTime - Scenes.GamePlay.CurrentMusicTime <= 0.05f) {
                        // Mirrored from UpdateForHold
                        SwitchToJudgedJudgeStatus(_grade);
                        SwitchToHitEffectRenderStatus();
                    }
                    else {
                        SwitchToJudgedJudgeStatus(NoteGrade.HoldReleased);
                    }
                }
            }

            // 该帧没有对hold进行判定，也就是抬手了
            //if (_judgementStatus is NoteJudgementStatus.Interval) {
            //    _intervalTime += Time.deltaTime;
            //    if (_intervalTime > 0.5f)
            //        _judgementStatus = NoteJudgementStatus.Judged;

            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hitPosition"></param>
        /// <param name="hitTime"></param>
        /// <param name="hitTimeOffset">negative means late</param>
        /// <returns></returns>
        private bool IsPositionOverlap(float hitPosition)
        {
            var noteJudgeSize = Note.Size;
            var positionOffset = Mathf.Abs(hitPosition - _noteRenderPos);
            return positionOffset < noteJudgeSize / 2f;
        }

        private NoteGrade GetGrade(float hitTimeOffset)
        {
            const float PerfectRange = 0.05f;
            const float GreatRange = 0.12f;
            const float BadRange = 0.16f;

            return (Note.Kind, hitTimeOffset) switch {
                (Note.NoteKind.Click, <= PerfectRange and >= -PerfectRange) => NoteGrade.Perfect,
                (Note.NoteKind.Click, <= GreatRange and >= -GreatRange) => NoteGrade.Great,
                (Note.NoteKind.Click, <= BadRange and >= -BadRange) => NoteGrade.Bad,
                (Note.NoteKind.Click, _) => NoteGrade.None,

                (Note.NoteKind.Slide, <= GreatRange and >= -PerfectRange) => NoteGrade.Perfect,
                (Note.NoteKind.Slide, < 0f and >= -GreatRange) => NoteGrade.Great,
                (Note.NoteKind.Slide, _) => NoteGrade.None,

                _ => throw new System.InvalidOperationException("Invalid NoteKind"),
            };
        }

        private void SwitchToHoldingJudgeStatus(NoteGrade grade)
        {
            Debug.Assert(Note.IsHold);
            Debug.Assert(grade is not NoteGrade.None);

            var scene = Scenes.GamePlay;
            _noteHeadRenderer.sprite = scene.HoldingNoteSpritePrefab;
            _noteHeadRenderer.color = grade.ToJudgeColor();

            _grade = grade;
            _judgementStatus = NoteJudgementStatus.Holding;
        }

        private void SwitchToJudgedJudgeStatus(NoteGrade grade)
        {
            Debug.Assert(grade is not NoteGrade.None);

            var scene = Scenes.GamePlay;

            _grade = grade;
            _judgementStatus = NoteJudgementStatus.Judged;
            if (Note.IsHold) {
                if (_grade is NoteGrade.Miss) {
                    _noteHeadRenderer.color = _noteHeadRenderer.color.WithAlpha(0.5f);
                }
                else if (_grade is NoteGrade.HoldReleased) {
                    _noteHeadRenderer.sprite = Note.Kind switch {
                        Note.NoteKind.Click => scene.ClickNoteSpritePrefab,
                        Note.NoteKind.Slide => scene.SlideNoteSpritePrefab,
                        _ => scene.ClickNoteSpritePrefab,
                    };
                    foreach (var node in NoteNodes) {
                        node.OnHoldReleased();
                    }
                    _noteHeadRenderer.color = _noteHeadRenderer.color.WithAlpha(0.5f);
                }
                // 有分数，说明按到尾了
                else { }
            }
            else {
                if (Note.Kind is Note.NoteKind.Click)
                    SwitchToHitEffectRenderStatus();
                // slide等到落到线上在转换，在UpdatePosition中进行
            }
            scene.ScoreController.UpdateScore(grade);
        }

        private void SwitchToHitEffectRenderStatus()
        {
            if (_renderStatus is NoteRenderStatus.HitEffect)
                return;

            _renderStatus = NoteRenderStatus.HitEffect;
            // perfect情况下吸附到线上
            if (_grade is NoteGrade.Perfect)
                gameObject.transform.position = gameObject.transform.position.WithZ(0f);
            _noteHeadRenderer.sprite = null;
            _hitEffectTransform.localPosition = _hitEffectTransform.localPosition.WithX(_noteHeadTransform.localPosition.x);
            _hitEffectRenderer.color = _grade.ToJudgeColor();

            var scene = Scenes.GamePlay;
            _hitEffectStartTime = scene.CurrentMusicTime;
        }

        private void SwitchToReleaseRenderStatus()
        {
            _renderStatus = NoteRenderStatus.ToBeReleased;
            _noteHeadRenderer.sprite = null;
            _hitEffectRenderer.sprite = null;
            foreach (var node in NoteNodes) {
                node.OnRelease();
            }
        }

        /// <summary>
        /// Slicing 模式下，scene直接调用以关闭该note
        /// </summary>
        public void OnReleasing()
        {
            SwitchToReleaseRenderStatus();
        }

        private enum NoteRenderStatus
        {
            Falling,
            HitEffect,
            ToBeReleased,
        }

        public enum NoteJudgementStatus
        {
            Unhitted,
            Holding,
            Judged,
        }
    }
}
