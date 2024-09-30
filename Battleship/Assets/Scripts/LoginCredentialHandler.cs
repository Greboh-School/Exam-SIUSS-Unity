using System.Net;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginCredentialHandler : MonoBehaviour
{
    public APIRequests netRequests;
    public Canvas loginView;
    public Canvas registerView;
    public IPAddress gameServerAPI_IP;
    private LoginView login;
    private RegisterView register;

    private void Start()
    {
        login = new LoginView(loginView, registerView, netRequests);
        register = new RegisterView(registerView, loginView, netRequests);

        registerView.enabled = false;
    }
}

public class LoginView : View
{
    public LoginView(Canvas selfView, Canvas otherView, APIRequests netRequests) : base(selfView, otherView, netRequests) {}

    protected override void ReadValues()
    {
        error.text = string.Empty;
        string user = username.text;
        string pass = password.text;

        var dto = new DTO.User() { Username = user, Password = pass };

        netRequests.Login(dto, OnLoginSucces, OnError);
    }
}

public class RegisterView : View
{
    protected TMP_InputField passwordCheck;
    private string user;
    private string pass;

    public RegisterView(Canvas selfView, Canvas otherView, APIRequests netRequests) : base(selfView, otherView, netRequests)
    {
        passwordCheck = selfView.transform.Find("passwordCheck").GetComponent<TMP_InputField>();
    }

    protected override void ReadValues()
    {
        error.text = string.Empty;
        user = username.text;
        pass = password.text;
        string passCheck = passwordCheck.text;
        if(pass == passCheck)
        {
            var dto = new DTO.User() { Username = user, Password = pass };

            netRequests.Register(dto, RegisterSucces, OnError);
        }
        else
        {
            error.text = "Password does not match!";
        }
    }

    protected void RegisterSucces(string response)
    {
        var dto = new DTO.User() { Username = user, Password = pass };

        netRequests.Login(dto, OnLoginSucces, OnError);
    }
}

public abstract class View
{
    protected Canvas otherView;
    protected Canvas selfView;
    protected TMP_InputField password;
    protected TMP_InputField username;
    protected Button viewChange;
    protected Button confirm;
    protected TMP_Text error;
    protected APIRequests netRequests;

    public View(Canvas selfView, Canvas otherView, APIRequests netRequests)
    {
        this.netRequests = netRequests;
        this.selfView = selfView;
        this.otherView = otherView;
        username = selfView.transform.Find("username").GetComponent<TMP_InputField>();
        password = selfView.transform.Find("password").GetComponent<TMP_InputField>();

        viewChange = selfView.transform.Find("viewChange").GetComponent<Button>();
        viewChange.onClick.AddListener(() => ChangeView(true, false));

        confirm = selfView.transform.Find("confirm").GetComponent<Button>();
        confirm.onClick.AddListener(ReadValues);

        error = selfView.transform.Find("error").GetComponent<TMP_Text>();
        error.text = string.Empty;
    }

    protected abstract void ReadValues();

    protected void OnLoginSucces(string response)
    {
        Debug.Log(response);

        NetworkSession netSession = GameObject.Find("NetworkManager").GetComponent<NetworkSession>();

        netSession.jwt = response;

        netRequests.GetServer(OnRegistryAPISucces, OnError);
    }

    protected void OnError(string errorText)
    {
        error.text = errorText;
    }

    private void OnRegistryAPISucces(DTO.IPResponse IP)
    {
        GameObject networkObject = GameObject.Find("Network Manager");
        NetworkSession network = networkObject.GetComponent<NetworkSession>();

        Debug.Log($"IP setting to {IP}");

        network.defaultServerIP = IP.IP;
        network.defaultServerPort = IP.Port;
        network.StartSession();

        ChangeView(false, false);
    }

    private void ChangeView(bool other, bool self)
    {
        otherView.enabled = other;
        error.text = string.Empty;
        selfView.enabled = self;
    }
}