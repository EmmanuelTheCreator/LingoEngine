namespace OldBlingoEngine.Lingo.Core.Tokenizer
{
    public interface IBlingoAstVisitor
    {
        void Visit(BlingoHandlerNode node);
        void Visit(BlingoErrorNode node);
        void Visit(BlingoCommentNode node);
        void Visit(BlingoNewObjNode node);
        void Visit(BlingoLiteralNode node);
        void Visit(BlingoIfStmtNode node);
        void Visit(BlingoIfElseStmtNode node);
        void Visit(BlingoEndCaseNode node);
        void Visit(BlingoObjCallNode node);
        void Visit(BlingoPutStmtNode node);
        void Visit(BlingoTheExprNode node);
        void Visit(BlingoBinaryOpNode node);
        void Visit(BlingoCaseStmtNode node);
        void Visit(BlingoExitStmtNode node);
        void Visit(BlingoReturnStmtNode node);
        void Visit(BlingoTellStmtNode node);
        void Visit(BlingoWhenStmtNode node);
        void Visit(BlingoOtherwiseNode node);
        void Visit(BlingoCaseLabelNode node);
        void Visit(BlingoChunkExprNode node);
        void Visit(BlingoInverseOpNode node);
        void Visit(BlingoObjCallV4Node node);
        void Visit(BlingoMemberExprNode node);
        void Visit(BlingoObjPropExprNode node);
        void Visit(BlingoPlayCmdStmtNode node);
        void Visit(BlingoThePropExprNode node);
        void Visit(BlingoMenuPropExprNode node);
        void Visit(BlingoSoundCmdStmtNode node);
        void Visit(BlingoSoundPropExprNode node);
        void Visit(BlingoCursorStmtNode node);
        void Visit(BlingoGoToStmtNode node);
        void Visit(BlingoAssignmentStmtNode node);
        void Visit(BlingoSendSpriteStmtNode node);
        void Visit(BlingoSendSpriteExprNode node);
        void Visit(BlingoExitRepeatStmtNode node);
        void Visit(BlingoNextRepeatStmtNode node);
        void Visit(BlingoRangeExprNode node);
        void Visit(BlingoObjBracketExprNode node);
        void Visit(BlingoSpritePropExprNode node);
        void Visit(BlingoChunkDeleteStmtNode node);
        void Visit(BlingoChunkHiliteStmtNode node);
        void Visit(BlingoGlobalDeclStmtNode node);
        void Visit(BlingoPropertyDeclStmtNode node);
        void Visit(BlingoInstanceDeclStmtNode node);
        void Visit(BlingoRepeatWhileStmtNode node);
        void Visit(BlingoMenuItemPropExprNode node);
        void Visit(BlingoObjPropIndexExprNode node);
        void Visit(BlingoRepeatWithInStmtNode node);
        void Visit(BlingoRepeatWithToStmtNode node);
        void Visit(BlingoSpriteWithinExprNode node);
        void Visit(BlingoLastStringChunkExprNode node);
        void Visit(BlingoSpriteIntersectsExprNode node);
        void Visit(BlingoStringChunkCountExprNode node);
        void Visit(BlingoNotOpNode node);
        void Visit(BlingoCallNode node);
        void Visit(BlingoVarNode node);
        void Visit(BlingoBlockNode node);
        void Visit(BlingoDatumNode datumNode);
        void Visit(BlingoRepeatWithStmtNode repeatWithStmtNode);
        void Visit(BlingoRepeatUntilStmtNode repeatUntilStmtNode);
        void Visit(BlingoRepeatForeverStmtNode repeatForeverStmtNode);
        void Visit(BlingoRepeatTimesStmtNode repeatTimesStmtNode);
        void Visit(BlingoExitRepeatIfStmtNode exitRepeatIfStmtNode);
        void Visit(BlingoNextRepeatIfStmtNode nextRepeatIfStmtNode);
        void Visit(BlingoNextStmtNode nextStmtNode);
    }

}





