using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Windows.Storage.Pickers;
using WinRT;

namespace Sugar.WinUI3.Helpers;

internal static class WinUIConversionHelper
{
    internal static void InitFileOpenPicker(FileOpenPicker picker)
    {
        if (Window.Current == null)
        {
            var initializeWithWindowWrapper = picker.As<IInitializeWithWindow>();
            var hwnd = GetActiveWindow();
            initializeWithWindowWrapper.Initialize(hwnd);
        }
    }

    internal static void InitFolderPicker(FolderPicker picker)
    {
        if (Window.Current == null)
        {
            var initializeWithWindowWrapper = picker.As<IInitializeWithWindow>();
            var hwnd = GetActiveWindow();
            initializeWithWindowWrapper.Initialize(hwnd);
        }
    }

    [ComImport, Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IInitializeWithWindow
    {
        void Initialize([In] IntPtr hwnd);
    }

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto, PreserveSig = true, SetLastError = false)]
    private static extern IntPtr GetActiveWindow();
}