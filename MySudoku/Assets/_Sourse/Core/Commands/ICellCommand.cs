namespace Core.Commands
{
    public interface ICellCommand
    {
        void Execute();
        void Undo();
    }
}
