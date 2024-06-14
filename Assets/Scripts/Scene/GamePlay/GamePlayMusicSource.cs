using UnityEngine;

namespace Echoin.Scene.GamePlay
{
    public sealed class GamePlayMusicSource : MonoBehaviour
    {
        [SerializeField]
        private AudioSource _audioSource;

        public AudioClip Clip
        {
            get => _audioSource.clip;
            set => _audioSource.clip = value;
        }

        public float StartTime { get; set; }
        public float EndTime { get; set; }

        public bool Fade { get; set; }

        public bool IsPaused => !_audioSource.isPlaying;

        public float FadeTime => 0.5f;

        // Manually sync
        private float _currentMusicTime;
        public float CurrentMusicTime
        {
            get => _currentMusicTime;
            set => _audioSource.time = _currentMusicTime = value;
        }

        public void SyncMusicTime()
        {
            _currentMusicTime = _audioSource.time;
        }

        public float CurrentTime
        {
            get => CurrentMusicTime - StartTime;
            set => CurrentMusicTime = value + StartTime;
        }

        public float PlayTotalTime => EndTime - StartTime;

        public bool IsEnd { get; private set; }

        private void Update()
        {

            if (Fade) {
                // 每帧需要同步
                var musicTime = _audioSource.time;
                if (musicTime < StartTime + FadeTime) {
                    _audioSource.volume = (musicTime - StartTime) / FadeTime;
                }
                else if (musicTime > EndTime - FadeTime) {
                    _audioSource.volume = (EndTime - musicTime) / FadeTime;
                }
            }

            if (_audioSource.time >= EndTime) {
                _audioSource.Stop();
                IsEnd = true;
            }
        }

        public void Play()
        {
            _audioSource.Play();
        }

        public void Pause()
        {
            _audioSource.Pause();
        }

        public void Replay()
        {
            CurrentTime = 0f;
            _audioSource.Play();
        }
    }
}