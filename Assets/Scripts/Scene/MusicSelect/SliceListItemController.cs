using Echoin.Models.InfoModels;
using TMPro;
using UnityEngine;

namespace Echoin.Scene.MusicSelect
{
    public sealed class SliceListItemController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] TMP_Text _displayText;

        private ChartListItemController _chartItem;

        private int _selfIndex;
        private ChartSliceInfoModel _model;

        public void Initialize(ChartListItemController chartItem, int index)
        {
            _chartItem = chartItem;
            _selfIndex = index;
            _model = chartItem.Model.SliceInfos[index];
            _displayText.text = $"{_model.Name}({_model.StartTime:F2} - {_model.EndTime:F2})";
        }

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        public void OnSelect()
        {
            var scene = Scenes.MusicSelect;
            scene.LoadGamePlay(_chartItem.SelfIndex, _selfIndex, GamePlayMode.Play);
        }
    }
}