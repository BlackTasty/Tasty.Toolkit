using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tasty.MaterialDesign.FilePicker.Core;
using Tasty.MaterialDesign.FilePicker.ViewModel;

namespace Tasty.MaterialDesign.FilePicker
{
    /// <summary>
    /// Interaction logic for FilePicker.xaml
    /// </summary>
    public partial class FilePicker : Grid
    {
        private bool isWindowDialog;
        private Window dialogWindow;

        //TODO: Try to fix dependency property of InitialDirectory (get/set and Changed event not fired when using Binding)
        #region DialogClosed Event
        public event EventHandler<FilePickerClosedEventArgs> DialogClosed;

        protected virtual void OnDialogClosed(FilePickerClosedEventArgs e)
        {
            DialogClosed?.Invoke(this, e);
        }

        /*public static readonly RoutedEvent DialogClosedEvent = EventManager.RegisterRoutedEvent(
            "DialogClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FilePicker));

        public event RoutedEventHandler DialogClosed
        {
            add { AddHandler(DialogClosedEvent, value); }
            remove { RemoveHandler(DialogClosedEvent, value); }
        }

        private void RaiseDialogClosedEvent(string path)
        {
            FilePickerClosedEventArgs eventArgs = new FilePickerClosedEventArgs(DialogClosedEvent, path);
            //RaiseEvent(eventArgs);
        }*/
        #endregion

