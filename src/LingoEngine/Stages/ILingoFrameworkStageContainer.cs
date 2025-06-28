namespace LingoEngine.Stages
{

    public interface ILingoFrameworkStageContainer
    {
        /// <summary>Assigns the framework-specific stage object.</summary>
        void SetStage(ILingoFrameworkStage stage);
    }
}
