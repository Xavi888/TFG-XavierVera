using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderBuilder : MonoBehaviour 
{

    [SerializeField] private Renderer tube;
    private Vector3 spawnPoint;

    [SerializeField] private Plate plate;
    [SerializeField] private Vector3 traySpawnPoint;
    [SerializeField] private GameObject tray;

    private void Awake() {
        spawnPoint = tube.bounds.center;
    } 

    public (GameObject tray, GameObject plate) SpawnOrder(List<IngredientType> ingredientTypes)
    {
        GameObject trayGameObject = Instantiate(tray, traySpawnPoint, Quaternion.identity);
        Plate plateGameObject = Instantiate(plate, spawnPoint, Quaternion.identity);
        plateGameObject.IsStaticObject = false;
        plateGameObject.supplyRenderer.enabled = true;

        BuildOnPlate(plateGameObject, ingredientTypes);

        return (trayGameObject, plateGameObject.gameObject);
    }

    
    public void BuildOnPlate(Plate plate, List<IngredientType> ingredientTypes)
    {
        Debug.Log("Building order on plate");
        List<Ingredient> ingredients = new List<Ingredient>();
        foreach (IngredientType ingredientType in ingredientTypes)
        {
            ingredients.Add(IngredientFactory.CreateIngredient(ingredientType));
        }

        plate.BuildOnPlate(ingredients);
    }
}
