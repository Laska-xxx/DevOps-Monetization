using System.Collections.Generic;

namespace Core.Generation
{
    public static class HumanTechniqueSolver
    {
        public static bool TrySolveWithBasicTechniques(byte[] puzzle, int n, int boxSize)
        {
            var grid = (byte[])puzzle.Clone();
            bool progress = true;

            while (progress)
            {
                progress = false;

                for (int row = 0; row < n; row++)
                {
                    for (int col = 0; col < n; col++)
                    {
                        int index = row * n + col;
                        if (grid[index] != 0) continue;

                        var candidates = GetCandidates(grid, n, boxSize, row, col);

                        if (candidates.Count == 1)
                        {
                            grid[index] = (byte)candidates[0];
                            progress = true;
                            continue;
                        }

                        foreach (int value in candidates)
                        {
                            if (IsHiddenSingleInRow(grid, n, boxSize, row, col, value) ||
                                IsHiddenSingleInCol(grid, n, boxSize, row, col, value) ||
                                (boxSize > 0 && IsHiddenSingleInBox(grid, n, boxSize, row, col, value)))
                            {
                                grid[index] = (byte)value;
                                progress = true;
                                break;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < grid.Length; i++)
                if (grid[i] == 0) return false;

            return true;
        }

        private static List<int> GetCandidates(byte[] grid, int n, int boxSize, int row, int col)
        {
            var used = new bool[n + 1];

            for (int i = 0; i < n; i++)
            {
                byte rowVal = grid[row * n + i];
                if (rowVal != 0) used[rowVal] = true;
                byte colVal = grid[i * n + col];
                if (colVal != 0) used[colVal] = true;
            }

            if (boxSize > 0)
            {
                int boxRow = (row / boxSize) * boxSize;
                int boxCol = (col / boxSize) * boxSize;
                for (int r = 0; r < boxSize; r++)
                    for (int c = 0; c < boxSize; c++)
                    {
                        byte v = grid[(boxRow + r) * n + (boxCol + c)];
                        if (v != 0) used[v] = true;
                    }
            }

            var result = new List<int>();
            for (int v = 1; v <= n; v++)
                if (!used[v]) result.Add(v);
            return result;
        }

        private static bool IsHiddenSingleInRow(byte[] grid, int n, int boxSize, int row, int col, int value)
        {
            for (int c = 0; c < n; c++)
            {
                if (c == col) continue;
                if (grid[row * n + c] == 0 && GetCandidates(grid, n, boxSize, row, c).Contains(value))
                    return false;
            }
            return true;
        }

        private static bool IsHiddenSingleInCol(byte[] grid, int n, int boxSize, int row, int col, int value)
        {
            for (int r = 0; r < n; r++)
            {
                if (r == row) continue;
                if (grid[r * n + col] == 0 && GetCandidates(grid, n, boxSize, r, col).Contains(value))
                    return false;
            }
            return true;
        }

        private static bool IsHiddenSingleInBox(byte[] grid, int n, int boxSize, int row, int col, int value)
        {
            int boxRow = (row / boxSize) * boxSize;
            int boxCol = (col / boxSize) * boxSize;

            for (int r = 0; r < boxSize; r++)
            {
                for (int c = 0; c < boxSize; c++)
                {
                    int rr = boxRow + r;
                    int cc = boxCol + c;
                    if (rr == row && cc == col) continue;
                    if (grid[rr * n + cc] == 0 && GetCandidates(grid, n, boxSize, rr, cc).Contains(value))
                        return false;
                }
            }
            return true;
        }
    }
}
