﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class SqliteDataSetter : Attribute
    {
        private bool setChildData;

        internal bool SetChildData => setChildData;

        internal SqliteDataSetter()
        {

        }

        internal SqliteDataSetter(bool setChildData)
        {
            this.setChildData = setChildData;
        }
    }
}
