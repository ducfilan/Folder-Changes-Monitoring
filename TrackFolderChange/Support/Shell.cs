using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace TrackFolderChange.Support
{
    public static class Shell
    {
        public static void StartWebPage(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                Process.Start(Convert.ToString(Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\IEXPLORE.EXE", string.Empty, "C:\\Program Files\\Internet Explorer\\IEXPLORE.EXE")), url);
            }
        }

        public static void StartFolder(string path)
        {
            Process.Start(path);
        }

        public static void StartFile(string path)
        {
            Process.Start(path);
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("Shell32", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern int SHILCreateFromPath([MarshalAs(UnmanagedType.VBByRefStr)] ref string pszPath, ref IntPtr ppidl, ref int rgflnOut);

        [DllImport("Shell32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern void ILFree(IntPtr ppidl);

        [DllImport("Shell32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, IntPtr apidl, int dwFlags);

        public static void OpenFolderAndSelectItem(string file)
        {
            try
            {
                var rgflnOut = 0;
                var ppidl = IntPtr.Zero;
                var result = SHILCreateFromPath(ref file, ref ppidl, ref rgflnOut);
                if (ppidl == (IntPtr)0)
                {
                    throw new Exception();
                }
                var e = SHOpenFolderAndSelectItems(ppidl, 0u, IntPtr.Zero, 0);
                ILFree(ppidl);
            }
            catch
            {
                Process.Start("explorer.exe", "/select, \"" + file + "\"");
            }
        }
    }
}
