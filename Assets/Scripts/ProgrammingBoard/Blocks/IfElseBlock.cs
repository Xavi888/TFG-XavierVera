using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IfElseBlock : ProgrammingBlock
{
    private ProgrammingBlock successBlocks;
    private ProgrammingBlock failureBlocks;

    public IfElseBlock() {
        BlockType = ProgrammingBlockType.IfElse;
    }
    public override List<IngredientType> Execute()
    {
        throw new System.NotImplementedException();
    }
}
