using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.Patcher.Core.Configuration
{
    public class ProfileRenamedEventArgs
    {
        private AppConfiguration profile;
        private string oldName;
        private string newName;

        public AppConfiguration Profile { get => profile; }

        public string OldName { get => oldName; }

        public string NewName { get => newName; }

        public ProfileRenamedEventArgs(AppConfiguration profile, string oldName, string newName)
        {
            this.profile = profile;
            this.oldName = oldName;
            this.newName = newName;
        }
    }
}
