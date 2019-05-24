using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table
{
    public class DescriptorBase
    {
        protected DescriptorBase original;

        protected string name;

        public string Name { get => name; }

        protected void SaveState()
        {
            original = this;
        }
    }
}
