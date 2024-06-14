using Echoin.Models.InfoModels;
using Echoin.Scene.GamePlay;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Echoin.Scene.MusicSelect
{
    public sealed class ChartListItemController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] TMP_Text _chartInfoDisplayText;
        [SerializeField] TMP_Text _scoreInfoDisplayText;
        [SerializeField] Transform _slicePanelTransform;
        // [SerializeField] Button _sliceButton;

        public Transform SlicePanelTransform => _slicePanelTransform;

        private List<SliceListItemController> _sliceItems;
        private int _selfIndex;
        private ChartInfoModel _model;

        /// <summary>
        /// true时，再次点击开始游戏
        /// false时，点击打开slice列表
        /// </summary>
        private bool IsSelected { get; set; }
        public int SelfIndex => _selfIndex;
        public ChartInfoModel Model => _model;

        public void Initialize(ChartInfoModel model, int selfIndex)
        {
            _selfIndex = selfIndex;
            _model = model;
            _chartInfoDisplayText.text = $"{model.Difficulty} {model.Level}";
            if (model.ScoreInfo is null)
                _scoreInfoDisplayText.text = $"0.00 %";
            else
                _scoreInfoDisplayText.text = $"{model.ScoreInfo.Score:F2} % {model.ScoreInfo.ScoreRank.ToDisplayString()}";
        }

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            IsSelected = false;
            CollapseSliceList();
            _sliceItems = null;
            gameObject.SetActive(false);
        }

        public void OnSelect()
        {
            if (IsSelected) {
                Scenes.MusicSelect.LoadGamePlay(_selfIndex, GamePlayMode.Play);
                return;
            }
            else {
#pragma warning disable UNT0008 // here null means no item selected
                Scenes.MusicSelect.ChartListController.SelectedItem?.Unselect();
#pragma warning restore UNT0008
                Scenes.MusicSelect.ChartListController.SelectedIndex = _selfIndex;
                IsSelected = true;
                ExpandSliceList();
            }
        }

        private void Unselect()
        {
            IsSelected = false;
            CollapseSliceList();
        }

        public void ToChartSlicing()
        {
            Scenes.MusicSelect.LoadGamePlay(_selfIndex, GamePlayMode.Slicing);
        }

        public void ExpandSliceList()
        {
            // _sliceButton.gameObject.SetActive(true);

            var scene = Scenes.MusicSelect;
            if (_model.SliceInfos is null) {
                GlobalSettings.LoadSliceInfos(scene.MusicListController.SelectedMusic, _selfIndex);
                _model.SliceInfos ??= new();
            }

            _slicePanelTransform.gameObject.SetActive(true);
            _sliceItems ??= new();
            for (int i = 0; i < _model.SliceInfos.Count; i++) {
                var sliceItem = scene.ChartListController.GetPooledSliceListItem(this);
                sliceItem.Initialize(this, i);
                sliceItem.Activate();
                _sliceItems.Add(sliceItem);
            }
        }

        public void CollapseSliceList()
        {
            // _sliceButton.gameObject.SetActive(false);

            var scene = Scenes.MusicSelect;
            
            if (_sliceItems is not null) {
                foreach (var slice in _sliceItems) {
                    scene.ChartListController.ReleasePooledSliceListItem(slice);
                }
                _sliceItems.Clear();
            }
            _slicePanelTransform.gameObject.SetActive(false);
        }
    }
}