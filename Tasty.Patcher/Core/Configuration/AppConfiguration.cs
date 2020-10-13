using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.Logging;

namespace Tasty.Patcher.Core.Configuration
{
    public class AppConfiguration : ConfigurationFile
    {
        public event EventHandler<ProfileEventArgs> SettingsSaved;
        public event EventHandler<EventArgs> SettingsLoaded;
        public event EventHandler<ProfileRenamedEventArgs> SettingsRenamed;

        private string mProfileName;
        private string mRedirectPath;
        private string mDefaultDirectory;

        private bool mRawModuleEnabled = true;
        private bool mDiscordModuleEnabled;
        private bool mHtmlModuleEnabled;

        private bool mIsDummy;

        public bool IsDummy => mIsDummy;

        public bool RawModuleEnabled
        {
            get => mRawModuleEnabled;
            set
            {
                mRawModuleEnabled = value;
                InvokePropertyChanged();
            }
        }

        public bool DiscordModuleEnabled
        {
            get => mDiscordModuleEnabled;
            set
            {
                mDiscordModuleEnabled = value;
                InvokePropertyChanged();
            }
        }

        public bool HtmlModuleEnabled
        {
            get => mHtmlModuleEnabled;
            set
            {
                mHtmlModuleEnabled = value;
                InvokePropertyChanged();
            }
        }

        public string ProfileName
        {
            get => mProfileName;
            set
            {
                string oldName = mProfileName;
                mProfileName = value;
                RenameFile(value);
                mRedirectPath = FilePath;
                Save();
                OnSettingsRenamed(new ProfileRenamedEventArgs(this, oldName, value));
            }
        }

        public string DefaultDirectory
        {
            get => mDefaultDirectory;
            set
            {
                mDefaultDirectory = value;
                InvokePropertyChanged();
            }
        }

        public AppConfiguration() : base("_")
        {
            mIsDummy = true;
        }

        public AppConfiguration(string profileName) : base(profileName, ".cfg", "Profiles/")
        {
            this.mProfileName = profileName;
            mRedirectPath = FilePath;
            Initialize();
        }

        /// <summary>
        /// Set default values for every property and load settings if exist. Otherwise a file with default settings is generated
        /// </summary>
        private void Initialize()
        {
            SetDefaults();

            if (!Exists())
            {
                Save();
            }
            else
            {
                Load();
            }
        }

        public void Save()
        {
            if (mIsDummy)
            {
                return;
            }

            string[] content = new string[]
            {
                "RawModuleEnabled:" + mRawModuleEnabled,
                "HtmlModuleEnabled:" + mHtmlModuleEnabled,
                "DiscordModuleEnabled:" + mDiscordModuleEnabled
            };

            SaveTo(mRedirectPath, content);
            OnSettingsSaved(new ProfileEventArgs(this));
        }

        public void Load()
        {
            if (mIsDummy)
            {
                return;
            }
            Load(LoadFile(this));
        }

        public void Reset()
        {
            SetDefaults();
            Save();
            Load();
        }

        private void Load(string[] content)
        {
            SetDefaults();
            if (content != null)
            {
                bool corrupted = false;
                foreach (string propertyRaw in content)
                {
                    try
                    {
                        string[] property = GetPropertyPair(propertyRaw);
                        if (property == null)
                        {
                            continue;
                        }

                        switch (property[0])
                        {
                            case "RawModuleEnabled":
                                RawModuleEnabled = Parser.TryParse(property[1], true);
                                break;
                            case "DiscordModuleEnabled":
                                DiscordModuleEnabled = Parser.TryParse(property[1], false);
                                break;
                            case "HtmlModuleEnabled":
                                HtmlModuleEnabled = Parser.TryParse(property[1], false);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!corrupted)
                        {
                            Logger.Instance.WriteLog("{0}.cfg seems to be corrupt!", ex, mProfileName);
                            corrupted = true;
                        }
                    }
                }
                Logger.Instance.WriteLog("{0}.cfg loaded!", mProfileName);
                if (corrupted)
                {
                    Save();
                }
            }
            else
            {
                Logger.Instance.WriteLog("No {0}.cfg found!", mProfileName);
                Save();
            }

            OnSettingsLoaded(EventArgs.Empty);
        }

        private void SetDefaults()
        {
            mRawModuleEnabled = true;
            mHtmlModuleEnabled = false;
            mDiscordModuleEnabled = false;
        }

        protected virtual void OnSettingsSaved(ProfileEventArgs e)
        {
            SettingsSaved?.Invoke(this, e);
        }

        protected virtual void OnSettingsLoaded(EventArgs e)
        {
            SettingsLoaded?.Invoke(this, e);
        }

        protected virtual void OnSettingsRenamed(ProfileRenamedEventArgs e)
        {
            SettingsRenamed?.Invoke(this, e);
        }
    }
}
