using UnityEngine;

public class Meat : Ingredient
{
    public bool Cooked { get; set; }

    [SerializeField] private float cookingDuration = 10f;
    [SerializeField] private Color initialColor = new Color(165, 42, 42); // Color de carne cruda
    [SerializeField] private Color endColor = new Color(160, 82, 45); // Color de carne cocinada
    [SerializeField] private string cookingPlateTag = "CookingPlate"; // Tag de la plancha de cocinado

    private Renderer meatRenderer;
    private GameObject cookingParticles;
    private AudioSource cookingSound;
    private float remainingCookingTime;
    private bool cooking, cookingStarted;

    protected override void Start()
    {
        base.Start();
        IngredientType = IngredientType.Meat;
        cookingParticles = transform.Find("ParticleSystem").gameObject;
        cookingParticles.SetActive(false);
        cookingSound = GetComponent<AudioSource>();
        meatRenderer = GetComponent<Renderer>();
        meatRenderer.material.color = initialColor;
    }

    protected override void Update()
    {
        base.Update();
        
        if (Cooked) {
            StopCooking();
            meatRenderer.material.color = endColor;
        } else if (cooking && !Cooked)
        {
            remainingCookingTime -= Time.deltaTime;

            if (remainingCookingTime <= 0f) {
                Cooked = true;
                meatRenderer.material.color = endColor;
            } else {
                float cookingProgress = 1f - (remainingCookingTime / cookingDuration);
                Cook(cookingProgress);
            }
            
            
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        string supplierName = "P_MeatSupplier";
        GameObject og = GameObject.Find(supplierName);
        if (og != null)
            origin = og.GetComponent<Supplier>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(cookingPlateTag))
        {
            if (!cookingStarted)
            {
                StartCooking();
            }
            else
            {
                ContinueCooking();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(cookingPlateTag))
        {
            if (cooking)
            {
                StopCooking();
            }
        }
    }

    private void StartCooking()
    {
        cookingSound.Play();
        cookingParticles.SetActive(true);
        cooking = true;
        cookingStarted = true;
        remainingCookingTime = cookingDuration;
    }

    private void ContinueCooking()
    {
        cookingSound.Play();
        cookingParticles.SetActive(true);
        cooking = true;
    }

    private void StopCooking()
    {
        cookingSound.Stop();
        cookingParticles.SetActive(false);
        cooking = false;
    }


    private void Cook(float cookingProgress)
    {
        Color currentColor = Color.Lerp(initialColor, endColor, cookingProgress);
        meatRenderer.material.color = currentColor;
    }
}
