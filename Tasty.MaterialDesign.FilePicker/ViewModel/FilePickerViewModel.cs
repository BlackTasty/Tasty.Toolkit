using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.MaterialDesign.FilePicker.Core;
using Tasty.ViewModel;

namespace Tasty.MaterialDesign.FilePicker.ViewModel
{
    class FilePickerViewModel : ViewModelBase
    {
        public event EventHandler<DefaultDestinationSelectedEventArgs> DefaultDestinationSelected;

        private FileExplorerInstance fileExplorer;
        private FolderEntry mSelectedFolder;
        private IFilePickerEntry mSelectedEntry;
        private int mDisplayOption;
        private int mSelectedIndex = -1;
        private string mCurrentPath;

        private VeryObservableCollection<string> mDefaultDestinations = new VeryObservableCollection<string>("DefaultDestinations");

        public VeryObservableCollection<FolderEntry> RootFolders
        {
            get
            {
                if (fileExplorer == null)
                {
                    fileExplorer = FileExplorerInstance.Instance;
                }
                return fileExplorer.RootFolders;
            }
        }

        public VeryObservableCollection<string> DefaultDestinations
        {
            get => mDefaultDestinations;
            private set
            {
                mDefaultDestinations = value;
                InvokePropertyChanged();
            }
        }

        public string SelectedDestination
        {
            get => mCurrentPath;
            set
            {
                SetCurrentDirectory(value);
                OnDefaultDestinationSelected(new DefaultDestinationSelectedEventArgs(SelectedFolder));
            }
        }

        public FolderEntry SelectedFolder
        {
            get => mSelectedFolder;
            set
            {
                mSelectedFolder = value;
                if (value != null)
                {
                    CurrentPath = value.Path;
                    InvokePropertyChanged();
                    InvokePropertyChanged("FileList");
                    InvokePropertyChanged("IsRoot");
                }
            }
        }

        public IFilePickerEntry SelectedEntry
        {
            get => mSelectedEntry;
            set
            {
                mSelectedEntry = value;
                InvokePropertyChanged("SelectedEntry");
                InvokePropertyChanged("IsSelectEnabled");

                /*if (!(value is ClassicEntry))
                {
                    InvokePropertyChanged("SelectedEntry");
                    InvokePropertyChanged("IsSelectEnabled");
                }*/
            }
        }

        public IEnumerable<IFilePickerEntry> FileList
        {
            //get => IsFolderSelect ? SelectedFolder?.SubDirectories?.Cast<IFilePickerEntry>() : SelectedFolder?.JoinedContent;
            get
            {
                if (!IsFolderSelect)
                {
                    if (Filters != null && Filters.Count > 0)
                    {
                        return SelectedFolder?.JoinedContent?.Where(x => !x.IsFile ||
                        Filters.Any(f => f.FilterMatch((x as FileEntry).Extension)));
                    }
                    else
                    {
                        return SelectedFolder?.JoinedContent;
                    }
                }
                else
                {
                    return SelectedFolder?.SubDirectories?.Cast<IFilePickerEntry>();
                }
            }
        }

        public int DisplayOption
        {
            get => mDisplayOption;
            set
            {
                mDisplayOption = value;
                InvokePropertyChanged();
            }
        }

        public int SelectedIndex
        {
            get => mSelectedIndex;
            set
            {
                mSelectedIndex = value;
                InvokePropertyChanged();
            }
        }

        public string CurrentPath
        {
            get => mCurrentPath;
            set
            {
                mCurrentPath = value;
                InvokePropertyChanged();
                InvokePropertyChanged("SelectedDestination");
                InvokePropertyChanged("IsSelectEnabled");
            }
        }

        public bool IsSelectEnabled
        {
            get => IsFolderSelect ? (!mSelectedEntry?.IsFile ?? false) || (!mSelectedFolder?.IsFile ?? false) :
                mSelectedEntry?.IsFile ?? false;
        }

        public bool IsRoot
        {
            get => SelectedFolder?.IsRoot ?? true;
        }

        public List<FileFilter> Filters { get; private set; }

        public bool IsFolderSelect { get; set; }

        public FilePickerViewModel()
        {
            fileExplorer = FileExplorerInstance.Instance;
            fileExplorer.DrivesLoaded += FileExplorer_DrivesLoaded;
        }

        private void FileExplorer_DrivesLoaded(object sender, EventArgs e)
        {
            RefreshDefaultDestinations();
        }

        public void RefreshDefaultDestinations()
        {
            mDefaultDestinations.Clear();
            mDefaultDestinations.AddRange(new List<string>()
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
            });

            foreach (FolderEntry rootFolder in fileExplorer.RootFolders)
            {
                mDefaultDestinations.Add(rootFolder.Path);
            }
        }

        public void SetFilters(string filtersRaw)
        {
            Filters = new List<FileFilter>();
            string[] filters = filtersRaw.Split('|');
            for (int i = 0; i < filters.Length; i += 2)
            {
                Filters.Add(new FileFilter(filters[i], filters[i + 1]));
            }
        }

        public StructureBuilder SetCurrentDirectory(string path)
        {
            if (!path.Equals(SelectedFolder?.Path))
            {
                StructureBuilder structure = new StructureBuilder(path);
                SelectedFolder = RootFolders.FirstOrDefault(x => x.Path.Equals(structure.RootFolder))?.BuildSubTree(structure, !IsFolderSelect);
                return structure;
            }
            return null;
        }

        public void RefreshDrives()
        {
            if (fileExplorer == null)
            {
                fileExplorer = FileExplorerInstance.Instance;
            }
            fileExplorer.LoadDrives();
        }

        protected virtual void OnDefaultDestinationSelected(DefaultDestinationSelectedEventArgs e)
        {
            DefaultDestinationSelected?.Invoke(this, e);
        }
    }
}
