using Echoin.Models.InfoModels;
using Echoin.Scene.GamePlay;
using TMPro;
using UnityEngine;

namespace Echoin.Scene.MusicSelect
{
    public sealed class InfoPanelController : MonoBehaviour
    {
        public MusicInfoModel Model;

        [SerializeField]
        private TMP_Text _musicNameText;
        [SerializeField]
        private TMP_Text _composerText;

        public void Initialize(MusicInfoModel model)
        {
            Model = model;
            _musicNameText.text = model.Name;
            _composerText.text = model.Composer;
        }
    }
}