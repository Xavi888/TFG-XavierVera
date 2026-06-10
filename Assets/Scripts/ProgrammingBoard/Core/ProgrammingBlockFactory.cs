using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgrammingBlockFactory : MonoBehaviour
{
    public static ProgrammingBlock CreateBlock(ProgrammingBlockType programmingBlockType, params object[] args)
    {
        switch(programmingBlockType)
        {
            case ProgrammingBlockType.Put:
                PutBlock putBlock = Resources.Load<PutBlock>("Prefabs/ProgrammingBlock/P_PutBlock");
                PutBlock putGameObject = Instantiate(putBlock);
                putGameObject.Type = args.Length > 0 ? (IngredientType) args[0] : putGameObject.Type;
                putGameObject.InitBlock();
                return putGameObject;
            case ProgrammingBlockType.If:
                IfBlock ifBlock = Resources.Load<IfBlock>("Prefabs/ProgrammingBlock/P_IfBlock");
                IfBlock ifGameObject = Instantiate(ifBlock);
                ifGameObject.SuccessBlocks = args.Length > 0 ? (List<ProgrammingBlock>) args[0] : ifGameObject.SuccessBlocks;
                ifGameObject.Variable = args.Length > 1 ? (IngredientType) args[1] : ifGameObject.Variable;
                ifGameObject.Operator = args.Length > 2 ? (ConditionalOperatorType) args[2] : ifGameObject.Operator;
                ifGameObject.Value = args.Length > 3 ? (int) args[3] : ifGameObject.Value;
                return ifGameObject;  
            case ProgrammingBlockType.For:
                ForBlock forBlock = Resources.Load<ForBlock>("Prefabs/ProgrammingBlock/P_ForBlock");
                ForBlock forGameObject = Instantiate(forBlock);
                forGameObject.IterationBlocks = args.Length > 0 ? (List<ProgrammingBlock>) args[0] : forGameObject.IterationBlocks;
                forGameObject.Iterations = args.Length > 1 ? (int) args[1] : forGameObject.Iterations;
                forGameObject.InitBlock();
                return forGameObject;  
            default:
                return null;  
        }
    }
}
