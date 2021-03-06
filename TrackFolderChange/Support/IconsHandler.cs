using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TrackFolderChange.Support
{

    public class IconsHandler : IDisposable
    {

        private ImageList _smallImageList;
        private ImageList _largeImageList;

        private bool _useSmallIcons;

        private bool _useLargeIcons;

        private Dictionary<string, int> loadedIcons = new Dictionary<string, int>();

        public ImageList SmallIcons => _smallImageList;

        public ImageList LargeIcons => _largeImageList;

        public void Clear()
        {
            _smallImageList = null;
            _largeImageList = null;
            if (_useSmallIcons)
            {
                _smallImageList = new ImageList();
                _smallImageList.ColorDepth = ColorDepth.Depth32Bit;
            }
            if (_useLargeIcons)
            {
                _largeImageList = new ImageList();
                _smallImageList.ColorDepth = ColorDepth.Depth32Bit;
            }
            _length = 0;
            loadedIcons.Clear();
        }

        public int Add(Image smallImage, Image largeImage)
        {
            _length++;
            _smallImageList?.Images.Add(smallImage);
            _largeImageList?.Images.Add(largeImage);
            return _length - 1;
        }

        public IconsHandler(bool useSmallIcons, bool useLargeIcons)
        {
            if ((!useSmallIcons && !useLargeIcons))
            {
                throw new ArgumentException("Cannot create an IconsHandler without ImageLists");
            }

            _useSmallIcons = useSmallIcons;
            _useLargeIcons = useLargeIcons;
            Clear();
        }

        public IconsHandler(ImageList smallImagesList, ImageList largeImagesList)
        {
            if (largeImagesList == null && smallImagesList == null)
            {
                throw new ArgumentException("Cannot create an IconsHandler without ImageLists");
            }
            if (largeImagesList != null && smallImagesList != null && largeImagesList.Images.Count != smallImagesList.Images.Count)
            {
                throw new ArgumentException("Initial ImageLists do not have the same number of elements");
            }

            _useSmallIcons = smallImagesList != null;
            _useLargeIcons = largeImagesList != null;

            _smallImageList = smallImagesList;
            _largeImageList = largeImagesList;

            _length = _useLargeIcons ? largeImagesList.Images.Count : smallImagesList.Images.Count;
        }

        private int _length;

        public int GetIcon(string path)
        {
            if (loadedIcons.ContainsKey(path))
            {
                return loadedIcons[path];
            }
            if (_useLargeIcons)
            {
                _largeImageList.Images.Add(GetIcon(path, true));
            }
            if (_useSmallIcons)
            {
                _smallImageList.Images.Add(GetIcon(path, false));
            }
            _length++;
            loadedIcons.Add(path, (_length - 1));
            return (_length - 1);
        }

        public void ApplyToListView(ListView ListView)
        {
            ListView.SmallImageList = SmallIcons;
            ListView.LargeImageList = LargeIcons;
        }

        private static Image GetIcon(string path, bool large, bool realFile)
        {
            var shInfo = new SHFILEINFO();
            try
            {
                uint flags = SHGFI_ICON;
                flags |= large ? SHGFI_LARGEICON : SHGFI_SMALLICON;
                if (!realFile) flags |= SHGFI_USEFILEATTRIBUTES;

                var isProbablyFolder = !realFile && string.IsNullOrEmpty(Path.GetExtension(path));

                IntPtr hImg;
                hImg = SHGetFileInfo(path, isProbablyFolder ? FILE_ATTRIBUTE_DIRECTORY : 0, ref shInfo, (uint)Marshal.SizeOf(shInfo), flags);

                if (realFile && hImg == IntPtr.Zero)
                {
                    return GetIcon(path, large, false);
                }

                var icon = Icon.FromHandle(shInfo.hIcon);
                return icon.ToBitmap();
            }
            finally
            {
                if (shInfo.hIcon != IntPtr.Zero)
                    DestroyIcon(shInfo.hIcon);
            }
        }

        public static Image GetIcon(string path, bool large)
        {
            var dontSearchRealFile = path.StartsWith(".") || path.StartsWith("\\\\");
            return GetIcon(path, large, !dontSearchRealFile);
        }

        [DllImport("shell32.dll")]
        private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_SMALLICON = 0x1;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        private const uint SHGFI_SYSICONINDEX = 0x4000;
        private const uint SHGFI_PIDL = 0x8;
        private const uint SHGFI_OPENICON = 0x2;

        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;

        public void Dispose()
        {
            LargeIcons?.Dispose();
            SmallIcons?.Dispose();
        }
    }
}