namespace LingoEngine.FrameworkCommunication
{
    /// <summary>
    /// Optional container around the stage used by some frameworks.
    /// Allows embedding the stage in a separate window.
    /// </summary>
    public interface ILingoFrameworkStageWindow
    {
        /// <summary>Assigns the framework-specific stage object.</summary>
        void SetStage(ILingoFrameworkStage stage);
    }
}
