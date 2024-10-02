using Requests;
using System.Net;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginCredentialHandler : MonoBehaviour
{
    public APIHandler apiHandler;
    public Canvas loginView;
    public Canvas registerView;
    public IPAddress gameServerAPI_IP;
    private LoginView login;
    private RegisterView register;

    private void Start()
    {
        login = new LoginView(loginView, registerView, apiHandler);
        register = new RegisterView(registerView, loginView, apiHandler);

        registerView.enabled = false;
    }

    public void Update()
    {
        login.Update();
        register.Update();
    }
}

public class LoginView : View
{
    public LoginView(Canvas view, Canvas otherView, APIHandler netRequests) : base(view, otherView, netRequests) {}

    protected override async void ReadValues()
    {
        error.text = string.Empty;
        string user = username.text;
        string pass = password.text;

        var request = new LoginRequest() { UserName = user, Password = pass };

        var dto = await apiHandler.Login(request);

        Debug.Log(dto.ToString());
    }
}

public class RegisterView : View
{
    protected TMP_InputField passwordCheck;
    private string user;
    private string pass;

    public RegisterView(Canvas view, Canvas otherView, APIHandler netRequests) : base(view, otherView, netRequests)
    {
        passwordCheck = view.transform.Find("passwordCheck").GetComponent<TMP_InputField>();
    }

    protected override void ReadValues()
    {
        error.text = string.Empty;
        user = username.text;
        pass = password.text;
        string passCheck = passwordCheck.text;
        if(pass == passCheck)
        {
            //var dto = new DTO.ApplicationUser() { Username = user, Password = pass };

            //apiHandler.Register(dto, RegisterSucces, OnError);
        }
        else
        {
            error.text = "Password does not match!";
        }
    }

    protected void RegisterSucces(string response)
    {
       // var dto = new DTO.ApplicationUser() { Username = user, Password = pass };

        //apiHandler.Login(dto, OnLoginSucces, OnError);
    }

    protected override void SwitchInputField()
    {
        if (username.isFocused)
        {
            password.Select();
        }
        else if (password.isFocused)
        {
            passwordCheck.Select();
        }
        else if (passwordCheck.isFocused)
        {
            username.Select();
        }
    }
}

public abstract class View
{
    protected Canvas otherView;
    protected Canvas view;
    protected TMP_InputField password;
    protected TMP_InputField username;
    protected Button viewChange;
    protected Button confirm;
    protected TMP_Text error;
    protected APIHandler apiHandler;

    public View(Canvas view, Canvas otherView, APIHandler apiHandler)
    {
        this.apiHandler = apiHandler;
        this.view = view;
        this.otherView = otherView;
        username = view.transform.Find("username").GetComponent<TMP_InputField>();
        password = view.transform.Find("password").GetComponent<TMP_InputField>();

        username.Select();

        viewChange = view.transform.Find("viewChange").GetComponent<Button>();
        viewChange.onClick.AddListener(() => ChangeView(true, false));

        confirm = view.transform.Find("confirm").GetComponent<Button>();
        confirm.onClick.AddListener(ReadValues);

        error = view.transform.Find("error").GetComponent<TMP_Text>();
        error.text = string.Empty;
    }

    public void Update()
    {
        if (view.enabled)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SwitchInputField();
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ReadValues();
            }
        }
    }

    protected virtual void SwitchInputField()
    {
        if (username.isFocused)
        {
            password.Select();
        }
        else if (password.isFocused)
        {
            username.Select();
        }
    }

    protected virtual async void ReadValues() { }

    protected void OnLoginSucces(string response)
    {
        Debug.Log(response);

        NetworkHandler netSession = GameObject.Find("NetworkManager").GetComponent<NetworkHandler>();

      //  netSession.jwt = response;

       // apiHandler.GetServer(OnRegistryAPISucces, OnError);
    }

    protected void OnError(string errorText)
    {
        error.text = errorText;
    }

    /*private void OnRegistryAPISucces(DTO.IPResponse IP)
    {
        GameObject networkObject = GameObject.Find("Network Manager");
        NetworkHandler network = networkObject.GetComponent<NetworkHandler>();

        Debug.Log($"IP setting to {IP}");

        network.serverIP = $"{IP.IP}:{IP.Port}";
        network.StartSession();

        ChangeView(false, false);
    }*/

    private void ChangeView(bool other, bool self)
    {
        otherView.enabled = other;
        error.text = string.Empty;
        view.enabled = self;
    }
}