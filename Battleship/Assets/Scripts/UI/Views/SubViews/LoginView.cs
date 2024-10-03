using System;
using Requests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Views;

public class LoginView : View
{
    public override ViewType ViewType => ViewType.Login;

    [Header("Input Fields")]
    [SerializeField]
    private TMP_InputField _username;
    [SerializeField]
    private TMP_InputField _password;

    [Header("Buttons")]
    [SerializeField]
    private Button _loginButton;
    [SerializeField]
    private Button _registrationButton;

    protected override void Awake()
    {
        base.Awake();
        
        if (_loginButton is null)
        {
            Debug.LogError("Login button is null");
        }
        if (_registrationButton is null)
        {
            Debug.LogError("Registration button is null");
        }
    }

    private void Start()
    {
        _loginButton?.onClick.AddListener(OnLoginClicked);
        _registrationButton?.onClick.AddListener(OnRegistrationClicked);
    }

    private async void OnLoginClicked()
    {
        var request = new LoginRequest
        {
            Username = _username.text,
            Password = _password.text
        };

        await APIHandler.Login(request);
    }

    private void OnRegistrationClicked()
    {
        ViewManager.SwitchView(ViewType.Registration);
    }
}