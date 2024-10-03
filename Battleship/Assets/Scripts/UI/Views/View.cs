using UnityEngine;

public enum ViewType
{
    Unknown = 0,
    Login,
    Registration,
}

namespace Views
{
    public abstract class View : MonoBehaviour
    {
        public abstract ViewType ViewType { get; }

        protected APIHandler APIHandler;
        protected ViewManager ViewManager;
        
        protected virtual void Awake()
        {
            APIHandler = FindFirstObjectByType<APIHandler>();
            ViewManager = FindFirstObjectByType<ViewManager>();
            
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