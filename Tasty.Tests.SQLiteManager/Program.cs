using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;
using Tasty.Logging;
using Tasty.SQLiteManager;
using Tasty.SQLiteManager.Table;
using Tasty.SQLiteManager.Table.Column;
using Tasty.SQLiteManager.Table.Conditions;
using Tasty.Tests.Base;
using Tasty.Tests.SQLiteManager.Test;

namespace Tasty.Tests.SQLiteManager
{
    class Program
    {
        private static string dbPath = AppDomain.CurrentDomain.BaseDirectory + "\\test.db";
        private static List<ITable> tables = new List<ITable>()
        {
            new TableDefinition<dynamic>("foobar", new List<IColumn>()
            {
                new ColumnDefinition<int>("ID", ColumnMode.PRIMARY_KEY),
                new ColumnDefinition<string>("name", ColumnMode.NOT_NULL),
                new ColumnDefinition<string>("email")
            })
        };

        static void Main(string[] args)
        {
            Database.Initialize(dbPath, tables, Logger.Initialize(false));
            Logger.Default.DisableLogging = true;

            System.Console.Clear();
            TestRunner.RunTest(Test_CheckDatabase, "Checking if database has been setup properly"); //, "Database contains all tables and columns"
            TestRunner.RunTest(Test_Insert_GetIndex, "Testing Insert_GetIndex method"); //, "Insert_GetIndex completed"
            TestRunner.RunTest(Test_Select, "Testing Select method");
            TestRunner.RunTest(Test_DefineByClass, "Testing table generation with class method");

            //Console.Write("Correct data returned:\t\t", Test_Select());
            if (TestRunner.FailedTests == 0)
            {
                Base.Console.WriteLine_Status(string.Format("Successfully ran {0}/{0} tests!", TestRunner.TestCount), Status.Success);
            }
            else if (TestRunner.FailedTests < TestRunner.TestCount)
            {
                Base.Console.WriteLine_Status(string.Format("Ran {0}/{1} tests, but some failed!", TestRunner.TestCount - TestRunner.FailedTests, TestRunner.TestCount), Status.Warning);
            }
            else
            {
                Base.Console.WriteLine_Status("All tests failed!", Status.Fail);
            }
            System.Console.WriteLine("Press any key to exit.");
            System.Console.ReadLine();
        }

        static bool Test_CheckDatabase()
        {
            foreach (var table in tables)
            {
                bool tableExists = table.TableExists();
                Base.Console.WriteLine_Status(string.Format("Table {0} exists", table.Name), tableExists);

                if (!tableExists)
                {
                    System.Console.WriteLine("Test aborted!");
                    return false;
                }

                foreach (var column in table)
                {
                    bool columnExists = table.ColumnExists(column);
                    Base.Console.WriteLine_Status(string.Format("Column {0} exists", column.Name), columnExists);

                    if (!columnExists)
                    {
                        System.Console.WriteLine("Test aborted!");
                        return false;
                    }
                }
            }

            return true;
        }

        static bool Test_Insert_GetIndex()
        {
            var table = Database.Instance["foobar"];
            int index = table.Insert_GetIndex(new Dictionary<IColumn, dynamic>()
            {
                { table["name"], "Jon Doe" }
            });

            bool insertSuccess = index == 1;
            Base.Console.WriteLine_Status(string.Format("Insert into table {0} success", table.Name), insertSuccess);

            if (!insertSuccess)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }

            return true;
        }

        static bool Test_Select()
        {
            int index = 1;
            var table = Database.Instance["foobar"];

            var result = table.Select(new Condition(table["ID"], index));
            bool rowExists = !result.IsEmpty;
            Base.Console.WriteLine_Status(string.Format("Entry with ID {0} exists", index), rowExists);

            if (!rowExists)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }

            string name = "Jon Doe";
            bool nameCorrect = result[0]["name"] == name;
            Base.Console.WriteLine_Status(string.Format("Entry name is {0}", name), nameCorrect);

            if (!nameCorrect)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }

            return true;
        }

        static bool Test_DefineByClass()
        {
            var table = new TableDefinition<DemoDbClass>("TestByClass");
            return true;
        }
    }
}
