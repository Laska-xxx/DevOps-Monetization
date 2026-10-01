using System.Collections.Generic;
using Core.Board;
using Core.Generation.Strategies;

namespace Core.Generation
{
    public interface IBoardGeneratorFactory
    {
        IBoardGenerationStrategy Get(BoardSize size);
    }

    public class BoardGeneratorFactory : IBoardGeneratorFactory
    {
        private readonly Dictionary<BoardSize, IBoardGenerationStrategy> _strategies;

        public BoardGeneratorFactory(
            Classic9x9Strategy classic,
            Grid16x16Strategy grid16,
            Grid25x25Strategy grid25)
        {
            _strategies = new Dictionary<BoardSize, IBoardGenerationStrategy>
            {
                { BoardSize.Nine, classic },
                { BoardSize.Sixteen, grid16 },
                { BoardSize.TwentyFive, grid25 },
            };
        }

        public IBoardGenerationStrategy Get(BoardSize size) => _strategies[size];
    }
}
