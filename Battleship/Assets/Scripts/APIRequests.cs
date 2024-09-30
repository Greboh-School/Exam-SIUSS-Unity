using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;

public class APIRequests : MonoBehaviour
{
    private NetworkSession netSession;

    public void Start()
    {
        netSession = GameObject.Find("NetworkManager").GetComponent<NetworkSession>();
    }

    public void GetServer(Action<DTO.IPResponse> onSucces, Action<string> onError)
    {
        StartCoroutine(GetServerIP(onSucces, onError));
    }

    public void Login(DTO.User dto, Action<string> onSuccess, Action<string> onError)
    {
        string url = $"{netSession.loginAPI}/login";

        StartCoroutine(PostRequest(url, dto, onSuccess, onError));
    }

    public void Register(DTO.User dto, Action<string> onSuccess, Action<string> onError)
    {
        string url = $"{netSession.loginAPI}/new";

        StartCoroutine(PostRequest(url, dto, onSuccess, onError));
    }

    private IEnumerator GetServerIP(Action<DTO.IPResponse> onSuccess, Action<string> onError)
    {
        string url = $"{netSession.registryAPI}/{netSession.jwt}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");

        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;

            Debug.Log(jsonResponse);

            DTO.IPResponse ip = JsonUtility.FromJson<DTO.IPResponse>(jsonResponse);

            Debug.Log($"IP extracted: {ip}");

            onSuccess?.Invoke(ip);
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }

    private IEnumerator PostRequest(string url, DTO.User user, Action<string> onSuccess, Action<string> onError)
    {
        Debug.Log($"{user.Username}, {user.Password}");

        string json = JsonUtility.ToJson(user);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.SetRequestHeader("Content-Type", "application/json");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string text = request.downloadHandler.text;

            if (text.Contains("incorrect"))
            {
                onError?.Invoke(text);
            }
            else
            {
                onSuccess?.Invoke(text);
            }
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }
}