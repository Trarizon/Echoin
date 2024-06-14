using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Echoin.Scene.MusicSelect
{
    public sealed class SettingsController : MonoBehaviour
    {
        private const float MaxOffset = 0.5f;
        private const float MinOffset = 0f;
        private const int MaxSpeed = 9;
        private const int MinSpeed = 1;

        [Header("UI")]
        [SerializeField] TMP_Text _offsetText;
        [SerializeField] Button _decButton;
        [SerializeField] Button _incButton;
        [SerializeField] TMP_Text _speedText;
        [SerializeField] Button _speedDecButton;
        [SerializeField] Button _speedIncButton;

        private float _offset;
        private int _speed;

        public void Activate()
        {
            _offset = GlobalSettings.ChartOffset;
            _speed = GlobalSettings.Speed;
            gameObject.SetActive(true);
            UpdateSettingsText();
            UpdateSpeedText();
        }

        public void Deactivate()
        {
            GlobalSettings.ChartOffset = _offset;
            GlobalSettings.Speed = _speed;
            GlobalSettings.SaveSettings();
            gameObject.SetActive(false);
        }

        public void OffsetDec()
        {
            _offset = Mathf.Clamp(_offset - 0.01f, MinOffset, MaxOffset);
            UpdateSettingsText();
        }

        public void OffsetInc()
        {
            _offset = Mathf.Clamp(_offset + 0.01f, MinOffset, MaxOffset);
            UpdateSettingsText();
        }

        public void SpeedDec()
        {
            _speed = Mathf.Clamp(_speed - 1, MinSpeed, MaxSpeed);
            UpdateSpeedText();
        }

        public void SpeedInc()
        {
            _speed = Mathf.Clamp(_speed + 1, MinSpeed, MaxSpeed);
            UpdateSpeedText();
        }

        private void UpdateSettingsText()
        {
            _offsetText.text = $"{_offset:F2}";
            _decButton.gameObject.SetActive(_offset > MinOffset);
            _incButton.gameObject.SetActive(_offset < MaxOffset);

        }

        private void UpdateSpeedText()
        {
            _speedText.text = $"{_speed}";
            _speedDecButton.gameObject.SetActive(_speed > MinSpeed);
            _speedIncButton.gameObject.SetActive(_speed < MaxSpeed);
        }
    }
}