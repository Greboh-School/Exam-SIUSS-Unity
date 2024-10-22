using Assets.Scripts.API.Models.Requests;
using Network;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace Assets.Scripts.UI.Views.SubViews
{
    public class MainMenuView : View
    {
        [Header("Text Fields")]
        [SerializeField]
        private TMP_Text _loggedInAs;

        [Header("Buttons")]
        [SerializeField]
        private Button _getServer;
        [SerializeField]
        private Button _logOut;
        [SerializeField]
        private Button _exit;

        private ProfileManager _profileManager;
        public override ViewType ViewType => ViewType.MainMenu;

        protected override void Awake()
        {
            base.Awake();

            if (_getServer is null)
            {
                Debug.LogError("GetServer button is null");
            }
            if (_logOut is null)
            {
                Debug.LogError("Logout button is null");
            }
            if (_exit is null)
            {
                Debug.LogError("exit button is null");
            }
        }

        private void Start()
        {
            _getServer?.onClick.AddListener(OnGetServerClicked);
            _logOut?.onClick.AddListener(OnLogOutClicked);
            _exit?.onClick.AddListener(OnExitClicked);

            _profileManager = FindObjectOfType<ProfileManager>();
        }

        public void DisplayPlayerProfile(PlayerProfile profile)
        {
            string roles = string.Empty;
            foreach (var role in profile.Roles)
            {
                roles += $"{role}";
            }

            _loggedInAs.text = $"Welcome {profile.Username} !\n\n" +
                $"{profile.UserId}\n" +
                $"{roles}";
        }

        private async void OnGetServerClicked()
        {
            //TODO: Implement Registry once its done!
            //var getServerRequest = new GetServerRequest { AccessToken = _profileManager.Profile.AccessToken };
            //var dto = await RegistryClient.GetServer(getServerRequest);

<<<<<<< Updated upstream
            //TODO: Remove debug Bypass once Registry is done!
            var dto = new API.Models.DTOs.ServerDTO { IP = "127.0.0.1", Port = "40000"}; //TODO: REMOVE DEBUG BYPASS
=======
            var dto = await API.RegisterClient(request);
>>>>>>> Stashed changes

            if (dto is null)
            {
                Debug.LogError("Failed attempt at getting ServerIP from Registry");

                return;
            }

            var networkSession = FindObjectOfType<NetworkHandler>();

            networkSession.serverIP = $"{dto.IP}:{dto.Port}";
            networkSession.StartSession();

            ViewManager.SwitchView(ViewType.GameHUD);
        }

        private void OnLogOutClicked()
        {
            _profileManager.Profile = null;

            _loggedInAs.text = string.Empty;

            ViewManager.SwitchView(ViewType.Login);
        }

        private void OnExitClicked()
        {
            Application.Quit();
        }
    }
}