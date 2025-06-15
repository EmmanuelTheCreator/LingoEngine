namespace LingoEngine.Lingo.Core.Tokenizer
{
    public interface ILingoAstVisitor
    {
        void Visit(LingoHandlerNode node);
        void Visit(LingoErrorNode node);
        void Visit(LingoCommentNode node);
        void Visit(LingoNewObjNode node);
        void Visit(LingoLiteralNode node);
        void Visit(LingoIfStmtNode node);
        void Visit(LingoIfElseStmtNode node);
        void Visit(LingoEndCaseNode node);
        void Visit(LingoObjCallNode node);
        void Visit(LingoPutStmtNode node);
        void Visit(LingoTheExprNode node);
        void Visit(LingoBinaryOpNode node);
        void Visit(LingoCaseStmtNode node);
        void Visit(LingoExitStmtNode node);
        void Visit(LingoReturnStmtNode node);
        void Visit(LingoTellStmtNode node);
        void Visit(LingoWhenStmtNode node);
        void Visit(LingoOtherwiseNode node);
        void Visit(LingoCaseLabelNode node);
        void Visit(LingoChunkExprNode node);
        void Visit(LingoInverseOpNode node);
        void Visit(LingoObjCallV4Node node);
        void Visit(LingoMemberExprNode node);
        void Visit(LingoObjPropExprNode node);
        void Visit(LingoPlayCmdStmtNode node);
        void Visit(LingoThePropExprNode node);
        void Visit(LingoMenuPropExprNode node);
        void Visit(LingoSoundCmdStmtNode node);
        void Visit(LingoSoundPropExprNode node);
        void Visit(LingoAssignmentStmtNode node);
        void Visit(LingoExitRepeatStmtNode node);
        void Visit(LingoNextRepeatStmtNode node);
        void Visit(LingoObjBracketExprNode node);
        void Visit(LingoSpritePropExprNode node);
        void Visit(LingoChunkDeleteStmtNode node);
        void Visit(LingoChunkHiliteStmtNode node);
        void Visit(LingoGlobalDeclStmtNode node);
        void Visit(LingoPropertyDeclStmtNode node);
        void Visit(LingoInstanceDeclStmtNode node);
        void Visit(LingoRepeatWhileStmtNode node);
        void Visit(LingoMenuItemPropExprNode node);
        void Visit(LingoObjPropIndexExprNode node);
        void Visit(LingoRepeatWithInStmtNode node);
        void Visit(LingoRepeatWithToStmtNode node);
        void Visit(LingoSpriteWithinExprNode node);
        void Visit(LingoLastStringChunkExprNode node);
        void Visit(LingoSpriteIntersectsExprNode node);
        void Visit(LingoStringChunkCountExprNode node);
        void Visit(LingoNotOpNode node);
        void Visit(LingoCallNode node);
        void Visit(LingoVarNode node);
        void Visit(LingoBlockNode node);
        void Visit(LingoDatumNode datumNode);
        void Visit(LingoRepeatWithStmtNode repeatWithStmtNode);
        void Visit(LingoRepeatUntilStmtNode repeatUntilStmtNode);
        void Visit(LingoRepeatForeverStmtNode repeatForeverStmtNode);
        void Visit(LingoRepeatTimesStmtNode repeatTimesStmtNode);
        void Visit(LingoExitRepeatIfStmtNode exitRepeatIfStmtNode);
        void Visit(LingoNextRepeatIfStmtNode nextRepeatIfStmtNode);
        void Visit(LingoNextStmtNode nextStmtNode);
    }

}




