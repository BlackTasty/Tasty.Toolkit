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
        private static readonly string dbPath = AppDomain.CurrentDomain.BaseDirectory + "\\test.db";

        static void Main()
        {
            Database.Initialize(dbPath, Logger.Initialize(false));
            Database.Instance.DropDatabase();
            Logger.Default.DisableLogging = true;

            System.Console.Clear();
            RunAllTests();

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

        static void RunAllTests()
        {
            if (!TestRunner.RunTest(Test_CheckDatabase, "Checking if database has been setup properly")) // "Database contains all tables and columns"
            {
                return;
            }
            if (!TestRunner.RunTest(Test_Insert_GetIndex, "Testing Insert_GetIndex method")) // "Insert_GetIndex completed"
            {
                return;
            }
            if (!TestRunner.RunTest(Test_Select, "Testing Select method"))
            {
                return;
            }
            if (!TestRunner.RunTest(Test_OneToManyRelationship, "Testing one-to-many (1-n) relationship"))
            {
                return;
            }
            if (!TestRunner.RunTest(Test_OneToOneRelationship, "Testing one-to-one (1-1) relationship"))
            {
                return;
            }
            if (!TestRunner.RunTest(Test_ManyToManyRelationship, "Testing many-to-many (n-n) relationship"))
            {
                return;
            }
        }


        #region Tests
        static bool Test_CheckDatabase()
        {
            if (!CheckTables(Database.Instance))
            {
                return false;
            }

            if (!CheckTables(Database.Instance.ChildTables))
            {
                return false;
            }

            return true;
        }

        private static bool CheckTables(IEnumerable<ITableBase> tables)
        {
            foreach (var table in tables)
            {
                bool tableExists = table.TableExists();
                if (table is ITable rootTable)
                {
                    Base.Console.WriteLine_Status(string.Format("Table {0} ({1}) exists", table.Name, rootTable.TableType.Name), tableExists);
                }
                else
                {
                    Base.Console.WriteLine_Status(string.Format("Child table {0} exists", table.Name), tableExists);
                }

                if (!tableExists)
                {
                    System.Console.WriteLine(string.Format("Test aborted! Table \"{0}\" doesn't exist.", table.Name));
                    return false;
                }

                foreach (var column in table)
                {
                    bool columnExists = table.ColumnExists(column);
                    Base.Console.WriteLine_Status(string.Format("  {0} {1} ({2}) exists", 
                        !column.IsForeignKey ? "Column" : "Foreign column", column.Name, column.DataType.Name), columnExists);

                    if (!columnExists)
                    {
                        System.Console.WriteLine(string.Format("Test aborted! Column \"{0}\" doesn't exist.", column.Name));
                        return false;
                    }
                }
            }

            return true;
        }

        static bool Test_Insert_GetIndex()
        {
            var table = Database.Instance.GetTable<DemoUser>();

            DemoUser demoUser = new DemoUser()
            {
                Name = "Jon Doe",
                Age = 20,
                Password = "Abc123"
            };

            int result = demoUser.SaveToDatabase();

            bool insertSuccess = result == 0;
            Base.Console.WriteLine_Status(string.Format("Insert into table {0} success", table.Name), insertSuccess);

            if (!insertSuccess)
            {
                System.Console.WriteLine("Test aborted! Error code " + result);
                return false;
            }

            return true;
        }

        static bool Test_Select()
        {
            int index = 1;
            var table = Database.Instance.GetTable<DemoUser>();

            var result = DemoUser.LoadFromDatabase(new Condition("ID", index));
            bool rowExists = result != null;
            Base.Console.WriteLine_Status(string.Format("User with ID {0} exists", index), rowExists);

            if (!rowExists)
            {
                System.Console.WriteLine("Test aborted! User doesn't exist.");
                return false;
            }

            string name = "Jon Doe";
            bool nameCorrect = result.Name == name;
            Base.Console.WriteLine_Status(string.Format("User name is {0}", name), nameCorrect);

            if (!nameCorrect)
            {
                System.Console.WriteLine("Test aborted! Name incorrect.");
                return false;
            }

            return true;
        }

        static bool Test_OneToManyRelationship()
        {
            var postTable = Database.Instance.GetTable<DemoPost>();

            DemoUser demoUser = DemoUser.LoadFromDatabase(new Condition("ID", 1));
            bool rowExists = demoUser != null;

            Base.Console.WriteLine_Status("Entry with ID 1 exists", rowExists);
            if (!rowExists)
            {
                System.Console.WriteLine("Test aborted! User doesn't exist.");
                return false;
            }

            DemoPost demoPost = new DemoPost(postTable)
            {
                Author = demoUser,
                CreateDate = DateTime.Now,
                Title = "Foobar"
            };
            demoUser.Posts.Add(demoPost);
            demoUser.SaveToDatabase();

            DemoUser dbUser = DemoUser.LoadFromDatabase(new Condition("ID", 1));

            bool usersIdentical = AreUsersIdentical(demoUser, dbUser);
            Base.Console.WriteLine_Status("Is user same after saving to database", usersIdentical);
            if (!usersIdentical)
            {
                System.Console.WriteLine("Test aborted! Loaded user data not identical.");
                return false;
            }

            return true;
        }

        static bool Test_OneToOneRelationship()
        {
            var settingsTable = Database.Instance.GetTable<DemoUserSettings>();

            DemoUserSettings userSettings = new DemoUserSettings(settingsTable)
            {
                Language = Test.Enum.DemoLanguageType.Swedish
            };

            DemoUser demoUser = DemoUser.LoadFromDatabase(new Condition("ID", 1));
            bool rowExists = demoUser != null;

            Base.Console.WriteLine_Status("Entry with ID 1 exists", rowExists);
            if (!rowExists)
            {
                System.Console.WriteLine("Test aborted! User doesn't exist.");
                return false;
            }

            demoUser.UserSettings = userSettings;

            demoUser.SaveToDatabase();

            DemoUser dbUser = DemoUser.LoadFromDatabase(new Condition("ID", 1));

            bool usersIdentical = AreUsersIdentical(demoUser, dbUser);
            Base.Console.WriteLine_Status("Is user same after saving to database", usersIdentical);
            if (!usersIdentical)
            {
                System.Console.WriteLine("Test aborted! Loaded user data not identical.");
                return false;
            }

            return true;
        }

        static bool Test_ManyToManyRelationship()
        {
            DemoUser demoUser = DemoUser.LoadFromDatabase(new Condition("ID", 1));
            bool rowExists = demoUser != null;

            Base.Console.WriteLine_Status("Entry with ID 1 exists", rowExists);
            if (!rowExists)
            {
                System.Console.WriteLine("Test aborted! User doesn't exist.");
                return false;
            }

            DemoUser demoFriend = new DemoUser()
            {
                Name = "Jane Doe",
                Age = 27,
                Password = "XYZ987"
            };

            demoFriend.SaveToDatabase();
            demoUser.Friends.Add(demoFriend);

            demoUser.SaveToDatabase();

            DemoUser dbUser = DemoUser.LoadFromDatabase(new Condition("ID", 1));

            bool usersIdentical = AreUsersIdentical(demoUser, dbUser);
            Base.Console.WriteLine_Status("Is user same after saving to database", usersIdentical);
            if (!usersIdentical)
            {
                System.Console.WriteLine("Test aborted! Loaded user data not identical.");
                return false;
            }

            return true;
        }
        #endregion

        private static bool AreUsersIdentical(DemoUser originalUser, DemoUser dbUser, bool checkLists = true)
        {
            if (originalUser.Age != dbUser.Age || originalUser.Guid != dbUser.Guid ||
                originalUser.Name != dbUser.Name || originalUser.Password != dbUser.Password ||
                originalUser.Posts.Count != dbUser.Posts.Count)
            {
                return false;
            }

            if (checkLists)
            {
                foreach (DemoPost dbPost in dbUser.Posts)
                {
                    DemoPost originalPost = originalUser.Posts.FirstOrDefault(x => x.ID == dbPost.ID);
                    if (originalPost == null)
                    {
                        return false;
                    }

                    double timeDeviation = Math.Round(originalPost.CreateDate.Subtract(dbPost.CreateDate).TotalSeconds);

                    if (originalPost.Title != dbPost.Title || timeDeviation < -1 || timeDeviation > 1)
                    {
                        return false;
                    }
                }

                foreach (DemoUser dbFriend in dbUser.Friends)
                {
                    DemoUser originalFriend = originalUser.Friends.FirstOrDefault(x => x.ID == dbFriend.ID);
                    if (originalUser == null)
                    {
                        return false;
                    }

                    return AreUsersIdentical(originalFriend, dbFriend, false);
                }
            }

            return true;
        }
    }
}