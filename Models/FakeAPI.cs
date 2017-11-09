using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace LaborNeedsScheduling.Models
{
    public class FakeAPI
    {
        public static string[] ScheduleHalfHourSlots = { "12:00AM","12:30AM","1:00AM","1:30AM","2:00AM","2:30AM","3:00AM","3:30AM","4:00AM","4:30AM","5:00AM","5:30AM",
                                                  "6:00AM", "6:30AM", "7:00AM", "7:30AM","8:00AM", "8:30AM","9:00AM", "9:30AM","10:00AM", "10:30AM",
                                                  "11:00AM", "11:30AM","12:00PM","12:30PM", "1:00PM", "1:30PM", "2:00PM", "2:30PM", "3:00PM", "3:30PM",
                                                  "4:00PM", "4:30PM", "5:00PM", "5:30PM", "6:00PM", "6:30PM", "7:00PM", "7:30PM", "8:00PM", "8:30PM",
                                                  "9:00PM", "9:30PM", "10:00PM", "10:30PM", "11:00PM", "11:30PM"};

        public static string[] SQLHours = {"00:00:00","00:30:00","01:00:00","01:30:00","02:00:00","02:30:00","03:00:00","03:30:00","04:00:00",
                                    "04:30:00","05:00:00","05:30:00","06:00:00", "06:30:00", "07:00:00", "07:30:00", "08:00:00", "08:30:00",
                                    "09:00:00", "09:30:00", "10:00:00", "10:30:00", "11:00:00", "11:30:00", "12:00:00", "12:30:00",
                                    "13:00:00", "13:30:00", "14:00:00", "14:30:00","15:00:00", "15:30:00", "16:00:00", "16:30:00", "17:00:00",
                                    "17:30:00", "18:00:00", "18:30:00", "19:00:00", "19:30:00", "20:00:00", "20:30:00", "21:00:00", "21:30:00",
                                    "22:00:00", "22:30:00", "23:00:00", "23:30:00"};

        public static string[] days = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        //public static string strSQLCon = @"Data Source=AFORTINE\SQLEXPRESS;Initial Catalog=LaborNeedsScheduling;Integrated Security=True; MultipleActiveResultSets=True;";
        public static string strSQLCon = @"Data Source = 192.168.1.250; Integrated Security = False; User ID = LaborScheduler; Password=H@!ey121 ;Connect Timeout = 180; Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public static void dothething(DateTime[] weekdates)
        {
            //List<Employees> employees = CreateEmployees();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    List<Employees> employees = GetAllEmployees();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();
                    //foreach (Employees emp in employees)
                    //{
                    //    for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
                    //    {
                    //        cmd.CommandText = "insert into dbo.EmployeeAvailability(EmployeeId, Hour, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday)"
                    //                        + "values('" + emp.id + "', '" + ScheduleHalfHourSlots[i] + "', '1', '1', '1', '1', '1', '1', '1')";
                    //        cmd.ExecuteNonQuery();
                    //    }
                    //}



                    foreach (Employees emp in employees)
                    {
                        for (int i = 0; i < weekdates.Length; i++)
                        {

                            cmd.CommandText = "insert into dbo.EmployeeSchedule(EmployeeId, LocationCode, ScheduleDate, BeginTime, EndTime, OnSchedule)"
                                            + " values('" + emp.id + "', '" + emp.storeCode + "', '" + weekdates[i] + "', '', '', '0')";
                            cmd.ExecuteNonQuery();
                        }
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
            }
        }

        public static string GetStoreCode(string User)
        {
            string StoreCode = "";

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();
                    DataTable dt = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    cmd.CommandText = "select StoreCode from dbo.UserAttributes where UserId='" + User + "'";
                    da.Fill(dt);

                    StoreCode = dt.Rows[0][0].ToString();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }

            return StoreCode;
        }

        public static int GetMinEmployeesEarly(string LocationCode)
        {
            int MinEmployees = 0;

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable min = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "select MinEmployeesEarly from dbo.StoreVariables where LocationCode = '" + LocationCode + "'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(min);

                    MinEmployees = Convert.ToInt32(min.Rows[0][0]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
            return MinEmployees;
        }

        public static int GetMinEmployeesLater(string LocationCode)
        {
            int MinEmployees = 0;

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable min = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "select MinEmployeesLater from dbo.StoreVariables where LocationCode = '" + LocationCode + "'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(min);

                    MinEmployees = Convert.ToInt32(min.Rows[0][0]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
            return MinEmployees;
        }

        public static void UpdateMinEmployeesEarly(int MinEmployees, string LocationCode)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "update dbo.StoreVariables set MinEmployeesEarly = '" + MinEmployees + "' where LocationCode = '" + LocationCode + "'";

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static void UpdateMinEmployeesLater(int MinEmployees, string LocationCode)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "update dbo.StoreVariables set MinEmployeesLater = '" + MinEmployees + "' where LocationCode = '" + LocationCode + "'";

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static int GetMaxEmployees(string LocationCode)
        {
            int MaxEmployees = 0;

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable max = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "select MaxEmployees from dbo.StoreVariables where LocationCode = '" + LocationCode + "'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(max);

                    MaxEmployees = Convert.ToInt32(max.Rows[0][0]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
            return MaxEmployees;
        }

        public static void UpdateMaxEmployees(int MaxEmployees, string LocationCode)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "update dbo.StoreVariables set MaxEmployees = '" + MaxEmployees + "' where LocationCode = '" + LocationCode + "'";

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static int GetWeekdayPowerHours(string LocationCode)
        {
            int WeekdayPowerHours = 0;

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable weekday = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "select WeekdayPowerHours from dbo.StoreVariables where LocationCode = '" + LocationCode + "'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(weekday);

                    WeekdayPowerHours = Convert.ToInt32(weekday.Rows[0][0]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
            return WeekdayPowerHours;
        }

        public static void UpdateWeekdayPowerHours(int WeekdayPowerHours, string LocationCode)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "update dbo.StoreVariables set WeekdayPowerHours = '" + WeekdayPowerHours + "' where LocationCode = '" + LocationCode + "'";

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static int GetWeekendPowerHours(string LocationCode)
        {
            int WeekendPowerHours = 0;

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable weekend = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "select WeekendPowerHours from dbo.StoreVariables where LocationCode = '" + LocationCode + "'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(weekend);

                    WeekendPowerHours = Convert.ToInt32(weekend.Rows[0][0]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
            return WeekendPowerHours;
        }

        public static void UpdateWeekendPowerHours(int WeekendPowerHours, string LocationCode)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "update dbo.StoreVariables set WeekendPowerHours = '" + WeekendPowerHours + "' where LocationCode = '" + LocationCode + "'";

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static DataTable GetStoreHours(string LocationCode)
        {
            DataTable OpenCloseTimes = new DataTable();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable OpenTimes = new DataTable();
                    DataTable CloseTimes = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "select SundayOpen, MondayOpen, TuesdayOpen, WednesdayOpen, ThursdayOpen, FridayOpen, SaturdayOpen " +
                                      "from dbo.StoreVariables where LocationCode = '" + LocationCode + "'";
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(OpenTimes);

                    cmd.CommandText = "select SundayClose, MondayClose, TuesdayClose, WednesdayClose, ThursdayClose, FridayClose, SaturdayClose " +
                                      "from dbo.StoreVariables where LocationCode = '" + LocationCode + "'";
                    da = new SqlDataAdapter(cmd);
                    da.Fill(CloseTimes);

                    string[] OpenHours = new string[7];
                    string[] CloseHours = new string[7];

                    for (int i = 0; i < OpenHours.Length; i++)
                    {
                        string currenthour = OpenTimes.Rows[0][i].ToString();
                        string convertedhour = "";
                        for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                        {
                            if (currenthour == SQLHours[n])
                            {
                                convertedhour = ScheduleHalfHourSlots[n];
                                break;
                            }
                        }
                        OpenHours[i] = convertedhour;
                    }

                    for (int i = 0; i < CloseHours.Length; i++)
                    {
                        string currenthour = CloseTimes.Rows[0][i].ToString();
                        string convertedhour = "";
                        for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                        {
                            if (currenthour == SQLHours[n])
                            {
                                convertedhour = ScheduleHalfHourSlots[n];
                                break;
                            }
                        }
                        CloseHours[i] = convertedhour;
                    }

                    for (int i = 0; i < days.Length; i++)
                    {
                        OpenCloseTimes.Columns.Add(days[i]);
                    }
                    DataRow openrow = OpenCloseTimes.NewRow();
                    for (int i = 0; i < 7; i++)
                    {
                        openrow[i] = OpenHours[i];
                    }
                    OpenCloseTimes.Rows.Add(openrow);

                    DataRow closerow = OpenCloseTimes.NewRow();
                    for (int i = 0; i < 7; i++)
                    {
                        closerow[i] = CloseHours[i];
                    }
                    OpenCloseTimes.Rows.Add(closerow);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }

            }
            return OpenCloseTimes;
        }

        public static void UpdateStoreHours(string LocationCode, string[] OpenHours, string[] CloseHours)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    for (int i = 0; i < 7; i++)
                    {
                        for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                        {
                            if (OpenHours[i] == ScheduleHalfHourSlots[n])
                            {
                                OpenHours[i] = SQLHours[n];
                            }
                            if (CloseHours[i] == ScheduleHalfHourSlots[n])
                            {
                                CloseHours[i] = SQLHours[n];
                            }
                        }
                    }

                    cmd.CommandText = "update dbo.StoreVariables set SundayOpen = '" + OpenHours[0] + "', MondayOpen = '" + OpenHours[1] +
                                      "', TuesdayOpen = '" + OpenHours[2] + "', WednesdayOpen = '" + OpenHours[3] + "', ThursdayOpen = '"
                                      + OpenHours[4] + "', FridayOpen = '" + OpenHours[5] + "', SaturdayOpen = '" + OpenHours[6] + "' " +
                                      "where LocationCode = '" + LocationCode + "'";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "update dbo.StoreVariables set SundayClose = '" + CloseHours[0] + "', MondayClose = '" + CloseHours[1] +
                                      "', TuesdayClose = '" + CloseHours[2] + "', WednesdayClose = '" + CloseHours[3] + "', ThursdayClose = '"
                                      + CloseHours[4] + "', FridayClose = '" + CloseHours[5] + "', SaturdayClose = '" + CloseHours[6] + "' " +
                                      "where LocationCode = '" + LocationCode + "'";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static int[] GetWeekWeighting(int Weeks, string LocationCode)
        {
            List<int> WeekWeights = new List<int>();
            DataTable weights = new DataTable();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();
                    string[] WeeksBack = { "WeightingOne", "WeightingTwo", "WeightingThree", "WeightingFour", "WeightingFive", "WeightingSix" };
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    cmd.CommandText = "select WeightingOne, WeightingTwo, WeightingThree, WeightingFour, WeightingFive, WeightingSix " +
                                      "from dbo.StoreVariables where LocationCode = '" + LocationCode + "'";
                    da.Fill(weights);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }

            for (int i = 0; i < Weeks; i++)
            {
                WeekWeights.Add(Convert.ToInt32(weights.Rows[0][i]));
            }

            return WeekWeights.ToArray();
        }

        public static void approveRequest(int messageId, string managerId, string LocationCode)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable RequestInfo = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "select EmployeeCode, StartDate, EndDate, StartTime, EndTime from dbo.Messages where ID = '" + messageId + "'" +
                                      " and LocationCode = '" + LocationCode + "'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(RequestInfo);

                    string EmployeeId = RequestInfo.Rows[0][0].ToString();
                    string StartDate = RequestInfo.Rows[0][1].ToString();
                    string EndDate = RequestInfo.Rows[0][2].ToString();
                    string StartTime = RequestInfo.Rows[0][3].ToString();
                    string EndTime = RequestInfo.Rows[0][4].ToString();
                    bool Approved = true;
                    DateTime Created = DateTime.Now;
                    string ManagerName = "";






                    string EmployeeCode = RequestInfo.Rows[0][0].ToString();
                    string startDateString = RequestInfo.Rows[0][1].ToString();
                    string endDateString = RequestInfo.Rows[0][2].ToString();
                    DateTime startDate = Convert.ToDateTime(startDateString);
                    DateTime endDate = new DateTime();
                    if (endDateString != "")
                    {
                        endDate = Convert.ToDateTime(endDateString);
                    }
                    string startTime = Convert.ToString(RequestInfo.Rows[0][3]);
                    string endTime = Convert.ToString(RequestInfo.Rows[0][4]);
                    string[] allTimes = ScheduleHalfHourSlots;

                    // fill out arrays with assigned dates and times
                    //check if the selected employee is assigned during those times or just unassign those times
                    //string date  if date is length 1
                    //string[] dates   if date is length > 1
                    //string[] hours   if date is length 1


                    List<DateTime> dates = new List<DateTime>();
                    List<string> times = new List<string>();

                    // if the request is multiple days
                    if (endDateString != "")
                    {
                        DateTime date = startDate;
                        for (int i = 0; date <= endDate; i++)
                        {
                            date = date.AddDays(i);
                            dates.Add(date);
                        }

                        //string[] employeeId = { EmployeeId };
                        //Dictionary<string, Dictionary<string, string[]>> EmployeeScheduledDates = GetEmployeeScheduledTimes(employeeId, LocationCode, dates.ToArray());
                        //Dictionary<string, string[]> EmployeeScheduledTimes = EmployeeScheduledDates[EmployeeId];

                        for (int i = 0; i < dates.Count; i++)
                        {
                            DataTable AssignedHourBlocks = new DataTable();
                            cmd.CommandText = "select * from dbo.EmployeeSchedule where ScheduleDate = '" + dates[i] + "' " +
                                              "and EmployeeId = '" + EmployeeId + "' and LocationCode = '" + LocationCode + "'";
                            da.Fill(AssignedHourBlocks);

                            if (AssignedHourBlocks.Rows.Count > 0 && AssignedHourBlocks.Rows[0][4].ToString() != "00:00:00" || AssignedHourBlocks.Rows[0][5].ToString() != "00:00:00")
                            {

                                cmd.CommandText = "delete from dbo.EmployeeSchedule where ScheduleDate = '" + dates[i] + "' " +
                                               "and EmployeeId = '" + EmployeeId + "' and LocationCode = '" + LocationCode + "'";
                                cmd.ExecuteNonQuery();

                                cmd.CommandText = "insert into dbo.EmployeeSchedule(EmployeeId, LocationCode, ScheduleDate, BeginTime, EndTime, OnSchedule)" +
                                                  "values('" + EmployeeId + "', '" + LocationCode + "', '" + dates[i] + "', " +
                                                  "'00:00:00', '00:00:00', '0')";
                                cmd.ExecuteNonQuery();
                            }
                        }

                    }
                    // if the request is one day
                    else
                    {
                        // all day - currently unassigning the entire day even if times are specified
                        //if (startTime == "--" && endTime == "--")
                        //{
                        DataTable AssignedHourBlocks = new DataTable();
                        cmd.CommandText = "select * from dbo.EmployeeSchedule where ScheduleDate = '" + startDate + "' " +
                                          "and EmployeeId = '" + EmployeeId + "' and LocationCode = '" + LocationCode + "'";
                        da.Fill(AssignedHourBlocks);

                        if (AssignedHourBlocks.Rows.Count > 0 && AssignedHourBlocks.Rows[0][4].ToString() != "00:00:00" || AssignedHourBlocks.Rows[0][5].ToString() != "00:00:00")
                        {
                            cmd.CommandText = "delete from dbo.EmployeeSchedule where ScheduleDate = '" + startDate + "' " +
                                          "and EmployeeId = '" + EmployeeId + "' and LocationCode = '" + LocationCode + "'";
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = "insert into dbo.EmployeeSchedule(EmployeeId, LocationCode, ScheduleDate, BeginTime, EndTime, OnSchedule)" +
                                              "values('" + EmployeeId + "', '" + LocationCode + "', '" + startDate + "', " +
                                              "'00:00:00', '00:00:00', '0')";
                            cmd.ExecuteNonQuery();
                        }
                        //}
                        // specific hours - not including in the current build
                        //else
                        //{
                        //    DataTable AssignedHourBlocks = new DataTable();
                        //    cmd.CommandText = "select * from dbo.EmployeeSchedule where ScheduleDate = '" + startDate + "' " +
                        //                      "and EmployeeId = '" + EmployeeId + "' and LocationCode = '" + LocationCode + "'";
                        //    da.Fill(AssignedHourBlocks);

                        //    if (AssignedHourBlocks.Rows.Count > 0 && AssignedHourBlocks.Rows[0][4].ToString() != "00:00:00" || AssignedHourBlocks.Rows[0][5].ToString() != "00:00:00")
                        //    {
                        //        for (int n = 0; n < AssignedHourBlocks.Rows.Count; n++)
                        //        {
                        //            string starthoursql = AssignedHourBlocks.Rows[n][4].ToString();
                        //            string endhoursql = AssignedHourBlocks.Rows[n][5].ToString();
                        //            string starthour = "";
                        //            string endhour = "";
                        //            for (int h = 0; h < SQLHours.Length; h++)
                        //            {
                        //                if (SQLHours[h] == starthoursql)
                        //                {
                        //                    starthour = ScheduleHalfHourSlots[h];
                        //                }
                        //                if (SQLHours[h] == endhoursql)
                        //                {
                        //                    endhour = ScheduleHalfHourSlots[h];
                        //                }
                        //            }

                        //            // get the employee's scheduled hours
                        //            bool block = false;
                        //            List<string> ScheduledHours = new List<string>();
                        //            for (int h = 0; h < ScheduleHalfHourSlots.Length; h++)
                        //            {
                        //                if (ScheduleHalfHourSlots[h] == starthour)
                        //                {
                        //                    block = true;
                        //                }
                        //                if (block == true)
                        //                {
                        //                    ScheduledHours.Add(ScheduleHalfHourSlots[h]);
                        //                }
                        //                if (ScheduleHalfHourSlots[h] == endhour)
                        //                {
                        //                    block = false;
                        //                }
                        //            }
                        //            // get the time off request hours
                        //            block = false;
                        //            List<string> TimeOffHours = new List<string>();
                        //            for (int h = 0; h < SQLHours.Length; h++)
                        //            {
                        //                if (SQLHours[h] == startTime)
                        //                {
                        //                    block = true;
                        //                }
                        //                if (block == true)
                        //                {
                        //                    TimeOffHours.Add(ScheduleHalfHourSlots[h]);
                        //                }
                        //                if (SQLHours[h] == endhour)
                        //                {
                        //                    block = false;
                        //                }
                        //            }
                        //            // if any scheduled hours are in the time off span, unassign them
                        //            block = false;
                        //            List<string> UnassignHours = new List<string>();
                        //            for (int h = 0; h < ScheduledHours.Count; h++)
                        //            {
                        //                for (int j = 0; j < TimeOffHours.Count; j++)
                        //                {
                        //                    if (ScheduledHours[h] == TimeOffHours[j])
                        //                    {
                        //                        UnassignHours.Add(ScheduledHours[h]);
                        //                    }
                        //                }
                        //            }

                        //            //need a way to unassign by date and time here or in a new function

                        //        }
                        //    }
                        //}
                    }






                    foreach (Employees emp in GetEmployeesForStore(LocationCode))
                    {
                        if (emp.id == managerId)
                        {
                            ManagerName = emp.firstName + " " + emp.lastName;
                        }
                    }
                    //string response = GenerateManagerResponse(managerId, EmployeeId, StartDate, EndDate, StartTime, EndTime, Approved);

                    cmd.CommandText = "insert into dbo.Messages (Created, Type, ManagerCode, EmployeeCode, LocationCode, StartDate, EndDate, StartTime, EndTime, Approved)"
                                    + " values ('" + Created + "', 'ManagerResponse', '" + managerId + "', '" + EmployeeId + "', '" + LocationCode + "', '" + StartDate + "', '"
                                    + EndDate + "', '" + StartTime + "', '" + EndTime + "', '" + Approved + "')";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "delete from dbo.Messages where ID = '" + messageId + "' and LocationCode = '" + LocationCode + "'";
                    cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        //public static void Unassign(string unassignTimes, string employeeId, int selectedDay, Dictionary<string, Dictionary<string, string[]>> EmployeeScheduledTimes, DateTime[] CurrentWeekDates, string LocationCode)
        //{
        //    Dictionary<string, string[]> employeeWeekSchedule = EmployeeScheduledTimes[employeeId];
        //    string[] employeeAssignedHours = employeeWeekSchedule[days[selectedDay]];
        //    string[] unassignHours = unassignTimes.Split(',');  // split the single element array into multiple elements

        //    for (int i = 0; i < employeeAssignedHours.Length; i++)
        //    {
        //        for (int h = 0; h < unassignHours.Length; h++)
        //        {
        //            if (employeeAssignedHours[i] == unassignHours[h])
        //            {
        //                var foos = new List<string>(employeeAssignedHours);
        //                foos.RemoveAt(i);
        //                employeeAssignedHours = foos.ToArray();
        //            }
        //        }
        //    }
        //    List<string[]> assignedBlocks = new List<string[]>();

        //    List<string> employeeBlock = new List<string>();
        //    int hourcount = 0;
        //    bool bl = false;

        //    if (employeeAssignedHours.Length > 0)
        //    {
        //        for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
        //        {
        //            if (hourcount == employeeAssignedHours.Length - 1)
        //            {
        //                employeeBlock.Add(ScheduleHalfHourSlots[n]);
        //                string[] hours = employeeBlock.ToArray();

        //                List<string> StartEndTimes = new List<string>();
        //                StartEndTimes.Add(hours[0]);
        //                StartEndTimes.Add(hours[hours.Length - 1]);

        //                assignedBlocks.Add(StartEndTimes.ToArray());
        //                break;
        //            }
        //            if (ScheduleHalfHourSlots[n] == employeeAssignedHours[hourcount])
        //            {
        //                bl = true;
        //            }
        //            if (bl == true && n < ScheduleHalfHourSlots.Length - 1 &&
        //                ScheduleHalfHourSlots[n + 1] != employeeAssignedHours[hourcount + 1])
        //            {
        //                bl = false;
        //                employeeBlock.Add(ScheduleHalfHourSlots[n]);
        //                string[] hours = employeeBlock.ToArray();

        //                List<string> StartEndTimes = new List<string>();
        //                StartEndTimes.Add(hours[0]);
        //                StartEndTimes.Add(hours[hours.Length - 1]);

        //                assignedBlocks.Add(StartEndTimes.ToArray());
        //                employeeBlock.Clear();
        //                hourcount++;
        //            }
        //            if (bl == true)
        //            {
        //                employeeBlock.Add(ScheduleHalfHourSlots[n]);
        //                hourcount++;
        //            }
        //        }
        //    }

        //    //convert to sql hours
        //    for (int i = 0; i < assignedBlocks.Count; i++)
        //    {
        //        string[] startend = assignedBlocks[i];
        //        string start = startend[0];
        //        string end = startend[1];

        //        for (int n = 0; n < ScheduleHalfHourSlots.Length - 1; n++)
        //        {
        //            if (ScheduleHalfHourSlots[n] == start)
        //            {
        //                start = SQLHours[n];
        //            }
        //            if (ScheduleHalfHourSlots[n] == end)
        //            {
        //                end = SQLHours[n + 1];
        //            }
        //        }
        //        startend[0] = start;
        //        startend[1] = end;
        //        assignedBlocks[i] = startend;
        //    }

        //    using (SqlConnection conn = new SqlConnection(strSQLCon))
        //    {
        //        try
        //        {
        //            SqlCommand cmd = new SqlCommand();
        //            cmd.CommandType = CommandType.Text;
        //            cmd.Connection = conn;
        //            conn.Open();


        //            cmd.CommandText = "delete from dbo.EmployeeSchedule where ScheduleDate = '" + CurrentWeekDates[selectedDay] + "' and EmployeeId = '" + employeeId + "'";
        //            cmd.ExecuteNonQuery();

        //            for (int i = 0; i < assignedBlocks.Count; i++)
        //            {
        //                string[] hourBlock = assignedBlocks[i];

        //                TimeSpan BeginTime = Convert.ToDateTime(hourBlock[0]).TimeOfDay;
        //                TimeSpan EndTime = Convert.ToDateTime(hourBlock[1]).TimeOfDay;

        //                cmd.CommandText = "insert into dbo.EmployeeSchedule (EmployeeId, LocationCode, ScheduleDate, BeginTime, EndTime, OnSchedule)"
        //                                + " values ('" + employeeId + "', '" + LocationCode + "', '" + CurrentWeekDates[selectedDay] + "', '" + BeginTime + "', '"
        //                                + EndTime + "', '1')";
        //                cmd.ExecuteNonQuery();
        //            }



        //            //cmd.CommandText = "update dbo.EmployeeSchedule set BeginTime = '" + startHours + "', EndTime = '" + endHours + "', "
        //            //                + "OnSchedule = 1 where EmployeeId = '" + employeeId + "' and ScheduleDate = '" + CurrentWeekDates[selectedDay] + "'";

        //            //cmd.ExecuteNonQuery();

        //            if (assignedBlocks.Count == 0)
        //            {
        //                cmd.CommandText = "update dbo.EmployeeSchedule set OnSchedule = 0 where EmployeeId = '"
        //                                 + employeeId + "' and ScheduleDate = '" + CurrentWeekDates[selectedDay] + "'";

        //                cmd.ExecuteNonQuery();
        //            }
        //        }

        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //        finally
        //        {
        //            conn.Close();
        //        }
        //    }
        //}

        public static void denyRequest(int messageId, string managerId, string LocationCode)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable RequestInfo = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "select EmployeeCode, StartDate, EndDate, StartTime, EndTime from dbo.Messages where ID = '" + messageId + "'" +
                                      " and LocationCode = '" + LocationCode + "'";
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(RequestInfo);

                    string EmployeeId = RequestInfo.Rows[0][0].ToString();
                    string StartDate = RequestInfo.Rows[0][1].ToString();
                    string EndDate = RequestInfo.Rows[0][2].ToString();
                    string StartTime = RequestInfo.Rows[0][3].ToString();
                    string EndTime = RequestInfo.Rows[0][4].ToString();
                    bool Approved = false;
                    DateTime Created = DateTime.Now;
                    string ManagerName = "";

                    foreach (Employees emp in GetEmployeesForStore(LocationCode))
                    {
                        if (emp.id == managerId)
                        {
                            ManagerName = emp.firstName + " " + emp.lastName;
                        }
                    }
                    //string response = GenerateManagerResponse(managerId, EmployeeId, StartDate, EndDate, StartTime, EndTime, Approved);

                    cmd.CommandText = "insert into dbo.Messages (Created, Type, ManagerCode, EmployeeCode, LocationCode, StartDate, EndDate, StartTime, EndTime, Approved)"
                                    + " values ('" + Created + "', 'ManagerResponse', '" + managerId + "', '" + EmployeeId + "', '" + LocationCode + "', '" + StartDate + "', '"
                                    + EndDate + "', '" + StartTime + "', '" + EndTime + "', '" + Approved + "')";

                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "delete from dbo.Messages where ID = '" + messageId + "' and LocationCode = '" + LocationCode + "'";

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static DataTable CreateConsolidatedSchedule(DateTime[] CurrentWeekDates)
        {
            Dictionary<string, Dictionary<string, string[]>> AssignedEmployeeSchedules = new Dictionary<string, Dictionary<string, string[]>>();
            DataTable ScheduledEmployees = new DataTable();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable Schedule = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    Schedule.Columns.Add("Time");

                    for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
                    {
                        Schedule.Rows.Add(ScheduleHalfHourSlots[i]);
                    }

                    for (int i = 0; i < 7; i++)
                    {
                        Schedule.Columns.Add(days[i]);

                        string date = CurrentWeekDates[i].ToString("yyyy'-'MM'-'dd");

                        cmd.CommandText = "select * from dbo.EmployeeSchedule where ScheduleDate = '" + date + "' "
                                        + "and OnSchedule = '1';";

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(ScheduledEmployees);
                    }

                    List<string> empIds = new List<string>();
                    string emp = "";

                    //for (int i = 0; i < ScheduledEmployees.Rows.Count; i++)
                    //{
                    //    string employeeId = ScheduledEmployees.Rows[i][1].ToString();

                    //    if (employeeId != emp)
                    //    {
                    //        empIds.Add(employeeId);
                    //        emp = employeeId;
                    //    }

                    //}

                    //string lastEmployee = "";

                    //Dictionary<string, string[]> employeeScheduledHours = new Dictionary<string, string[]>();
                    //List<string> scheduledHours = new List<string>();
                    //for (int n = 0; n < 7; n++)
                    //{
                    //    //List<string> empty = new List<string>();
                    //    string[] empty = new string[0];
                    //    employeeScheduledHours.Add(CurrentWeekDates[n].ToString(), empty);
                    //}

                    //int count = 0;
                    //for (int i = 0; i < ScheduledEmployees.Rows.Count; i++)
                    //{
                    //    string employeeId = ScheduledEmployees.Rows[i][1].ToString();

                    //    if (employeeId != lastEmployee && lastEmployee != "")
                    //    {

                    //        AssignedEmployeeSchedules.Add(lastEmployee, employeeScheduledHours);
                    //        scheduledHours.Clear();

                    //        for (int n = 0; n < 7; n++)
                    //        {
                    //            string[] empty = new string[0];
                    //            employeeScheduledHours[CurrentWeekDates[i].ToString()] = empty;
                    //        }
                    //        count = 0;

                    //    }

                    //    List<string> hours = new List<string>();

                    //    string date = Convert.ToString(ScheduledEmployees.Rows[i][3]);
                    //    string starthour = ScheduledEmployees.Rows[i][4].ToString();
                    //    string endhour = ScheduledEmployees.Rows[i][5].ToString();
                    //    for (int m = 0; m < SQLHours.Length; m++)
                    //    {
                    //        if (SQLHours[m] == starthour)
                    //        {
                    //            starthour = ScheduleHalfHourSlots[m];
                    //        }
                    //        if (SQLHours[m] == endhour)
                    //        {
                    //            endhour = ScheduleHalfHourSlots[m];
                    //            break;
                    //        }
                    //    }
                    //    bool start = false;
                    //    string[] dammit = new string[24];
                    //    for (int m = 0; m < ScheduleHalfHourSlots.Length; m++)
                    //    {
                    //        if (ScheduleHalfHourSlots[m] == starthour)
                    //        {
                    //            start = true;
                    //        }
                    //        if (start == true)
                    //        {
                    //            //dammit[count] = ScheduleHalfHourSlots[m];
                    //            //count++;
                    //            scheduledHours.Add(ScheduleHalfHourSlots[m]);
                    //        }
                    //        if (ScheduleHalfHourSlots[m] == endhour)
                    //        {
                    //            start = false;
                    //            employeeScheduledHours[date] = scheduledHours.ToArray();
                    //            break;
                    //        }
                    //    }
                    //    lastEmployee = employeeId;
                    //}
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
            return ScheduledEmployees;
        }

        public static void DeleteEmployeeMessage(int messageId)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable RequestInfo = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "delete from dbo.Messages where ID ='" + messageId + "'";

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static string GenerateMessageForEmployee(string managerId, string managerName, string startDate, string endDate, string startTime, string endTime, string approved)
        {
            string response = "";
            string startDay = Convert.ToDateTime(startDate).ToString("dddd");
            string Approved = "";

            if (approved == "True")
            {
                Approved = "approved";
            }
            else
            {
                Approved = "denied";
            }


            if (endDate != "")
            {
                string endDay = Convert.ToDateTime(endDate).ToString("dddd");
                response = managerName + " has " + Approved + " your time off request for " + startDate + " (" + startDay + ") - " + endDate + " (" + endDay + ")";

                return response;
            }
            else
            {
                if (startTime == "--" && endTime == "--")
                {
                    response = managerName + " has " + Approved + " your time off request for " + startDate + " (" + startDay + ")";
                }
                else
                {
                    response = managerName + " has " + Approved + " your time off request for " + startDate + " (" + startDay + ") from " + startTime + " to " + endTime;
                }
            }

            return response;
        }

        public static void UpdateExcludedDates(string LocationCode, DateTime[] DatesToExclude)
        {
            //without more input this will only work for up to 6 weeks back
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "delete from dbo.ExcludedDates where LocationCode = '" + LocationCode + "'";
                    cmd.ExecuteNonQuery();

                    for (int i = 0; i < DatesToExclude.Length; i++)
                    {
                        cmd.CommandText = "insert into dbo.ExcludedDates (LocationCode, Date) " +
                                          "values ('" + LocationCode + "','" + DatesToExclude[i] + "')";
                        cmd.ExecuteNonQuery();
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
            }
        }

        public static List<DateTime> GetExcludedDates(string LocationCode)
        {
            List<DateTime> ExcludedDates = new List<DateTime>();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable excluded = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "select * from dbo.ExcludedDates where LocationCode = '" + LocationCode + "'";
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(excluded);

                    for (int i = 0; i < excluded.Rows.Count; i++)
                    {
                        ExcludedDates.Add(Convert.ToDateTime(excluded.Rows[i][1]));
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
            }

            return ExcludedDates;
        }

        public static void ImportLastWeekSchedule(string LocationCode, string[] EmployeeIds, DateTime[] RequestedDates, DateTime[] PreviousWeekDates)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable LastWeekSchedule = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    for (int i = 0; i < RequestedDates.Length; i++)
                    {
                        for (int n = 0; n < EmployeeIds.Length; n++)
                        {
                            cmd.CommandText = "select * from dbo.EmployeeSchedule where EmployeeId = '" + EmployeeIds[n] + "' and ScheduleDate = '" + PreviousWeekDates[i] + "'";
                            da.Fill(LastWeekSchedule);
                            cmd.CommandText = "delete from dbo.EmployeeSchedule where EmployeeId = '" + EmployeeIds[n] + "' and ScheduleDate = '" + RequestedDates[i] + "'";
                            cmd.ExecuteNonQuery();
                        }
                    }

                    //for (int i = 0; i < RequestedDates.Length; i++)
                    //{
                    //    for (int n = 0; n < EmployeeIds.Length; n++)
                    //    {
                    //        cmd.CommandText = "delete from dbo.EmployeeSchedule where EmployeeId = '" + EmployeeIds[n] + "' and ScheduleDate = '" + RequestedDates[i] + "'";
                    //        cmd.ExecuteNonQuery();
                    //    }
                    //}

                    for (int i = 0; i < LastWeekSchedule.Rows.Count; i++)
                    {
                        for (int n = 0; n < 7; n++)
                        {
                            if (Convert.ToDateTime(LastWeekSchedule.Rows[i][3]) == PreviousWeekDates[n])
                            {
                                LastWeekSchedule.Rows[i][3] = RequestedDates[n];
                            }
                        }
                    }

                    for (int i = 0; i < LastWeekSchedule.Rows.Count; i++)
                    {
                        cmd.CommandText = "insert into dbo.EmployeeSchedule(EmployeeId, LocationCode, ScheduleDate, BeginTime, EndTime, OnSchedule)" +
                                          "values('" + LastWeekSchedule.Rows[i][1] + "', '" + LastWeekSchedule.Rows[i][2] + "', '" + LastWeekSchedule.Rows[i][3] + "', " +
                                          "'" + LastWeekSchedule.Rows[i][4] + "', '" + LastWeekSchedule.Rows[i][5] + "', '" + LastWeekSchedule.Rows[i][6] + "')";
                        cmd.ExecuteNonQuery();
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
            }
        }
        public static void ClearRequestedWeekSchedule(string LocationCode, string[] EmployeeIds, DateTime[] RequestedDates)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    for (int i = 0; i < RequestedDates.Length; i++)
                    {
                        for (int n = 0; n < EmployeeIds.Length; n++)
                        {
                            cmd.CommandText = "delete from dbo.EmployeeSchedule where EmployeeId = '" + EmployeeIds[n] + "' and  LocationCode = '" + LocationCode + "' " +
                                              "and ScheduleDate = '" + RequestedDates[i] + "'";
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = "insert into dbo.EmployeeSchedule(EmployeeId, LocationCode, ScheduleDate, BeginTime, EndTime, OnSchedule)" +
                                              "values('" + EmployeeIds[n] + "', '" + LocationCode + "', '" + RequestedDates[i] + "', " +
                                              "'00:00:00', '00:00:00', '0')";
                            cmd.ExecuteNonQuery();
                        }
                    }
                    //for (int i = 0; i < RequestedDates.Length; i++)
                    //{
                    //    for (int n = 0; n < EmployeeIds.Length; n++)
                    //    {
                    //        cmd.CommandText = "insert into dbo.EmployeeSchedule(EmployeeId, LocationCode, ScheduleDate, BeginTime, EndTime, OnSchedule)" +
                    //                          "values('" + EmployeeIds[n] + "', '" + LocationCode + "', '" + RequestedDates[i] + "', " +
                    //                          "'00:00:00', '00:00:00', '0')";
                    //        cmd.ExecuteNonQuery();
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }

            }
        }

        /// <summary>
        /// Get employee availability times in text for the assignment area
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string[]>> GetEmployeeAvailableTimes(string[] employeeIds)
        {
            Dictionary<string, Dictionary<string, string[]>> EmployeeAvailableTimes = new Dictionary<string, Dictionary<string, string[]>>();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable AvailableTimesUnordered = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    for (int i = 0; i < employeeIds.Length; i++)
                    {
                        DataTable ExistingEmployeeAvailability = new DataTable();
                        cmd.CommandText = "select * from dbo.EmployeeAvailability where EmployeeId = '" + employeeIds[i] + "'";

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(ExistingEmployeeAvailability);
                        if (ExistingEmployeeAvailability.Rows.Count == 0)
                        {
                            CreateEmployeeAvailability(employeeIds[i]);
                        }

                        da.Fill(AvailableTimesUnordered);
                    }

                    DataTable AvailableTimesOrdered = AvailableTimesUnordered.Clone();
                    for (int i = 0; i < employeeIds.Length; i++)
                    {
                        for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                        {
                            for (int m = 0; m < AvailableTimesUnordered.Rows.Count; m++)
                            {
                                if (AvailableTimesUnordered.Rows[m][0].ToString() == employeeIds[i] &&
                                    AvailableTimesUnordered.Rows[m][1].ToString() == ScheduleHalfHourSlots[n])
                                {
                                    DataRow dr = AvailableTimesUnordered.Rows[m];
                                    AvailableTimesOrdered.ImportRow(AvailableTimesUnordered.Rows[m]);
                                }
                            }
                        }
                    }

                    for (int i = 0; i < employeeIds.Length; i++)
                    {
                        Dictionary<string, string[]> innerDict = new Dictionary<string, string[]>();
                        for (int n = 0; n < days.Length; n++)
                        {
                            List<string> availableHours = new List<string>();
                            for (int m = 0; m < AvailableTimesOrdered.Rows.Count; m++)
                            {
                                if (AvailableTimesOrdered.Rows[m][0].ToString() == employeeIds[i] &&
                                    AvailableTimesOrdered.Rows[m][n + 2].ToString() == "True")
                                {
                                    availableHours.Add(AvailableTimesOrdered.Rows[m][1].ToString());
                                }
                            }
                            innerDict[days[n]] = availableHours.ToArray();
                        }
                        EmployeeAvailableTimes[employeeIds[i]] = innerDict;
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
            }
            return EmployeeAvailableTimes;
        }

        /// <summary>
        /// Create an employee's availability in the database if they are active but do not have availability yet
        /// </summary>
        /// <param name="EmployeeCode"></param>
        public static void CreateEmployeeAvailability(string EmployeeCode)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
                    {
                        cmd.CommandText = "insert into dbo.EmployeeAvailability (EmployeeId, Hour, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday) " +
                                           "values('" + EmployeeCode + "', '" + ScheduleHalfHourSlots[i] + "', '1', '1', '1', '1', '1', '1', '1')";
                        cmd.ExecuteNonQuery();
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
            }
        }

        /// <summary>
        /// Get employee scheduled times in text for the assignment area
        /// </summary>
        /// <param name="weekdays"></param>
        /// <param name="employeeIds"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string[]>> GetEmployeeScheduledTimes(string[] employeeIds, string LocationCode, DateTime[] dates)
        {
            Dictionary<string, Dictionary<string, string[]>> EmployeeScheduledTimes = new Dictionary<string, Dictionary<string, string[]>>();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable EmployeeListSQL = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    for (int i = 0; i < employeeIds.Length; i++)
                    {
                        Dictionary<string, string[]> innerDict = new Dictionary<string, string[]>();
                        for (int n = 0; n < days.Length; n++)
                        {
                            DataTable str = new DataTable();

                            List<string> startTimes = new List<string>();
                            List<string> endTimes = new List<string>();

                            string starts;
                            string ends;

                            //cmd.CommandText = "select Hour from dbo.emp_" + employeeIds[i]
                            //                  + " where " + weekdays[n] + " = 'True'";

                            cmd.CommandText = "select BeginTime, EndTime from dbo.EmployeeSchedule"
                                            + " where EmployeeId = '" + employeeIds[i] + "'"
                                            + " and ScheduleDate = '" + dates[n] + "'"
                                            + " and LocationCode = '" + LocationCode + "'";

                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            da.Fill(str);

                            for (int m = 0; m < str.Rows.Count; m++)
                            {
                                starts = str.Rows[m][0].ToString();
                                ends = str.Rows[m][1].ToString();

                                if (starts == "00:00:00" && ends == "00:00:00")
                                {
                                    starts = "";
                                    ends = "";
                                }
                                else
                                {
                                    for (int j = 0; j < SQLHours.Length; j++)
                                    {
                                        if (SQLHours[j] == starts)
                                        {
                                            starts = ScheduleHalfHourSlots[j];
                                        }
                                        if (SQLHours[j] == ends)
                                        {
                                            ends = ScheduleHalfHourSlots[j - 1];
                                            break;
                                        }
                                    }
                                }

                                startTimes.Add(starts);
                                endTimes.Add(ends);
                            }


                            //string[] startsArray = starts.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            //string[] endsArray = ends.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            List<string> assignedHours = new List<string>();

                            for (int h = 0; h < startTimes.Count; h++)
                            {
                                string[] startend = new string[2];
                                startend[0] = startTimes[h];
                                startend[1] = endTimes[h];
                                bool check = false;
                                for (int j = 0; j < ScheduleHalfHourSlots.Length; j++)
                                {
                                    if (startend[0] == ScheduleHalfHourSlots[j])
                                    {
                                        check = true;
                                    }
                                    if (check == true)
                                    {
                                        assignedHours.Add(ScheduleHalfHourSlots[j]);
                                    }
                                    if (startend[1] == ScheduleHalfHourSlots[j])
                                    {
                                        check = false;
                                    }
                                }
                            }
                            string[] hours = new string[assignedHours.Count];
                            for (int j = 0; j < assignedHours.Count; j++)
                            {
                                hours[j] = assignedHours[j];
                            }

                            innerDict[days[n]] = hours;
                        }
                        EmployeeScheduledTimes.Add(employeeIds[i], innerDict);
                        //EmployeeScheduledTimes[employeeIds[i]] = innerDict;
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
            }
            return EmployeeScheduledTimes;
        }

        public static Dictionary<string, Dictionary<DateTime, string[]>> GetEmployeeTimeOff(string LocationCode, DateTime[] currentWeekDates)
        {
            Dictionary<string, Dictionary<DateTime, string[]>> EmployeeTimeOff = new Dictionary<string, Dictionary<DateTime, string[]>>();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();


                    Dictionary<DateTime, string[]> innerDict = new Dictionary<DateTime, string[]>();
                    for (int n = 0; n < currentWeekDates.Length; n++)
                    {
                        innerDict.Add(currentWeekDates[n], new string[0]);
                    }
                    DataTable str = new DataTable();

                    cmd.CommandText = "select EmployeeCode, StartDate, EndDate, StartTime, EndTime from dbo.Messages "
                                    + "where LocationCode = '" + LocationCode + "'"
                                    + " and Approved = '1'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(str);

                    string[] hours = new string[str.Rows.Count];

                    for (int j = 0; j < str.Rows.Count; j++)
                    {
                        string EmployeeCode = str.Rows[j][0].ToString();
                        string startDateString = str.Rows[j][1].ToString();
                        string endDateString = str.Rows[j][2].ToString();
                        DateTime startDate = Convert.ToDateTime(startDateString);
                        DateTime endDate = new DateTime();
                        if (endDateString != "")
                        {
                            endDate = Convert.ToDateTime(endDateString);
                        }
                        string startTime = Convert.ToString(str.Rows[j][3]);
                        string endTime = Convert.ToString(str.Rows[j][4]);
                        string[] allTimes = ScheduleHalfHourSlots;

                        if (endDateString != "")
                        {
                            //multiple days, set all hours for each day
                            bool start = false;
                            for (int n = 0; n < 7; n++)
                            {
                                if (currentWeekDates[n] == startDate)
                                {
                                    innerDict[startDate] = allTimes;
                                    start = true;
                                }
                                if (start == true)
                                {
                                    innerDict[currentWeekDates[n]] = allTimes;
                                }
                                if (currentWeekDates[n] == endDate)
                                {
                                    innerDict[endDate] = allTimes;
                                    start = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (startTime != "--" && endTime != "--")
                            {
                                //one day, specific times
                                for (int n = 0; n < 28; n++)
                                {
                                    if (currentWeekDates[n] == startDate)
                                    {
                                        List<string> offHours = new List<string>();
                                        bool start = false;

                                        for (int h = 0; h < ScheduleHalfHourSlots.Length; h++)
                                        {
                                            if (ScheduleHalfHourSlots[h] == startTime)
                                            {
                                                start = true;
                                            }
                                            if (ScheduleHalfHourSlots[h] == endTime)
                                            {
                                                start = false;
                                                break;
                                            }
                                            if (start == true)
                                            {
                                                offHours.Add(ScheduleHalfHourSlots[h]);
                                            }
                                        }

                                        string[] offhours = new string[offHours.Count];
                                        for (int h = 0; h < offHours.Count; h++)
                                        {
                                            offhours[h] = offHours[h];
                                        }

                                        innerDict[startDate] = offhours;
                                    }
                                }
                            }
                            else
                            {
                                //one day, all times
                                //for (int n = 0; n < 28; n++)
                                //{
                                //    if (currentWeekDates[n] == startDate)
                                //    {
                                innerDict[startDate] = allTimes;
                                //    }
                                //}
                            }
                        }
                        EmployeeTimeOff[EmployeeCode] = innerDict;
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
            }
            return EmployeeTimeOff;
        }

        /// <summary>
        /// Get messages for a specific employee to display on their home page
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public static List<EmployeeNotification> GetMessagesForEmployee(string employeeId)
        {
            List<EmployeeNotification> notifications = new List<EmployeeNotification>();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable messages = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "select * from dbo.Messages" +
                                      " where Type = 'ManagerResponse' and EmployeeId = '" + employeeId + "'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(messages);

                    for (int i = 0; i < messages.Rows.Count; i++)
                    {
                        int MessageId = Convert.ToInt32(messages.Rows[i][0]);
                        string ManagerId = messages.Rows[i][3].ToString();
                        string ManagerName = messages.Rows[i][5].ToString();
                        string StartDate = messages.Rows[i][6].ToString();
                        string EndDate = messages.Rows[i][7].ToString();
                        string StartTime = messages.Rows[i][8].ToString();
                        string EndTime = messages.Rows[i][9].ToString();
                        string Approved = messages.Rows[i][10].ToString();
                        bool approved = false;

                        if (Approved == "True")
                        {
                            approved = true;
                        }

                        notifications.Add(new EmployeeNotification()
                        {
                            messageId = MessageId,
                            approved = approved,
                            message = GenerateMessageForEmployee(ManagerId, ManagerName, StartDate, EndDate, StartTime, EndTime, Approved)
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

                return notifications;
            }
        }

        /// <summary>
        /// Get messages for the manager from the time off request table
        /// </summary>
        /// <param name="EmployeeID"></param>
        /// <returns></returns>
        public static List<ManagerNotification> GetMessagesForManager(string LocationCode)
        {
            List<ManagerNotification> notifications = new List<ManagerNotification>();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable requests = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    //cmd.CommandText = "select Id, Message, Created from dbo.TimeOffRequests";
                    cmd.CommandText = "select ID, Created, EmployeeCode, Name, StartDate, EndDate, StartTime, EndTime from dbo.Messages"
                                    + " where LocationCode = '" + LocationCode + "' and Type = 'TimeOffRequest'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(requests);

                    for (int i = 0; i < requests.Rows.Count; i++)
                    {
                        int messageId = Convert.ToInt32(requests.Rows[i][0]);
                        string created = Convert.ToString(requests.Rows[i][1]);
                        string id = Convert.ToString(requests.Rows[i][2]);
                        string name = Convert.ToString(requests.Rows[i][3]);
                        string startDate = Convert.ToString(requests.Rows[i][4]);
                        string endDate = Convert.ToString(requests.Rows[i][5]);
                        string startTime = Convert.ToString(requests.Rows[i][6]);
                        string endTime = Convert.ToString(requests.Rows[i][7]);
                        string generatedMessage = GenerateMessageForManager(name, startDate, endDate, startTime, endTime);

                        notifications.Add(new ManagerNotification()
                        {
                            messageId = messageId,
                            created = created,
                            id = id,
                            name = name,
                            startDate = startDate,
                            endDate = endDate,
                            startTime = startTime,
                            endTime = endTime,
                            message = generatedMessage
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

                return notifications;
            }
        }

        /// <summary>
        /// Create a message string to display to the manager
        /// </summary>
        /// <param name="name"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static string GenerateMessageForManager(string name, string startDate, string endDate, string startTime, string endTime)
        {
            string message = "";
            string startDay = Convert.ToDateTime(startDate).ToString("dddd");

            if (endDate != "")
            {
                string endDay = Convert.ToDateTime(endDate).ToString("dddd");
                message = name + " has requested the time off for " + startDate + " (" + startDay + ") - " + endDate + " (" + endDay + ")";

                return message;
            }
            else
            {
                // if condition to check if employee wants all day off
                if (startTime == "--" && endTime == "--")
                {
                    message = name + " has requested the day off on " + startDate + " (" + startDay + ")";
                }
                else
                {
                    message = name + " has requested time off on " + startDate + " (" + startDay + ") from " + startTime + " to " + endTime;
                }

                return message;
            }
        }

        /// <summary>
        /// Get an employee's schedule to display on their home page
        /// </summary>
        /// <param name="EmployeeID"></param>
        /// <returns></returns>
        public static DataTable GetEmployeeSchedule(string EmployeeID, DateTime[] currentWeekDates)
        {
            DataTable StartEndTimes = new DataTable();
            DataTable EmployeeSchedule = new DataTable();

            EmployeeSchedule.Columns.Add("Hour");
            EmployeeSchedule.Columns.Add("Sunday");
            EmployeeSchedule.Columns.Add("Monday");
            EmployeeSchedule.Columns.Add("Tuesday");
            EmployeeSchedule.Columns.Add("Wednesday");
            EmployeeSchedule.Columns.Add("Thursday");
            EmployeeSchedule.Columns.Add("Friday");
            EmployeeSchedule.Columns.Add("Saturday");

            for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
            {
                DataRow dr = EmployeeSchedule.NewRow();
                dr[0] = ScheduleHalfHourSlots[i];
                for (int n = 1; n <= 7; n++)
                {
                    dr[n] = "False";
                }
                EmployeeSchedule.Rows.Add(dr);
            }

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    for (int i = 0; i < 7; i++)
                    {
                        string date = currentWeekDates[i].ToString("yyyy'-'MM'-'dd");

                        cmd.CommandText = "select BeginTime, EndTime from dbo.EmployeeSchedule where EmployeeId = '" + EmployeeID + "' "
                            + "and ScheduleDate ='" + date + "'";

                        SqlDataAdapter da = new SqlDataAdapter(cmd);

                        da.Fill(StartEndTimes);
                    }

                    for (int i = 0; i < StartEndTimes.Rows.Count; i++)
                    {
                        string startTime = StartEndTimes.Rows[i][0].ToString();
                        string endTime = StartEndTimes.Rows[i][1].ToString();

                        int startMark = 0;
                        int endMark = 0;
                        bool startCheck = false;
                        bool endCheck = false;

                        for (int n = 0; n < SQLHours.Length; n++)
                        {
                            if (startTime == SQLHours[n])
                            {
                                startMark = n;
                                startCheck = true;
                            }
                            if (endTime == SQLHours[n])
                            {
                                endMark = n;
                                endCheck = true;
                            }
                        }
                        if (startCheck = true && endCheck == true)
                        {
                            for (int j = startMark; j <= endMark; j++)
                            {
                                EmployeeSchedule.Rows[j][i + 1] = "True";
                            }
                        }
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

                return EmployeeSchedule;
            }
        }

        /// <summary>
        /// Get an employee's availability schedule for the availability page
        /// </summary>
        /// <param name="EmployeeId"></param>
        /// <returns></returns>
        public static DataTable GetEmployeeAvailability(string EmployeeId)
        {
            DataTable EmployeeSchedule = new DataTable();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
                    {
                        cmd.CommandText = "select Hour, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday"
                                        + " from dbo.EmployeeAvailability where EmployeeId = '" + EmployeeId + "'"
                                        + " and Hour = '" + ScheduleHalfHourSlots[i] + "'";

                        SqlDataAdapter da = new SqlDataAdapter(cmd);

                        da.Fill(EmployeeSchedule);
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

                return EmployeeSchedule;
            }
        }

        public static Employees FindEmployee(string EmployeeId)
        {
            Employees emp = new Employees();
            DataTable employeeInfo = new DataTable();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "select * from dbo.Employees where EmployeeCode = '" + EmployeeId + "'";
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(employeeInfo);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
            if (employeeInfo.Rows.Count == 0)
            {
                emp.id = "0";
            }
            else
            {
                emp.firstName = employeeInfo.Rows[0][2].ToString();
                emp.lastName = employeeInfo.Rows[0][3].ToString();
                emp.id = employeeInfo.Rows[0][1].ToString();
                emp.storeCode = employeeInfo.Rows[0][5].ToString();
                emp.role = employeeInfo.Rows[0][7].ToString();
                emp.rank = Convert.ToInt16(employeeInfo.Rows[0][6]);
            }
            return emp;
        }

        public static void AddBorrowedEmployee(string EmployeeId, string BorrowedLocationCode, DateTime[] NextFourWeeksDates)
        {
            DataTable employeeInfo = new DataTable();
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "select * from dbo.Employees where EmployeeCode = '" + EmployeeId + "'";
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(employeeInfo);

                    string EmployeeCode = employeeInfo.Rows[0][1].ToString();
                    string LocationCode = employeeInfo.Rows[0][5].ToString();

                    //check if the employee is already in the borrowed table
                    //if they are - set their flag to active for the store
                    //if they aren't - create them and set their flag to active for the store
                    DataTable EmployeeCheck = new DataTable();
                    cmd.CommandText = "select * from dbo.BorrowedEmployees where EmployeeCode = '" + EmployeeId + "' " +
                                      "and BorrowedLocationCode = '" + BorrowedLocationCode + "'";
                    da.Fill(EmployeeCheck);

                    if (EmployeeCheck.Rows.Count != 0)
                    {
                        if (EmployeeCheck.Rows[0][3].ToString() == "False")
                        {
                            cmd.CommandText = "update dbo.BorrowedEmployees set Active = '1' where EmployeeCode = '" + EmployeeId + "' " +
                                          "and BorrowedLocationCode = '" + BorrowedLocationCode + "'";
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        cmd.CommandText = "insert into dbo.BorrowedEmployees (EmployeeCode, LocationCode, BorrowedLocationCode, Active) " +
                                          "values ('" + EmployeeCode + "','" + LocationCode + "','" + BorrowedLocationCode + "','1')";
                        cmd.ExecuteNonQuery();
                    }

                    //create schedule slots for the added employee if they don't exist already
                    DataTable AddedEmployeeScheduleForStore = new DataTable();
                    for (int i = 0; i < 28; i += 7)
                    {
                        cmd.CommandText = "select EmployeeId from dbo.EmployeeSchedule where EmployeeId = '" + EmployeeId + "' " +
                                          "and LocationCode = '" + BorrowedLocationCode + "' and ScheduleDate = '" + NextFourWeeksDates[i] + "'";
                        da.Fill(AddedEmployeeScheduleForStore);

                        //if the first day of the week does not exist, create the week in the schedule table 
                        if (AddedEmployeeScheduleForStore.Rows.Count == 0)
                        {
                            for (int n = 0; n < 7; n++)
                            {
                                cmd.CommandText = "insert into dbo.EmployeeSchedule(EmployeeId, LocationCode, ScheduleDate, BeginTime, EndTime, OnSchedule)" +
                                                  "values('" + EmployeeId + "', '" + BorrowedLocationCode + "', '" + NextFourWeeksDates[i + n] + "', " +
                                                  "'00:00:00', '00:00:00', '0')";
                                cmd.ExecuteNonQuery();
                            }
                        }
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
            }
        }

        public static void RemoveBorrowedEmployee(string EmployeeId, string BorrowedLocationCode)
        {
            DataTable employeeInfo = new DataTable();
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "update dbo.BorrowedEmployees set Active = '0' where EmployeeCode = '" + EmployeeId + "' " +
                                      "and BorrowedLocationCode = '" + BorrowedLocationCode + "'";
                    cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }


        /// <summary>
        /// Create a list of employees for a store
        /// </summary>
        /// <returns></returns>
        public static List<Employees> GetAllEmployees()
        {
            //if you hit this breakpoint you need to kill this

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

                    cmd.CommandText = "select EmployeeCode, PrimaryLocationCode, FirstName, LastName, POSSecurityDescription, " +
                                      "POSSecurityLevel from Employees where POSSecurityLevel <> '0'  order by POSSecurityLevel";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(EmployeeListSQL);

                    Dictionary<int, int> EmployeeHours = new Dictionary<int, int>();
                    EmployeeHours.Add(10, 28);
                    EmployeeHours.Add(20, 28);
                    EmployeeHours.Add(30, 26);
                    EmployeeHours.Add(40, 40);
                    EmployeeHours.Add(50, 40);
                    EmployeeHours.Add(60, 45);
                    for (int i = 0; i < EmployeeListSQL.Rows.Count; i++)
                    {
                        if (Convert.ToInt32(EmployeeListSQL.Rows[i][5]) < 70)
                        {
                            employees.Add(new Employees()
                            {
                                id = Convert.ToString(EmployeeListSQL.Rows[i][0]),
                                storeCode = Convert.ToString(EmployeeListSQL.Rows[i][1]),
                                firstName = Convert.ToString(EmployeeListSQL.Rows[i][2]),
                                lastName = Convert.ToString(EmployeeListSQL.Rows[i][3]),
                                role = Convert.ToString(EmployeeListSQL.Rows[i][4]),
                                rank = Convert.ToInt32(EmployeeListSQL.Rows[i][5]),
                                hours = EmployeeHours[Convert.ToInt32(EmployeeListSQL.Rows[i][5])]
                            });
                        }
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

        public static DataTable GetStoreTrafficData(DateTime CurrentWeekMarker, List<DateTime> TrafficDates, string LocationCode, int[] Weights, int HistoricalWeeks = 6)
        {
            DataTable TrafficData = new DataTable();
            Dictionary<DateTime, double> TrafficWeighting = new Dictionary<DateTime, double>();
            int WeightCount = 0;
            int WeightIterator = 0;
            for (int i = 0; i < TrafficDates.Count; i++)
            {
                if (WeightIterator == 7)
                {
                    WeightIterator = 0;
                    WeightCount++;
                }

                TrafficWeighting[TrafficDates[i]] = Weights[WeightCount];
                WeightIterator++;
            }
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    for (int i = 0; i < TrafficDates.Count; i++)
                    {
                        string DateOnly = TrafficDates[i].ToString("yyyy-MM-dd");
                        cmd.CommandText = "select TrafficDate, TrafficTime, TrafficCount from StoreTraffic where Convert(VARCHAR(10), TrafficDate, 120) = "
                                          + "Convert(Varchar(10), '" + DateOnly + "', 120)" + " and LocationCode = '" + LocationCode + "'";
                        //SELECT* FROM  StoreTraffic WHERE Convert(VARCHAR(10), TrafficDate, 120) = Convert(Varchar(10), '2017-11-04', 120)

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(TrafficData);
                    }

                    for (int i = 0; i < TrafficData.Rows.Count; i++)
                    {
                        string hour = TrafficData.Rows[i][1].ToString().Substring(0, 2);
                        string minutes = TrafficData.Rows[i][1].ToString().Substring(2, 2);
                        string TrafficTime = hour + ":" + minutes + ":00";

                        TrafficData.Rows[i][1] = TrafficTime;
                    }

                    DataTable HalfHourData = TrafficData.Clone();
                    int rowCount = 0;
                    for (int i = 0; i < TrafficData.Rows.Count; i += 2)
                    {
                        DataRow TrafficRow = HalfHourData.NewRow();
                        TrafficRow[0] = TrafficData.Rows[i][0];
                        TrafficRow[1] = TrafficData.Rows[i][1];
                        double data1 = Convert.ToInt32(TrafficData.Rows[i][2]);
                        double data2 = Convert.ToInt32(TrafficData.Rows[i + 1][2]);
                        data1 = data1 * (TrafficWeighting[Convert.ToDateTime(TrafficRow[0])] / 100);
                        data2 = data2 * (TrafficWeighting[Convert.ToDateTime(TrafficRow[0])] / 100);

                        TrafficRow[2] = data1 + data2;

                        HalfHourData.Rows.InsertAt(TrafficRow, rowCount);
                        rowCount++;
                    }
                    TrafficData = HalfHourData;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }

            return TrafficData;
        }

        public static List<Employees> GetEmployeesForStore(string storeCode)
        {
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

                    cmd.CommandText = "select EmployeeCode, PrimaryLocationCode, FirstName, LastName, POSSecurityDescription, " +
                                      "POSSecurityLevel from Employees where POSSecurityLevel <> '0' and PrimaryLocationCode = '" +
                                      storeCode + "' order by POSSecurityLevel";
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(EmployeeListSQL);

                    //check for borrowed employees
                    DataTable BorrowedEmployeeIds = new DataTable();
                    cmd.CommandText = "select EmployeeCode from BorrowedEmployees where BorrowedLocationCode = '" + storeCode + "' " +
                                      " and Active = '1'";
                    da.Fill(BorrowedEmployeeIds);


                    for (int i = 0; i < BorrowedEmployeeIds.Rows.Count; i++)
                    {
                        cmd.CommandText = "select EmployeeCode, PrimaryLocationCode, FirstName, LastName, POSSecurityDescription, " +
                                          "POSSecurityLevel from Employees where POSSecurityLevel <> '0' and EmployeeCode = '" +
                                          BorrowedEmployeeIds.Rows[i][0].ToString() + "' order by POSSecurityLevel";
                        da.Fill(EmployeeListSQL);
                    }


                    Dictionary<int, int> EmployeeHours = new Dictionary<int, int>();
                    EmployeeHours.Add(10, 28);
                    EmployeeHours.Add(20, 28);
                    EmployeeHours.Add(30, 26);
                    EmployeeHours.Add(40, 40);
                    EmployeeHours.Add(50, 40);
                    EmployeeHours.Add(60, 45);
                    for (int i = 0; i < EmployeeListSQL.Rows.Count; i++)
                    {
                        if (Convert.ToInt32(EmployeeListSQL.Rows[i][5]) < 70)
                        {
                            employees.Add(new Employees()
                            {
                                id = Convert.ToString(EmployeeListSQL.Rows[i][0]),
                                storeCode = Convert.ToString(EmployeeListSQL.Rows[i][1]),
                                firstName = Convert.ToString(EmployeeListSQL.Rows[i][2]),
                                lastName = Convert.ToString(EmployeeListSQL.Rows[i][3]),
                                role = Convert.ToString(EmployeeListSQL.Rows[i][4]),
                                rank = Convert.ToInt32(EmployeeListSQL.Rows[i][5]),
                                hours = EmployeeHours[Convert.ToInt32(EmployeeListSQL.Rows[i][5])]
                            });
                        }
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

        public static void CheckForCurrentWeek(List<Employees> employees, DateTime[] CurrentWeekDates)
        {
            DataTable CurrentWeek = new DataTable();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    for (int i = 0; i < employees.Count; i++)
                    {
                        for (int n = 0; n < CurrentWeekDates.Length; n++)
                        {
                            CurrentWeek.Rows.Clear();
                            cmd.CommandText = "select EmployeeId from dbo.EmployeeSchedule where EmployeeId = '" + employees[i].id + "' " +
                                              "and ScheduleDate = '" + CurrentWeekDates[n] + "'";
                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            da.Fill(CurrentWeek);

                            if (CurrentWeek.Rows.Count == 0)
                            {
                                cmd.CommandText = "insert into dbo.EmployeeSchedule(EmployeeId, LocationCode, ScheduleDate, BeginTime, EndTime, OnSchedule) "
                                                + "values('" + employees[i].id + "', '" + employees[i].storeCode + "', '" + CurrentWeekDates[n] + "', '', '', '')";
                                cmd.ExecuteNonQuery();
                            }
                        }
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
            }
        }

        /// <summary>
        /// Submit and employee's time off request to the database
        /// </summary>
        public static void SubmitTimeOffRequest(string created, string startDate, string endDate, string Name, string Id, string LocationCode, string startTime, string endTime, string message)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    //cmd.CommandText = "insert into dbo.TimeOffRequests (Created, DateStart, DateEnd, Name, Id, StartTime, EndTime, Message)" +
                    //                  "values ('" + created + "', '" + startDate + "', '" + endDate + "', '" + Name + "', '" + Id + "', '" + startTime +
                    //                  "', '" + endTime + "', '" + message + "');";

                    cmd.CommandText = "insert into dbo.Messages (Created, Type, EmployeeCode, LocationCode, Name, StartDate, EndDate, StartTime, EndTime)" +
                                      " values ('" + created + "', 'TimeOffRequest', '" + Id + "', '" + LocationCode + "', '" + Name + "', '" + startDate + "', '"
                                      + endDate + "', '" + startTime + "', '" + endTime + "')";

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Update the selected employee's availability table in the database
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="employeeId"></param>
        public static void UpdateEmployeeAvailability(AvailabilityViewModel avm, List<string> schedule, string employeeId)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    int counter = 0;
                    int row = 0;

                    foreach (string value in schedule)
                    {
                        int bit = 0;
                        if (value == "True")
                        {
                            bit = 1;
                        }
                        cmd.CommandText = "update dbo.EmployeeAvailability set " + days[counter] + " = '" + bit +
                                          "' where EmployeeId = '" + employeeId + "' and Hour = '" + ScheduleHalfHourSlots[row] + "'";
                        cmd.ExecuteNonQuery();
                        counter++;

                        if (counter == 7)
                        {
                            counter = 0;
                            row++;
                        }
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
            }
        }

        /// <summary>
        /// Update the selected employee's scheduled hours for the week
        /// </summary>
        /// <param name="hourCount"></param>
        /// <param name="employeeHours"></param>
        /// <param name="employeeId"></param>
        public static string[] UpdateEmployeeSchedule(int hourCount, bool[] employeeHours, string employeeId, DateTime[] RequestedDates,
                            Dictionary<string, string[]> EmployeeScheduledTimes, int weekday, string LocationCode)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                conn.Open();
                DateTime date = new DateTime();
                int offset = 0;
                string startTime = "";
                string endTime = "";
                int counter = 0;
                List<string[]> assignedHourBlocks = new List<string[]>();
                //string storeCode = "";
                //foreach (Employees emp in GetEmployeesForStore(LocationCode))
                //{
                //    if (emp.id == employeeId)
                //    {
                //        storeCode = emp.storeCode;
                //        break;
                //    }
                //}

                for (int n = 0; n < ((hourCount) * 7); n++)
                {
                    if (employeeHours[n] == true)
                    {
                        if (n > 287)
                        {
                            date = RequestedDates[6];
                            offset = n - 288;
                        }
                        else if (n > 239)
                        {
                            date = RequestedDates[5];
                            offset = n - 240;
                        }
                        else if (n > 191)
                        {
                            date = RequestedDates[4];
                            offset = n - 192;
                        }
                        else if (n > 143)
                        {
                            date = RequestedDates[3];
                            offset = n - 144;
                        }
                        else if (n > 95)
                        {
                            date = RequestedDates[2];
                            offset = n - 96;
                        }
                        else if (n > 47)
                        {
                            date = RequestedDates[1];
                            offset = n - 48;
                        }
                        else
                        {
                            date = RequestedDates[0];
                            offset = n;
                        }

                        if (counter == 0)
                        {
                            startTime = ScheduleHalfHourSlots[offset];
                            endTime = ScheduleHalfHourSlots[offset];
                        }
                        else
                        {
                            endTime = ScheduleHalfHourSlots[offset];
                        }
                        counter++;
                    }
                }
                List<string> addHours = new List<string>();
                bool check = false;
                for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                {
                    if (startTime == ScheduleHalfHourSlots[n])
                    {
                        check = true;
                    }
                    if (check == true)
                    {
                        addHours.Add(ScheduleHalfHourSlots[n]);
                    }
                    if (endTime == ScheduleHalfHourSlots[n])
                    {
                        check = false;
                    }
                }

                string[] existingHours = EmployeeScheduledTimes[days[weekday]];
                for (int n = 0; n < existingHours.Length; n++)
                {
                    addHours.Add(existingHours[n]);
                }
                List<string> appendedHours = addHours.Distinct().ToList();

                string[] unorderedHours = new string[ScheduleHalfHourSlots.Length];
                for (int i = 0; i < appendedHours.Count; i++)
                {
                    for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                    {
                        if (appendedHours[i] == ScheduleHalfHourSlots[n])
                        {
                            unorderedHours[n] = ScheduleHalfHourSlots[n];
                        }
                    }
                }

                string[] orderedHours = unorderedHours.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                // set them in blocks, find the start/end times, submit
                List<string> startTimes = new List<string>();
                List<string> endTimes = new List<string>();
                List<int> hoursForBlock = new List<int>();

                int[] hourPositions = new int[orderedHours.Length];

                for (int i = 0; i < orderedHours.Length; i++)
                {
                    for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                    {
                        if (orderedHours[i] == ScheduleHalfHourSlots[n])
                        {
                            hourPositions[i] = n;
                        }
                    }
                }

                bool blockstarter = false;
                int blockcount = 0;
                for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
                {
                    if (i == hourPositions[blockcount])
                    {
                        blockstarter = true;
                        hoursForBlock.Add(hourPositions[blockcount]);
                        blockcount++;
                    }
                    else
                    {
                        blockstarter = false;
                    }
                    if (blockstarter == false && hoursForBlock.Count > 0 || blockcount == hourPositions.Length)
                    {
                        string[] block = new string[2];
                        block[0] = ScheduleHalfHourSlots[hoursForBlock[0]].ToString();
                        block[1] = ScheduleHalfHourSlots[hoursForBlock[hoursForBlock.Count - 1]].ToString();
                        assignedHourBlocks.Add(block);
                        hoursForBlock.Clear();
                    }
                    if (blockcount == hourPositions.Length)
                    {
                        break;
                    }
                }

                string startHours = "";
                string endHours = "";
                for (int i = 0; i < assignedHourBlocks.Count; i++)
                {
                    string[] hours = assignedHourBlocks[i];
                    if (i == assignedHourBlocks.Count - 1)
                    {
                        startHours += hours[0];
                        endHours += hours[1];
                    }
                    else
                    {
                        startHours += hours[0] + ", ";
                        endHours += hours[1] + ", ";
                    }
                }
                string SQLStartHour;
                string SQLEndHour;

                cmd.CommandText = "delete from dbo.EmployeeSchedule where ScheduleDate = '" + date + "' and EmployeeId = '" + employeeId + "'";
                cmd.ExecuteNonQuery();

                for (int i = 0; i < assignedHourBlocks.Count; i++)
                {
                    string[] block = assignedHourBlocks[i];
                    SQLStartHour = block[0];
                    SQLEndHour = block[1];
                    for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                    {
                        if (ScheduleHalfHourSlots[n] == SQLStartHour)
                        {
                            SQLStartHour = SQLHours[n];
                        }
                        if (n < ScheduleHalfHourSlots.Length - 1 && ScheduleHalfHourSlots[n] == SQLEndHour)
                        {
                            SQLEndHour = SQLHours[n + 1];
                            break;
                        }
                    }

                    TimeSpan BeginTime = Convert.ToDateTime(SQLStartHour).TimeOfDay;
                    TimeSpan EndTime = Convert.ToDateTime(SQLEndHour).TimeOfDay;

                    //cmd.CommandText = "update dbo.EmployeeSchedule set BeginTime = '" + BeginTime + "', EndTime = '" + EndTime + "', "
                    //                + "OnSchedule = 1 where EmployeeId = '" + employeeId + "' and ScheduleDate = '" + date + "'";

                    cmd.CommandText = "insert into dbo.EmployeeSchedule (EmployeeId, LocationCode, ScheduleDate, BeginTime, EndTime, OnSchedule)"
                                    + " values ('" + employeeId + "', '" + LocationCode + "', '" + date + "', '" + BeginTime + "', '"
                                    + EndTime + "', '1')";
                    cmd.ExecuteNonQuery();
                }
                conn.Close();

                return orderedHours;
            }
        }

        /// <summary>
        /// Unassign an employee from the specified times
        /// </summary>
        /// <param name="unassignTimes"></param>
        /// <param name="employeeId"></param>
        /// <param name="selectedDay"></param>
        public static void UnassignEmployee(string unassignTimes, string employeeId, int selectedDay, Dictionary<string,
                                            Dictionary<string, string[]>> EmployeeScheduledTimes, DateTime[] CurrentWeekDates, string LocationCode)
        {
            Dictionary<string, string[]> employeeWeekSchedule = EmployeeScheduledTimes[employeeId];
            string[] employeeAssignedHours = employeeWeekSchedule[days[selectedDay]];
            string[] unassignHours = unassignTimes.Split(',');  // split the single element array into multiple elements

            for (int i = 0; i < employeeAssignedHours.Length; i++)
            {
                for (int h = 0; h < unassignHours.Length; h++)
                {
                    if (employeeAssignedHours[i] == unassignHours[h])
                    {
                        var foos = new List<string>(employeeAssignedHours);
                        foos.RemoveAt(i);
                        employeeAssignedHours = foos.ToArray();
                    }
                }
            }
            List<string[]> assignedBlocks = new List<string[]>();

            List<string> employeeBlock = new List<string>();
            int hourcount = 0;
            bool bl = false;

            if (employeeAssignedHours.Length > 1)
            {
                for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                {
                    if (hourcount == employeeAssignedHours.Length - 1)
                    {
                        employeeBlock.Add(ScheduleHalfHourSlots[n]);
                        string[] hours = employeeBlock.ToArray();

                        List<string> StartEndTimes = new List<string>();
                        StartEndTimes.Add(hours[0]);
                        StartEndTimes.Add(hours[hours.Length - 1]);

                        assignedBlocks.Add(StartEndTimes.ToArray());
                        break;
                    }
                    if (ScheduleHalfHourSlots[n] == employeeAssignedHours[hourcount])
                    {
                        bl = true;
                    }
                    if (bl == true && n < ScheduleHalfHourSlots.Length - 1 &&
                        ScheduleHalfHourSlots[n + 1] != employeeAssignedHours[hourcount + 1])
                    {
                        bl = false;
                        employeeBlock.Add(ScheduleHalfHourSlots[n]);
                        string[] hours = employeeBlock.ToArray();

                        List<string> StartEndTimes = new List<string>();
                        StartEndTimes.Add(hours[0]);
                        StartEndTimes.Add(hours[hours.Length - 1]);

                        assignedBlocks.Add(StartEndTimes.ToArray());
                        employeeBlock.Clear();
                        hourcount++;
                    }
                    if (bl == true)
                    {
                        employeeBlock.Add(ScheduleHalfHourSlots[n]);
                        hourcount++;
                    }
                }
            }
            else if (employeeAssignedHours.Length == 1)
            {
                List<string> StartEndTimes = new List<string>();
                StartEndTimes.Add(employeeAssignedHours[0]);
                StartEndTimes.Add(employeeAssignedHours[0]);
                assignedBlocks.Add(StartEndTimes.ToArray());
            }

            //convert to sql hours
            for (int i = 0; i < assignedBlocks.Count; i++)
            {
                string[] startend = assignedBlocks[i];
                string start = startend[0];
                string end = startend[1];

                for (int n = 0; n < ScheduleHalfHourSlots.Length - 1; n++)
                {
                    if (ScheduleHalfHourSlots[n] == start)
                    {
                        start = SQLHours[n];
                    }
                    if (ScheduleHalfHourSlots[n] == end)
                    {
                        end = SQLHours[n + 1];
                    }
                }
                startend[0] = start;
                startend[1] = end;
                assignedBlocks[i] = startend;
            }

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();


                    cmd.CommandText = "delete from dbo.EmployeeSchedule where ScheduleDate = '" + CurrentWeekDates[selectedDay] + "' and EmployeeId = '" + employeeId + "'";
                    cmd.ExecuteNonQuery();

                    for (int i = 0; i < assignedBlocks.Count; i++)
                    {
                        string[] hourBlock = assignedBlocks[i];

                        TimeSpan BeginTime = Convert.ToDateTime(hourBlock[0]).TimeOfDay;
                        TimeSpan EndTime = Convert.ToDateTime(hourBlock[1]).TimeOfDay;

                        cmd.CommandText = "insert into dbo.EmployeeSchedule (EmployeeId, LocationCode, ScheduleDate, BeginTime, EndTime, OnSchedule)"
                                        + " values ('" + employeeId + "', '" + LocationCode + "', '" + CurrentWeekDates[selectedDay] + "', '" + BeginTime + "', '"
                                        + EndTime + "', '1')";
                        cmd.ExecuteNonQuery();
                    }



                    //cmd.CommandText = "update dbo.EmployeeSchedule set BeginTime = '" + startHours + "', EndTime = '" + endHours + "', "
                    //                + "OnSchedule = 1 where EmployeeId = '" + employeeId + "' and ScheduleDate = '" + CurrentWeekDates[selectedDay] + "'";

                    //cmd.ExecuteNonQuery();

                    if (assignedBlocks.Count == 0)
                    {
                        cmd.CommandText = "insert into dbo.EmployeeSchedule (EmployeeId, LocationCode, ScheduleDate, BeginTime, EndTime, OnSchedule)"
                                        + " values ('" + employeeId + "', '" + LocationCode + "', '" + CurrentWeekDates[selectedDay] + "', '00:00:00', '00:00:00', '0')";
                        cmd.ExecuteNonQuery();
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
            }
        }

        public static void AddNewWeekDates(DateTime[] CurrentWeekDates)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                conn.Open();

                SqlCommand dateCheck = new SqlCommand();
                dateCheck.CommandType = CommandType.Text;
                dateCheck.Connection = conn;
                DataTable dateCheckTable = new DataTable();

                dateCheck.CommandText = "select ScheduleDate from dbo.EmployeeSchedule where ScheduleDate = '" + CurrentWeekDates[0] + "'";
                SqlDataAdapter dateCheckFill = new SqlDataAdapter(dateCheck);
                dateCheckFill.Fill(dateCheckTable);

                if (dateCheckTable.Rows.Count == 0)
                {
                    List<Employees> employees = GetAllEmployees();

                    foreach (Employees emp in employees)
                    {
                        for (int i = 0; i < CurrentWeekDates.Length; i++)
                        {
                            cmd.CommandText = "insert into dbo.EmployeeSchedule (EmployeeId, LocationCode, ScheduleDate, BeginTime, EndTime, OnSchedule)"
                                            + " values ('" + emp.id + "', '" + emp.storeCode + "', '" + CurrentWeekDates[i]
                                            + "', '', '', '0')";

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fills the initial table with source data
        /// </summary>
        /// <param name="weights"></param>
        /// <param name="weekMarker"></param>
        /// <returns></returns>
        public static DataTable FillWTGTable(int[] weights, int historicalWeeks, DateTime weekMarker)
        {

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();
                    dt.Clear();

                    for (int i = 0; i < historicalWeeks; i++)
                    {
                        if (i == 0)
                        {
                            cmd.CommandText = "Select FORMAT([Date], 'M/d/yyyy') as Date, [WeekDay], HourofDay,"
                            + "TrafficOut, Round((TrafficOut * " + (weights[i] / 100) + "),2) as WTGTraffic From Backup_LastSixWeeksTraffic "
                            + "Where[Date] < dateadd(WEEK," + (-1 * (weights.Length - 1 - i)).ToString() + ", '" + weekMarker.ToShortDateString() + "')";
                        }
                        else if (i < weights.Length - 1)
                        {
                            cmd.CommandText = "Select FORMAT([Date], 'M/d/yyyy') as Date, [WeekDay], HourofDay,"
                                + "TrafficOut, Round((TrafficOut * " + (weights[i] / 100) + "),2) as WTGTraffic From Backup_LastSixWeeksTraffic "
                                + "Where[Date] < dateadd(WEEK," + (-1 * (weights.Length - 1 - i)).ToString() + ", '" +
                                weekMarker.ToShortDateString() + "') AND[Date] >= dateadd(WEEK, " + (-1 * (weights.Length - i)).ToString() +
                                ", '" + weekMarker.ToShortDateString() + "')";
                        }
                        else
                        {
                            cmd.CommandText = "Select FORMAT([Date], 'M/d/yyyy') as Date, [WeekDay], HourofDay,"
                                + "TrafficOut, Round((TrafficOut * " + (weights[i] / 100) + "),2) as WTGTraffic From Backup_LastSixWeeksTraffic "
                                + "Where[Date] >= dateadd(WEEK," + (-1).ToString() + ", '" + weekMarker.ToShortDateString() + "')";
                        }

                        SqlDataAdapter da = new SqlDataAdapter(cmd);

                        da.Fill(dt);
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
            }
            return dt;
        }

    }
}