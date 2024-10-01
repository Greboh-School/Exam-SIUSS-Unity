using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class APIHandler : MonoBehaviour
{
    private NetworkHandler networkHandler;

    private void Start()
    {
        networkHandler = GetComponent<NetworkHandler>();
    }

    public void GetRequest<T>(string endpoint, Action<T> onSuccess, Action<string> onError)
    {
        string url = $"{networkHandler.registryAPI}/{endpoint}";
        StartCoroutine(SendRequest<T>(url, UnityWebRequest.kHttpVerbGET, null, onSuccess, onError));
    }

    public void PostRequest<T>(string endpoint, T dto, Action<string> onSuccess, Action<string> onError)
    {
        string url = $"{networkHandler.loginAPI}/{endpoint}";
        string json = JsonUtility.ToJson(dto);
        StartCoroutine(SendRequest(url, UnityWebRequest.kHttpVerbPOST, json, onSuccess, onError));
    }

    public void PutRequest<T>(string endpoint, T dto, Action<string> onSuccess, Action<string> onError)
    {
        string url = $"{networkHandler.loginAPI}/{endpoint}";
        string json = JsonUtility.ToJson(dto);
        StartCoroutine(SendRequest(url, UnityWebRequest.kHttpVerbPUT, json, onSuccess, onError));
    }

    public void GetServer(Action<DTO.IPResponse> onSuccess, Action<string> onError)
    {
        GetRequest<DTO.IPResponse>($"{networkHandler.jwt}", onSuccess, onError);
    }

    public void Login(DTO.User dto, Action<string> onSuccess, Action<string> onError)
    {
        PostRequest("login", dto, onSuccess, onError);
    }

    public void Register(DTO.User dto, Action<string> onSuccess, Action<string> onError)
    {
        PostRequest("new", dto, onSuccess, onError);
    }

    private IEnumerator SendRequest<T>(string url, string method, string jsonData, Action<T> onSuccess, Action<string> onError)
    {
        UnityWebRequest request = new UnityWebRequest(url, method);

        if (!string.IsNullOrEmpty(jsonData))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        }

        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            if (typeof(T) == typeof(string))
            {
                onSuccess?.Invoke((T)(object)responseText);
            }
            else
            {
                T responseDto = JsonUtility.FromJson<T>(responseText);
                onSuccess?.Invoke(responseDto);
            }
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }
}