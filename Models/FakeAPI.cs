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

        public static string[] ScheduleHourSlots = {"6AM-7AM", "7AM-8AM","8AM-9AM","9AM-10AM","10AM-11AM","11AM-12PM","12PM-1PM", "1PM-2PM", "2PM-3PM",
                                                    "3PM-4PM","4PM-5PM", "5PM-6PM", "6PM-7PM", "7PM-8PM", "8PM-9PM", "9PM-10PM", "10PM-11PM", "11PM-12AM", "12AM-1AM"};

        public static string[] AlternateHourSlots = {"6:00AM", "7:00AM","8:00AM","9:00AM","10:00AM","11:00AM","12:00PM", "1:00PM", "2:00PM", "3:00PM",
                                                     "4:00PM", "5:00PM", "6:00PM", "7:00PM", "8:00PM", "9:00PM", "10:00PM", "11:00PM", "12:00AM"};

        public static string[] ScheduleHalfHourSlots = { "6:00AM", "6:30AM", "7:00AM", "7:30AM","8:00AM", "8:30AM","9:00AM", "9:30AM","10:00AM", "10:30AM",
                                                         "11:00AM", "11:30AM","12:00PM","12:30PM", "1:00PM", "1:30PM", "2:00PM", "2:30PM", "3:00PM", "3:30PM",
                                                         "4:00PM", "4:30PM", "5:00PM", "5:30PM", "6:00PM", "6:30PM", "7:00PM", "7:30PM", "8:00PM", "8:30PM",
                                                         "9:00PM", "9:30PM", "10:00PM", "10:30PM", "11:00PM", "11:30PM", "12:00AM"};

        //public static string[] SQLHours = {"06:00:00", "07:00:00", "08:00:00", "09:00:00", "10:00:00", "11:00:00", "12:00:00", "13:00:00", "14:00:00",
        //                                   "15:00:00", "16:00:00", "17:00:00", "18:00:00", "19:00:00", "20:00:00", "21:00:00", "22:00:00", "23:00:00", "00:00:00"};

        public static string[] SQLHours = {"06:00:00", "06:30:00", "07:00:00", "07:30:00", "08:00:00", "08:30:00", "09:00:00", "09:30:00", "10:00:00", "10:30:00", "11:00:00", "11:30:00", "12:00:00", "12:30:00", "13:00:00", "13:30:00", "14:00:00", "14:30:00",
                                           "15:00:00", "15:30:00", "16:00:00", "16:30:00", "17:00:00", "17:30:00", "18:00:00", "18:30:00", "19:00:00", "19:30:00", "20:00:00", "20:30:00", "21:00:00", "21:30:00", "22:00:00", "22:30:00", "23:00:00", "23:30:00", "00:00:00"};

        public static string[] days = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        public static string strSQLCon = @"Data Source=AFORTINE\SQLEXPRESS;Initial Catalog=LaborNeedsScheduling;Integrated Security=True; MultipleActiveResultSets=True;";

        public static void dothething()
        {
            //List<Employees> employees = CreateEmployees();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    List<Employees> employees = GetAllEmployees();
                    DataTable EmployeeListSQL = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();
                    foreach (Employees emp in employees)
                    {
                        for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
                        {
                            cmd.CommandText = "insert into dbo.EmployeeAvailability(EmployeeId, Hour, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday)"
                                            + "values('" + emp.id + "', '" + ScheduleHalfHourSlots[i] + "', '0', '0', '0', '0', '0', '0', '0')";
                            cmd.ExecuteNonQuery();
                        }
                    }



                    //foreach (Employees emp in employees)
                    //{
                    //    for (int i = 0; i < CurrentWeekDates.Length; i++)
                    //    {

                    //        cmd.CommandText = "insert into dbo.EmployeeSchedule(EmployeeId, StoreCode, ScheduleDate, BeginTime, EndTime, OnSchedule)"
                    //                        + " values('" + emp.id + "', '" + emp.storeCode + "', '" + CurrentWeekDates[i] + "', '', '', '0')";
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

        public static int GetMinEmployees(string StoreCode)
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

                    cmd.CommandText = "select MinEmployees from dbo.StoreVariables where StoreCode = '" + StoreCode + "'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(min);

                    MinEmployees =Convert.ToInt32(min.Rows[0][0]);
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

        public static void UpdateMinEmployees(int MinEmployees, string StoreCode)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "update dbo.StoreVariables set MinEmployees = '" + MinEmployees + "' where StoreCode = '" + StoreCode + "'";

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

        public static int GetMaxEmployees(string StoreCode)
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

                    cmd.CommandText = "select MaxEmployees from dbo.StoreVariables where StoreCode = '" + StoreCode + "'";

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

        public static void UpdateMaxEmployees(int MaxEmployees, string StoreCode)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "update dbo.StoreVariables set MaxEmployees = '" + MaxEmployees + "' where StoreCode = '" + StoreCode + "'";

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

        public static int GetWeekdayPowerHours(string StoreCode)
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

                    cmd.CommandText = "select WeekdayPowerHours from dbo.StoreVariables where StoreCode = '" + StoreCode + "'";

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

        public static void UpdateWeekdayPowerHours(int WeekdayPowerHours, string StoreCode)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "update dbo.StoreVariables set WeekdayPowerHours = '" + WeekdayPowerHours + "' where StoreCode = '" + StoreCode + "'";

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

        public static int GetWeekendPowerHours(string StoreCode)
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

                    cmd.CommandText = "select WeekendPowerHours from dbo.StoreVariables where StoreCode = '" + StoreCode + "'";

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

        public static void UpdateWeekendPowerHours(int WeekendPowerHours, string StoreCode)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "update dbo.StoreVariables set WeekendPowerHours = '" + WeekendPowerHours + "' where StoreCode = '" + StoreCode + "'";

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

        public static DataTable GetStoreHours(string StoreCode)
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
                                      "from dbo.StoreVariables where StoreCode = '" + StoreCode + "'";
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(OpenTimes);

                    cmd.CommandText = "select SundayClose, MondayClose, TuesdayClose, WednesdayClose, ThursdayClose, FridayClose, SaturdayClose " +
                                      "from dbo.StoreVariables where StoreCode = '" + StoreCode + "'";
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

        public static void UpdateStoreHours(string StoreCode, string[] OpenHours, string[] CloseHours)
        {
            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    for(int i = 0; i < 7; i++)
                    {
                        for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                        {
                            if(OpenHours[i] == ScheduleHalfHourSlots[n])
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
                                      "where StoreCode = '" + StoreCode + "'";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "update dbo.StoreVariables set SundayClose = '" + CloseHours[0] + "', MondayClose = '" + CloseHours[1] +
                                      "', TuesdayClose = '" + CloseHours[2] + "', WednesdayClose = '" + CloseHours[3] + "', ThursdayClose = '"
                                      + CloseHours[4] + "', FridayClose = '" + CloseHours[5] + "', SaturdayClose = '" + CloseHours[6] + "' " +
                                      "where StoreCode = '" + StoreCode + "'";
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

        public static int[] GetWeekWeighting(int Weeks, string StoreCode)
        {
            List<int> WeekWeights = new List<int>();

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    DataTable weights = new DataTable();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();
                    string[] WeeksBack = { "WeightingOne", "WeightingTwo", "WeightingThree", "WeightingFour", "WeightingFive", "WeightingSix" };
                    for (int i = 0; i < Weeks; i++)
                    {
                        cmd.CommandText = "select" + WeeksBack[i] + "' " +
                                          "from dbo.StoreVariables where StoreCode = '" + StoreCode + "'";
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(weights);
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
                return WeekWeights.ToArray();
        }

        public static void approveRequest(int messageId, string managerId)
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

                    cmd.CommandText = "select EmployeeId, StartDate, EndDate, StartTime, EndTime from dbo.Messages where ID = '" + messageId + "'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(RequestInfo);

                    string EmployeeId = RequestInfo.Rows[0][0].ToString();
                    string StartDate = RequestInfo.Rows[0][1].ToString();
                    string EndDate = RequestInfo.Rows[0][2].ToString();
                    string StartTime = RequestInfo.Rows[0][3].ToString();
                    string EndTime = RequestInfo.Rows[0][4].ToString();
                    bool Approved = true;
                    DateTime Created = DateTime.Now;
                    string ManagerName = ""; //////////////////////////////////

                    foreach (Employees emp in GetAllEmployees())
                    {
                        if (emp.id == managerId)
                        {
                            ManagerName = emp.firstName + " " + emp.lastName;
                        }
                    }
                    //string response = GenerateManagerResponse(managerId, EmployeeId, StartDate, EndDate, StartTime, EndTime, Approved);

                    cmd.CommandText = "insert into dbo.Messages (Created, Type, ManagerId, EmployeeId, Name, StartDate, EndDate, StartTime, EndTime, Approved)"
                                    + " values ('" + Created + "', 'ManagerResponse', '" + managerId + "', '" + EmployeeId + "', '" + ManagerName + "', '" + StartDate + "', '"
                                    + EndDate + "', '" + StartTime + "', '" + EndTime + "', '" + Approved + "')";

                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "delete from dbo.Messages where ID = '" + messageId + "'";

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

        public static void denyRequest(int messageId, string managerId)
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

                    cmd.CommandText = "select EmployeeId, StartDate, EndDate, StartTime, EndTime from dbo.Messages where ID = '" + messageId + "'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(RequestInfo);

                    string EmployeeId = RequestInfo.Rows[0][0].ToString();
                    string StartDate = RequestInfo.Rows[0][1].ToString();
                    string EndDate = RequestInfo.Rows[0][2].ToString();
                    string StartTime = RequestInfo.Rows[0][3].ToString();
                    string EndTime = RequestInfo.Rows[0][4].ToString();
                    bool Approved = false;
                    DateTime Created = DateTime.Now;
                    string ManagerName = ""; //////////////////////////////////

                    foreach (Employees emp in GetAllEmployees())
                    {
                        if (emp.id == managerId)
                        {
                            ManagerName = emp.firstName + " " + emp.lastName;
                        }
                    }
                    //string response = GenerateManagerResponse(managerId, EmployeeId, StartDate, EndDate, StartTime, EndTime, Approved);

                    cmd.CommandText = "insert into dbo.Messages (Created, Type, ManagerId, EmployeeId, Name, StartDate, EndDate, StartTime, EndTime, Approved)"
                                    + " values ('" + Created + "', 'ManagerResponse', '" + managerId + "', '" + EmployeeId + "', '" + ManagerName + "', '" + StartDate + "', '"
                                    + EndDate + "', '" + StartTime + "', '" + EndTime + "', '" + Approved + "')";

                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "delete from dbo.Messages where ID = '" + messageId + "'";

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

                            //cmd.CommandText = "select Hour from dbo.emp_" + employeeIds[i] + "_Availability"
                            //                  + " where " + weekdays[n] + " = 'True'";

                            cmd.CommandText = "select Hour from dbo.EmployeeAvailability where EmployeeId = '" + employeeIds[i] + "'"
                                            + " and " + days[n] + " = '1'";

                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            da.Fill(str);

                            string[] hours = new string[str.Rows.Count];

                            for (int j = 0; j < str.Rows.Count; j++)
                            {
                                hours[j] = Convert.ToString(str.Rows[j][0]);
                            }

                            innerDict[days[n]] = hours;
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
        /// Get employee scheduled times in text for the assignment area
        /// </summary>
        /// <param name="weekdays"></param>
        /// <param name="employeeIds"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string[]>> GetEmployeeScheduledTimes(string[] employeeIds, DateTime[] dates)
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
                                            + " and ScheduleDate = '" + dates[n] + "'";

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

        public static Dictionary<string, Dictionary<DateTime, string[]>> GetEmployeeTimeOff(string[] employeeIds, DateTime[] currentWeekDates)
        {
            Dictionary<string, Dictionary<DateTime, string[]>> EmployeeTimeOff = new Dictionary<string, Dictionary<DateTime, string[]>>();
            string[] weekDates = new string[7];
            for (int i = 0; i < 7; i++)
            {
                weekDates[i] = currentWeekDates[i].ToString();
            }

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    for (int i = 0; i < employeeIds.Length; i++)
                    {
                        Dictionary<DateTime, string[]> innerDict = new Dictionary<DateTime, string[]>();
                        for (int n = 0; n < currentWeekDates.Length; n++)
                        {
                            innerDict.Add(currentWeekDates[n], new string[0]);
                        }
                        DataTable str = new DataTable();

                        cmd.CommandText = "select StartDate, EndDate, StartTime, EndTime from dbo.Messages "
                                        + "where EmployeeId = '" + employeeIds[i] + "'"
                                        + " and Approved = '1'";

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(str);

                        string[] hours = new string[str.Rows.Count];

                        for (int j = 0; j < str.Rows.Count; j++)
                        {
                            DateTime defaultDateTime = new DateTime(1, 1, 0001);
                            string startDateString = str.Rows[j][0].ToString();
                            string endDateString = str.Rows[j][1].ToString();
                            DateTime startDate = Convert.ToDateTime(startDateString);
                            DateTime endDate = new DateTime();
                            if (endDateString != "")
                            {
                                endDate = Convert.ToDateTime(endDateString);
                            }
                            string startTime = Convert.ToString(str.Rows[j][2]);
                            string endTime = Convert.ToString(str.Rows[j][3]);
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
                                    for (int n = 0; n < 7; n++)
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
                                                if (start == true)
                                                {
                                                    offHours.Add(ScheduleHalfHourSlots[h]);
                                                }
                                                if (currentWeekDates[n] == endDate)
                                                {
                                                    offHours.Add(ScheduleHalfHourSlots[h]);
                                                    start = false;
                                                    break;
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
                                    for (int n = 0; n < 7; n++)
                                    {
                                        if (currentWeekDates[n] == startDate)
                                        {
                                            innerDict[startDate] = allTimes;
                                        }
                                    }
                                }
                            }
                            EmployeeTimeOff[employeeIds[i]] = innerDict;
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
        public static List<ManagerNotification> GetMessagesForManager()
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
                    cmd.CommandText = "select ID, Created, EmployeeId, Name, StartDate, EndDate, StartTime, EndTime from dbo.Messages"
                                    + " where Type = 'TimeOffRequest'";

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
        /// <param name="EmployeeID"></param>
        /// <returns></returns>
        public static DataTable GetEmployeeAvailability(string EmployeeID)
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

                    cmd.CommandText = "select Hour, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday"
                                    + " from dbo.EmployeeAvailability where EmployeeId = '" + EmployeeID + "'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(EmployeeSchedule);

                    //for(int i = 0; i < EmployeeSchedule.Rows.Count; i++)
                    //{
                    //    for (int n = 1; n < EmployeeSchedule.Columns.Count; n++)
                    //    {
                    //        if (EmployeeSchedule.Rows[i][n].ToString() == "0")
                    //        {
                    //            EmployeeSchedule.Rows[i][n] = "False";
                    //        }
                    //        else
                    //        {
                    //            EmployeeSchedule.Rows[i][n] = "True";
                    //        }
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

                return EmployeeSchedule;
            }
        }

        /// <summary>
        /// Create a list of employees for a store
        /// </summary>
        /// <returns></returns>
        public static List<Employees> GetAllEmployees()
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

                    //cmd.CommandText = "SELECT [EmployeeID], [FirstName], [Role], [Rank], [Hours] FROM Employees";
                    cmd.CommandText = "select count(*) from Employees";
                    Int32 employeeRowCount = (Int32)cmd.ExecuteScalar();

                    cmd.CommandText = "select [EmployeeID], [StoreCode], [FirstName], [LastName], [Role], [Rank], [Hours] from Employees "
                                    + "order by Rank";
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(EmployeeListSQL);

                    for (int i = 0; i < EmployeeListSQL.Rows.Count; i++)
                    {
                        employees.Add(new Employees()
                        {
                            id = Convert.ToString(EmployeeListSQL.Rows[i][0]),
                            storeCode = Convert.ToString(EmployeeListSQL.Rows[i][1]),
                            firstName = Convert.ToString(EmployeeListSQL.Rows[i][2]),
                            lastName = Convert.ToString(EmployeeListSQL.Rows[i][3]),
                            role = Convert.ToString(EmployeeListSQL.Rows[i][4]),
                            rank = Convert.ToInt32(EmployeeListSQL.Rows[i][5]),
                            hours = Convert.ToInt32(EmployeeListSQL.Rows[i][6]),
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
                            cmd.CommandText = "select * from dbo.EmployeeSchedule where EmployeeId = '" + employees[i].id + "' " +
                                              "and ScheduleDate = '" + CurrentWeekDates[n] + "'";
                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            da.Fill(CurrentWeek);

                            if (CurrentWeek.Rows.Count == 0)
                            {
                                cmd.CommandText = "insert into dbo.EmployeeSchedule(EmployeeId, StoreCode, ScheduleDate, BeginTime, EndTime, OnSchedule) "
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

                    //cmd.CommandText = "SELECT [EmployeeID], [FirstName], [Role], [Rank], [Hours] FROM Employees";
                    cmd.CommandText = "select count(*) from Employees";
                    Int32 employeeRowCount = (Int32)cmd.ExecuteScalar();

                    cmd.CommandText = "select [EmployeeID], [StoreCode], [FirstName], [LastName], [Role], [Rank], [Hours] from Employees "
                                    + "where StoreCode = '" + storeCode + "' order by Rank";
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(EmployeeListSQL);

                    for (int i = 0; i < EmployeeListSQL.Rows.Count; i++)
                    {
                        employees.Add(new Employees()
                        {
                            id = Convert.ToString(EmployeeListSQL.Rows[i][0]),
                            storeCode = Convert.ToString(EmployeeListSQL.Rows[i][1]),
                            firstName = Convert.ToString(EmployeeListSQL.Rows[i][2]),
                            lastName = Convert.ToString(EmployeeListSQL.Rows[i][3]),
                            role = Convert.ToString(EmployeeListSQL.Rows[i][4]),
                            rank = Convert.ToInt32(EmployeeListSQL.Rows[i][5]),
                            hours = Convert.ToInt32(EmployeeListSQL.Rows[i][6]),
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

        /// <summary>
        /// Submit and employee's time off request to the database
        /// </summary>
        public static void SubmitTimeOffRequest(string created, string startDate, string endDate, string Name, string Id, string startTime, string endTime, string message)
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

                    cmd.CommandText = "insert into dbo.Messages (Created, Type, EmployeeId, Name, StartDate, EndDate, StartTime, EndTime)" +
                                      " values ('" + created + "', 'TimeOffRequest', '" + Id + "', '" + Name + "', '" + startDate + "', '"
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
        public static string[] UpdateEmployeeSchedule(int hourCount, bool[] employeeHours, string employeeId, DateTime[] RequestedDates, Dictionary<string, string[]> EmployeeScheduledTimes, int weekday)
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
                string storeCode = "";
                foreach (Employees emp in GetAllEmployees())
                {
                    if (emp.id == employeeId)
                    {
                        storeCode = emp.storeCode;
                        break;
                    }
                }

                for (int n = 0; n < ((hourCount) * 7); n++)
                {
                    if (employeeHours[n] == true)
                    {
                        if (n > 216)
                        {
                            date = RequestedDates[6];
                            offset = n - 217;
                        }
                        else if (n > 180)
                        {
                            date = RequestedDates[5];
                            offset = n - 181;
                        }
                        else if (n > 144)
                        {
                            date = RequestedDates[4];
                            offset = n - 145;
                        }
                        else if (n > 108)
                        {
                            date = RequestedDates[3];
                            offset = n - 109;
                        }
                        else if (n > 72)
                        {
                            date = RequestedDates[2];
                            offset = n - 73;
                        }
                        else if (n > 36)
                        {
                            date = RequestedDates[1];
                            offset = n - 37;
                        }
                        else
                        {
                            date = RequestedDates[0];
                            offset = n;
                        }
                        //if (n > 107)
                        //{
                        //    date = RequestedDates[6];
                        //    offset = n - 108;
                        //}
                        //else if (n > 89)
                        //{
                        //    date = RequestedDates[5];
                        //    offset = n - 90;
                        //}
                        //else if (n > 71)
                        //{
                        //    date = RequestedDates[4];
                        //    offset = n - 72;
                        //}
                        //else if (n > 53)
                        //{
                        //    date = RequestedDates[3];
                        //    offset = n - 54;
                        //}
                        //else if (n > 35)
                        //{
                        //    date = RequestedDates[2];
                        //    offset = n - 36;
                        //}
                        //else if (n > 17)
                        //{
                        //    date = RequestedDates[1];
                        //    offset = n - 18;
                        //}
                        //else
                        //{
                        //    date = RequestedDates[0];
                        //    offset = n;
                        //}

                        //cmd.CommandText =
                        //"update dbo.emp_" + employeeId
                        //+ " set " + day + " = '" + employeeHours[n] + "' "
                        //+ "where Hour = '" + ScheduleHourSlots[offset] + "'";
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

                    cmd.CommandText = "insert into dbo.EmployeeSchedule (EmployeeId, StoreCode, ScheduleDate, BeginTime, EndTime, OnSchedule)"
                                    + " values ('" + employeeId + "', '" + storeCode + "', '" + date + "', '" + BeginTime + "', '"
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
                                            Dictionary<string, string[]>> EmployeeScheduledTimes, DateTime[] CurrentWeekDates)
        {
            Dictionary<string, string[]> employeeWeekSchedule = EmployeeScheduledTimes[employeeId];
            string[] employeeAssignedHours = employeeWeekSchedule[days[selectedDay]];
            string[] unassignHours = unassignTimes.Split(',');  // split the single element array into multiple elements
            string storeCode = "";
            foreach (Employees emp in GetAllEmployees())
            {
                if (emp.id == employeeId)
                {
                    storeCode = emp.storeCode;
                    break;
                }
            }

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
            for (int i = 0; i < employeeAssignedHours.Length; i++)
            {
                bool checkHour = true;
                foreach (string[] block in assignedBlocks)
                {
                    string start = block[0];
                    string end = block[1];
                    List<string> blockHours = new List<string>();
                    bool blockCheck = false;

                    for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                    {
                        if (start == ScheduleHalfHourSlots[n])
                        {
                            blockCheck = true;
                        }
                        if (blockCheck == true)
                        {
                            blockHours.Add(ScheduleHalfHourSlots[n]);
                        }
                        if (end == ScheduleHalfHourSlots[n])
                        {
                            blockCheck = false;
                        }
                    }
                    for (int n = 0; n < blockHours.Count; n++)
                    {
                        if (employeeAssignedHours[i] == blockHours[n])
                        {
                            checkHour = false;
                        }
                    }
                }

                if (checkHour == true)
                {
                    int blockStart = 0; // the position of the block start hour in the hours array
                    for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                    {
                        if (employeeAssignedHours[i] == ScheduleHalfHourSlots[n])
                        {
                            blockStart = n;
                            break;
                        }
                    }
                    int blockEnd = 0; // the position of the block end hour in the hours array
                    int blockCounter = 1;
                    for (int n = 1; n < employeeAssignedHours.Length; n++)
                    {
                        if (i + blockCounter < employeeAssignedHours.Length)
                        {
                            if (employeeAssignedHours[i + blockCounter] == ScheduleHalfHourSlots[blockStart + blockCounter])
                            {
                                blockEnd = blockStart + n;
                                blockCounter++;
                            }
                            else
                            {
                                blockEnd = blockStart + blockCounter;
                                break;
                            }
                        }
                        else
                        {
                            blockEnd = blockStart + blockCounter;
                            break;
                        }
                        //else if (i+blockCounter == employeeAssignedHours.Length)
                        //{
                        //    blockEnd = blockStart;
                        //}
                    }

                    string[] assignedBlock = new string[2];
                    assignedBlock[0] = ScheduleHalfHourSlots[blockStart];
                    assignedBlock[1] = ScheduleHalfHourSlots[blockEnd+1];

                    assignedBlocks.Add(assignedBlock);
                }
            }

            List<string[]> updatedAssignedHours = new List<string[]>();
            for (int i = 0; i < assignedBlocks.Count; i++)
            {
                string[] block = assignedBlocks[i];
                string start = block[0];
                string end = block[1];
                List<string> blockHours = new List<string>();
                bool blockCheck = false;

                for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                {
                    if (start == ScheduleHalfHourSlots[n])
                    {
                        blockCheck = true;
                    }
                    if (blockCheck == true)
                    {
                        blockHours.Add(ScheduleHalfHourSlots[n]);
                    }
                    if (end == ScheduleHalfHourSlots[n])
                    {
                        blockCheck = false;
                    }
                }

                string[] blockStartEndHours = new string[2];
                for (int n = 0; n < blockHours.Count; n++)
                {
                    if (n == 0)
                    {
                        blockStartEndHours[0] = blockHours[n];
                    }
                    if (n == blockHours.Count - 1)
                    {
                        blockStartEndHours[1] = blockHours[n];
                    }
                }
                updatedAssignedHours.Add(blockStartEndHours);
            }

            List<string[]> updatedBlocks = new List<string[]>();

            string startHours = "";
            string endHours = "";
            for (int i = 0; i < updatedAssignedHours.Count; i++)
            {
                string[] hours = updatedAssignedHours[i];
                if (i == updatedAssignedHours.Count - 1)
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

                    for (int i = 0; i < updatedAssignedHours.Count; i++)
                    {
                        string[] hourBlock = updatedAssignedHours[i];

                        string SQLStartHour = hourBlock[0];
                        string SQLEndHour = hourBlock[1];

                        for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                        {
                            if (ScheduleHalfHourSlots[n] == SQLStartHour)
                            {
                                SQLStartHour = SQLHours[n];
                            }
                            if (ScheduleHalfHourSlots[n] == SQLEndHour)
                            {
                                SQLEndHour = SQLHours[n];
                                break;
                            }
                        }

                        TimeSpan BeginTime = Convert.ToDateTime(SQLStartHour).TimeOfDay;
                        TimeSpan EndTime = Convert.ToDateTime(SQLEndHour).TimeOfDay;

                        cmd.CommandText = "insert into dbo.EmployeeSchedule (EmployeeId, StoreCode, ScheduleDate, BeginTime, EndTime, OnSchedule)"
                                        + " values ('" + employeeId + "', '" + storeCode + "', '" + CurrentWeekDates[selectedDay] + "', '" + BeginTime + "', '"
                                        + EndTime + "', '1')";
                        cmd.ExecuteNonQuery();
                    }



                    cmd.CommandText = "update dbo.EmployeeSchedule set BeginTime = '" + startHours + "', EndTime = '" + endHours + "', "
                                    + "OnSchedule = 1 where EmployeeId = '" + employeeId + "' and ScheduleDate = '" + CurrentWeekDates[selectedDay] + "'";

                    cmd.ExecuteNonQuery();

                    if (startHours.Length == 0 && endHours.Length == 0)
                    {
                        cmd.CommandText = "update dbo.EmployeeSchedule set OnSchedule = 0 where EmployeeId = '"
                                         + employeeId + "' and ScheduleDate = '" + CurrentWeekDates[selectedDay] + "'";

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
                            cmd.CommandText = "insert into dbo.EmployeeSchedule (EmployeeId, StoreCode, ScheduleDate, BeginTime, EndTime, OnSchedule)"
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