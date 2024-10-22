using System.Threading.Tasks;
using Assets.Scripts.API.Models.DTOs;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace Assets.Scripts.UI.Views.SubViews
{
    public class GameHUD : View
    {
        [Header("Buttons")]
        [SerializeField]
        private Button _publicMessageButton;
        [SerializeField]
        private Button _privateMessageButton;
       
        [Header("InputFields")]
        [SerializeField]
        private TMP_InputField _privateTarget;

        [Header("TextFields")]
        [SerializeField]
        private TMP_Text _messageField;
        
        private PlayerProfile _profile;
        
        public override ViewType ViewType => ViewType.GameHUD;

        protected override void Awake()
        {
            base.Awake();
        
            if (_publicMessageButton is null)
            {
                Debug.LogError("button is null");
            }
            if (_privateMessageButton is null)
            {
                Debug.LogError("button is null");
            }        
        }

        private void Start()
        {
            _profile = FindFirstObjectByType<ProfileManager>().Profile;

            _privateMessageButton?.onClick.AddListener(OnMessagePrivateButtonClicked);
            if (!_profile.IsAdmin())
            {
                _publicMessageButton.interactable = false;
                return;
            }
            _publicMessageButton?.onClick.AddListener(OnMessagePublicButtonClicked);
        }

        private async void OnMessagePrivateButtonClicked()
        {
            if (string.IsNullOrWhiteSpace(_privateTarget.text))
            {
                return;
            }
            
            var message = new MessageDTO
            {
                Type = MessageType.Private,
                Content = "This is a private message!",
                Sender = _profile.Username,
                Recipient = _privateTarget.text
            };
            
            await API.SendMessage(message);
        }

        private async void OnMessagePublicButtonClicked()
        {
            var message = new MessageDTO
            {
                Type = MessageType.Public,
                Content = "This is a public message!",
                Sender = null,
                Recipient = null
            };

            await API.SendMessage(message);
        }
        
        public void SetMessage(string messageType, string message, string sender)
        {
            _messageField.text = 
                $"{messageType} message received from: {sender}: '{message}' ";
        }
        
        public void SetErrorMessage(string message)
        {
            _messageField.text = message;
        }
        
    }
}