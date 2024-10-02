using System.Net.Http;
using UnityEngine;

public class BaseClient : MonoBehaviour
{
    protected HttpClient HttpClient = default!;

    private void Awake()
    {
        HttpClient = new HttpClient();
    }
}