        #region DialogOpened Event
        public static readonly RoutedEvent DialogOpenedEvent = EventManager.RegisterRoutedEvent(
            "DialogOpened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FilePicker));

        public event RoutedEventHandler DialogOpened
        {
            add { AddHandler(DialogOpenedEvent, value); }
            remove { RemoveHandler(DialogOpenedEvent, value); }
        }

        private void RaiseDialogOpenedEvent()
        {
            RaiseEvent(new RoutedEventArgs(FilePicker.DialogOpenedEvent, this));
        }
        #endregion

        #region InitialDirectory
        public string InitialDirectory
        {
            get { return (string)GetValue(InitialDirectoryProperty); }
            set { SetValue(InitialDirectoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InitialDirectory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InitialDirectoryProperty =
            DependencyProperty.Register("InitialDirectory", typeof(string), typeof(FilePicker),
                new FrameworkPropertyMetadata(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    new PropertyChangedCallback(OnInitialDirectoryChanged)));
        #endregion

        #region IsFolderSelect
        public bool IsFolderSelect
        {
            get { return (bool)GetValue(IsFolderSelectProperty); }
            set
            {
                (DataContext as FilePickerViewModel).IsFolderSelect = value;
                SetValue(IsFolderSelectProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for IsFolderSelect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFolderSelectProperty =
            DependencyProperty.Register("IsFolderSelect", typeof(bool), typeof(FilePicker),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsFolderSelectChanged)));
        #endregion

        #region Filter
        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Filter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(string), typeof(FilePicker),
                new FrameworkPropertyMetadata(string.Format("{0}|*.*", Util.FindLocalisedString("filter_allFiles")), 
                    new PropertyChangedCallback(OnFilterChanged)));
        #endregion

        #region SuppressCloseCommand
        public bool SuppressCloseCommand
        {
            get { return (bool)GetValue(SuppressCloseCommandProperty); }
            set { SetValue(SuppressCloseCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SuppressCloseCommanf.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SuppressCloseCommandProperty =
            DependencyProperty.Register("SuppressCloseCommand", typeof(bool), typeof(FilePicker), new PropertyMetadata(false));
        #endregion

        #region Title
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(FilePicker), new PropertyMetadata(""));

        #endregion

        #region IsLoading
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLoading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(FilePicker), new PropertyMetadata(false));
        #endregion

        private FolderEntry originalFolder;
        private IFilePickerEntry originalEntry;
        private bool currentFolderSetByMouseDown;

        private static void OnIsFolderSelectChanged(DependencyObject depO, DependencyPropertyChangedEventArgs e)
        {
            if (depO is FilePicker filePicker)
            {
                (filePicker.DataContext as FilePickerViewModel).IsFolderSelect = Convert.ToBoolean(e.NewValue);
            }
        }

        private static void OnInitialDirectoryChanged(DependencyObject depO, DependencyPropertyChangedEventArgs e)
        {
            if (depO is FilePicker filePicker)
            {
                (filePicker.DataContext as FilePickerViewModel).CurrentPath = Convert.ToString(e.NewValue);
            }
        }

        private static void OnFilterChanged(DependencyObject depO, DependencyPropertyChangedEventArgs e)
        {
            if (depO is FilePicker filePicker)
            {
                (filePicker.DataContext as FilePickerViewModel).SetFilters(Convert.ToString(e.NewValue));
            }
        }

        private static void JumpToNodeAsync(FilePicker filePicker, ItemCollection items, StructureBuilder structure)
        {
            if (structure != null)
            {
                filePicker.IsLoading = true;
                bool done = false;
                int layer = 0;

                new Thread(() =>
                {
                    List<FolderEntry> entries = items.OfType<FolderEntry>().ToList();
                    while (!done)
                    {
                        FolderEntry entry = entries.FirstOrDefault(x => x.Path.Equals(structure.GetPathAtTreeLayer(layer)));

                        if (entry != null)
                        {
                            entry.IsExpanded = true;
                            entries = entry.SubDirectories.ToList();

                            if (Util.NormalizePath(entry.Path).Equals(Util.NormalizePath(structure.TargetPath)))
                            {
                                entry.IsSelected = true;
                                done = true;

                                filePicker.Dispatcher.Invoke(() =>
                                {
                                    filePicker.IsLoading = false;
                                });
                            }

                            layer++;
                        }
                    }
                }).Start();
            }
        }

        public FilePicker()
        {
            InitializeComponent();
            FilePickerViewModel vm = DataContext as FilePickerViewModel;
            vm.SetFilters(Filter);
            vm.DefaultDestinationSelected += FilePicker_DefaultDestinationSelected;
        }

        public static bool? ShowDialog(FilePicker filePicker, Window window)
        {
            filePicker.isWindowDialog = true;
            filePicker.SuppressCloseCommand = true;
            window.Content = filePicker;
            window.Title = filePicker.Title;
            window.MinWidth = 800;
            window.MinHeight = 500;

            filePicker.dialogWindow = window;

            return window.ShowDialog();
        }

        private void FilePicker_DefaultDestinationSelected(object sender, DefaultDestinationSelectedEventArgs e)
        {
            /*if (e.SelectedFolder != null)
            {
                FolderEntry target = tree.Items
                    .Cast<FolderEntry>()
                    .FirstOrDefault(x => x.Path == e.SelectedFolder.Path);

                if (target != null)
                {
                    target.IsExpanded = true;
                }
            }*/
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.OriginalSource as TreeViewItem;

            if (item.DataContext is FolderEntry entry)
            {
                entry.LoadFoldersOnDemand();
                if (currentFolderSetByMouseDown)
                {
                    FilePickerViewModel vm = DataContext as FilePickerViewModel;
                    vm.SelectedFolder = originalFolder;
                    vm.SelectedEntry = originalEntry;

                    originalFolder = null;
                    originalEntry = null;
                    currentFolderSetByMouseDown = false;
                }
            }
        }

        private void TreeViewItem_Collapsed(object sender, RoutedEventArgs e)
        {
            if (currentFolderSetByMouseDown)
            {
                FilePickerViewModel vm = DataContext as FilePickerViewModel;
                vm.SelectedFolder = originalFolder;
                vm.SelectedEntry = originalEntry;

                originalFolder = null;
                originalEntry = null;
                currentFolderSetByMouseDown = false;
            }
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as FilePickerViewModel).RefreshDrives();
        }

        private void FileView_Details_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileView_DoubleClick();
        }

        private void StepUpHierarchy_Click(object sender, RoutedEventArgs e)
        {
            FilePickerViewModel vm = DataContext as FilePickerViewModel;
            vm.SelectedFolder = vm.SelectedFolder.Parent;
        }

        private void FileView_Symbols_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileView_DoubleClick();
        }

        private void FileView_DoubleClick()
        {
            FilePickerViewModel vm = DataContext as FilePickerViewModel;

            if (vm.SelectedIndex > -1)
            {
                if (vm.SelectedEntry is FolderEntry folder)
                {
                    folder.LoadFoldersOnDemand();
                    if (folder != null)
                    {
                        bool folderAccessSuccess = folder.LoadAllOnDemand();
                        if (folderAccessSuccess)
                        {
                            (DataContext as FilePickerViewModel).SelectedFolder = folder;
                        }
                        else
                        {
                            DialogHost.Show(dialog_accessDenied);
                        }
                    }
                }
                else if (vm.SelectedEntry is FileEntry file)
                {
                    OnDialogClosed(new FilePickerClosedEventArgs(file));
                    //RaiseDialogClosedEvent(file.Path);

                    if (!SuppressCloseCommand && DialogHost.CloseDialogCommand.CanExecute(null, null))
                    {
                        DialogHost.CloseDialogCommand.Execute(null, null);
                    }

                    if (isWindowDialog)
                    {
                        dialogWindow.Close();
                    }
                }
            }
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            FilePickerViewModel vm = DataContext as FilePickerViewModel;
            if (vm.SelectedEntry != null)
            {
                OnDialogClosed(new FilePickerClosedEventArgs(vm.SelectedEntry));
                //RaiseDialogClosedEvent(vm.SelectedEntry.Path);
            }
            else if (vm.SelectedFolder != null)
            {
                OnDialogClosed(new FilePickerClosedEventArgs(vm.SelectedFolder));
                //RaiseDialogClosedEvent(vm.SelectedFolder.Path);
            }

            if (isWindowDialog)
            {
                dialogWindow.Close();
            }
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            OnDialogClosed(new FilePickerClosedEventArgs());
            //RaiseDialogClosedEvent(null);

            if (isWindowDialog)
            {
                dialogWindow.Close();
            }
        }

        private void filePicker_Loaded(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Collapsed)
            {
                Visibility = Visibility.Visible;
            }
            RaiseDialogOpenedEvent();
        }

        private void CurrentDirectory_TextChanged(object sender, TextChangedEventArgs e)
        {
            string path = (sender as TextBox).Text;
            if (Directory.Exists(path))
            {
                InitialDirectory = path;
                (DataContext as FilePickerViewModel).SetCurrentDirectory(path);
            }
        }

        private void RootFolderItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.BringIntoView();

                if (item.DataContext is FolderEntry entry)
                {
                    entry.LoadFoldersOnDemand();
                    entry.LoadFilesOnDemand();
                    FilePickerViewModel vm = DataContext as FilePickerViewModel;
                    originalFolder = vm.SelectedFolder;
                    originalEntry = vm.SelectedEntry;

                    vm.SelectedFolder = entry;
                    vm.SelectedEntry = entry;

                    currentFolderSetByMouseDown = true;
                }
            }
        }
    }
}
