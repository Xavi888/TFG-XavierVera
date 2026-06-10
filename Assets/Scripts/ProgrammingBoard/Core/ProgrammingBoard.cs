using UnityEngine;
using System.Collections.Generic;

using System.Collections;


public class ProgrammingBoard : MonoBehaviour
{
    private class Line
    {
        public int LineNumber { get; set; }
        public Vector3 LinePosition { get; set; }
        public ProgrammingBlock Block { get; set; }
        public Line ParentLine { get; set; }


        public Line(int lineNumber, Vector3 linePosition, ProgrammingBlock block, Line parentLine)
        {
            LineNumber = lineNumber;
            LinePosition = linePosition;
            Block = block;
            ParentLine = parentLine;
        }
    }

    private List<Line> lines = new();

    private List<GameObject> currentContentGameObjects = new();
    private List<GameObject> currentGameObjects = new();
    private float height, width, blockHeight, blockWidth, boardScaleMultiplierY, boardScaleMultiplierZ;

    private GameObject ifContentBlock, forContentBlock;

    [SerializeField] private Collider contentCollider;
    [SerializeField] private int numberOfLines;
    [SerializeField] private float blockDepth = 0.2f;
    [SerializeField] private float blockDepthOffset = 0.1f;


    private void Awake()
    {
        InitProgrammingBoard();
        //PlaceBlocks(OrderGenerator.GenerateProgrammingBlocksOrder());
    }

    public void PlaceBlocks(List<ProgrammingBlock> programmingBlocks)
    {   
        Debug.Log("Placing blocks");
        int line;
        foreach (ProgrammingBlock block in programmingBlocks)
        {
            line = GetEmptyLine();
            PlaceBlockAt(block, line);
        }
    }

    private void InitProgrammingBoard()
    {
        height = contentCollider.bounds.size.y;
        width = contentCollider.bounds.size.z;

        blockHeight = height / numberOfLines;
        blockWidth = width;

        ifContentBlock = Resources.Load<GameObject>("Prefabs/ProgrammingBlock/P_IfContentBlock");
        forContentBlock = Resources.Load<GameObject>("Prefabs/ProgrammingBlock/P_ForContentBlock");

        boardScaleMultiplierY = Mathf.Pow(contentCollider.transform.localScale.y, -1f);
        boardScaleMultiplierZ = Mathf.Pow(contentCollider.transform.localScale.z, -1f);

        InitLines();
    }

    private void InitLines()
    {
        float startY = contentCollider.bounds.center.y + height / 2 - blockHeight / 2;

        for (int i = 0; i < numberOfLines; i++)
        {
            float currentY = startY - (i * blockHeight);
            Vector3 linePosition = new(contentCollider.bounds.center.x + blockDepthOffset, currentY, contentCollider.bounds.center.z);
            lines.Add(new Line(i, linePosition, null, null));
        }
    }

    public void PlaceBlockAt(ProgrammingBlock block, int lineNumber)
    {
        block.tag = "ProgrammingBlockOnBoard";
        if (block != null)
        {
            Line currentLine = lines[lineNumber];
            if (currentLine.Block == null)
            {
                currentLine.Block = block;
                block.OnBlockGrabbed += OnBlockGrabbedHandler;
                if (currentLine.ParentLine == null)
                {
                    switch (block.BlockType)
                    {
                        case ProgrammingBlockType.Put:
                            PlaceBlockInLine(block.gameObject, currentLine);
                            break;
                        case ProgrammingBlockType.If:
                            PlaceIfBlock((IfBlock)block, currentLine);
                            break;
                        case ProgrammingBlockType.For:
                            PlaceForBlock((ForBlock)block, currentLine);
                            break;
                    }
                }
                else
                {
                    Line parentLine = currentLine.ParentLine;
                    if (parentLine.Block.BlockType == ProgrammingBlockType.If)
                    {
                        PlaceBlockTabbed(block.gameObject, currentLine, ifContentBlock);
                        IfBlock ifBlock = (IfBlock)parentLine.Block;
                        ifBlock.SuccessBlocks.Add(block);
                    }
                    else if (parentLine.Block.BlockType == ProgrammingBlockType.For)
                    {
                        PlaceBlockTabbed(block.gameObject, currentLine, forContentBlock);
                        ForBlock forBlock = (ForBlock)parentLine.Block;
                        forBlock.IterationBlocks.Add(block);
                    }
                }
            }
            else
            {
                Debug.Log("Hay bloque");
            }
        }
        else
        {
            Debug.Log("Bloque null");
        }
    }

