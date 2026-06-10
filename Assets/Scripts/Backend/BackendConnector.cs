using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BackendConnector : MonoBehaviour
{
    //private string baseUrl = "https://<tu-app-name>.azurewebsites.net";
    //private string baseUrl = "http://localhost:5000"; //DEV
    private string baseUrl = "https://tfg-backend.azurewebsites.net"; //PRO
    private string token;

    [System.Serializable]
    private class LoginResponse
    {
        public string access_token;
    }
    [System.Serializable]
    public class RegisterData
    {
        public string username;
        public string password;
    }

    public class LoginData
    {
        public string username;
        public string password;
    }

    


    public IEnumerator Register(string username, string password, Action<string> feedbackCallback)
    {
        RegisterData registerData = new RegisterData { username = username, password = password };
        string jsonData = JsonUtility.ToJson(registerData);

        using (UnityWebRequest www = UnityWebRequest.Put($"{baseUrl}/register", jsonData))
        {
            www.method = "POST";
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("User registered successfully!");
                feedbackCallback?.Invoke("User registered successfully!");
            }
            else
            {
                Debug.LogError($"Registration failed: {www.error}");
                feedbackCallback?.Invoke($"Registration failed: {www.error}");
            }
        }
    }

    public IEnumerator Login(string username, string password, Action callback, Action<string> feedbackCallback)
    {
        LoginData loginData = new LoginData { username = username, password = password };
        string jsonData = JsonUtility.ToJson(loginData);

        using (UnityWebRequest www = UnityWebRequest.Put($"{baseUrl}/login", jsonData))
        {
            www.method = "POST";
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<LoginResponse>(www.downloadHandler.text);
                token = response.access_token;
                Debug.Log("Login successful!");
                callback?.Invoke();
                feedbackCallback?.Invoke("Login successful!");
            }
            else
            {
                Debug.LogError($"Login failed: {www.error}");
                feedbackCallback?.Invoke($"Login failed: {www.error}");
            }
        }
    }

    public IEnumerator GetProperties(Action<PlayerProperties> callback)
    {
        using (UnityWebRequest www = UnityWebRequest.Get($"{baseUrl}/properties"))
        {
            www.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    Debug.Log($"Received JSON: {www.downloadHandler.text}");
                    var properties = JsonUtility.FromJson<PlayerProperties>(www.downloadHandler.text);
                    Debug.Log($"Deserialized Experience: {properties.experience}");
                    callback(properties);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error deserializing properties: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Failed to get properties: {www.error}");
            }
        }
    }

    public IEnumerator UpdateProperties(PlayerProperties properties)
    {
        string jsonData = JsonUtility.ToJson(properties);

        using (UnityWebRequest www = UnityWebRequest.Put($"{baseUrl}/properties", jsonData))
        {
            www.method = "PUT";
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Properties updated successfully!");
            }
            else
            {
                Debug.LogError($"Failed to update properties: {www.error}");
            }
        }
    }
}
