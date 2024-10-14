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

        protected APIHandler APIHandler;
        protected ViewManager ViewManager;
        protected RegistryClient RegistryClient;
        
        protected virtual void Awake()
        {
            APIHandler = FindFirstObjectByType<APIHandler>();
            ViewManager = FindFirstObjectByType<ViewManager>();
            RegistryClient = FindFirstObjectByType<RegistryClient>();
            
            if (APIHandler is null)
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