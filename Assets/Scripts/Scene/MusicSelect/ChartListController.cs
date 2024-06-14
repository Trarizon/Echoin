using Echoin.Models.InfoModels;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Echoin.Scene.MusicSelect
{
    public sealed class ChartListController : MonoBehaviour
    {
        private MusicInfoModel _model;

        [Header("UI")]
        [SerializeField] Transform _contentTransform;
        [SerializeField] VerticalLayoutGroup _verticalLG;

        [Header("Prefabs")]
        [SerializeField] ChartListItemController _chartListItemPrefab;
        [SerializeField] SliceListItemController _sliceListItemPrefab;

        private ObjectPool<ChartListItemController> _chartListItemPool;
        private ObjectPool<SliceListItemController> _sliceListItemPool;

        private List<ChartListItemController> _items;

        public int SelectedIndex { get; set; }

        public ChartListItemController SelectedItem => SelectedIndex < 0 ? null : _items[SelectedIndex];

        public void OnCreate()
        {
            _chartListItemPool = new ObjectPool<ChartListItemController>(
                () => Instantiate(_chartListItemPrefab, _contentTransform),
                actionOnGet: item => item.Activate(),
                actionOnRelease: item => item.Deactivate());

            _sliceListItemPool = new ObjectPool<SliceListItemController>(
                () => Instantiate(_sliceListItemPrefab),
                actionOnGet: item => item.Activate(),
                actionOnRelease: item => item.Deactivate());
        }

        public void Initialize(MusicInfoModel model)
        {
            _model = model;
            if (_items is null) {
                _items = new();
            }
            else {
                foreach (var item in _items) {
                    ReleasePooledChartListItem(item);
                }
                _items.Clear();
            }
            SelectedIndex = -1;

            for (int i = 0; i < _model.ChartInfos.Count; i++) {
                var cht = _model.ChartInfos[i];
                cht.ScoreInfo = GlobalSettings.LoadScore(model, i);
                _items.Add(GetPooledChartListItem(cht, i));
            }
        }

        private ChartListItemController GetPooledChartListItem(ChartInfoModel chartInfo, int index)
        {
            var ctrler = _chartListItemPool.Get();
            ctrler.Initialize(chartInfo, index);
            ctrler.transform.SetSiblingIndex(index);
            return ctrler;
        }

        private void ReleasePooledChartListItem(ChartListItemController chartListItem)
        {
            _chartListItemPool.Release(chartListItem);
        }

        public SliceListItemController GetPooledSliceListItem(ChartListItemController chartListItem)
        {
            var slice = _sliceListItemPool.Get();
            slice.gameObject.transform.SetParent(chartListItem.SlicePanelTransform, false);
            //slice.gameObject.transform.parent = chartListItem.SlicePanelTransform;
            return slice;
        }

        public void ReleasePooledSliceListItem(SliceListItemController sliceListItem)
        {
            _sliceListItemPool.Release(sliceListItem);
        }

        public void Select(ChartListItemController item)
        {
            if (SelectedIndex >= 0) {
                _items[SelectedIndex].CollapseSliceList();
            }
            SelectedIndex = item.SelfIndex;
            _items[SelectedIndex].ExpandSliceList();
        }
    }
}