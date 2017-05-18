using System;
using System.Collections.Generic;
using System.Data;
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
            bool[,] TimeCheck = new bool[14, 8];

            string[] tableRows = { "9AM-10AM","10AM-11AM","11AM-12PM","12PM-1PM", "1PM-2PM", "2PM-3PM", "3PM-4PM",
                                   "4PM-5PM", "5PM-6PM", "6PM-7PM", "7PM-8PM", "8PM-9PM", "9PM-10PM", "10PM-11PM"};

            DataSet empTables = new DataSet();
            empTables.Tables.Add(new DataTable());
            empTables.Tables.Add(new DataTable());
            empTables.Tables.Add(new DataTable());


            for (int l = 0; l < 3; l++)
            {
                Random rand = new Random();

                empTables.Tables[l].Columns.Add("HourOfDay");
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
                    for (int n = 0; n < 14; n++)
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

    }
}