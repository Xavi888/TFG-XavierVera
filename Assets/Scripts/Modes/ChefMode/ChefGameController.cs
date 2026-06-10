using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static NetworkPlayer;

public class ChefGameController : NetworkBehaviour
{
    [SerializeField] private ProgrammingBoard programmingBoard;
    [SerializeField] private ChefRobotTV robotTV;

    [System.Serializable]
    public class IngredientSupplierPair
    {
        public IngredientType key;
        public Supplier value;
    }

    [SerializeField] private List<IngredientSupplierPair> ingredientSuppliersList = new();
    [SerializeField] private Supplier plateSupplier;

    [SerializeField] private float gameDayDurationInSeconds = 300;
    [SerializeField] private GameObject multiplayerMenu;

    private readonly NetworkVariable<float> currentTimeInSeconds = new(
        9 * 60 * 60,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<bool> timePaused = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private List<ProgrammingBlock> currentOrder;
    private PlayerProperties playerProperties;

    private int dayExperience = 0;
    private int dayLevel;
    private bool daySummaryShown = false;

    private string currentGameMode;
    private bool rolesInitialized = false;

    private ulong pairChefClientId;
    private ulong pairAdvisorClientId;

    private ulong versionChefOneClientId;
    private ulong versionChefTwoClientId;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            playerProperties = new PlayerProperties();
            playerProperties.experience = 400;
            multiplayerMenu.SetActive(true);
            // IMPORTANTE!!! -> Depurar para escoger el modo de juego multijugador
        }
        else
        {
            playerProperties = GameManager.Instance.playerProperties;
            if (GameManager.Instance.isMultiplayer)
            {
                multiplayerMenu.SetActive(true);
            }
            else
            {
                // Single player
                if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsListening)
                    GameManager.Instance.SetGameMode("PairProgramming");
                    NetworkManager.Singleton.StartHost();
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        currentTimeInSeconds.OnValueChanged += OnTimeChanged;

        if (IsServer)
        {
            if (GameManager.Instance != null)
                currentGameMode = GameManager.Instance.GetGameMode();

            if (GameManager.Instance != null && GameManager.Instance.isMultiplayer)
            {
                StartCoroutine(WaitForPlayersAndStart());
            }
            else
            {
                NewDayServer();
            }
        }
        else
        {
            robotTV.DisplayTime(currentTimeInSeconds.Value);
        }
    }

    private IEnumerator WaitForPlayersAndStart()
    {
        timePaused.Value = true;

        robotTV.SetCenterText("Esperando a todos los jugadores...");
        robotTV.DisplayDelivered(true);

        while (NetworkManager.Singleton.ConnectedClientsIds.Count < 2)
        {
            yield return null;
        }

        //AssignRandomRolesServer();

        robotTV.DisplayDelivered(false);

        NewDayServer();
    }

    public override void OnNetworkDespawn()
    {
        currentTimeInSeconds.OnValueChanged -= OnTimeChanged;
    }

    private void Update()
    {
        if (!IsServer) return;
        if (timePaused.Value) return;

        currentTimeInSeconds.Value += Time.deltaTime * (24 * 60 * 60) / gameDayDurationInSeconds;

        if (!daySummaryShown && currentTimeInSeconds.Value >= 17 * 60 * 60)
        {
            ShowDaySummaryServer();
        }
    }

    private void OnTimeChanged(float previousValue, float newValue)
    {
        robotTV.DisplayTime(newValue);
    }

    public void NewOrder()
    {
        if (IsServer)
            NewOrderServer();
        else
            RequestNewOrderRpc();
    }

    [Rpc(SendTo.Server)]
    private void RequestNewOrderRpc()
    {
        NewOrderServer();
    }

    private void NewOrderServer()
    {
        HideDisplayDeliveredRpc(); // Ocultar texto al evaluar el plato --> Sustituye el evento OnClick DisplayDelivered del ContinueButton del OrderDelivered

        ResetEmptySuppliersServer();
        ClearSupplierGameObjectsClientRpc();
        GenerateOrderServer();
    }

    public void NewDay()
    {
        if (IsServer)
            NewDayServer();
        else
            RequestNewDayRpc();
    }

    [Rpc(SendTo.Server)]
    private void RequestNewDayRpc()
    {
        NewDayServer();
    }

