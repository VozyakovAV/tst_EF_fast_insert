using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.SetInitializer(new DbInitializer());
            DeleteItems();
            WriteToConsoleItemsCount();
            string connection;
            using (var ctx = new MyContext())
            {
                connection = ctx.Database.Connection.ConnectionString;
            }
            
            var items = new List<Item>();
            for (int i = 0; i < 1000; i++)
            {
                var item = new Item() { Number = i, Name = "name " + i.ToString() };
                for (int j = 0; j < 0; j++)
                {
                    var subItem = new SubItem() { Number = j, Name = "name " + i.ToString() + " " + j.ToString() };
                    item.SubItems.Add(subItem);
                }
                items.Add(item);
            }

            var sw = Stopwatch.StartNew();
            EFInsert(items);
            //EFInsertParallel(items);
            //SqlInsert(connection, items);
            //SqlInsertScope(ctx.Database.Connection.ConnectionString, list);
            //SqlBuilkInsert(connection, items);
            //SqlBuilkInsert2(connection, items);
            sw.Stop();
            Console.WriteLine(sw.Elapsed.TotalSeconds);
            WriteToConsoleItemsCount();
            Console.ReadKey();
        }

        static void EFInsert(List<Item> items)
        {
            while (items.Count > 0)
            {
                var step = 200;
                var items2 = items.Take(step).ToList();
                items = items.Skip(step).ToList();
                using (var ctx = new MyContext())
                {
                    ctx.Configuration.AutoDetectChangesEnabled = false;
                    ctx.Items.AddRange(items2); 
                    ctx.SaveChanges();
                }
            }
        }

        static void EFInsertParallel(List<Item> items)
        {
            var list = items.Split(300).ToList();

            Parallel.ForEach(list, listItem =>
            {
                using (var ctx = new MyContext())
                {
                    ctx.Configuration.AutoDetectChangesEnabled = true;
                    ctx.Items.AddRange(listItem);
                    ctx.SaveChanges();
                }
            });
        }

        static void SqlInsert(string sqlConnection, List<Item> items)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnection))
            {
                connection.Open();
                using (var tr = connection.BeginTransaction())
                {
                    foreach (var item in items)
                    {
                        String query = "INSERT INTO dbo.Items (Number,Name) VALUES(@number,@name)";

                        using (SqlCommand command = new SqlCommand(query, connection, tr))
                        {
                            command.Parameters.AddWithValue("@number", item.Number);
                            command.Parameters.AddWithValue("@name", item.Name);
                            command.ExecuteNonQuery();
                        }
                    }
                    tr.Commit();
                }
            }
        }

        static void SqlInsertScope(string sqlConnection, List<Item> items)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnection))
            {
                connection.Open();
                using (var tr = connection.BeginTransaction())
                {
                    while (items.Count > 0)
                    {
                        var step = 100;
                        var items2 = items.Take(step).ToList();
                        items = items.Skip(step).ToList();

                        string query = "";
                        SqlCommand command = new SqlCommand();
                        for (int i = 0; i < items2.Count; i++)
                        {
                            var item = items2[i];
                            var st = i.ToString();
                            query += string.Format("INSERT INTO dbo.Items (Number,Name) VALUES(@number{0},@name{0}); ", st);

                            command.Parameters.AddWithValue("@number" + st, item.Number);
                            command.Parameters.AddWithValue("@name" + st, item.Name);
                        }
                        command.CommandText = query;
                        command.Connection = connection;
                        command.Transaction = tr;
                        var t = command.ExecuteNonQuery();
                    }
                    tr.Commit();
                }
            }
        }

        static void SqlBuilkInsert(string sqlConnection, List<Item> items)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnection))
            {
                connection.Open();
                using (var sc = new SqlBulkCopy(connection))
                {
                    var table = new DataTable();
                    table.Columns.Add("Number");
                    table.Columns.Add("Name");

                    foreach (var item in items)
                    {
                        DataRow row = table.NewRow();
                        row["Number"] = item.Number;
                        row["Name"] = item.Name;
                        table.Rows.Add(row);
                    }
                    sc.DestinationTableName = "dbo.Items";
                    sc.WriteToServer(table);
                }
            }
        }

        static void SqlBuilkInsert2(string sqlConnection, List<Item> items)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnection))
            {
                connection.Open();
                using (var tr = connection.BeginTransaction())
                {
                    var columnGUID = "UniqueGUID";
                    //var cmd2 = new SqlCommand("ALTER TABLE dbo.Items DROP COLUMN " + columnGUID, connection, tr);
                    //cmd2.ExecuteNonQuery();
                    //var cmd = new SqlCommand("ALTER TABLE dbo.Items ADD " + columnGUID + " UniqueIdentifier", connection, tr);
                    //cmd.ExecuteNonQuery();

                    var dict = new Dictionary<Guid, Item>();
                    using (var sc = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, tr))
                    {
                        sc.BatchSize = 5000;
                        var table = new DataTable();
                        table.Columns.Add("Id");
                        table.Columns.Add("Number");
                        table.Columns.Add("Name");
                        table.Columns.Add(columnGUID, Type.GetType("System.Guid"));

                        foreach (var item in items)
                        {
                            var guid = Guid.NewGuid();
                            dict.Add(guid, item);
                            table.Rows.Add(0, item.Number, item.Name, guid);
                            /*DataRow row = table.NewRow();
                            row["Id"] = 0;
                            row["Number"] = item.Number;
                            row["Name"] = item.Name + "name";
                            row[columnGUID] = Guid.NewGuid();
                            table.Rows.Add(row);*/
                        }
                        sc.DestinationTableName = "dbo.Items";
                        sc.WriteToServer(table);
                    }

                    //var cmd = new SqlCommand("ALTER TABLE dbo.Items DROP COLUMN " + columnGUID, connection, tr);
                    //cmd.ExecuteNonQuery();


                    var dict2 = new Dictionary<Item, int>();
                    var cmd = new SqlCommand("SELECT Id, " + columnGUID + " FROM dbo.Items", connection, tr);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var valId = reader.GetInt32(reader.GetOrdinal("Id"));
                            var valGuid = reader.GetGuid(reader.GetOrdinal(columnGUID));

                            Item valItem;
                            if (dict.TryGetValue(valGuid, out valItem))
                            {
                                dict2.Add(valItem, valId);
                            }
                        }
                    }

                    InsertSubItems(connection, tr, items, dict2);

                    tr.Commit();
                }
            }
        }

        static void InsertSubItems(SqlConnection connection, SqlTransaction tr, List<Item> items, Dictionary<Item, int> dict)
        {
            using (var sc = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, tr))
            {
                sc.BatchSize = 5000;
                var table = new DataTable();
                table.Columns.Add("Id");
                table.Columns.Add("Number");
                table.Columns.Add("Name");
                table.Columns.Add("ItemId");

                foreach (var item in items)
                {
                    foreach (var subItem in item.SubItems)
                    {
                        int id;
                        if (dict.TryGetValue(item, out id))
                        {
                            table.Rows.Add(0, subItem.Number, subItem.Name, id);
                        }
                    }
                }
                sc.DestinationTableName = "dbo.SubItems";
                sc.WriteToServer(table);
            }
        }

        static void DeleteItems()
        {
            using (var ctx = new MyContext())
            {
                ctx.Database.ExecuteSqlCommand("DELETE FROM dbo.Items");
            }
        }

        static void WriteToConsoleItemsCount()
        {
            using (var ctx = new MyContext())
            {
                Console.WriteLine(ctx.Items.Count() + " " + ctx.SubItems.Count());
            }
        }
    }

}
