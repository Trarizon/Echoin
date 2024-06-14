using System.Diagnostics;
using UnityEngine;

namespace Echoin.Utility
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        private static T _instance;
        public static T Instance
        {
            [DebuggerStepThrough]
            get => _instance;
        }

        protected void Awake()
        {
            if (_instance is null) {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
                Instance.OnAwake(true);
            }
            else {
                Destroy(gameObject);
                Instance.gameObject.SetActive(true);
                Instance.OnAwake(false);
            }
        }

        protected virtual void OnAwake(bool onCreating)
        {

        }
    }
}