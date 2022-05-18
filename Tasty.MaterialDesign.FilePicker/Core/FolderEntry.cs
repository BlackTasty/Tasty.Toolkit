using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace Tasty.MaterialDesign.FilePicker.Core
{
    public class FolderEntry : ViewModelBase, IFilePickerEntry
    {
        private DirectoryInfo mDi;
        private UnloadedFolderEntry mUnloadedEntry;
        private bool mFoldersLoaded;
        private bool mFilesLoaded;

        private bool mIsExpanded;
        private bool mIsSelected;
        private bool mIsLocked;

        public string Path
        {
            get => mDi?.FullName ?? mUnloadedEntry.Path;
            set
            {
                InitializeFolderStructure(new DirectoryInfo(value));
            }
        }

        public string Name
        {
            get => mDi?.Name ?? mUnloadedEntry.Name;
        }

        public bool IsFile => false;

        public bool IsLocked => mIsLocked;

        public bool IsRoot { get; private set; }

        public IconType Icon { get => IconType.Folder; }

        public FolderEntry Parent { get; private set; }

        public FolderEntry Root { get; private set; }

        public object Preview => null;

        public bool FoldersLoaded
        {
            get => mFoldersLoaded;
            private set
            {
                mFoldersLoaded = value;
                InvokePropertyChanged("FoldersLoaded");
            }
        }

        public int TreeDepth { get; private set; }

        public List<FileEntry> Files { get; private set; }
            = new List<FileEntry>();

        public List<FolderEntry> SubDirectories { get; private set; }
            = new List<FolderEntry>();

        public List<IFilePickerEntry> JoinedContent { get; private set; }
            = new List<IFilePickerEntry>();

        public bool IsExpanded
        {
            get => mIsExpanded;
            set
            {
                mIsExpanded = value;
                InvokePropertyChanged("IsExpanded");
            }
        }

        public bool IsSelected
        {
            get => mIsSelected;
            set
            {
                mIsSelected = value;
                InvokePropertyChanged("IsSelected");
            }
        }

        public bool IsImage => false;

        public FolderEntry(DirectoryInfo di, bool isRoot, int treeDepth)
        {
            TreeDepth = treeDepth;
            IsRoot = isRoot;
            InitializeFolderStructure(di);
        }

        /// <summary>
        /// Initialize a new <see cref="FolderEntry"/> which calculates it's tree depth automatically
        /// </summary>
        /// <param name="di">The target directory</param>
        public FolderEntry(DirectoryInfo di)
        {
            string[] pathParts = di.FullName.Split('\\');
            TreeDepth = pathParts.Length - 1;
            IsRoot = TreeDepth == 0;
            InitializeFolderStructure(di);
        }

        private FolderEntry(DirectoryInfo di, bool isLocked)
        {
            string[] pathParts = di.FullName.Split('\\');
            TreeDepth = pathParts.Length - 1;
            IsRoot = TreeDepth == 0;
            mDi = di;

            mIsLocked = isLocked;
            InvokePropertyChanged("IsLocked");
            if (isLocked)
            {
                mFilesLoaded = true;
                SubDirectories.Add(new FolderEntry(di, false));
                JoinedContent.AddRange(SubDirectories);

                FoldersLoaded = true;
            }
        }

        private FolderEntry(UnloadedFolderEntry unloadedEntry, FolderEntry parent)
        {
            mUnloadedEntry = unloadedEntry;
            if (IsRoot)
            {
                parent.Root = this;
            }
            else
            {
                Root = parent.Root;
            }
            TreeDepth = parent.TreeDepth + 1;
            Parent = parent;
        }

        private bool InitializeFolderStructure(DirectoryInfo di)
        {
            mDi = di;

            bool folderAccessSuccess;
            try
            {
                foreach (DirectoryInfo subDi in FilterDirectories(di.EnumerateDirectories()))
                {
                    try
                    {
                        subDi.EnumerateDirectories();
                        FolderEntry entry = new FolderEntry(new UnloadedFolderEntry(subDi, false), this);
                        SubDirectories.Add(entry);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
#if DEBUG
                        Console.WriteLine("Error reading sub-folder \"" + subDi.Name + "\" in folder \"" + di.Name + "\"");
                        Console.WriteLine(ex.Message);
#endif
                        FolderEntry lockedEntry = new FolderEntry(new UnloadedFolderEntry(subDi, true), this);
                        SubDirectories.Add(lockedEntry);
                    }
                }

                SubDirectories.Sort(new NaturalFilePickerEntryComparer());
                JoinedContent.AddRange(SubDirectories);

                folderAccessSuccess = true;
            }
            catch (UnauthorizedAccessException ex)
            {
#if DEBUG
                Console.WriteLine("Error reading folder \"" + di.Name + "\"!");
                Console.WriteLine(ex.Message);
#endif
                mIsLocked = true;
                InvokePropertyChanged("IsLocked");
                folderAccessSuccess = false;
            }

            FoldersLoaded = true;
            return folderAccessSuccess;
        }

        private IEnumerable<DirectoryInfo> FilterDirectories(IEnumerable<DirectoryInfo> unfiltered)
        {
            return unfiltered.Where(x => x.Name != "$Recycle.Bin" && x.Name != "ProgramData" && x.Name != "Windows");
        }

        public bool LoadAllOnDemand()
        {
            bool folderAccessSuccess = true;
            if (!FoldersLoaded)
            {
                folderAccessSuccess = InitializeFolderStructure(mUnloadedEntry.DirectoryInfo); ;
            }

            LoadFoldersOnDemand();

            if (folderAccessSuccess)
            {
                LoadFilesOnDemand();
            }

            return folderAccessSuccess;
        }

        public void LoadFoldersOnDemand()
        {
            for (int i = 0; i < SubDirectories.Count; i++)
            {
                FolderEntry entry = SubDirectories[i];
                if (!entry.FoldersLoaded)
                {
                    UnloadedFolderEntry unloadedFolder = entry.mUnloadedEntry;
                    if (unloadedFolder == null)
                    {
                        return;
                    }

                    if (!unloadedFolder.IsLocked)
                    {
                        SubDirectories[i] = new FolderEntry(entry.mUnloadedEntry.DirectoryInfo, false, TreeDepth + 1);
                    }
                    else
                    {
                        SubDirectories[i] = new FolderEntry(entry.mUnloadedEntry.DirectoryInfo, true);
                    }
                }
            }
        }

        public void LoadFilesOnDemand()
        {
            if (!mFilesLoaded)
            {
                foreach (FileInfo fi in mDi.EnumerateFiles())
                {
                    FileEntry entry = new FileEntry(fi);
                    Files.Add(entry);
                }
                Files.Sort(new NaturalFilePickerEntryComparer());
                JoinedContent.AddRange(Files);
                mFilesLoaded = true;
            }
        }

        public FolderEntry BuildSubTree(StructureBuilder structure, bool loadFiles)
        {
            string nextPath = structure.GetPathAtTreeLayer(TreeDepth + 1);

            if (Path.Equals(structure.TargetPath) && loadFiles)
            {
                LoadAllOnDemand();
            }
            else if (!FoldersLoaded)
            {
                InitializeFolderStructure(mUnloadedEntry.DirectoryInfo);
            }

            //FolderEntry subFolder = SubDirectories.FirstOrDefault(x => x.Path.Equals(nextPath));
            //if (subFolder != null)
            //{
            //    LoadSingleFolderOnDemand(subFolder);
            //}
            //else
            //{
            //}
            return SubDirectories.FirstOrDefault(x => x.Path.Equals(nextPath))?.BuildSubTree(structure, loadFiles) ?? this;
        }

        public List<string> GetFolderStructure(bool alsoSubdirectories)
        {
            List<string> structure = new List<string>();
            foreach (FolderEntry folder in SubDirectories)
            {
                structure.Add(folder.Path + "\n");
                if (alsoSubdirectories)
                {
                    structure.AddRange(folder.GetFolderStructure(alsoSubdirectories));
                }
            }

            foreach (FileEntry file in Files)
            {
                structure.Add(file.Path + "\n");
            }

            return structure;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
