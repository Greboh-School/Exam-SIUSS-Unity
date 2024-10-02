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

//     public View(Canvas view, Canvas otherView, APIHandler apiHandler)
//     {
//         this.apiHandler = apiHandler;
//         this.view = view;
//         this.otherView = otherView;
//         username = view.transform.Find("username").GetComponent<TMP_InputField>();
//         password = view.transform.Find("password").GetComponent<TMP_InputField>();
//
//         username.Select();
//
//         viewChange = view.transform.Find("viewChange").GetComponent<Button>();
//         viewChange.onClick.AddListener(() => ChangeView(true, false));
//
//         confirm = view.transform.Find("confirm").GetComponent<Button>();
//         confirm.onClick.AddListener(ReadValues);
//
//         error = view.transform.Find("error").GetComponent<TMP_Text>();
//         error.text = string.Empty;
//     }
//
//     public void Update()
//     {
//         if (view.enabled)
//         {
//             if (Input.GetKeyDown(KeyCode.Tab))
//             {
//                 SwitchInputField();
//             }
//
//             if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
//             {
//                 ReadValues();
//             }
//         }
//     }
//
//     protected virtual void SwitchInputField()
//     {
//         if (username.isFocused)
//         {
//             password.Select();
//         }
//         else if (password.isFocused)
//         {
//             username.Select();
//         }
//     }
//
//     protected virtual async void ReadValues() { }
//
//     protected void OnLoginSucces(string response)
//     {
//         Debug.Log(response);
//
//         NetworkHandler netSession = GameObject.Find("NetworkManager").GetComponent<NetworkHandler>();
//
//         //  netSession.jwt = response;
//
//         // apiHandler.GetServer(OnRegistryAPISucces, OnError);
//     }
//
//     protected void OnError(string errorText)
//     {
//         error.text = errorText;
//     }
//
//     /*private void OnRegistryAPISucces(DTO.IPResponse IP)
//     {
//         GameObject networkObject = GameObject.Find("Network Manager");
//         NetworkHandler network = networkObject.GetComponent<NetworkHandler>();
//
//         Debug.Log($"IP setting to {IP}");
//
//         network.serverIP = $"{IP.IP}:{IP.Port}";
//         network.StartSession();
//
//         ChangeView(false, false);
//     }*/
//
//     private void ChangeView(bool other, bool self)
//     {
//         otherView.enabled = other;
//         error.text = string.Empty;
//         view.enabled = self;
//     }
//     }
// }