    // Place Methods //
    private void PlaceIfBlock(IfBlock ifBlock, Line line)
    {
        PlaceBlockInLine(ifBlock.gameObject, line);
        if (ifBlock.SuccessBlocks.Count == 0)
        {
            PlaceEmptyTabbed(ifContentBlock, lines[line.LineNumber + 1]);
            lines[line.LineNumber + 1].Block = null;
            lines[line.LineNumber + 1].ParentLine = line;
            PlaceEndBlockInLine(ifContentBlock, lines[line.LineNumber + 2]);
            lines[line.LineNumber + 2].Block = null;
            lines[line.LineNumber + 2].ParentLine = line;
        }
        else
        {
            for (int i = 0; i < ifBlock.SuccessBlocks.Count; i++)
            {
                ProgrammingBlock block = ifBlock.SuccessBlocks[i];
                block.tag = "ProgrammingBlockOnBoard";
                block.OnBlockGrabbed += OnBlockGrabbedHandler;
                PlaceBlockTabbed(block.gameObject, lines[line.LineNumber + i + 1], ifContentBlock);
                lines[line.LineNumber + i + 1].Block = block;
                lines[line.LineNumber + i + 1].ParentLine = line;
            }
            PlaceEndBlockInLine(ifContentBlock, lines[line.LineNumber + ifBlock.SuccessBlocks.Count + 1]);
            lines[line.LineNumber + ifBlock.SuccessBlocks.Count + 1].Block = null;
            lines[line.LineNumber + ifBlock.SuccessBlocks.Count + 1].ParentLine = line;
        }
    }
    private void PlaceForBlock(ForBlock forBlock, Line line)
    {
        PlaceBlockInLine(forBlock.gameObject, line);
        if (forBlock.IterationBlocks.Count == 0)
        {
            PlaceEmptyTabbed(forContentBlock, lines[line.LineNumber + 1]);
            lines[line.LineNumber + 1].Block = null;
            lines[line.LineNumber + 1].ParentLine = line;
            PlaceEndBlockInLine(forContentBlock, lines[line.LineNumber + 2]);
            lines[line.LineNumber + 2].Block = null;
            lines[line.LineNumber + 2].ParentLine = line;
        }
        else
        {
            for (int i = 0; i < forBlock.IterationBlocks.Count; i++)
            {
                ProgrammingBlock block = forBlock.IterationBlocks[i];
                block.tag = "ProgrammingBlockOnBoard";
                block.OnBlockGrabbed += OnBlockGrabbedHandler;
                PlaceBlockTabbed(block.gameObject, lines[line.LineNumber + i + 1], forContentBlock);
                lines[line.LineNumber + i + 1].Block = block; 
                lines[line.LineNumber + i + 1].ParentLine = line;
            }
            PlaceEndBlockInLine(forContentBlock, lines[line.LineNumber + forBlock.IterationBlocks.Count + 1]);
            lines[line.LineNumber + forBlock.IterationBlocks.Count + 1].Block = null;
            lines[line.LineNumber + forBlock.IterationBlocks.Count + 1].ParentLine = line;
        }
    }

    private void PlaceBlockInLine(GameObject block, Line line)
    {
        block.transform.SetPositionAndRotation(line.LinePosition, Quaternion.identity);
        block.transform.parent = transform;
        block.transform.localScale = new Vector3(blockDepth, blockHeight * boardScaleMultiplierY, blockWidth * boardScaleMultiplierZ);
        block.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        currentGameObjects.Add(block);
    }

