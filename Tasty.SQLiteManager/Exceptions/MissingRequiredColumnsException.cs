﻿using System;
using System.Collections.Generic;
using Tasty.SQLiteManager.Table.Column;

namespace Tasty.SQLiteManager.Exceptions
{
    class MissingRequiredColumnsException : Exception
    {
        private List<IColumn> missingColumns = new List<IColumn>();

        public List<IColumn> MissingColumns { get => missingColumns; }

        public MissingRequiredColumnsException() : base()
        {
        }

        public MissingRequiredColumnsException(string message) : base(message)
        {

        }

        public MissingRequiredColumnsException(string message, List<IColumn> missingColumns) : this(message)
        {
            this.missingColumns = missingColumns;
        }

        public MissingRequiredColumnsException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
