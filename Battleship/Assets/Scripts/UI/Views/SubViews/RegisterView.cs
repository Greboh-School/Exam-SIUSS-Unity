using Assets.Scripts.API.Models.Requests;
using Requests;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Views
{
    public class RegisterView : View
    {
        public override ViewType ViewType => ViewType.Registration;
        
        [Header("Input Fields")]
        [SerializeField]
        private TMP_InputField _username;
        [SerializeField]
        private TMP_InputField _password;

        [FormerlySerializedAs("_loginButton")]
        [Header("Buttons")]
        [SerializeField]
        private Button _registerButton;
        [FormerlySerializedAs("_registrationButton")]
        [SerializeField]
        private Button _backButton;

        protected override void Awake()
        {
            base.Awake();
        
            if (_registerButton is null)
            {
                Debug.LogError("Registration button is null");
            }
            if (_backButton is null)
            {
                Debug.LogError("Back button is null");
            }
        }

        private void Start()
        {
            _registerButton?.onClick.AddListener(OnRegistrationClicked);
            _backButton?.onClick.AddListener(OnBackButtonClicked);
        }

        private async void OnRegistrationClicked()
        {
            var request = new RegistrationRequest
            {
                Username = _username.text,
                Password = _password.text
            };

            var succes = await APIHandler.Register(request);

            if (succes)
            {
                ViewManager.SwitchView(ViewType.Login);
            }
        }

        private  void OnBackButtonClicked()
        {
            ViewManager.SwitchToPreviousView();
        }
    }
}