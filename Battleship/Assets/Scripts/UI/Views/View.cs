using Assets.Scripts.API.Clients;
using UnityEngine;

public enum ViewType
{
    Unknown = 0,
    Login,
    Registration,
    MainMenu,
    GameHUD
}

namespace Views
{
    public abstract class View : MonoBehaviour
    {
        public abstract ViewType ViewType { get; }

        protected APIHandler API;
        protected ViewManager ViewManager;
        protected RegistryClient RegistryClient;
        
        protected virtual void Awake()
        {
            API = FindFirstObjectByType<APIHandler>();
            ViewManager = FindFirstObjectByType<ViewManager>();
            RegistryClient = FindFirstObjectByType<RegistryClient>();
            
            if (API is null)
            {
                Debug.LogError("APIHandler is null");
            }
            if (ViewManager is null)
            {
                Debug.LogError("ViewManager is null");
            }
        }
    }
}