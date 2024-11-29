using System.Diagnostics;

namespace Hikaria.QC
{
    public abstract class ACommandBase
    {
        public abstract string CommandName { get; }
        public string LowerCaseCommand => CommandName.ToLowerInvariant();
        public abstract string Description { get; }
        public abstract Platform SupportedPlatforms { get; }
        public abstract MonoTargetType MonoTarget { get; }
        public virtual string[] ParameterDescriptions { get; } = Array.Empty<string>();
    }

    public abstract class CommandBase : ACommandBase
    {
        public sealed override string[] ParameterDescriptions => Array.Empty<string>();

        public abstract void Execute();
    }

    public abstract class CommandBase<T> : ACommandBase
    {
        public abstract void Execute(T arg);
    }

    public abstract class CommandBase<T1, T2> : ACommandBase
    {
        public abstract void Execute(T1 arg1, T2 arg2);
    }

    public abstract class CommandBase<T1, T2, T3> : ACommandBase
    {
        public abstract void Execute(T1 arg1, T2 arg2, T3 arg3);
    }

    public abstract class CommandBase<T1, T2, T3, T4> : ACommandBase
    {
        public abstract void Execute(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    }

    public abstract class CommandBase<T1, T2, T3, T4, T5> : ACommandBase
    {
        public abstract void Execute(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    }
}