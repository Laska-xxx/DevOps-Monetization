using System;

namespace Core.Generation.PuzzleBank
{
    public static class PuzzleTransformer
    {
        public static PuzzleBankEntry Randomize(PuzzleBankEntry source, int boxWidth, int boxHeight, Random rng)
        {
            int n = source.Size;
            var solution = (byte[])source.FullSolution.Clone();
            var mask = (byte[])source.PuzzleMask.Clone();

            RelabelDigits(solution, n, rng);

            if (boxWidth > 0 && boxHeight > 0)
            {
                ShuffleRowsWithinBands(solution, mask, n, boxHeight, rng);
                ShuffleColsWithinStacks(solution, mask, n, boxWidth, rng);
                ShuffleBands(solution, mask, n, boxHeight, rng);
                ShuffleStacks(solution, mask, n, boxWidth, rng);
            }
            else
            {
                ShuffleAnyRows(solution, mask, n, rng);
                ShuffleAnyCols(solution, mask, n, rng);
            }

            if (rng.Next(2) == 0)
                Transpose(ref solution, ref mask, n);

            int rotations = rng.Next(4);
            for (int i = 0; i < rotations; i++)
                Rotate90(ref solution, ref mask, n);

            return new PuzzleBankEntry
            {
                Size = n,
                Difficulty = source.Difficulty,
                FullSolution = solution,
                PuzzleMask = mask
            };
        }

        private static void RelabelDigits(byte[] solution, int n, Random rng)
        {
            var mapping = new byte[n + 1];
            for (int i = 1; i <= n; i++) mapping[i] = (byte)i;

            for (int i = n; i > 1; i--)
            {
                int j = rng.Next(1, i + 1);
                (mapping[i], mapping[j]) = (mapping[j], mapping[i]);
            }

            for (int i = 0; i < solution.Length; i++)
                solution[i] = mapping[solution[i]];
        }

        private static void ShuffleRowsWithinBands(byte[] solution, byte[] mask, int n, int bandSize, Random rng)
        {
            int bands = n / bandSize;
            for (int band = 0; band < bands; band++)
            {
                var rowsInBand = CreateShuffledIndices(bandSize, rng);
                ApplyRowPermutation(solution, mask, n, band * bandSize, rowsInBand);
            }
        }

        private static void ShuffleColsWithinStacks(byte[] solution, byte[] mask, int n, int stackSize, Random rng)
        {
            int stacks = n / stackSize;
            for (int stack = 0; stack < stacks; stack++)
            {
                var colsInStack = CreateShuffledIndices(stackSize, rng);
                ApplyColPermutation(solution, mask, n, stack * stackSize, colsInStack);
            }
        }

        private static void ShuffleBands(byte[] solution, byte[] mask, int n, int bandSize, Random rng)
        {
            int bands = n / bandSize;
            var order = CreateShuffledIndices(bands, rng);
            ApplyRowBlockPermutation(solution, mask, n, bandSize, order);
        }

        private static void ShuffleStacks(byte[] solution, byte[] mask, int n, int stackSize, Random rng)
        {
            int stacks = n / stackSize;
            var order = CreateShuffledIndices(stacks, rng);
            ApplyColBlockPermutation(solution, mask, n, stackSize, order);
        }

        private static void ShuffleAnyRows(byte[] solution, byte[] mask, int n, Random rng)
        {
            var order = CreateShuffledIndices(n, rng);
            ApplyRowPermutation(solution, mask, n, 0, order);
        }

        private static void ShuffleAnyCols(byte[] solution, byte[] mask, int n, Random rng)
        {
            var order = CreateShuffledIndices(n, rng);
            ApplyColPermutation(solution, mask, n, 0, order);
        }

        private static int[] CreateShuffledIndices(int count, Random rng)
        {
            var arr = new int[count];
            for (int i = 0; i < count; i++) arr[i] = i;
            for (int i = count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            return arr;
        }

        private static void ApplyRowPermutation(byte[] solution, byte[] mask, int n, int offset, int[] localOrder)
        {
            var solCopy = (byte[])solution.Clone();
            var maskCopy = (byte[])mask.Clone();

            for (int localRow = 0; localRow < localOrder.Length; localRow++)
            {
                int srcRow = offset + localOrder[localRow];
                int dstRow = offset + localRow;
                Array.Copy(solCopy, srcRow * n, solution, dstRow * n, n);
                Array.Copy(maskCopy, srcRow * n, mask, dstRow * n, n);
            }
        }

        private static void ApplyColPermutation(byte[] solution, byte[] mask, int n, int offset, int[] localOrder)
        {
            var solCopy = (byte[])solution.Clone();
            var maskCopy = (byte[])mask.Clone();

            for (int row = 0; row < n; row++)
            {
                for (int localCol = 0; localCol < localOrder.Length; localCol++)
                {
                    int srcCol = offset + localOrder[localCol];
                    int dstCol = offset + localCol;
                    solution[row * n + dstCol] = solCopy[row * n + srcCol];
                    mask[row * n + dstCol] = maskCopy[row * n + srcCol];
                }
            }
        }

        private static void ApplyRowBlockPermutation(byte[] solution, byte[] mask, int n, int blockSize, int[] blockOrder)
        {
            var solCopy = (byte[])solution.Clone();
            var maskCopy = (byte[])mask.Clone();

            for (int block = 0; block < blockOrder.Length; block++)
            {
                int srcBlock = blockOrder[block];
                for (int r = 0; r < blockSize; r++)
                {
                    int srcRow = srcBlock * blockSize + r;
                    int dstRow = block * blockSize + r;
                    Array.Copy(solCopy, srcRow * n, solution, dstRow * n, n);
                    Array.Copy(maskCopy, srcRow * n, mask, dstRow * n, n);
                }
            }
        }

        private static void ApplyColBlockPermutation(byte[] solution, byte[] mask, int n, int blockSize, int[] blockOrder)
        {
            var solCopy = (byte[])solution.Clone();
            var maskCopy = (byte[])mask.Clone();

            for (int row = 0; row < n; row++)
            {
                for (int block = 0; block < blockOrder.Length; block++)
                {
                    int srcBlock = blockOrder[block];
                    for (int c = 0; c < blockSize; c++)
                    {
                        int srcCol = srcBlock * blockSize + c;
                        int dstCol = block * blockSize + c;
                        solution[row * n + dstCol] = solCopy[row * n + srcCol];
                        mask[row * n + dstCol] = maskCopy[row * n + srcCol];
                    }
                }
            }
        }

        private static void Transpose(ref byte[] solution, ref byte[] mask, int n)
        {
            var newSolution = new byte[solution.Length];
            var newMask = new byte[mask.Length];

            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    newSolution[c * n + r] = solution[r * n + c];
                    newMask[c * n + r] = mask[r * n + c];
                }

            solution = newSolution;
            mask = newMask;
        }

        private static void Rotate90(ref byte[] solution, ref byte[] mask, int n)
        {
            var newSolution = new byte[solution.Length];
            var newMask = new byte[mask.Length];

            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    int newRow = c;
                    int newCol = n - 1 - r;
                    newSolution[newRow * n + newCol] = solution[r * n + c];
                    newMask[newRow * n + newCol] = mask[r * n + c];
                }

            solution = newSolution;
            mask = newMask;
        }
    }
}
