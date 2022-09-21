using System.Runtime.InteropServices;

namespace Sugar.Image.Similarity;

public static class SimilarityOperations
{
    /// <summary>
    /// Measure the similarity of two given images.
    /// </summary>
    /// <param name="path1">The file path of first image.</param>
    /// <param name="path2">The file path of second image.</param>
    /// <param name="mode">The mode for similarity measurement.</param>
    /// <returns>The result of measurement.</returns>
    [DllImport("Sugar.Image.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "similarityMeasure", CharSet = CharSet.Unicode)]
    public static extern MeasurementResult Measure(string path1, string path2, uint mode);
}