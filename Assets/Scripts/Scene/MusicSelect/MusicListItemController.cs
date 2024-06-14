using Echoin.Models.InfoModels;
using Echoin.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Echoin.Scene.MusicSelect
{
    public sealed class MusicListItemController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] TMP_Text _musicNameText;
        [SerializeField] Image _bgImage;

        private int _selfIndex;
        private MusicInfoModel _model;

        // TODO: mark Selected
        private bool IsSelected => _selfIndex == Scenes.MusicSelect.MusicListController.SelectedIndex;

        public void Initialize(MusicInfoModel model, int index)
        {
            _model = model;
            _selfIndex = index;
            _musicNameText.text = model.Name;
        }

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        public void Select()
        {
            _bgImage.color = _bgImage.color.WithAlpha(1);

            var scene = Scenes.MusicSelect;
            scene.InfoPanelController.Initialize(_model);
            scene.ChartListController.Initialize(_model);
            scene.PlayMusic(_model);
        }

        public void Unselect()
        {
            _bgImage.color = _bgImage.color.WithAlpha(0);
        }

        public void OnClick()
        {
            var scene = Scenes.MusicSelect;
            scene.MusicListController.Select(_selfIndex);
        }
    }
}