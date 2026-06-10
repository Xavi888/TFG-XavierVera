using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ProgrammingBlockGenerator : MonoBehaviour
{
    public Transform spawnPoint;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    public ProgrammingBlock programmingBlockPrefab;
    private bool firstBlock = true;

    void Start()
    {
        GenerateNewBlock();
        //grabInteractable.selectExited.AddListener(OnBlockGrabbed);
        //grabInteractable.selectEntered.AddListener(OnBlockSelected);
    }

    void GenerateNewBlock()
    {
        GameObject blockObject = ProgrammingBlockFactory.CreateBlock(programmingBlockPrefab.BlockType).gameObject;
        blockObject.transform.position = spawnPoint.position;
        blockObject.transform.rotation = spawnPoint.rotation;
        
        // Congelar las constraints del Rigidbody del bloque
        Rigidbody blockRigidbody = blockObject.GetComponent<Rigidbody>();
        if (blockRigidbody != null)
        {
            blockRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        //grabInteractable.interactionLayers = InteractionLayerMask.GetMask("Default"); // Ajusta según sea necesario
        //grabInteractable.attachTransform = blockObject.transform;

        // Asociar el XRGrabInteractable al nuevo bloque
        if (firstBlock) {
            grabInteractable = blockObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            grabInteractable.selectExited.AddListener(OnBlockGrabbed);
        } else {
            grabInteractable.selectExited.RemoveListener(OnBlockGrabbed);
            grabInteractable = blockObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            grabInteractable.selectExited.AddListener(OnBlockGrabbed);
        }
    }

    void OnBlockGrabbed(SelectExitEventArgs args)
    {
        GenerateNewBlock();
    }

    void OnDestroy()
    {
        grabInteractable.selectExited.RemoveListener(OnBlockGrabbed);
        //grabInteractable.selectEntered.RemoveListener(OnBlockSelected);
    }
}
