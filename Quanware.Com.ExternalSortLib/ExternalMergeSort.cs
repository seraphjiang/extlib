using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quanware.Com.ExternalSortLib
{
    public class ExternalMergeSort
    {
        private static Random rnd = new Random();
        public static void GenerateSorted(int n = 10000, string filename = "SortedFile.txt")
        {
            var lines = Enumerable.Range(1, n).Select(x => x.ToString()).ToArray();
            File.WriteAllLines(filename, lines);
        }
        public static void GenerateShuffleList(int n = 10000, string filename = "ShuffleList.txt")
        {
            var lines = Enumerable.Range(1, n).Select(x => x.ToString()).OrderBy(item => rnd.Next()).ToArray();
            File.WriteAllLines(filename, lines);
        }

        public static void MergeSort(string filename = "ShuffleList.txt", string sortedFile = "SortedFile.txt", int maxBuffer = 1024)
        {
            var totalBlocks = 0;
            var buffer = new int[maxBuffer];
            var i = 0;
            var tempFilename = Path.GetRandomFileName() + ".sort";
            var tempMergeFile = Path.GetRandomFileName() + ".merge";
            using (var fs = new StreamReader(filename))
            {
                // partial read, sort, and write to tmp;
                while (!fs.EndOfStream)
                {
                    buffer[i++] = int.Parse(fs.ReadLine());
                    if (i == maxBuffer || fs.EndOfStream)
                    {
                        if (fs.EndOfStream && i > 0)
                        {
                            buffer = buffer.Take(i).ToArray();
                        }
                        i = 0;
                        Array.Sort(buffer);
                        File.AppendAllLines(tempFilename, buffer.Select(x => x.ToString()));
                        totalBlocks++;
                    }
                }

                // bottom up merge sort
                for (var width = 1; width < totalBlocks; width *= 2)
                {
                    for (var j = 0; j < totalBlocks; j += width * 2)
                    {
                        // merge block[j..j+width-1] + block[j+width, j+2width-1];
                        BottomUpMerge(tempFilename, tempMergeFile, j, Math.Min(j + width, totalBlocks), j + width, Math.Min(totalBlocks, j + 2 * width), maxBuffer);
                    }
                    //finish one round sort
                    File.Copy(tempMergeFile, tempFilename, true);
                    File.Delete(tempMergeFile);
                }
            }

            File.Copy(tempFilename, sortedFile, true);
            File.Delete(tempFilename);
        }

        private static void BottomUpMerge(string filename, string tempMergeFile, int i, int j, int p, int q, int maxBuffer)
        {
            //while both range has data to read
            //  read to buffers
            //  compare, and update output buffer
            //  if output reach max, flush data
            // 

            // where there are still data in either of range
            //    copy to output buffer
            //    if reach max, flush data

            //create three buffers
            int[] run1 = null;
            int[] run2 = null;
            int[] output = new int[maxBuffer];

            int run1RangeIndex = i;
            int run2RangeIndex = p;
            int r1 = maxBuffer;
            int r2 = maxBuffer;
            int o = 0;

            while ((run1RangeIndex < j || (run1RangeIndex == j && run1 != null && r1 < run1.Length)) && (run2RangeIndex < q || (run2RangeIndex == q && run2 != null && r2 < run2.Length)))
            {
                if (run1 == null || r1 == run1.Length)
                {
                    run1 = ReadBlock(filename, run1RangeIndex++, maxBuffer);
                    r1 = 0;
                }

                if (run2 == null || r2 == run2.Length)
                {
                    run2 = ReadBlock(filename, run2RangeIndex++, maxBuffer);
                    r2 = 0;
                }

                while (r1 < run1.Length && r2 < run2.Length)
                {
                    output[o++] = run1[r1] < run2[r2] ? run1[r1++] : run2[r2++];

                    if (o == maxBuffer)
                    {
                        File.AppendAllLines(tempMergeFile, output.Select(x => x.ToString()));
                        o = 0;
                    }
                }
            }

            if (o > 0) File.AppendAllLines(tempMergeFile, output.Take(o).Select(x => x.ToString()));

            if (run1RangeIndex < j || (run1 != null && r1 < run1.Length)) CopyToOutput(filename, tempMergeFile, run1RangeIndex, j, run1, r1, maxBuffer);
            if (run2RangeIndex < q || (run2 != null && r2 < run2.Length)) CopyToOutput(filename, tempMergeFile, run2RangeIndex, q, run2, r2, maxBuffer);
        }

        private static void CopyToOutput(string filename, string tempFilename, int run1RangeIndex, int j, int[] run1, int r1, int maxBuffer)
        {
            int[] output = new int[maxBuffer];
            int o = 0;

            while (run1RangeIndex < j || (run1 != null && r1 < run1.Length))
            {
                if (run1 == null || r1 == run1.Length)
                {
                    run1 = ReadBlock(filename, run1RangeIndex++, maxBuffer);
                    r1 = 0;
                }

                while (r1 < run1.Length)
                {
                    output[o++] = run1[r1++];

                    if (o == maxBuffer)
                    {
                        File.AppendAllLines(tempFilename, output.Select(x => x.ToString()));
                        o = 0;
                    }
                }
            }
            if (o > 0) File.AppendAllLines(tempFilename, output.Take(o).Select(x => x.ToString()));
        }
        private static int[] ReadBlock(string filename, int blockIndex, int maxBuffer)
        {
            var list = new List<int>();
            var totalSkip = blockIndex * maxBuffer;
            using (var fs = new StreamReader(filename))
            {
                while (totalSkip > 0 && !fs.EndOfStream)
                {
                    totalSkip--;
                    fs.ReadLine();
                }

                for (var i = 0; i < maxBuffer && !fs.EndOfStream; i++)
                {
                    list.Add(int.Parse(fs.ReadLine()));
                }
            }

            return list.ToArray();
        }

    }
}
