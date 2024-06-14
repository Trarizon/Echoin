using Echoin.Models.InfoModels;
using Echoin.Utility;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Echoin.Scene.MusicSelect
{
    public sealed class MusicListController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] Transform _contentTransform;

        [Header("Prefabs")]
        [SerializeField] MusicListItemController _itemPrefab;

        private MusicInfoModel[] _musicInfoModels;
        private MusicListItemController[] _musicListItems;

        public int SelectedIndex { get; set; }
        public MusicInfoModel[] MusicInfoModels => _musicInfoModels;
        public MusicInfoModel SelectedMusic => SelectedIndex < 0 ? null : _musicInfoModels[SelectedIndex];

        public void OnCreate()
        {
            _musicInfoModels = GlobalSettings.LoadMusicInfos();

            _musicListItems = new MusicListItemController[_musicInfoModels.Length];
            for (int i = 0; i < _musicInfoModels.Length; i++) {
                var model = _musicInfoModels[i];
                ref var item = ref _musicListItems[i];
                item = Instantiate(_itemPrefab, _contentTransform);
                item.Initialize(model, i);
            }
        }

        public void Select(int index)
        {
            _musicListItems[SelectedIndex].Unselect();
            SelectedIndex = index;
            _musicListItems[SelectedIndex].Select();
        }

        public void Activate()
        {
            foreach (var item in _musicListItems) {
                item.Activate();
            }
        }

        public void Deactivate()
        {
            foreach (var item in _musicListItems) {
                item.Deactivate();
            }
        }
    }
}