    private void NewDayServer()
    {
        //SwapRolesServer();
        HandlePlayerRolesServer();

        HideDisplayDayEndedRpc(); // Ocultar texto resumen --> Sustituye el evento OnClick DisplayDayEnded del ContinueButton del DayEnded

        GameManager.Instance?.logger.Log("Starting new day", 0);

        if (GameManager.Instance != null && GameManager.Instance.backendConnector != null)
        {
            GameManager.Instance.UpdatePlayerProperties(playerProperties);
        }

        timePaused.Value = false;
        currentTimeInSeconds.Value = 9 * 60 * 60;
        daySummaryShown = false;
        dayExperience = 0;
        dayLevel = playerProperties.Level;

        ResetSuppliersServer();
        ClearSupplierGameObjectsClientRpc();
        GenerateOrderServer();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void HideDisplayDeliveredRpc()
    {
        robotTV.DisplayDelivered(false);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void HideDisplayDayEndedRpc()
    {
        robotTV.DisplayDayEnded(false);
    }

    public void PlateDelivered(List<Ingredient> ingredients)
    {
        if (!IsServer)
            return;

        PlateDeliveredServer(ingredients);
    }

    private void PlateDeliveredServer(List<Ingredient> ingredients)
    {
        GameManager.Instance?.logger.Log("Order delivered", 0);

        timePaused.Value = true;
        CheckOrderDeliveredServer(ingredients);
    }

    private void GenerateOrderServer()
    {
        GameManager.Instance?.logger.Log("Generated new order", 0);

        currentOrder = OrderGenerator.GenerateProgrammingBlocksChefOrder(dayLevel, 10);
        EvaluateOrderServer();

        programmingBoard.ClearProgrammingBoard(true);
        programmingBoard.PlaceBlocks(currentOrder);

        ProgrammingBlockData[] orderData = currentOrder
            .Select(ProgrammingBlockData.FromBlock)
            .ToArray();

        SetOrderClientRpc(orderData);
    }

    [Rpc(SendTo.NotServer)]
    private void SetOrderClientRpc(ProgrammingBlockData[] orderData)
    {
        programmingBoard.ClearProgrammingBoard(true);

        currentOrder = orderData
            .Select(data => data.ToBlock())
            .ToList();

        EvaluateOrderLocal();

        programmingBoard.PlaceBlocks(currentOrder);
    }

    private void EvaluateOrderServer()
    {
        EvaluateOrderLocal();
    }

    private void EvaluateOrderLocal()
    {
        foreach (ProgrammingBlock block in currentOrder)
        {
            if (block.BlockType != ProgrammingBlockType.If) continue;

            IngredientType neededType = ((IfBlock)block).Variable;
            Supplier matchedSupplier = ingredientSuppliersList
                .FirstOrDefault(pair => pair.key == neededType)?.value;

            if (matchedSupplier != null)
                ((IfBlock)block).Evaluate(matchedSupplier);
            else
                Debug.LogError("Supplier not assigned.");
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ClearSupplierGameObjectsClientRpc()
    {
        foreach (IngredientSupplierPair pair in ingredientSuppliersList)
        {
            pair.value.ClearSupplieds();
        }

        plateSupplier.ClearSupplieds();
    }

    private void ResetSuppliersServer()
    {
        foreach (IngredientSupplierPair pair in ingredientSuppliersList)
        {
            pair.value.ResetSupply();
            OrderGenerator.IngredientStock[pair.key] = pair.value.SupplyAmount;
        }
    }

    private void ResetEmptySuppliersServer()
    {
        foreach (IngredientSupplierPair pair in ingredientSuppliersList)
        {
            if (!pair.value.HasSupply)
            {
                pair.value.ResetSupply();
                OrderGenerator.IngredientStock[pair.key] = pair.value.SupplyAmount;
            }
        }
    }

    private void ShowDaySummaryServer()
    {
        GameManager.Instance?.logger.Log("Day ended", 0);

        daySummaryShown = true;
        timePaused.Value = true;

        string summary = GetSummaryMessage();
        ShowDaySummaryClientRpc(summary);

        //SwapRolesServer();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ShowDaySummaryClientRpc(string summary)
    {
        robotTV.Clear();
        programmingBoard.ClearProgrammingBoard(true);
        robotTV.SetDayEndedText(summary);
        robotTV.DisplayDayEnded(true);
    }

    private string GetSummaryMessage()
    {
        string message = "";
        message += "Hoy has ganado " + dayExperience + " puntos de experiencia. Estas en el nivel " + playerProperties.Level + ".";
        message += "\nBloques disponibles: ";

        if (playerProperties.Level < 3)
            message += "\nPut block.";
        else if (playerProperties.Level >= 4)
            message += "\nFor Block.";
        else if (playerProperties.Level >= 3)
            message += "\nIf Block.";

        return message;
    }

    private void CheckOrderDeliveredServer(List<Ingredient> ingredients)
    {
        List<FeedbackType> feedbacks = FeedbackGenerator.CheckPlateOrder(ingredients, currentOrder);

        string message = "";
        int experience = 0;
        bool rawMeat = false;
        bool isCorrect = false;

        foreach (FeedbackType feedback in feedbacks)
        {
            switch (feedback)
            {
                case FeedbackType.Correct:
                    isCorrect = true;
                    experience += 30;
                    break;

                case FeedbackType.Incorrect:
                    isCorrect = false;
                    break;

                case FeedbackType.ExtraIngredients:
                    message += "\nHay ingredientes extra.";
                    break;

                case FeedbackType.InsufficientIngredients:
                    message += "\nFaltan ingredientes.";
                    break;

                case FeedbackType.RawMeat:
                    rawMeat = true;
                    message += "\nLa carne está cruda.";
                    break;
            }
        }

        if (isCorrect)
        {
            GameManager.Instance?.logger.Log("Correct Order", 0);

            if (rawMeat)
            {
                GameManager.Instance?.logger.Log("Raw meat", 0);
                experience -= 15;
            }

            message = "Pedido correcto. \nExperiencia ganada: " + experience + " puntos." + message;
        }
        else
        {
            GameManager.Instance?.logger.Log("Incorrect order", 0);
            message = "Pedido incorrecto." + message;
        }

        dayExperience += experience;
        playerProperties.AddExperience(experience);

        ShowDeliveredClientRpc(message);
        timePaused.Value = false;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ShowDeliveredClientRpc(string message)
    {
        programmingBoard.ClearProgrammingBoard(true);
        robotTV.SetCenterText(message);
        robotTV.DisplayDelivered(true);
    }

    //private void AssignRandomRolesServer()
    //{
    //    if (!IsServer)
    //        return;

    //    List<ulong> clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToList();

    //    if (clientIds.Count < 2)
    //    {
    //        Debug.LogError("No hay suficientes jugadores para asignar roles.");
    //        return;
    //    }

    //    int chefIndex = UnityEngine.Random.Range(0, 2);

    //    chefClientId = clientIds[chefIndex];
    //    advisorClientId = clientIds[1 - chefIndex];

    //    foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
    //    {
    //        if (client.PlayerObject == null)
    //            continue;

    //        NetworkPlayer player = client.PlayerObject.GetComponent<NetworkPlayer>();

    //        if (player == null)
    //            continue;

    //        bool shouldBeChef = client.ClientId == chefClientId;
    //        player.SetChefServer(shouldBeChef);
    //    }

    //    Debug.Log($"Roles asignados. Chef={chefClientId}, Ayudante={advisorClientId}");
    //}

    //private void SwapRolesServer()
    //{
    //    if (!IsServer)
    //        return;

    //    ulong oldChef = chefClientId;
    //    chefClientId = advisorClientId;
    //    advisorClientId = oldChef;

    //    foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
    //    {
    //        if (client.PlayerObject == null)
    //            continue;

    //        NetworkPlayer player = client.PlayerObject.GetComponent<NetworkPlayer>();

    //        if (player == null)
    //            continue;

    //        bool shouldBeChef = client.ClientId == chefClientId;
    //        player.SetChefServer(shouldBeChef);
    //    }

    //    Debug.Log($"Roles cambiados. Chef={chefClientId}, Ayudante={advisorClientId}");
    //}

    private void HandlePlayerRolesServer()
    {
        if (!IsServer)
            return;

        switch (currentGameMode)
        {
            case "PairProgramming":
                if (!rolesInitialized)
                {
                    AssignPairProgrammingRolesServer();
                    rolesInitialized = true;
                }
                else
                {
                    SwapPairProgrammingRolesServer();
                }
                break;

            case "VersionControl":
                if (!rolesInitialized)
                {
                    AssignVersionControlRolesServer();
                    rolesInitialized = true;
                }
                break;

            default:
                Debug.LogWarning("Modo de juego no reconocido: " + currentGameMode);
                break;
        }
    }

    private void AssignPairProgrammingRolesServer()
    {
        if (!IsServer)
            return;

        List<ulong> clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToList();

        if (clientIds.Count < 2)
        {
            Debug.LogError("No hay suficientes jugadores para asignar roles.");
            return;
        }

        int chefIndex = UnityEngine.Random.Range(0, 2);

        pairChefClientId = clientIds[chefIndex];
        pairAdvisorClientId = clientIds[1 - chefIndex];

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            NetworkPlayer player = client.PlayerObject.GetComponent<NetworkPlayer>();

            if (player == null)
                continue;

            if (client.ClientId == pairChefClientId)
                player.SetRoleServer(NetworkPlayerRole.PairProgrammingChef);
            else if (client.ClientId == pairAdvisorClientId)
                player.SetRoleServer(NetworkPlayerRole.PairProgrammingAdvisor);
        }

        Debug.Log($"PairProgramming roles asignados. Chef={pairChefClientId}, Ayudante={pairAdvisorClientId}");
    }

    private void SwapPairProgrammingRolesServer()
    {
        if (!IsServer)
            return;

        ulong oldChef = pairChefClientId;
        pairChefClientId = pairAdvisorClientId;
        pairAdvisorClientId = oldChef;

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            NetworkPlayer player = client.PlayerObject.GetComponent<NetworkPlayer>();

            if (player == null)
                continue;

            if (client.ClientId == pairChefClientId)
                player.SetRoleServer(NetworkPlayerRole.PairProgrammingChef);
            else if (client.ClientId == pairAdvisorClientId)
                player.SetRoleServer(NetworkPlayerRole.PairProgrammingAdvisor);
        }

        Debug.Log($"PairProgramming roles cambiados. Chef={pairChefClientId}, Ayudante={pairAdvisorClientId}");
    }

    private void AssignVersionControlRolesServer()
    {
        if (!IsServer)
            return;

        List<ulong> clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToList();

        if (clientIds.Count < 2)
        {
            Debug.LogError("No hay suficientes jugadores para asignar posiciones de VersionControl.");
            return;
        }

        int chefOneIndex = UnityEngine.Random.Range(0, 2);

        versionChefOneClientId = clientIds[chefOneIndex];
        versionChefTwoClientId = clientIds[1 - chefOneIndex];

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            NetworkPlayer player = client.PlayerObject.GetComponent<NetworkPlayer>();

            if (player == null)
                continue;

            if (client.ClientId == versionChefOneClientId)
                player.SetRoleServer(NetworkPlayerRole.VersionControlChefOne);
            else if (client.ClientId == versionChefTwoClientId)
                player.SetRoleServer(NetworkPlayerRole.VersionControlChefTwo);
        }

        Debug.Log($"VersionControl posiciones asignadas. ChefOne={versionChefOneClientId}, ChefTwo={versionChefTwoClientId}");
    }
}

[Serializable]
public struct ProgrammingBlockData : INetworkSerializable
{
    public ProgrammingBlockType blockType;

    public IngredientType ingredientType;
    public ConditionalOperatorType operatorType;
    public int value;
    public int iterations;

    public ProgrammingBlockData[] childBlocks;

    public static ProgrammingBlockData FromBlock(ProgrammingBlock block)
    {
        ProgrammingBlockData data = new ProgrammingBlockData
        {
            blockType = block.BlockType
        };

        switch (block.BlockType)
        {
            case ProgrammingBlockType.Put:
                {
                    PutBlock putBlock = (PutBlock)block;
                    data.ingredientType = putBlock.Type;
                    break;
                }

            case ProgrammingBlockType.If:
                {
                    IfBlock ifBlock = (IfBlock)block;
                    data.ingredientType = ifBlock.Variable;
                    data.operatorType = ifBlock.Operator;
                    data.value = ifBlock.Value;

                    data.childBlocks = ifBlock.SuccessBlocks
                        .Select(FromBlock)
                        .ToArray();

                    break;
                }

            case ProgrammingBlockType.For:
                {
                    ForBlock forBlock = (ForBlock)block;
                    data.iterations = forBlock.Iterations;

                    data.childBlocks = forBlock.IterationBlocks
                        .Select(FromBlock)
                        .ToArray();

                    break;
                }
        }

        return data;
    }

    public ProgrammingBlock ToBlock()
    {
        switch (blockType)
        {
            case ProgrammingBlockType.Put:
                return ProgrammingBlockFactory.CreateBlock(
                    ProgrammingBlockType.Put,
                    ingredientType
                );

            case ProgrammingBlockType.If:
                {
                    List<ProgrammingBlock> successBlocks = childBlocks == null
                        ? new List<ProgrammingBlock>()
                        : childBlocks.Select(child => child.ToBlock()).ToList();

                    return ProgrammingBlockFactory.CreateBlock(
                        ProgrammingBlockType.If,
                        successBlocks,
                        ingredientType,
                        operatorType,
                        value
                    );
                }

            case ProgrammingBlockType.For:
                {
                    List<ProgrammingBlock> iterationBlocks = childBlocks == null
                        ? new List<ProgrammingBlock>()
                        : childBlocks.Select(child => child.ToBlock()).ToList();

                    return ProgrammingBlockFactory.CreateBlock(
                        ProgrammingBlockType.For,
                        iterationBlocks,
                        iterations
                    );
                }

            default:
                throw new NotImplementedException("Bloque no soportado: " + blockType);
        }
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref blockType);
        serializer.SerializeValue(ref ingredientType);
        serializer.SerializeValue(ref operatorType);
        serializer.SerializeValue(ref value);
        serializer.SerializeValue(ref iterations);

        int childCount = childBlocks?.Length ?? 0;
        serializer.SerializeValue(ref childCount);

        if (serializer.IsReader)
        {
            childBlocks = new ProgrammingBlockData[childCount];
        }

        for (int i = 0; i < childCount; i++)
        {
            serializer.SerializeValue(ref childBlocks[i]);
        }
    }
}