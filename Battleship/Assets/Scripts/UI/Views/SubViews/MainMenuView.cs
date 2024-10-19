using Assets.Scripts.API.Models.DTOs;
using Assets.Scripts.API.Models.Requests;
using Network;
using Player;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
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
            var getServerRequest = new PlayerConnectionRequest
            { 
                UserName = _profileManager.Profile.Username,
                UserId=_profileManager.Profile.UserId
            };

            var dto = await RegistryClient.GetServer(getServerRequest);

            if (dto is null)
            {
                Debug.LogError("Failed getting valid response from Registry");

                return;
            }

            if(dto.ServerAddress is null)
            {
                Debug.LogError("Registry returned DTO with no serverAddress");

                return;
            }

            var networkSession = FindObjectOfType<NetworkHandler>();

            networkSession.serverIP = $"{dto.ServerAddress}:{dto.ServerPort}";
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