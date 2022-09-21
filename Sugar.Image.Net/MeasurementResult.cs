using System.Runtime.InteropServices;

namespace Sugar.Image.Similarity;

[StructLayout(LayoutKind.Sequential)]
public struct MeasurementResult
{
    /// <summary>
    /// Whether the content of the given image is the same.
    /// </summary>
    [MarshalAs(UnmanagedType.I1)]
    public bool homogeneity;

    /// <summary>
    /// Differences in the features of the given images.
    /// </summary>
    public double dissimilarity;

    /// <summary>
    /// The feature of the first image.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
    public string feature1;

    /// <summary>
    /// The feature of the second image.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
    public string feature2;
}