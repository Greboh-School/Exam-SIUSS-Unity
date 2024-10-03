using Assets.Scripts.API.Models.Requests;
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
            _getServer?.onClick.AddListener(onGetServerClicked);
            _logOut?.onClick.AddListener(onLogOutClicked);
            _exit?.onClick.AddListener(onExitClicked);

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

        private async void onGetServerClicked()
        {
            var getServerRequest = new GetServerRequest { AccessToken = _profileManager.Profile.AccessToken };

            var dto = await RegistryClient.GetServer(getServerRequest);

            if (dto is null)
            {
                Debug.LogError("Failed attempt at getting ServerIP from Registry");
            }
            else
            {
                var networkSession = FindObjectOfType<NetworkHandler>();

                networkSession.serverIP = dto.IP;
                networkSession.StartSession();
            }
        }

        private void onLogOutClicked()
        {
            _profileManager.Profile = null;

            _loggedInAs.text = string.Empty;

            ViewManager.SwitchView(ViewType.Login);
        }

        private void onExitClicked()
        {
            Application.Quit();
        }
    }
}