using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LaborNeedsScheduling.Models
{
    public class FakeAPI
    {
        public static DataTable BuildBlankWeek(DateTime startDate)
        {
            //hardcode for now, but should pull start day (Sunday)
            // and open close hours

            //bonus: where sunday or saturday, black out cells where store is closed.
            // cell.value = "CLOSED"

            return new DataTable();
        }

        public static List<JakoEmployee> GetEmpsForStore(string storeCode)
        {
            List<JakoEmployee> emps = new List<JakoEmployee>();

            //create table and corresponding booleans for hour inclusion/exclusion
            DataTable TimeSelectionTable = new DataTable();

            string[] tableRows = {"7AM-8AM","8AM-9AM","9AM-10AM","10AM-11AM","11AM-12PM","12PM-1PM", "1PM-2PM", "2PM-3PM", "3PM-4PM",
                                  "4PM-5PM", "5PM-6PM", "6PM-7PM", "7PM-8PM", "8PM-9PM", "9PM-10PM", "10PM-11PM", "11PM-12AM"};


            bool[,] TimeCheck = new bool[tableRows.Length, 8];

            DataSet empTables = new DataSet();
            empTables.Tables.Add(new DataTable());
            empTables.Tables.Add(new DataTable());
            empTables.Tables.Add(new DataTable());


            for (int l = 0; l < 3; l++)
            {
                Random rand = new Random();

                empTables.Tables[l].Columns.Add("Hour");
                empTables.Tables[l].Columns.Add("Sunday");
                empTables.Tables[l].Columns.Add("Monday");
                empTables.Tables[l].Columns.Add("Tuesday");
                empTables.Tables[l].Columns.Add("Wednesday");
                empTables.Tables[l].Columns.Add("Thursday");
                empTables.Tables[l].Columns.Add("Friday");
                empTables.Tables[l].Columns.Add("Saturday");

                for (int i = 0; i < tableRows.Length; i++)
                {
                    empTables.Tables[l].Rows.Add(tableRows[i]);
                }

                for (int i = 1; i < 8; i++)
                {
                    for (int n = 0; n < tableRows.Length; n++)
                    {
                        if (rand.Next(0, 2) == 0)
                            TimeCheck[n, i] = false;
                        else
                            TimeCheck[n, i] = true;
                        empTables.Tables[l].Rows[n][i] = TimeCheck[n, i];
                    }
                }
            }

            emps.Add(new JakoEmployee()
            {
                id = "001",
                name = "tom",
                storeno = storeCode,
                AvailabilityTable = empTables.Tables[0],
                TimeCheck = TimeCheck
            });

            emps.Add(new JakoEmployee()
            {
                id = "002",
                name = "john",
                storeno = storeCode,
                AvailabilityTable = empTables.Tables[1],
                TimeCheck = TimeCheck
            });

            emps.Add(new JakoEmployee()
            {
                id = "003",
                name = "bill",
                storeno = storeCode,
                AvailabilityTable = empTables.Tables[2],
                TimeCheck = TimeCheck
            });

            return emps;
        }

        public static List<EmployeeNotification> CreateResponses(string EmployeeID)
        {
            List<EmployeeNotification> notifications = new List<EmployeeNotification>();

            notifications.Add(new EmployeeNotification()
            {
                id = EmployeeID,
                date = DateTime.Today,
                message = "Time off request approved",
                accepted = true
            });

            notifications.Add(new EmployeeNotification()
            {
                id = EmployeeID,
                date = DateTime.Today,
                message = "Time off request denied",
                accepted = false
            });

            notifications.Add(new EmployeeNotification()
            {
                id = EmployeeID,
                date = DateTime.Today.AddDays(-1),
                message = "Time off request approved",
                accepted = true
            });

            return notifications;
        }

        public static DataTable GetEmployeeSchedule(string EmployeeID)
        {
            DataTable EmployeeSchedule = new DataTable();

            string strSQLCon = @"Data Source=AFORTINE\SQLEXPRESS;Initial Catalog=LaborNeedsScheduling;Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {

                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "select * from dbo.emp_" + EmployeeID;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(EmployeeSchedule);


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }

                return EmployeeSchedule;
            }
        }

        public static List<Employees> CreateEmployees()
        {

            string strSQLCon = @"Data Source=AFORTINE\SQLEXPRESS;Initial Catalog=LaborNeedsScheduling;Integrated Security=True;";

            List<Employees> employees = new List<Employees>();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable EmployeeListSQL = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    //cmd.CommandText = "SELECT [EmployeeID], [FirstName], [Role], [Rank], [Hours] FROM Employees";
                    cmd.CommandText = "SELECT COUNT(*) FROM Employees";
                    Int32 employeeRowCount = (Int32)cmd.ExecuteScalar();

                    cmd.CommandText = "SELECT [EmployeeID], [FirstName], [Role], [Rank], [Hours] FROM Employees";
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(EmployeeListSQL);

                    for (int i = 0; i < EmployeeListSQL.Rows.Count; i++)
                    {
                        employees.Add(new Employees()
                        {
                            id = Convert.ToString(EmployeeListSQL.Rows[i][0]),
                            name = Convert.ToString(EmployeeListSQL.Rows[i][1]),
                            role = Convert.ToString(EmployeeListSQL.Rows[i][2]),
                            rank = Convert.ToString(EmployeeListSQL.Rows[i][3]),
                            hours = Convert.ToString(EmployeeListSQL.Rows[i][4]),
                        });

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }

                return employees;
            }
        }
    }
}