    private void PlaceBlockTabbed(GameObject block, Line line, GameObject contentBlock)
    {
        Vector3 position = line.LinePosition;
        Vector3 tabPosition = new(position.x, position.y, position.z + (blockWidth * 0.425f));
        Vector3 blockPosition = new(position.x, position.y, position.z - (blockWidth * 0.15f / 2));
        block.transform.SetPositionAndRotation(blockPosition, Quaternion.identity);
        block.transform.parent = transform;
        GameObject contentBlockGameObject = Instantiate(contentBlock, tabPosition, Quaternion.identity);
        contentBlockGameObject.transform.parent = transform;
        block.transform.localScale = new Vector3(blockDepth, blockHeight * boardScaleMultiplierY, blockWidth * boardScaleMultiplierZ * 0.85f);
        contentBlockGameObject.transform.localScale = new Vector3(blockDepth, blockHeight * boardScaleMultiplierY, blockWidth * boardScaleMultiplierZ * 0.15f);
        block.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        contentBlockGameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        currentContentGameObjects.Add(contentBlockGameObject);
        currentGameObjects.Add(block);
    }

    private void PlaceEndBlockInLine(GameObject block, Line line)
    {
        GameObject endBlockGameObject = Instantiate(block, line.LinePosition, Quaternion.identity);
        endBlockGameObject.transform.parent = transform;
        endBlockGameObject.transform.localScale = new Vector3(blockDepth, blockHeight * boardScaleMultiplierY, blockWidth * boardScaleMultiplierZ);
        block.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        currentContentGameObjects.Add(endBlockGameObject);
    }


    private void PlaceEmptyTabbed(GameObject contentBlock, Line line)
    {
        Vector3 position = line.LinePosition;
        Vector3 tabPosition = new(position.x, position.y, position.z + (blockWidth * 0.425f));
        GameObject contentBlockGameObject = Instantiate(contentBlock, tabPosition, Quaternion.identity);
        contentBlockGameObject.transform.parent = transform;
        contentBlockGameObject.transform.localScale = new Vector3(blockDepth, blockHeight * boardScaleMultiplierY, blockWidth * boardScaleMultiplierZ * 0.15f);
        contentBlockGameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        currentContentGameObjects.Add(contentBlockGameObject);
    }

    // Utilities //
    private Line GetNearestLine(Vector3 position)
    {
        float minDistance = Mathf.Infinity;
        Line nearestLine = null;

        foreach (Line line in lines)
        {
            float distance = Vector3.Distance(position, line.LinePosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestLine = line;
            }
        }

        return nearestLine;
    }

    private int GetEmptyLine()
    {
        for (int i = 0; i < numberOfLines; i++)
        {
            if (lines[i].Block == null && lines[i].ParentLine == null)
            {
                return i;
            }
        }
        return numberOfLines;
    }

    public List<ProgrammingBlock> GetCurrentProgrammingBlocks()
    {
        List<ProgrammingBlock> currentProgrammingBlocks = new();
        foreach (Line line in lines)
        {
            if (line.ParentLine == null && line.Block != null)
            {
                currentProgrammingBlocks.Add(line.Block);
            }
        }
        return currentProgrammingBlocks;
    }

    public void ClearProgrammingBoard(bool deleteBlocks = false)
    {
        if (deleteBlocks) {
            foreach (GameObject gameObject in currentGameObjects)
            {
                if (gameObject != null && gameObject.scene.name != null)
                {
                    Destroy(gameObject);
                }
            }
        }
        foreach (GameObject gameObject in currentContentGameObjects)
        {
            if (gameObject != null && gameObject.scene.name != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("Intento de destruir un objeto que no es una instancia: " + gameObject.name);
            }
        }
        currentContentGameObjects.Clear();
        foreach (Line line in lines)
        {
            if (line.Block != null)
            {
                line.Block.OnBlockGrabbed -= OnBlockGrabbedHandler;
            }

        }
        lines.Clear();
        InitLines();
    }

