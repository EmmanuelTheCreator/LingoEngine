namespace LingoEngine.Lingo.Core.Tokenizer
{
    public interface ILingoAstVisitor
    {
        void Visit(HandlerNode node);
        void Visit(ErrorNode node);
        void Visit(CommentNode node);
        void Visit(NewObjNode node);
        void Visit(LiteralNode node);
        void Visit(IfStmtNode node);
        void Visit(IfElseStmtNode node);
        void Visit(EndCaseNode node);
        void Visit(ObjCallNode node);
        void Visit(PutStmtNode node);
        void Visit(TheExprNode node);
        void Visit(BinaryOpNode node);
        void Visit(CaseStmtNode node);
        void Visit(ExitStmtNode node);
        void Visit(ReturnStmtNode node);
        void Visit(TellStmtNode node);
        void Visit(WhenStmtNode node);
        void Visit(OtherwiseNode node);
        void Visit(CaseLabelNode node);
        void Visit(ChunkExprNode node);
        void Visit(InverseOpNode node);
        void Visit(ObjCallV4Node node);
        void Visit(MemberExprNode node);
        void Visit(ObjPropExprNode node);
        void Visit(PlayCmdStmtNode node);
        void Visit(ThePropExprNode node);
        void Visit(MenuPropExprNode node);
        void Visit(SoundCmdStmtNode node);
        void Visit(SoundPropExprNode node);
        void Visit(AssignmentStmtNode node);
        void Visit(ExitRepeatStmtNode node);
        void Visit(NextRepeatStmtNode node);
        void Visit(ObjBracketExprNode node);
        void Visit(SpritePropExprNode node);
        void Visit(ChunkDeleteStmtNode node);
        void Visit(ChunkHiliteStmtNode node);
        void Visit(GlobalDeclStmtNode node);
        void Visit(PropertyDeclStmtNode node);
        void Visit(InstanceDeclStmtNode node);
        void Visit(RepeatWhileStmtNode node);
        void Visit(MenuItemPropExprNode node);
        void Visit(ObjPropIndexExprNode node);
        void Visit(RepeatWithInStmtNode node);
        void Visit(RepeatWithToStmtNode node);
        void Visit(SpriteWithinExprNode node);
        void Visit(LastStringChunkExprNode node);
        void Visit(SpriteIntersectsExprNode node);
        void Visit(StringChunkCountExprNode node);
        void Visit(NotOpNode node);
        void Visit(CallNode node);
        void Visit(VarNode node);
        void Visit(BlockNode node);
        void Visit(DatumNode datumNode);
        void Visit(RepeatWithStmtNode repeatWithStmtNode);
        void Visit(RepeatUntilStmtNode repeatUntilStmtNode);
        void Visit(RepeatForeverStmtNode repeatForeverStmtNode);
        void Visit(RepeatTimesStmtNode repeatTimesStmtNode);
        void Visit(ExitRepeatIfStmtNode exitRepeatIfStmtNode);
        void Visit(NextRepeatIfStmtNode nextRepeatIfStmtNode);
        void Visit(NextStmtNode nextStmtNode);
    }

}




