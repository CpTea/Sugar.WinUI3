namespace Sugar.WinUI3.Helpers;

internal static class FilePathHelper
{
    internal static string GetUniqueFilePath(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);

        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(extension))
        {
            return filePath;
        }

        var counter = 1;
        var newFileName = $"{fileName}{extension}";
        var newFullPath = Path.Combine(directory, newFileName);
        while (File.Exists(newFullPath))
        {
            newFileName = $"{fileName}({counter}){extension}";
            newFullPath = Path.Combine(directory, newFileName);
            ++counter;
        }
        return newFullPath;
    }
}