    private int GetLineSizeOfBlock(ProgrammingBlock block)
    {
        switch (block.BlockType)
        {
            case ProgrammingBlockType.Put:
                return 1;
            case ProgrammingBlockType.If:
                if (((IfBlock)block).SuccessBlocks.Count == 0)
                {
                    return 3;
                }
                else
                {
                    return ((IfBlock)block).SuccessBlocks.Count;
                }
            case ProgrammingBlockType.For:
                if (((ForBlock)block).IterationBlocks.Count == 0)
                {
                    return 3;
                }
                else
                {
                    return ((ForBlock)block).IterationBlocks.Count;
                }
            default:
                return 1;
        }
    }

    private Line GetLineFromBlock(ProgrammingBlock block)
    {
        foreach (Line line in lines)
        {
            if (line.Block == block)
            {
                return line;
            }
        }
        return null;
    }

    // Events //
    // Remove block //
    private void OnBlockGrabbedHandler(ProgrammingBlock block)
    {
        Line line = GetLineFromBlock(block);
        if (line.ParentLine == null)
        {
            line.Block = null;
            if (block.BlockType == ProgrammingBlockType.If){
                foreach (ProgrammingBlock iterBlock in ((IfBlock)block).SuccessBlocks) {
                    iterBlock.OnBlockGrabbed -= OnBlockGrabbedHandler;
                    Destroy(iterBlock.gameObject);
                }
                ((IfBlock)block).SuccessBlocks.Clear();
            }
            if (block.BlockType == ProgrammingBlockType.For){
                foreach (ProgrammingBlock iterBlock in ((ForBlock) block).IterationBlocks) {
                    iterBlock.OnBlockGrabbed -= OnBlockGrabbedHandler;
                    Destroy(iterBlock.gameObject);
                }
                ((ForBlock)block).IterationBlocks.Clear();
            }
        }
        else
        {
            if (line.ParentLine.Block.BlockType == ProgrammingBlockType.If)
            {
                IfBlock ifBlock = (IfBlock)line.ParentLine.Block;
                ifBlock.SuccessBlocks.Remove(block);
            }
            else if (line.ParentLine.Block.BlockType == ProgrammingBlockType.For)
            {
                ForBlock forBlock = (ForBlock)line.ParentLine.Block;
                forBlock.IterationBlocks.Remove(block);
            }
        }
        block.OnBlockGrabbed -= OnBlockGrabbedHandler;
        List<ProgrammingBlock> currentProgrammingBlocks = GetCurrentProgrammingBlocks();
        ClearProgrammingBoard();
        PlaceBlocks(currentProgrammingBlocks);
    }
    // Add block //
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Programming board trigger enter");
        if (other.CompareTag("ProgrammingBlock") && !other.gameObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().isSelected)
        {
            int lastLine = GetEmptyLine();
            Line collisionLine = GetNearestLine(other.ClosestPointOnBounds(transform.position));
            ProgrammingBlock block = other.GetComponent<ProgrammingBlock>();
            if (GetLineSizeOfBlock(block) <= (numberOfLines - lastLine))
            {
                if (collisionLine.ParentLine == null)
                {
                    // Realmente solo queremos agregar la linea para obtener la lista actualizada. Asi que solo tenemos que tener el valor de ProgrammingBlock correcto.
                    lines.Insert(collisionLine.LineNumber, new Line(collisionLine.LineNumber, collisionLine.LinePosition, block, null));
                }
                else if (block.BlockType == ProgrammingBlockType.Put)
                {
                    if (collisionLine.ParentLine.Block.BlockType == ProgrammingBlockType.If)
                    {
                        ((IfBlock)collisionLine.ParentLine.Block).SuccessBlocks.Add(block);
                    }
                    else if (collisionLine.ParentLine.Block.BlockType == ProgrammingBlockType.For)
                    {
                        ((ForBlock)collisionLine.ParentLine.Block).IterationBlocks.Add(block);
                    }
                }
                List<ProgrammingBlock> currentProgrammingBlocks = GetCurrentProgrammingBlocks();
                ClearProgrammingBoard();
                PlaceBlocks(currentProgrammingBlocks);
            }
        }
    }
}
