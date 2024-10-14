using Network;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Views
{
    public class ViewManager : MonoBehaviour
    {
        [field:Header("References")]
        [field:SerializeField]
        public List<View> Views { get; private set; } = new List<View>();
        
        [Header("Settings")]
        [SerializeField]
        private ViewType _startView = ViewType.Login;

        private View _currentView;
        private View _previousView;

        private void Start()
        {
            var networkType = FindObjectOfType<NetworkHandler>().sessionType;

            if (networkType is NetworkType.Client)
            {
                SwitchView(_startView);
            }
        }
        
        public void SwitchView(ViewType type)
        {
            var view = Views.FirstOrDefault(x => x.ViewType == type);

            if (view is null)
            {
                Debug.LogError($"Tried to switch to a view that does not exist, {type.ToString()}");
                return;
            }
            _currentView?.gameObject.SetActive(false);
            _previousView = _currentView;

            _currentView = view;
            _currentView.gameObject.SetActive(true);
        }

        public void SwitchToPreviousView()
        {
            SwitchView(_previousView.ViewType);
        }
    }
}