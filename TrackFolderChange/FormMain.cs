using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TrackFolderChange.Model;
using TrackFolderChange.Support;

namespace TrackFolderChange
{
    public partial class FormMain : Form
    {
        private Dictionary<string, ChangedFolder> _nodes;
        private string _rootFolder;
        private LogWriter _logWriter;
        private FilePropertiesExtractor _filePropertiesExtractor;
        private const string FileExtensionPattern = @"(\..*)$";

        IconsHandler _icons;

        public FormMain()
        {
            InitializeComponent();
            
            _icons = new IconsHandler(true, false);
            treeView.ImageList = _icons.SmallIcons;
        }

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;

            txtFolderPath.Text = Properties.Settings.Default.FolderPath = folderBrowserDialog.SelectedPath;
            Properties.Settings.Default.Save();
        }

        private void UpdateTree(string rootFolder)
        {
            fileSystemWatcher.EnableRaisingEvents = false;

            rootFolder = rootFolder.Trim('"');

            if (rootFolder.Length == 2 && rootFolder[1] == ':') rootFolder += '\\';
            rootFolder = Path.GetFullPath(rootFolder);
            if (rootFolder.Length > 3) rootFolder = rootFolder.TrimEnd('\\');
            rootFolder = char.ToUpper(rootFolder[0]) + rootFolder.Substring(1);

            _rootFolder = rootFolder;
            txtFolderPath.Text = rootFolder;
            _nodes = new Dictionary<string, ChangedFolder>();

            fileSystemWatcher.Path = rootFolder;
            fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | 
                                             NotifyFilters.DirectoryName | NotifyFilters.FileName | 
                                             NotifyFilters.LastWrite | NotifyFilters.Security | 
                                             NotifyFilters.Size;

            treeView.Nodes.Clear();

            var folder = CreateNode(rootFolder).Node;
            folder.Tag = new ChangedFolder(rootFolder, folder);
            treeView.Nodes.Add(folder);
            fileSystemWatcher.EnableRaisingEvents = true;

            treeView.Focus();
        }


        private ChangedFolder CreateNode(string path)
        {
            var oldTopNode = treeView.TopNode;
            var name = Path.GetFileName(path);
            if (path.Equals(_rootFolder, StringComparison.CurrentCultureIgnoreCase))
                name = path;
            var folder = new ChangedFolder(path, new TreeNode(name));
            _nodes[path.ToLower()] = folder;
            folder.Node.ExpandAll();
            folder.Node.SelectedImageIndex = folder.Node.ImageIndex = _icons.GetIcon(path);
            treeView.TopNode = oldTopNode;
            return folder;
        }

        private ChangedFolder GetOrCreateNode(string path, WatcherChangeTypes changeType)
        {
            var changedUsername = _filePropertiesExtractor.GetSpecificFileProperties(path, 10);
            if (!string.IsNullOrEmpty(changedUsername))
			{
				_logWriter.Write("User: " + changedUsername + "; Path: " + path + "; Type: " + changeType);
			}

            ChangedFolder folder;
            var lowerCaseName = Path.GetFullPath(path).ToLower();
            if (lowerCaseName == _rootFolder.ToLower()) return (ChangedFolder)treeView.Nodes[0].Tag;
            if (!_nodes.TryGetValue(lowerCaseName, out folder))
            {
                var parentNode = GetOrCreateNode(Path.GetDirectoryName(path), WatcherChangeTypes.Changed);
                folder = CreateNode(path);
                parentNode.Node.Nodes.Add(folder.Node);
                if (parentNode.Node.Nodes.Count == 1)
                    parentNode.Node.ExpandAll();
            }
            if (changeType == WatcherChangeTypes.Deleted) folder.MarkAllAsDeleted();

            if (!(changeType == WatcherChangeTypes.Changed && folder.Status == WatcherChangeTypes.Created))
            {
                folder.Status = changeType;
            }
            return folder;
        }

        private void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            GetOrCreateNode(e.FullPath, e.ChangeType);
        }

        private void fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            GetOrCreateNode(e.FullPath, e.ChangeType);
        }

        private void fileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            GetOrCreateNode(e.FullPath, e.ChangeType);
        }

        private void fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            GetOrCreateNode(e.OldFullPath, WatcherChangeTypes.Deleted);
            GetOrCreateNode(e.FullPath, WatcherChangeTypes.Created);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            txtFolderPath.Text = Properties.Settings.Default.FolderPath;
            txtLogFilePath.Text = Properties.Settings.Default.LogFilePath;

            _logWriter = new LogWriter(Regex.Replace(txtLogFilePath.Text, FileExtensionPattern, DateTime.Now.ToString("yyyyMMdd") + "$1"));
            _filePropertiesExtractor = new FilePropertiesExtractor();
		}

        private void btnClear_Click(object sender, EventArgs e)
        {
            TryUpdateTree(_rootFolder);
        }

        private void TryUpdateTree(string path)
        {
            try
            {
                UpdateTree(path);
            }
            catch (Exception ex)
            {
                FormManipulator.ShowError(ex.Message);
            }
        }

        private void btnBrowseLogFile_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

            txtLogFilePath.Text = Properties.Settings.Default.LogFilePath = saveFileDialog.FileName;
            Properties.Settings.Default.Save();

            _logWriter = new LogWriter(Regex.Replace(txtLogFilePath.Text, FileExtensionPattern, DateTime.Now.ToString("yyyyMMdd") + "$1"));
        }

        private void btnMonitor_Click(object sender, EventArgs e)
        {
            TryUpdateTree(txtFolderPath.Text);
        }

		private void BtnHelp_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Access rights manipulator by Duc Filan!", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}
}
