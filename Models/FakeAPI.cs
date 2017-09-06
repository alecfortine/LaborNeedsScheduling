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

        public static string[] ScheduleHourSlots = { "6AM-7AM", "7AM-8AM","8AM-9AM","9AM-10AM","10AM-11AM","11AM-12PM","12PM-1PM", "1PM-2PM", "2PM-3PM",
                                       "3PM-4PM","4PM-5PM", "5PM-6PM", "6PM-7PM", "7PM-8PM", "8PM-9PM", "9PM-10PM", "10PM-11PM", "11PM-12AM", "12AM-1AM"};
        public static string[] days = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        public static string strSQLCon = @"Data Source=AFORTINE\SQLEXPRESS;Initial Catalog=LaborNeedsScheduling;Integrated Security=True; MultipleActiveResultSets=True;";

        //public static void dothething()
        //{
        //    //List<Employees> employees = CreateEmployees();

        //    using (SqlConnection conn = new SqlConnection(strSQLCon))
        //    {
        //        try
        //        {
        //            DataTable EmployeeListSQL = new DataTable();
        //            SqlCommand cmd = new SqlCommand();
        //            cmd.CommandType = CommandType.Text;
        //            cmd.Connection = conn;
        //            conn.Open();
        //            //foreach (Employees emp in employees)
        //            //{
        //                for (int i = 0; i < ScheduleHourSlots.Length; i++)
        //                {
        //                    cmd.CommandText = "insert into dbo.EmployeeAvailability(EmployeeId, Hour, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday)"
        //                                    + "values('" + 2122 + "', '" + ScheduleHourSlots[i] + "', '0', '0', '0', '0', '0', '0', '0')";
        //                    cmd.ExecuteNonQuery();
        //                }
        //            //}
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

        public static void approveRequest()
        {
            //store the time somewhere to remove that employee's availability
        }

        public static void denyRequest()
        {

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

                            starts = str.Rows[0][0].ToString();
                            ends = str.Rows[0][1].ToString();

                            string[] startsArray = starts.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] endsArray = ends.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            List<string> assignedHours = new List<string>();

                            for (int h = 0; h < startsArray.Length; h++)
                            {
                                string[] startend = new string[2];
                                startend[0] = startsArray[h];
                                startend[1] = endsArray[h];
                                bool check = false;
                                for (int j = 0; j < ScheduleHourSlots.Length; j++)
                                {
                                    if (startend[0] == ScheduleHourSlots[j])
                                    {
                                        check = true;
                                    }
                                    if (check == true)
                                    {
                                        assignedHours.Add(ScheduleHourSlots[j]);
                                    }
                                    if (startend[1] == ScheduleHourSlots[j])
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
                        EmployeeScheduledTimes[employeeIds[i]] = innerDict;
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

        /// <summary>
        /// Get messages for a specific employee to display on their home page
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public static List<EmployeeNotification> GetEmployeeMessages(string employeeId)
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

                    cmd.CommandText = "select * from dbo.ManagerMessages" +
                                      " where EmployeeId = '" + employeeId + "'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(messages);

                    for (int i = 0; i < messages.Rows.Count; i++)
                    {
                        notifications.Add(new EmployeeNotification()
                        {
                            id = Convert.ToString(messages.Rows[i][0]),
                            message = Convert.ToString(messages.Rows[i][1]),
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
        public static List<ManagerNotification> GetManagerMessages()
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

                    cmd.CommandText = "select Id, Message from dbo.TimeOffRequests";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(requests);

                    for (int i = 0; i < requests.Rows.Count; i++)
                    {
                        notifications.Add(new ManagerNotification()
                        {
                            id = Convert.ToString(requests.Rows[i][0]),
                            message = Convert.ToString(requests.Rows[i][1]),
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
        /// Get an employee's schedule to display on their home page
        /// </summary>
        /// <param name="EmployeeID"></param>
        /// <returns></returns>
        public static DataTable GetEmployeeSchedule(string EmployeeID)
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

            for (int i = 0; i < ScheduleHourSlots.Length; i++)
            {
                DataRow dr = EmployeeSchedule.NewRow();
                dr[0] = ScheduleHourSlots[i];
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

                    cmd.CommandText = "select BeginTime, EndTime from dbo.EmployeeSchedule where EmployeeId = '" + EmployeeID + "'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(StartEndTimes);

                    for (int i = 0; i < StartEndTimes.Rows.Count; i++)
                    {
                        string startTime = StartEndTimes.Rows[i][0].ToString();
                        string endTime = StartEndTimes.Rows[i][1].ToString();

                        int startMark = 0;
                        int endMark = 0;
                        bool startCheck = false;
                        bool endCheck = false;

                        for (int n = 0; n < ScheduleHourSlots.Length; n++)
                        {
                            if (startTime == ScheduleHourSlots[n])
                            {
                                startMark = n;
                                startCheck = true;
                            }
                            if (endTime == ScheduleHourSlots[n])
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

                    cmd.CommandText = "select [EmployeeID], [StoreCode], [FirstName], [Role], [Rank], [Hours] from Employees "
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
                            role = Convert.ToString(EmployeeListSQL.Rows[i][3]),
                            rank = Convert.ToInt32(EmployeeListSQL.Rows[i][4]),
                            hours = Convert.ToInt32(EmployeeListSQL.Rows[i][5]),
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

                    cmd.CommandText = "select [EmployeeID], [StoreCode], [FirstName], [Role], [Rank], [Hours] from Employees "
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
                            role = Convert.ToString(EmployeeListSQL.Rows[i][3]),
                            rank = Convert.ToInt32(EmployeeListSQL.Rows[i][4]),
                            hours = Convert.ToInt32(EmployeeListSQL.Rows[i][5]),
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

            if (endDate == null)
            {
                endDate = "null";
            }

            using (SqlConnection conn = new SqlConnection(strSQLCon))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                    cmd.CommandText = "insert into dbo.TimeOffRequests (Created, DateStart, DateEnd, Name, Id, StartTime, EndTime, Message)" +
                                      "values ('" + created + "', '" + startDate + "', '" + endDate + "', '" + Name + "', '" + Id + "', '" + startTime +
                                      "', '" + endTime + "', '" + message + "');";
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
                                          "' where EmployeeId = '" + employeeId + "' and Hour = '" + ScheduleHourSlots[row] + "'";
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
        public static string[] UpdateEmployeeSchedule(int hourCount, bool[] employeeHours, string employeeId, DateTime[] CurrentWeekDates,
                                                  Dictionary<string, string[]> EmployeeScheduledTimes, int weekday)
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

                for (int n = 0; n < ((hourCount) * 7); n++)
                {
                    if (employeeHours[n] == true)
                    {
                        if (n > 107)
                        {
                            date = CurrentWeekDates[6];
                            offset = n - 108;
                        }
                        else if (n > 89)
                        {
                            date = CurrentWeekDates[5];
                            offset = n - 90;
                        }
                        else if (n > 71)
                        {
                            date = CurrentWeekDates[4];
                            offset = n - 72;
                        }
                        else if (n > 53)
                        {
                            date = CurrentWeekDates[3];
                            offset = n - 54;
                        }
                        else if (n > 35)
                        {
                            date = CurrentWeekDates[2];
                            offset = n - 36;
                        }
                        else if (n > 17)
                        {
                            date = CurrentWeekDates[1];
                            offset = n - 18;
                        }
                        else
                        {
                            date = CurrentWeekDates[0];
                            offset = n;
                        }

                        //cmd.CommandText =
                        //"update dbo.emp_" + employeeId
                        //+ " set " + day + " = '" + employeeHours[n] + "' "
                        //+ "where Hour = '" + ScheduleHourSlots[offset] + "'";
                        if (counter == 0)
                        {
                            startTime = ScheduleHourSlots[offset];
                            endTime = ScheduleHourSlots[offset];
                        }
                        else
                        {
                            endTime = ScheduleHourSlots[offset];
                        }
                        counter++;
                    }
                }
                List<string> addHours = new List<string>();
                bool check = false;
                for (int n = 0; n < ScheduleHourSlots.Length; n++)
                {
                    if (startTime == ScheduleHourSlots[n])
                    {
                        check = true;
                    }
                    if (check == true)
                    {
                        addHours.Add(ScheduleHourSlots[n]);
                    }
                    if (endTime == ScheduleHourSlots[n])
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

                string[] unorderedHours = new string[ScheduleHourSlots.Length];
                for (int i = 0; i < appendedHours.Count; i++)
                {
                    for (int n = 0; n < ScheduleHourSlots.Length; n++)
                    {
                        if (appendedHours[i] == ScheduleHourSlots[n])
                        {
                            unorderedHours[n] = ScheduleHourSlots[n];
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
                    for (int n = 0; n < ScheduleHourSlots.Length; n++)
                    {
                        if (orderedHours[i] == ScheduleHourSlots[n])
                        {
                            hourPositions[i] = n;
                        }
                    }
                }

                bool blockstarter = false;
                int blockcount = 0;
                for (int i = 0; i < ScheduleHourSlots.Length; i++)
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
                        block[0] = ScheduleHourSlots[hoursForBlock[0]].ToString();
                        block[1] = ScheduleHourSlots[hoursForBlock[hoursForBlock.Count - 1]].ToString();
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
                cmd.CommandText = "update dbo.EmployeeSchedule set BeginTime = '" + startHours + "', EndTime = '" + endHours + "', "
                                + "OnSchedule = 1 where EmployeeId = '" + employeeId + "' and ScheduleDate = '" + date + "'";

                cmd.ExecuteNonQuery();

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

                    for (int n = 0; n < ScheduleHourSlots.Length; n++)
                    {
                        if (start == ScheduleHourSlots[n])
                        {
                            blockCheck = true;
                        }
                        if (blockCheck == true)
                        {
                            blockHours.Add(ScheduleHourSlots[n]);
                        }
                        if (end == ScheduleHourSlots[n])
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
                    for (int n = 0; n < ScheduleHourSlots.Length; n++)
                    {
                        if (employeeAssignedHours[i] == ScheduleHourSlots[n])
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
                            if (employeeAssignedHours[i + blockCounter] == ScheduleHourSlots[blockStart + blockCounter])
                            {
                                blockEnd = blockStart + n;
                                blockCounter++;
                            }
                            else
                            {
                                blockEnd = blockStart + blockCounter - 1;
                                break;
                            }
                        }
                        else
                        {
                            blockEnd = blockStart;
                        }
                    }

                    string[] assignedBlock = new string[2];
                    assignedBlock[0] = ScheduleHourSlots[blockStart];
                    assignedBlock[1] = ScheduleHourSlots[blockEnd];

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

                for (int n = 0; n < ScheduleHourSlots.Length; n++)
                {
                    if (start == ScheduleHourSlots[n])
                    {
                        blockCheck = true;
                    }
                    if (blockCheck == true)
                    {
                        blockHours.Add(ScheduleHourSlots[n]);
                    }
                    if (end == ScheduleHourSlots[n])
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
                            cmd.CommandText = "insert into dbo.EmployeeSchedule (ID, EmployeeId, StoreCode, ScheduleDate, BeginTime, EndTime, OnSchedule)"
                                            + " values ('**auto-increment value**', '" + emp.id + "', '" + emp.storeCode + "', '" + CurrentWeekDates[i]
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
        public static DataTable FillWTGTable(double[] weights, int historicalWeeks, DateTime weekMarker)
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