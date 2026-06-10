using TMPro;
using UnityEngine;

public class LoginRegisterController : MonoBehaviour
{
    public TMP_InputField usernameRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField usernameLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text feedbackText;
    public BackendConnector backendConnector;

    public GameObject startMenu;
    public GameObject loginRegisterMenu;
    public GameObject gameStartMenu;
    public GameObject singleMultiMenu;
    public GameObject gameModeMenu;
    public GameObject loginMenu;

    public void OnLoginSuccess()
    {
        Debug.Log("username" + usernameLoginField.text + "password" + passwordLoginField.text);
        StartCoroutine(backendConnector.Login(usernameLoginField.text, passwordLoginField.text, OnLoginCallback, UpdateFeedback));
        //Debug.Log("FAKE LOGIN: entrando sin backend");
        //OnLoginCallback();
    }

    private void OnLoginCallback()
    {
        GameManager.Instance.FetchPlayerProperties(properties =>
        {
            GameManager.Instance.InitializeLogger(usernameLoginField.text + ".txt");
            ShowModeSelectionMenu();
            loginMenu.SetActive(false);
        });
        //Debug.Log("LOGIN SIMULADO OK");
        //GameManager.Instance.SetInstanceNull();
        //ShowModeSelectionMenu();
        //loginMenu.SetActive(false);
    }

    public void OnRegister()
    {
        Debug.Log("username" + usernameRegisterField.text + "password" + passwordRegisterField.text);
        StartCoroutine(backendConnector.Register(usernameRegisterField.text, passwordRegisterField.text, UpdateFeedback));
    }

    private void UpdateFeedback(string message)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
    }

    public void OnCasualGame()
    {
        GameManager.Instance.SetCasualGame();
        ShowModeSelectionMenu();
    }

    private void ShowModeSelectionMenu()
    {
        gameStartMenu.SetActive(true);
    }

    public void EnterChefMode()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ChefScene");
    }

    public void EnterWaiterMode()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("WaiterScene");
    }

    public void MultiplayerChefMode()
    {
        GameManager.Instance.SetMultiplayerGame();
        gameModeMenu.SetActive(true);
        //UnityEngine.SceneManagement.SceneManager.LoadScene("ChefScene");
    }

    public void PairProgrammingMode()
    {
        GameManager.Instance.SetGameMode("PairProgramming");
        UnityEngine.SceneManagement.SceneManager.LoadScene("ChefScene");
    }

    public void VersionControlMode()
    {
        GameManager.Instance.SetGameMode("VersionControl");
        UnityEngine.SceneManagement.SceneManager.LoadScene("ChefScene");
    }

    public void TeamRolesMode()
    {
        GameManager.Instance.SetGameMode("TeamRoles");
        UnityEngine.SceneManagement.SceneManager.LoadScene("ChefScene");
    }

    public void OnBackButton(GameObject menu)
    {
        menu.SetActive(false);
        switch (menu)
        {
            case GameObject m when m == gameStartMenu:
                if (GameManager.Instance.backendConnector == null)
                {
                    startMenu.SetActive(true);
                }
                else
                {
                    loginRegisterMenu.SetActive(true);
                }
                break;
            case GameObject m when m == loginRegisterMenu:
                startMenu.SetActive(true);
                break;
            case GameObject m when m == singleMultiMenu:
                gameStartMenu.SetActive(true);
                break;
            case GameObject m when m == gameModeMenu:
                singleMultiMenu.SetActive(true);
                break;
        }
    }
}
