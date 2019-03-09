using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TrackFolderChange.Model
{
    public class ChangedFolder
    {
        public ChangedFolder(string path, TreeNode node)
        {
            Path = path;
            Node = node;
            node.Tag = this;
        }

        public string Path { get; }

        private WatcherChangeTypes _status;
        public WatcherChangeTypes Status
        {
            set
            {
                if (value == _status) return;

                _status = value;
                Node.BackColor = GetColorForChangeType(value);
            }
            get
            {
                return _status;
            }
        }
        public TreeNode Node { get; }


        public void MarkAllAsDeleted()
        {
            Status = WatcherChangeTypes.Deleted;
            foreach (TreeNode item in Node.Nodes)
            {
                ((ChangedFolder)item.Tag).MarkAllAsDeleted();
            }
        }

        public bool IsFolder => Directory.Exists(Path);

        private Color GetColorForChangeType(WatcherChangeTypes changeType)
        {
            switch (changeType)
            {
                case WatcherChangeTypes.Changed: return IsFolder ? Color.Empty : Color.LightBlue;
                case WatcherChangeTypes.Created: return Color.LightGreen;
                case WatcherChangeTypes.Deleted: return Color.Wheat;
                default: return Color.Empty;
            }
        }

    }
}
