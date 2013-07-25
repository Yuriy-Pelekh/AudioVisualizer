using System;
using System.Collections.Generic;
using AudioProcessor;

namespace AudioVisualizer
{
    public class AudioProcessorHelper
    {
        private readonly string _waterMark;
        private readonly Mode _mode;

        public AudioProcessorHelper(string waterMark, Mode mode)
        {
            _waterMark = waterMark;
            _mode = mode;
        }

        public string Process(byte[] originalBytes, out byte[] markedBytesArray, out byte[] unWaterMarkedBytesArray, out int[] markedIndexes, out int[] unMarkedIndexes)
        {
            var processor = new Processor(originalBytes, _mode);

            markedBytesArray = processor.GetWaterMarkedBytes(_waterMark);
            unWaterMarkedBytesArray = processor.ExtractWaterMark(markedBytesArray);

            markedIndexes = processor.InseredWaterMarkIndexes.ToArray();
            unMarkedIndexes = processor.ExtractedWaterMarkIndexes.ToArray();

            var percentage = CalculateResult(processor.OriginalWaterMarkBits, processor.ExtractedWaterMarkBits);
            var percentageByIndexes = ResultByIndexes(markedIndexes, unMarkedIndexes);
            var extractedWaterMark = WaterMark.FromBitArray(processor.ExtractedWaterMarkBits);
            return string.Format("Identity: {0}% / {1}% -> {2}", percentage.ToString("F0"), percentageByIndexes.ToString("F0"), extractedWaterMark);
        }

        private static double CalculateResult(IList<byte> originalWaterMarkBits, IList<byte> extractedWaterMarkBits)
        {
            var assertBitsCount = extractedWaterMarkBits.Count;
            var persentage = 0.0;

            for (var i = 0; i < assertBitsCount; i++)
            {
                if (extractedWaterMarkBits[i%extractedWaterMarkBits.Count] == originalWaterMarkBits[i%originalWaterMarkBits.Count])
                {
                    persentage++;
                }
            }

            persentage /= assertBitsCount;

            return persentage*100;
        }

        private static double ResultByIndexes(IList<int> markedIndexes, IList<int> unMarkedIndexes)
        {
            var count = Math.Min(markedIndexes.Count, unMarkedIndexes.Count);
            var result = 0.0;
            for (var i = 0; i < count; i++)
            {
                if (markedIndexes[i] == unMarkedIndexes[i])
                {
                    result++;
                }
            }
            result /= count;
            result *= 100;
            return result;
        }
    }
}
