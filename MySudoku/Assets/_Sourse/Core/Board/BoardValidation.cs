namespace Core.Board
{
    public static class BoardValidation
    {
        public static bool IsPlacementValid(BoardModel board, int row, int col, int value, int boxWidth, int boxHeight)
        {
            int n = board.SideLength;

            for (int i = 0; i < n; i++)
            {
                if (i != col && board.GetValue(row, i) == value) return false;
                if (i != row && board.GetValue(i, col) == value) return false;
            }

            if (boxWidth > 0 && boxHeight > 0)
            {
                int boxRow = (row / boxHeight) * boxHeight;
                int boxCol = (col / boxWidth) * boxWidth;

                for (int r = 0; r < boxHeight; r++)
                {
                    for (int c = 0; c < boxWidth; c++)
                    {
                        int rr = boxRow + r;
                        int cc = boxCol + c;
                        if ((rr != row || cc != col) && board.GetValue(rr, cc) == value)
                            return false;
                    }
                }
            }

            return true;
        }

        public static bool IsComplete(BoardModel board)
        {
            int n = board.SideLength;
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    if (board.GetValue(r, c) != board.Solution[r * n + c])
                        return false;
            return true;
        }
    }
}
