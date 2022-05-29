using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table;
using Tasty.SQLiteManager.Table.Column;

namespace Tasty.SQLiteManager
{
    class Util
    {
        private static readonly PluralizationService pluralService = PluralizationService.CreateService(CultureInfo.GetCultureInfo("en-us"));

        internal static string GetColumnName(string propertyName)
        {
            StringBuilder sb = new StringBuilder(char.ToLower(propertyName[0]).ToString());

            for (int i = 1; i < propertyName.Length; i++)
            {
                if (char.IsUpper(propertyName[i]))
                {
                    sb.Append("_" + char.ToLower(propertyName[i]));
                }
                else
                {
                    sb.Append(propertyName[i]);
                }
            }

            return sb.ToString();
        }

        internal static string GetTableName(string className)
        {
            string tableNameSingular = GetColumnName(className);
            return pluralService.Pluralize(tableNameSingular);
        }

        internal static string GetSingular(string pluralWord)
        {
            return pluralService.Singularize(pluralWord);
        }

        internal static Type MakeGenericTableDefinition(Type tableType)
        {
            return typeof(TableDefinition<>).MakeGenericType(new Type[] { tableType });
        }

        internal static Type MakeGenericColumnDefinition(Type columnType)
        {
            return typeof(ColumnDefinition<>).MakeGenericType(new Type[] { columnType });
        }

        internal static Type MakeGenericDatabaseEntry(Type tableType)
        {
            return typeof(DatabaseEntry<>).MakeGenericType(new Type[] { tableType, typeof(RowData) });
        }
    }
}
