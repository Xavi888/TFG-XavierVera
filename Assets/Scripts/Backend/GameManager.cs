using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PlayerProperties playerProperties { get; private set; }
    public BackendConnector backendConnector;
    public bool isMultiplayer { get; private set; }

    public Logger logger { get; private set; }

    public enum GameMode
    {
        PairProgramming,
        VersionControl,
        TeamRoles
    }
    private GameMode currentGameMode;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        backendConnector = GetComponent<BackendConnector>();
    }

    public void InitializeLogger(string username) {
        logger = new Logger(username);
    }

    public void SetPlayerProperties(PlayerProperties properties)
    {
        playerProperties = properties;
    }

    public void UpdatePlayerProperties(PlayerProperties properties)
    {
        StartCoroutine(backendConnector.UpdateProperties(properties));
    }

    public void FetchPlayerProperties(System.Action<PlayerProperties> callback)
    {
        StartCoroutine(backendConnector.GetProperties(properties =>
        {
            SetPlayerProperties(properties);
            callback?.Invoke(properties);
        }));
    }

    public void SetCasualGame()
    {
        InitializeLogger("CasualGame.txt");
        backendConnector = null;
        playerProperties = new PlayerProperties();
        playerProperties.Level = 1;
    }

    public void SetMultiplayerGame()
    {
        isMultiplayer = true;
    }

    public void SetGameMode(string mode)
    {
        switch (mode)
        {
            case "PairProgramming":
                currentGameMode = GameMode.PairProgramming;
                break;
            case "VersionControl":
                currentGameMode = GameMode.VersionControl;
                break;
            case "TeamRoles":
                currentGameMode = GameMode.TeamRoles;
                break;
        }
    }

    public string GetGameMode()
    {
        return currentGameMode.ToString();
    }
}
