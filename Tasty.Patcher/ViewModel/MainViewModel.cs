using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.Patcher.Core;
using Tasty.Patcher.Core.Configuration;
using Tasty.Patcher.UI;

namespace Tasty.Patcher.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        private int mSelectedProfileIndex = -1;
        private int mSelectedSidebarIndex = 0;

        public MainViewModel()
        {
            Items = new List<SidebarEntry>()
            {
                new SidebarEntry("Raw patchnotes", MaterialDesignThemes.Wpf.PackIconKind.FormatListBulleted, RawPatchnotes.Instance, 0)
            };
        }

        public AppConfiguration Configuration => App.ProfileManager.Profile;

        public VeryObservableCollection<AppConfiguration> Profiles
        {
            get => App.ProfileManager.Profiles;
        }

        public List<SidebarEntry> Items { get; private set; }

        public int SelectedSidebarIndex
        {
            get => mSelectedSidebarIndex;
            set
            {
                if (Items[value].HasChildren)
                {
                    Items[mSelectedSidebarIndex].ToggleSubEntries(false);
                    Items[value].ToggleSubEntries(true);
                }
                else if (Items[mSelectedSidebarIndex] is SidebarSubEntry subEntry)
                {
                    subEntry.ParentEntry.ToggleSubEntries(false);
                    Items[value].ToggleSubEntries(true);
                }

                mSelectedSidebarIndex = value;

                //SelectedEntry = Items[value];
                InvokePropertyChanged("Items");
                InvokePropertyChanged();
            }
        }

        public int SelectedProfileIndex
        {
            get => mSelectedProfileIndex;
            set
            {
                mSelectedProfileIndex = value;
                App.ProfileManager.ChangeActiveProfile(App.ProfileManager.Profiles[value]);
                InvokePropertyChanged();
            }
        }
    }
}
