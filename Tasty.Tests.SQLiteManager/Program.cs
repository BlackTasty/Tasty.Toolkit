using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager;
using Tasty.SQLiteManager.Table;
using Tasty.SQLiteManager.Table.Column;
using Tasty.SQLiteManager.Table.Conditions;

namespace Tasty.Tests.SQLiteManager
{
    class Program
    {
        private static string dbPath = AppDomain.CurrentDomain.BaseDirectory + "\\test.db";
        private static List<TableDescriptor> tables = new List<TableDescriptor>()
        {
            new TableDescriptor("foobar", new List<IColumn>()
            {
                new ColumnDescriptor<int>("ID", ColumnMode.PRIMARY_KEY),
                new ColumnDescriptor<string>("name", ColumnMode.NOT_NULL),
                new ColumnDescriptor<string>("email")
            })
        };

        static void Main(string[] args)
        {
            Database.Initialize(dbPath, tables);

            int testCount = 1;

            Console.WriteLine("Test {0}: Checking if database has been setup properly...", testCount);
            testCount++;

            bool databaseCorrect = Test_CheckDatabase();
            Console.Write("Database contains all tables and columns:\t\t");
            WriteLine_Status(databaseCorrect);

            Console.WriteLine("Test {0}: Testing Select method...");
            testCount++;
            
            //Console.Write("Correct data returned:\t\t", Test_Select());

            Console.ReadLine();
        }

        static bool Test_CheckDatabase()
        {
            foreach (var table in tables)
            {
                Console.Write("Table {0} exists:\t\t\t\t\t", table.Name);
                bool tableExists = table.TableExists();
                WriteLine_Status(tableExists);
                if (tableExists)
                {
                    foreach (var column in table)
                    {
                        Console.Write("\tColumn {0} exists:\t\t\t\t", column.Name);
                        bool columnExists = table.ColumnExists(column);
                        WriteLine_Status(columnExists);

                        if (!columnExists)
                        {
                            Console.WriteLine("Test aborted!");
                            return false;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Test aborted!");
                    return false;
                }
            }

            return true;
        }

        static bool Test_Select()
        {
            var result = Database.Instance["foobar"].Select(
                new Condition[] {
                    new Condition(Database.Instance["foobar"]["name"], "Jon")
                }
            );

            return true;
        }

        static void WriteLine(string message, ConsoleColor backgroundColor)
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(message);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void WriteLine_Status(bool success)
        {
            if (success)
            {
                Console.Write(" ");
                WriteLine(" OK ", ConsoleColor.DarkGreen);
            }
            else
            {
                WriteLine(" FAIL ", ConsoleColor.DarkRed);
            }
        }
    }
}
