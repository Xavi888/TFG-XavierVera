
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaiterGameController : MonoBehaviour
{
    [SerializeField] private ProgrammingBoard programmingBoard;
    [SerializeField] private OrderButton button;

    [SerializeField] private OrderBuilder orderBuilder;

    [SerializeField] private WaiterRobotTV robotTV;
    [SerializeField] private Floor floor;
    [SerializeField] private ClientTable[] clientTables;
    
    [SerializeField] private float gameDayDurationInSeconds = 30;
    private bool timePaused = false;
    private float currentTimeInSeconds;

    private List<IngredientType> currentOrder;

    private List<IngredientType> currentOrderCreated;

    private string orderDescription;
    private ClientTable currentTable;

    private PlayerProperties playerProperties;

    GameObject currentPlate, currentTray;
    private int dayLevel;
    private int dayExperience;

    public WaiterGameController()
    {
        //playerProperties = new PlayerProperties();
    }

    private void Start()
    {
        if (GameManager.Instance == null) {
            playerProperties = new PlayerProperties();
        } else {
            playerProperties = GameManager.Instance.playerProperties;
        }

        button.OnButtonPressed += OnButtonPressed;
        floor.OnPlateContact += OnPlateDropped;
        NewDay();
    }

    private void Update()
    {
        if (!timePaused)
        {
            currentTimeInSeconds += Time.deltaTime * (24 * 60 * 60) / gameDayDurationInSeconds;
            if (currentTimeInSeconds >= 17 * 60 * 60)
            {
                ShowDaySummary();
                
            }
            robotTV.DisplayTime(currentTimeInSeconds);
        }
    }

    private void ShowDaySummary() {
        if (GameManager.Instance != null) {
            GameManager.Instance.logger.Log("Day ended", 1);
        }
        timePaused = true;
        currentTable.DisplayPanel(false);
        robotTV.Clear();
        DestroyCurrentOrder();  
        robotTV.SetDayEndedText(GetSummaryMessage());
        robotTV.DisplayDayEnded(true);
    }

    private string GetSummaryMessage() {
        string message = "";
        message += "Hoy has ganado " + dayExperience + " puntos de experiencia. Estas en el nivel " + playerProperties.Level + ".";
        /*
        message += "\nBloques disponibles: ";
        if (playerProperties.Level < 3) {
            message += "\nPut block.";
        } else if (playerProperties.Level >= 3) {
            message += "\nIf Block.";
        } else if (playerProperties.Level >= 4) {
            message += "\nFor Block.";
        }*/
        return message;
    }

    public void NewDay()
    {
        if (GameManager.Instance != null) {
            GameManager.Instance.logger.Log("Staring new day", 1);
        }
        if (GameManager.Instance != null) {
            GameManager.Instance.UpdatePlayerProperties(playerProperties);
        }
        timePaused = false;
        currentTimeInSeconds = 9 * 60 * 60;
        dayExperience = 0;
        dayLevel = playerProperties.Level;
        NewOrder();
    }

    private void ShowOrderCreated() {
        Debug.Log("Mostrando el pedido creado");
        robotTV.SetOrderCreatedText("Ve a atender a los clientes.");
        robotTV.DisplayOrderCreated(true);
    }

    private void ShowIncorrectOrder() {
        if (GameManager.Instance != null) {
            GameManager.Instance.logger.Log("Incorrect Order", 1);
        }
        robotTV.SetOrderCreatedText("Pedido incorrecto: \n" + orderDescription);
    }

    public void NewOrder()
    {
        if (GameManager.Instance != null) {
            GameManager.Instance.logger.Log("Generated new order", 1);
        }
        SelectTable();
    }

    private void SelectTable()
    {
        Debug.Log("Seleccionando una mesa");
        ShowOrderCreated();
        int randomTableIndex = Random.Range(0, clientTables.Length);
        currentTable = clientTables[randomTableIndex];
        currentTable.SetOrderText("¡Aqui!");
        currentTable.DisplayPanel(true);
        currentTable.OnTableActivated += OnTableActivated;
        currentTable.OnTableOrderDelivered += OnTableOrderDelivered;

    }

    private void GenerateOrder()
    {
        currentOrder = OrderGenerator.GenerateIngredientListWaiterOrder(dayLevel);
        orderDescription = OrderUtils.GetOrderDescription(currentOrder);
        currentTable.SetOrderText(orderDescription);
        robotTV.SetOrderCreatedText(orderDescription);
    }

    private void SpawnOrder() {
        if (GameManager.Instance != null) {
            GameManager.Instance.logger.Log("Correct order", 1);
        }
        currentOrderCreated = OrderUtils.GetIngredientTypesFromBlocks(programmingBoard.GetCurrentProgrammingBlocks());
        (currentPlate, currentTray) = orderBuilder.SpawnOrder(currentOrderCreated);
        currentTable.SetOrderText("¡Es mi pedido!");
        robotTV.SetOrderCreatedText("Ve a entregar el pedido.");
    }

    private void ShowOrderDelivered()
    {
        if (GameManager.Instance != null) {
            GameManager.Instance.logger.Log("Order delivered", 1);
        }
        robotTV.DisplayOrderCreated(false);
        robotTV.SetOrderDeliveredText("¡Pedido entregado correctamente!\nExperiencia ganada: 30");
        robotTV.DisplayOrderDelivered(true);
    }

    private void ShowOrderDropped()
    {
        if (GameManager.Instance != null) {
            GameManager.Instance.logger.Log("Plate dropped", 1);
        }
        robotTV.DisplayOrderCreated(false);
        robotTV.SetOrderDeliveredText("¡Se te ha caido el plato!\nExperiencia ganada: 0" );
        robotTV.DisplayOrderDelivered(true);
    }
    private bool CheckOrderCreated()
    {
        if (currentOrder.SequenceEqual(currentOrderCreated)) {
            return true;
        } else {
            return false;
        }
    }

    private void OnTableActivated() {
        GenerateOrder();
    }

    private void OnTableOrderDelivered() {
        currentTable.DisplayPanel(false);
        currentTable.OnTableOrderDelivered -= OnTableOrderDelivered;
        currentTable.OnTableActivated -= OnTableActivated;
        DestroyCurrentOrder();
        dayExperience += 30;
        playerProperties.experience += 30;
        ShowOrderDelivered();
    }

    private void OnButtonPressed()
    {
        currentOrderCreated = OrderUtils.GetIngredientTypesFromBlocks(programmingBoard.GetCurrentProgrammingBlocks());
        if (CheckOrderCreated()) {
            SpawnOrder();
        } else {
            ShowIncorrectOrder();
        }
    }

    private void DestroyCurrentOrder() {
        if (currentPlate) {
            Destroy(currentPlate);
        }
        if (currentTray) {
            Destroy(currentTray);
        }
    }

    private void OnPlateDropped()
    {
        DestroyCurrentOrder();  
        currentTable.DisplayPanel(false);
        currentTable.OnTableOrderDelivered -= OnTableOrderDelivered;
        currentTable.OnTableActivated -= OnTableActivated;
        ShowOrderDropped();
    }

}
