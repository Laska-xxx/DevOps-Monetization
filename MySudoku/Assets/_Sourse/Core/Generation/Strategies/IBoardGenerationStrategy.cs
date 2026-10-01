using System.Threading;
using Cysharp.Threading.Tasks;
using Core.Board;

namespace Core.Generation.Strategies
{
    public interface IBoardGenerationStrategy
    {
        BoardSize Size { get; }
        int SideLength { get; }
        int BoxWidth { get; }
        int BoxHeight { get; }

        UniTask<BoardModel> GenerateAsync(DifficultyLevel difficulty, CancellationToken ct);
        bool ValidatePlacement(BoardModel board, int row, int col, int value);
        bool IsBoardComplete(BoardModel board);
    }
}
