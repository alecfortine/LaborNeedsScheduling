using LSAData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace LaborNeedsScheduling.Models
{
    public class WorkWeek
    {
        #region Variables

        #region Selected Employee Scheduling
        public int selectedWeekday = 3;
        public string selectedEmployee { get; set; }
        public int selectedEmployeeId { get; set; }
        public string startHour { get; set; }
        public string endHour { get; set; }
        public List<Employees> employeeList { get; set; }
        public Dictionary<string, bool[]> employeeHourSchedule { get; set; }
        //LaborScheduling scheduling = new LaborScheduling();
        #endregion

        #region DataTables
        public DataTable TimeSelectionTable = new DataTable();

        public DataTable WTGTrafficPercent = new DataTable();
        public DataTable ExcludedDates = new DataTable();

        public DataTable WeightedAverageTraffic = new DataTable();
        public DataTable WeightedAverageTrafficTotal = new DataTable();

        public DataTable PercentWeeklyTotal = new DataTable();
        public DataTable PercentDailyTotal = new DataTable();
        public DataTable AllocatedHours = new DataTable();
        public DataTable PowerHourForecast = new DataTable();
        public DataTable CurrentWeekHours = new DataTable();
        public DataTable AllocatedHoursDisplay = new DataTable();

        public DataTable AssignmentTable = new DataTable();
        public DataTable AssignmentView = new DataTable();
        #endregion

        /// <summary>
        /// Rows added to tables for times/totals {{need to elminate this}}{{only being used in commented out code}}
        /// </summary>
        public string[] tableRows = { "9AM-10AM","10AM-11AM","11AM-12PM","12PM-1PM", "1PM-2PM", "2PM-3PM", "3PM-4PM",
                                      "4PM-5PM", "5PM-6PM", "6PM-7PM", "7PM-8PM", "8PM-9PM", "9PM-10PM", "10PM-11PM", "Total" };

        /// <summary>
        /// Three letter abbreviations for each day of the week
        /// </summary>
        public string[] weekdayAbv = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

        /// <summary>
        /// Set of possible hours for the schedule, hours shown in each table are based on selections of the ScheduleHours dictionary
        /// </summary>
        public string[] ScheduleHourSlots = { "6AM-7AM", "7AM-8AM","8AM-9AM","9AM-10AM","10AM-11AM","11AM-12PM","12PM-1PM", "1PM-2PM", "2PM-3PM",
                                              "3PM-4PM","4PM-5PM", "5PM-6PM", "6PM-7PM", "7PM-8PM", "8PM-9PM", "9PM-10PM", "10PM-11PM", "11PM-12AM", "12AM-1AM"};

        public string[] replaceRows = {"6:00AM", "7:00AM","8:00AM","9:00AM","10:00AM","11:00AM","12:00PM", "1:00PM", "2:00PM", "3:00PM",
                                       "4:00PM", "5:00PM", "6:00PM", "7:00PM", "8:00PM", "9:00PM", "10:00PM", "11:00PM", "12:00AM"};

        /// <summary>
        /// List of times for dropdown lists and start/end times for the week
        /// </summary>
        public Dictionary<string, string> ScheduleHours = new Dictionary<string, string>()
        {
            {"6AM", "6AM-7AM"},
            {"7AM", "7AM-8AM"},
            {"8AM", "8AM-9AM"},
            {"9AM", "9AM-10AM"},
            {"10AM", "10AM-11AM"},
            {"11AM", "11AM-12PM"},
            {"12PM", "12PM-1PM"},
            {"1PM", "1PM-2PM"},
            {"2PM", "2PM-3PM"},
            {"3PM", "3PM-4PM"},
            {"4PM", "4PM-5PM"},
            {"5PM", "5PM-6PM"},
            {"6PM", "6PM-7PM"},
            {"7PM", "7PM-8PM"},
            {"8PM", "8PM-9PM"},
            {"9PM", "9PM-10PM"},
            {"10PM", "10PM-11PM"},
            {"11PM", "11PM-12AM"},
            {"12AM", "12AM-1AM"},
        };

        /// <summary>
        /// Beginning hours for each day
        /// </summary>
        public string[] WeekStartHours { get; set; }

        /// <summary>
        /// End hours for each day
        /// </summary>
        public string[] WeekEndHours { get; set; }

        public bool[] PowerHourCells = new bool[400];

        public bool[] LowHourCells = new bool[400];


        /// <summary>
        /// The final schedule which determines which hours are generated
        /// </summary>
        public List<string> WeekHourSchedule = new List<string>();

        /// <summary>
        /// Checks whether or not to exclude certain dates from calculations
        /// </summary>
        public Dictionary<DateTime, bool> ExclusionDates { get; set; }

        /// <summary>
        /// Checks wheter or not to exclude certain hours from the work week
        /// </summary>
        public List<bool> HoursOfWeek { get; set; }

        /// <summary>
        /// The final set of weighting values to be passed to the view
        /// </summary>
        public double[] weekWeighting { get; set; }

        /// <summary>
        /// Final schedule to be printed to the view
        /// </summary>
        public DataTable LaborSchedule { get; set; }

        /// <summary>
        /// Schedule of hours to be printed to the view
        /// </summary>
        public DataTable HourSchedule { get; set; }

        /// <summary>
        /// Total hours to be allocated for the week
        /// </summary>
        public double PayrollWeeklyHours { get; set; }

        /// <summary>
        /// Set the minimum number of employees that should be on the floor
        /// </summary>
        public int MinEmployees { get; set; }

        /// <summary>
        /// Set the maximum number of employees that can be on the floor
        /// </summary>
        public int MaxEmployees { get; set; }

        public int MinEmployeesDefault { get; set; }

        public int MaxEmployeesDefault { get; set; }

        /// <summary>
        /// Number of sequential power hours for weekdays
        /// </summary>
        public int WeekdayPowerHours { get; set; }

        /// <summary>
        /// Number of sequential power hours for weekends
        /// </summary>
        public int WeekendPowerHours { get; set; }

        public List<string> powerHoursDisplay = new List<string>();

        public List<string> lowHoursDisplay = new List<string>();

        /// <summary>
        /// Number of weeks ago from the current week
        /// </summary>
        public int WeeksAgo { get; set; }

        /// <summary>
        /// Days of the work week
        /// </summary>
        public List<DayOfWorkWeek> DaysOfWorkWeek { get; set; }

        /// <summary>
        /// The most recent Saturday (the day before the current week)
        /// </summary>
        public static DateTime weekMarker = DateTime.Parse("05/09/2015"); //DateTime.Today.AddDays(-1 * (int)DayOfWeek.Saturday); //change later

        // days of the current week
        public string saturday0 = weekMarker.AddDays(7).ToString("M/d/yyyy");
        public string friday0 = weekMarker.AddDays(6).ToString("M/d/yyyy");
        public string thursday0 = weekMarker.AddDays(5).ToString("M/d/yyyy");
        public string wednesday0 = weekMarker.AddDays(4).ToString("M/d/yyyy");
        public string tuesday0 = weekMarker.AddDays(3).ToString("M/d/yyyy");
        public string monday0 = weekMarker.AddDays(2).ToString("M/d/yyyy");
        public string sunday0 = weekMarker.AddDays(1).ToString("M/d/yyyy");

        public string[] Hours = {
            "8AM-9AM","9AM-10AM","10AM-11AM","11AM-12PM","12PM-1PM","1PM-2PM","2PM-3PM","3PM-4PM","4PM-5PM","5PM-6PM",
            "6PM-7PM","7PM-8PM", "8PM-9PM","9PM-10PM","10PM-11PM"
            };

        /// <summary>
        /// The number of weeks to use for calculations
        /// </summary>
        public int NumberHistoricalWeeks { get; set; }

        public string StartTimeOff { get; set; }

        public string EndTimeOff { get; set; }

        public bool AllDay = false;

        /// <summary>
        /// The default number of weeks back to base calculations on
        /// </summary>
        public int DefaultHistoricalWeeks = 6;

        public WorkWeek()
        {
            ExclusionDates = new Dictionary<DateTime, bool>();

            //HoursOfWeek = new Dictionary<string, bool>();

            LaborSchedule = new DataTable();

            if (NumberHistoricalWeeks == 0)
            {
                NumberHistoricalWeeks = DefaultHistoricalWeeks;

                for (int i = 0; i < DefaultHistoricalWeeks; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        ExclusionDates.Add(weekMarker.AddDays((-7 * (i + 1)) + (j + 1)), false);
                    }
                }
            }
            else
            {
                for (int i = 0; i < NumberHistoricalWeeks; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        ExclusionDates.Add(weekMarker.AddDays((-7 * (i + 1)) + (j + 1)), false);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Runs all the table procedures and fills the table with data
        /// </summary>
        public void FillDatatables()
        {
            try
            {
                //GenerateTimeSelectionTable();

                // weighting input
                for (int i = 0; i < 6; i++)
                {
                    if (weekWeighting[i] == 0)
                    {
                        weekWeighting = LSData.getDefaultWeights(weekWeighting.Length);
                    }
                }

                if (NumberHistoricalWeeks == 0)
                {
                    NumberHistoricalWeeks = DefaultHistoricalWeeks;
                }

                if (MinEmployees == 0)
                {
                    MinEmployees = MinEmployeesDefault;
                }

                if (MaxEmployees == 0)
                {
                    MaxEmployees = MaxEmployeesDefault;
                }

                // procedures
                GenerateWeeklyTimeSlots();

                WTGTrafficPercent = LSData.FillWTGTable(weekWeighting, DateTime.Parse("2015-05-10"));
                FillExcludedDatesTable();
                FillWeightedAverageTrafficTable();
                FillPercentWeeklyTable();
                FillPercentDailyTable();
                FillAllocatedHoursTable();
                //FillCurrentWeekHoursTable();
                FillPowerHourForecastTable();
                GenerateAllocatedHoursDisplay();
                GeneratePowerHourCells();
                FillAssignmentTable(selectedWeekday);

                UpdateEmployees();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Generate future tables based on time selection inputs
        /// </summary>
        public void GenerateWeeklyTimeSlots()
        {
            int counter = 0;
            bool startCheck = false;

            for (int i = 0; i < 1; i++)
            {
                string start = WeekStartHours[i];
                string end = WeekEndHours[i];

                for (int j = 0; j < ScheduleHourSlots.Length; j++)
                {
                    if (ScheduleHourSlots[j] == end)
                    {
                        break;
                    }

                    if (ScheduleHourSlots[j] == start)
                    {
                        startCheck = true;
                    }

                    if (startCheck == true)
                    {
                        //WeekHourSchedule[counter] = ScheduleHourSlots[j];
                        WeekHourSchedule.Add(ScheduleHourSlots[j]);
                        counter++;
                    }
                }
            }
        }

        /// <summary>
        /// Generate an array used by the view to check which cells are power hours
        /// </summary>
        public void GeneratePowerHourCells()
        {
            int y = 0;
            bool[] dayCheck = new bool[8];

            // change to be based on input for how many should be in a block
            int powerHourAmount = 3;
            int powerHourAmountWeekend = 4;

            // set power hour bools
            for (int i = 0; i < AllocatedHoursDisplay.Rows.Count; i++)
            {
                for (int n = 0; n < 8; n++)
                {
                    if (n > 0)
                    {
                        // check if the first column of the cell matches the time string in powerHourRows
                        if (Convert.ToString(AllocatedHoursDisplay.Rows[i][0]) == HourOfWorkDay.powerHourRows[n - 1])
                        {
                            if (dayCheck[n] == false)
                            {
                                PowerHourCells[y] = true;
                                dayCheck[n] = true;

                                if (n != 1 && n != 7)
                                {
                                    for (int j = 1; j < powerHourAmount; j++)
                                    {
                                        PowerHourCells[y + (j * 8)] = true;
                                    }
                                }
                                else
                                {
                                    for (int j = 1; j < powerHourAmountWeekend; j++)
                                    {
                                        PowerHourCells[y + (j * 8)] = true;
                                    }
                                }
                            }
                            y++;
                        }
                        else
                        {
                            //PowerHourCells[y] = false;
                            y++;
                        }
                    }
                    else
                    {
                        // first column always false
                        PowerHourCells[y] = false;
                        y++;
                    }
                }
            }

            int[] columnCount = new int[21];
            for (int i = 0; i < columnCount.Length; i++)
            {
                if (i == 0 || i == 1 || i == 2) columnCount[i] = 1;
                if (i == 3 || i == 4 || i == 5) columnCount[i] = 2;
                if (i == 6 || i == 7 || i == 8) columnCount[i] = 3;
                if (i == 9 || i == 10 || i == 11) columnCount[i] = 4;
                if (i == 12 || i == 13 || i == 14) columnCount[i] = 5;
                if (i == 15 || i == 16 || i == 17) columnCount[i] = 6;
                if (i == 18 || i == 19 || i == 20) columnCount[i] = 7;
            }

            y = 0;

            // set low hour bools
            for (int i = 0; i < AllocatedHoursDisplay.Rows.Count; i++)
            {
                for (int n = 0; n < 8; n++)
                {
                    if (n > 0)
                    {
                        for (int h = 0; h < columnCount.Length; h++)
                        {
                            if (n == columnCount[h] && Convert.ToString(AllocatedHoursDisplay.Rows[i][0]) == HourOfWorkDay.lowHourRows[h])
                            {
                                LowHourCells[y] = true;
                            }
                        }
                        y++;
                    }
                    else
                    {
                        // first column always false
                        LowHourCells[y] = false;
                        y++;
                    }
                }
            }
        }

        //public DataTable GenerateTimeSelectionTable()
        //{
        //    TimeSelectionTable.Columns.Add("HourOfDay");
        //    TimeSelectionTable.Columns.Add("Sunday");
        //    TimeSelectionTable.Columns.Add("Monday");
        //    TimeSelectionTable.Columns.Add("Tuesday");
        //    TimeSelectionTable.Columns.Add("Wednesday");
        //    TimeSelectionTable.Columns.Add("Thursday");
        //    TimeSelectionTable.Columns.Add("Friday");
        //    TimeSelectionTable.Columns.Add("Saturday");

        //    for (int i = 0; i < tableRows.Length - 1; i++)
        //    {
        //        TimeSelectionTable.Rows.Add(tableRows[i]);
        //    }

        //    for (int i = 1; i < 7; i++)
        //    {
        //        for (int n = 0; n < 14; n++)
        //        {
        //            TimeSelectionTable.Rows[n][i] = "";
        //        }
        //    }
        //    return TimeSelectionTable;
        //}

        /// <summary>
        /// Uses the data from the WTGTrafficPercent table and excludes the dates specified by the manager. 
        /// These dates will be ignored by calculations so they do not affect any weighting.
        /// </summary>
        private void FillExcludedDatesTable()
        {
            try
            {
                ExcludedDates = WTGTrafficPercent.Clone();

                #region Exclusion Check
                foreach (DataRow row in WTGTrafficPercent.Rows)
                {
                    // if the date in the dictionary is true don't add it to the table
                    if (!ExclusionDates[DateTime.Parse(row["Date"].ToString())])
                    {
                        ExcludedDates.Rows.Add(row.ItemArray);
                    }
                    else
                    {
                        Debug.WriteLine(row["Date"].ToString());
                    }

                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Calculates the weight each hour should have based on past weeks of data.
        /// </summary>
        /// <returns></returns>
        public void FillWeightedAverageTrafficTable()
        {
            #region Table Setup
            WeightedAverageTraffic.Columns.Add("HourOfDay", typeof(string));
            WeightedAverageTraffic.Columns.Add("SunWeightedAverage", typeof(double));
            WeightedAverageTraffic.Columns.Add("MonWeightedAverage", typeof(double));
            WeightedAverageTraffic.Columns.Add("TueWeightedAverage", typeof(double));
            WeightedAverageTraffic.Columns.Add("WedWeightedAverage", typeof(double));
            WeightedAverageTraffic.Columns.Add("ThuWeightedAverage", typeof(double));
            WeightedAverageTraffic.Columns.Add("FriWeightedAverage", typeof(double));
            WeightedAverageTraffic.Columns.Add("SatWeightedAverage", typeof(double));
            WeightedAverageTraffic.Columns.Add("Total", typeof(double));


            for (int i = 0; i < WeekHourSchedule.Count; i++)
            {
                WeightedAverageTraffic.Rows.Add(WeekHourSchedule[i]);
            }
            WeightedAverageTraffic.Rows.Add("Total");
            #endregion

            try
            {

                #region Calculations

                double total = 0;
                int col = 1;
                int row = 0;

                //string[] hours = { "9AM-10AM","10AM-11AM","11AM-12PM","12PM-1PM", "1PM-2PM", "2PM-3PM", "3PM-4PM",
                //                   "4PM-5PM", "5PM-6PM", "6PM-7PM", "7PM-8PM", "8PM-9PM", "9PM-10PM", "10PM-11PM" };

                for (int i = 0; i < 7; i++)
                {
                    DataRow[] datarow;

                    for (int n = 0; n <= WeekHourSchedule.Count - 1; n++)
                    {
                        datarow = ExcludedDates.Select("WeekDay = '" + weekdayAbv[col - 1] + "' AND HourOfDay = '" + WeekHourSchedule[n] + "'");

                        if (datarow.Length == 0)
                        {
                            WeightedAverageTraffic.Rows[row][col] = 0;
                            row++;
                        }

                        for (int j = 0; j < datarow.Length; j++)
                        {
                            if (j < datarow.Length - 1)
                            {
                                total += Convert.ToDouble(datarow[j]["WTGTraffic"]);
                            }
                            else
                            {
                                total += Convert.ToDouble(datarow[j]["WTGTraffic"]);
                                Math.Round(total, MidpointRounding.AwayFromZero);
                                WeightedAverageTraffic.Rows[row][col] = (Math.Round(total, MidpointRounding.AwayFromZero));
                                total = 0;
                                row++;
                            }
                        }
                    }

                    row = 0;
                    col++;
                }

                #region Totals
                int totalCol = 1;
                double[] daySums = new double[7];

                for (int i = 0; i < 7; i++)
                {
                    daySums[i] = (double)WeightedAverageTraffic.Compute("Sum(" + weekdayAbv[i] + "WeightedAverage)", "");
                    WeightedAverageTraffic.Rows[WeightedAverageTraffic.Rows.Count - 1][totalCol] = daySums[i];
                    totalCol++;
                }

                foreach (DataRow trow in WeightedAverageTraffic.Rows)
                {
                    double rowSum = 0;
                    foreach (DataColumn tcol in WeightedAverageTraffic.Columns)
                    {
                        if (!trow.IsNull(tcol))
                        {
                            string stringValue = trow[tcol].ToString();
                            double d;
                            if (double.TryParse(stringValue, out d))
                                rowSum += d;
                        }
                    }
                    trow.SetField("Total", rowSum);
                }

                #endregion
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }

            #region Total WTG Table
            #region Table Setup

            WeightedAverageTrafficTotal.Columns.Add("HourOfDay", typeof(string));
            WeightedAverageTrafficTotal.Columns.Add("SunWeightedAverage", typeof(double));
            WeightedAverageTrafficTotal.Columns.Add("MonWeightedAverage", typeof(double));
            WeightedAverageTrafficTotal.Columns.Add("TueWeightedAverage", typeof(double));
            WeightedAverageTrafficTotal.Columns.Add("WedWeightedAverage", typeof(double));
            WeightedAverageTrafficTotal.Columns.Add("ThuWeightedAverage", typeof(double));
            WeightedAverageTrafficTotal.Columns.Add("FriWeightedAverage", typeof(double));
            WeightedAverageTrafficTotal.Columns.Add("SatWeightedAverage", typeof(double));
            WeightedAverageTrafficTotal.Columns.Add("Total", typeof(double));


            for (int i = 0; i < ScheduleHourSlots.Length; i++)
            {
                WeightedAverageTrafficTotal.Rows.Add(ScheduleHourSlots[i]);
            }
            WeightedAverageTrafficTotal.Rows.Add("Total");
            #endregion

            try
            {

                #region Calculations

                double total = 0;
                int col = 1;
                int row = 0;

                //string[] hours = { "9AM-10AM","10AM-11AM","11AM-12PM","12PM-1PM", "1PM-2PM", "2PM-3PM", "3PM-4PM",
                //                   "4PM-5PM", "5PM-6PM", "6PM-7PM", "7PM-8PM", "8PM-9PM", "9PM-10PM", "10PM-11PM" };

                for (int i = 0; i < 7; i++)
                {
                    DataRow[] datarow;

                    for (int n = 0; n < ScheduleHourSlots.Length; n++)
                    {
                        datarow = ExcludedDates.Select("WeekDay = '" + weekdayAbv[col - 1] + "' AND HourOfDay = '" + ScheduleHourSlots[n] + "'");

                        if (datarow.Length == 0)
                        {
                            WeightedAverageTrafficTotal.Rows[row][col] = 0;
                            row++;
                        }

                        for (int j = 0; j < datarow.Length; j++)
                        {
                            if (j < datarow.Length - 1)
                            {
                                total += Convert.ToDouble(datarow[j]["WTGTraffic"]);
                            }
                            else
                            {
                                total += Convert.ToDouble(datarow[j]["WTGTraffic"]);
                                Math.Round(total, MidpointRounding.AwayFromZero);
                                WeightedAverageTrafficTotal.Rows[row][col] = (Math.Round(total, MidpointRounding.AwayFromZero));
                                total = 0;
                                row++;
                            }
                        }
                    }

                    row = 0;
                    col++;
                }

                #region Totals
                int totalCol = 1;
                double[] daySums = new double[7];

                for (int i = 0; i < 7; i++)
                {
                    daySums[i] = (double)WeightedAverageTrafficTotal.Compute("Sum(" + weekdayAbv[i] + "WeightedAverage)", "");
                    WeightedAverageTrafficTotal.Rows[WeightedAverageTrafficTotal.Rows.Count - 1][totalCol] = daySums[i];
                    totalCol++;
                }

                foreach (DataRow trow in WeightedAverageTrafficTotal.Rows)
                {
                    double rowSum = 0;
                    foreach (DataColumn tcol in WeightedAverageTrafficTotal.Columns)
                    {
                        if (!trow.IsNull(tcol))
                        {
                            string stringValue = trow[tcol].ToString();
                            double d;
                            if (double.TryParse(stringValue, out d))
                                rowSum += d;
                        }
                    }
                    trow.SetField("Total", rowSum);
                }

                #endregion
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
            #endregion

        }

        /// <summary>
        /// Calculates each hour's percentage of the weekly total based on data from the WeightedAverageTraffic table.
        /// </summary>
        /// <returns></returns>
        public void FillPercentWeeklyTable()
        {
            #region Table Setup
            PercentWeeklyTotal.Columns.Add("HourOfDay", typeof(string));
            PercentWeeklyTotal.Columns.Add("SunPercentOfWeeklyTotal", typeof(double));
            PercentWeeklyTotal.Columns.Add("MonPercentOfWeeklyTotal", typeof(double));
            PercentWeeklyTotal.Columns.Add("TuePercentOfWeeklyTotal", typeof(double));
            PercentWeeklyTotal.Columns.Add("WedPercentOfWeeklyTotal", typeof(double));
            PercentWeeklyTotal.Columns.Add("ThuPercentOfWeeklyTotal", typeof(double));
            PercentWeeklyTotal.Columns.Add("FriPercentOfWeeklyTotal", typeof(double));
            PercentWeeklyTotal.Columns.Add("SatPercentOfWeeklyTotal", typeof(double));
            PercentWeeklyTotal.Columns.Add("Total", typeof(double));


            for (int i = 0; i < ScheduleHourSlots.Length; i++)
            {
                PercentWeeklyTotal.Rows.Add(ScheduleHourSlots[i]);
            }
            PercentWeeklyTotal.Rows.Add("Total");

            #endregion

            try
            {

                #region Calculations
                double weeklyPercent;
                double weeklyTotal = Convert.ToDouble(WeightedAverageTrafficTotal.Rows[WeightedAverageTrafficTotal.Rows.Count - 1]["Total"]);

                for (int col = 1; col < 8; col++)
                {
                    for (int row = 0; row < WeightedAverageTrafficTotal.Rows.Count - 1; row++)
                    {
                        double hourAvg = Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row][col]);

                        weeklyPercent = (hourAvg / weeklyTotal) * 100;
                        PercentWeeklyTotal.Rows[row][col] = Math.Round(weeklyPercent, 1, MidpointRounding.AwayFromZero);
                    }
                }

                #region Totals
                int totalCol = 1;

                double[] sums = new double[7];
                string[] days = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

                for (int i = 0; i < sums.Length; i++)
                {
                    sums[i] = (double)PercentWeeklyTotal.Compute("Sum(" + days[i] + "PercentOfWeeklyTotal)", "");
                    PercentWeeklyTotal.Rows[PercentWeeklyTotal.Rows.Count - 1][totalCol] = Math.Round(sums[i], MidpointRounding.AwayFromZero);
                    totalCol++;
                }

                foreach (DataRow trow in PercentWeeklyTotal.Rows)
                {
                    double rowSum = 0;
                    foreach (DataColumn tcol in PercentWeeklyTotal.Columns)
                    {
                        if (!trow.IsNull(tcol))
                        {
                            string stringValue = trow[tcol].ToString();
                            double d;
                            if (double.TryParse(stringValue, out d))
                                rowSum += d;
                        }
                    }
                    trow.SetField("Total", Math.Round(rowSum, MidpointRounding.AwayFromZero));
                }

                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Calculates each hour's percentage of the daily total based on data from the WeightedAverageTraffic table.
        /// </summary>
        /// <returns></returns>
        public void FillPercentDailyTable()
        {
            #region Table Setup
            PercentDailyTotal.Columns.Add("HourOfDay", typeof(string));
            PercentDailyTotal.Columns.Add("SunPercentOfDailyTotal", typeof(double));
            PercentDailyTotal.Columns.Add("MonPercentOfDailyTotal", typeof(double));
            PercentDailyTotal.Columns.Add("TuePercentOfDailyTotal", typeof(double));
            PercentDailyTotal.Columns.Add("WedPercentOfDailyTotal", typeof(double));
            PercentDailyTotal.Columns.Add("ThuPercentOfDailyTotal", typeof(double));
            PercentDailyTotal.Columns.Add("FriPercentOfDailyTotal", typeof(double));
            PercentDailyTotal.Columns.Add("SatPercentOfDailyTotal", typeof(double));

            for (int i = 0; i < ScheduleHourSlots.Length; i++)
            {
                PercentDailyTotal.Rows.Add(ScheduleHourSlots[i]);
            }
            PercentDailyTotal.Rows.Add("Total");

            #endregion

            try
            {

                #region Calculations
                double dailyPercent = 0;

                for (int col = 1; col < 8; col++)
                {
                    double dailyTotal = Convert.ToDouble(WeightedAverageTrafficTotal.Rows[WeightedAverageTrafficTotal.Rows.Count - 1][col]);

                    for (int row = 0; row < WeightedAverageTrafficTotal.Rows.Count - 1; row++)
                    {
                        double hourAvg = Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row][col]);

                        dailyPercent = (hourAvg / dailyTotal) * 100;
                        PercentDailyTotal.Rows[row][col] = Math.Round(dailyPercent, 1, MidpointRounding.AwayFromZero);
                    }
                }

                #region Totals
                int totalCol = 1;
                double[] sums = new double[7];

                for (int i = 0; i < sums.Length; i++)
                {
                    sums[i] = (double)PercentDailyTotal.Compute("Sum(" + weekdayAbv[i] + "PercentOfDailyTotal)", "");
                    PercentDailyTotal.Rows[PercentDailyTotal.Rows.Count - 1][totalCol] = Math.Round(sums[i], MidpointRounding.AwayFromZero);
                    totalCol++;
                }
                #endregion
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Calculates the amount of employees that should be scheduled for each hour based on input from the manager.
        /// </summary>
        /// <returns></returns>
        public DataTable FillAllocatedHoursTable(/*bool[,] hourChecks*/)
        {
            #region Table Setup
            DateTime[] dates = new DateTime[7];

            DateTime sundayDate = DateTime.Today.AddDays((-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Sunday) + 7);
            DateTime mondayDate = DateTime.Today.AddDays((-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday) + 7);
            DateTime tuesdayDate = DateTime.Today.AddDays((-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Tuesday) + 7);
            DateTime wednesdayDate = DateTime.Today.AddDays((-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Wednesday) + 7);
            DateTime thursdayDate = DateTime.Today.AddDays((-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Thursday) + 7);
            DateTime fridayDate = DateTime.Today.AddDays((-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Friday) + 7);
            DateTime saturdayDate = DateTime.Today.AddDays((-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday) + 7);

            AllocatedHours.Columns.Add("HourOfDay", typeof(string));

            AllocatedHours.Columns.Add("Sunday " + Environment.NewLine + String.Format("{0:M/dd}", sundayDate), typeof(string));
            AllocatedHours.Columns.Add("Monday " + Environment.NewLine + String.Format("{0:M/dd}", mondayDate), typeof(string));
            AllocatedHours.Columns.Add("Tuesday " + Environment.NewLine + String.Format("{0:M/dd}", tuesdayDate), typeof(string));
            AllocatedHours.Columns.Add("Wednesday " + Environment.NewLine + String.Format("{0:M/dd}", wednesdayDate), typeof(string));
            AllocatedHours.Columns.Add("Thursday " + Environment.NewLine + String.Format("{0:M/dd}", thursdayDate), typeof(string));
            AllocatedHours.Columns.Add("Friday " + Environment.NewLine + String.Format("{0:M/dd}", fridayDate), typeof(string));
            AllocatedHours.Columns.Add("Saturday " + Environment.NewLine + String.Format("{0:M/dd}", saturdayDate), typeof(string));

            for (int i = 0; i < ScheduleHourSlots.Length; i++)
            {
                AllocatedHours.Rows.Add(ScheduleHourSlots[i]);
            }

            #endregion

            try
            {

                #region Calculations

                double[] allocatedHours = new double[7];

                for (int i = 0; i < 7; i++)
                {
                    allocatedHours[i] = Math.Round(PayrollWeeklyHours * (Convert.ToDouble(PercentWeeklyTotal.Rows[PercentWeeklyTotal.Rows.Count - 1][weekdayAbv[i] + "PercentOfWeeklyTotal"]) / 100));
                }

                //attemtping 2 dimensional 
                string blank = "";

                for (int col = 1; col < 8; col++)
                {
                    for (int row = 0; row < PercentWeeklyTotal.Rows.Count - 1; row++)
                    {
                        // if the hour is not in the schedule leave it blank
                        //if (hourChecks[col - 1, row] == false)
                        //{

                        double hourPercentage = (Convert.ToDouble(PercentDailyTotal.Rows[row][col]) / 100);
                        double employees = hourPercentage * allocatedHours[col - 1];

                        if (employees < MinEmployees)
                        {
                            employees = MinEmployees;
                        }

                        if (employees > MaxEmployees)
                        {
                            employees = MaxEmployees;
                        }

                        AllocatedHours.Rows[row][col] = Math.Round(employees, 0);
                        //}
                        //else
                        //{

                        //    AllocatedHours.Rows[row][col] = blank;
                        //}
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return AllocatedHours;
        }

        /// <summary>
        /// Calculates the highest trafficked hours for the day based on 3 hour periods. Also calulates the lowest trafficked hours for the day.
        /// </summary>
        /// <returns></returns>
        public void FillPowerHourForecastTable(/*bool[,] hourChecks*/)
        {
            #region Table Setup
            PowerHourForecast.Columns.Add("HourOfDay", typeof(string));
            PowerHourForecast.Columns.Add("SunPowerHours", typeof(double));
            PowerHourForecast.Columns.Add("MonPowerHours", typeof(double));
            PowerHourForecast.Columns.Add("TuePowerHours", typeof(double));
            PowerHourForecast.Columns.Add("WedPowerHours", typeof(double));
            PowerHourForecast.Columns.Add("ThuPowerHours", typeof(double));
            PowerHourForecast.Columns.Add("FriPowerHours", typeof(double));
            PowerHourForecast.Columns.Add("SatPowerHours", typeof(double));

            for (int i = 0; i < ScheduleHourSlots.Length; i++)
            {
                PowerHourForecast.Rows.Add(ScheduleHourSlots[i]);
            }
            #endregion

            try
            {
                #region Calculations
                double forecast;
                double[] dayHighs = new double[7];
                double[] dayLows = new double[21];
                int n = 0;

                for (int col = 1; col < PowerHourForecast.Columns.Count; col++)
                {
                    double[] dayNumbers = new double[20];

                    for (int row = 0; row < PowerHourForecast.Rows.Count; row++)
                    {
                        //if (hourChecks[col - 1, row] == false)
                        //{
                        if (row <= PowerHourForecast.Rows.Count - 3)
                        {
                            double currentHour = Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row][col]);
                            forecast = currentHour + Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row + 1][col]) + Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row + 2][col]);

                            dayNumbers[row] = forecast;
                        }
                        else if (row == PowerHourForecast.Rows.Count - 2)
                        {
                            double currentHour = Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row][col]);
                            forecast = currentHour + Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row + 1][col]);

                            dayNumbers[row] = forecast;
                        }
                        else
                        {
                            double currentHour = Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row][col]);
                            forecast = currentHour;

                            dayNumbers[row] = forecast;
                        }


                        PowerHourForecast.Rows[row][col] = Math.Round(forecast, 0);

                        //}
                        //else
                        //{
                        //    PowerHourForecast.Rows[row][col] = 0;
                        //}
                    }

                    dayHighs[col - 1] = dayNumbers.Max();

                    n += 3;
                    int smallest = 9999;
                    int second = 9999;
                    int third = 9999;

                    foreach (int i in dayNumbers)
                    {
                        if (i < smallest && i != 0 && i != 1)
                        {
                            third = second;
                            second = smallest;
                            smallest = i;
                        }
                        else if (i < second && i != 0 && i != 1)
                        {
                            third = second;
                            second = i;
                        }
                        else if (i < third && i != 0 && i != 1)
                        {
                            third = i;
                        }
                    }
                    dayLows[n - 3] = smallest;
                    dayLows[n - 2] = second;
                    dayLows[n - 1] = third;

                }
                #endregion

                #region Pass highs and lows

                // high hours for each day
                for (int i = 0; i < dayHighs.Length; i++)
                {
                    HourOfWorkDay.powerHours[i] = dayHighs[i];
                }

                // low hours for each day (excluding zeros)
                for (int i = 0; i < dayLows.Length; i++)
                {
                    HourOfWorkDay.lowHours[i] = dayLows[i];
                }

                // get the names of the rows containing power hours
                for (int i = 1; i <= HourOfWorkDay.powerHourRows.Length; i++)
                {
                    for (int j = 0; j < PowerHourForecast.Rows.Count; j++)
                    {
                        if (Convert.ToString(PowerHourForecast.Rows[j][i]) == Convert.ToString(HourOfWorkDay.powerHours[i - 1]))
                        {
                            //HourOfWorkDay.powerHourRows[i - 1] = Convert.ToString(j);
                            HourOfWorkDay.powerHourRows[i - 1] = Convert.ToString(PowerHourForecast.Rows[j][0]);
                        }
                    }
                }

                // get the names of the rows containing low hours
                int lowCount = 0;
                int lowHourIterator = 0;

                for (int i = 1; i <= HourOfWorkDay.powerHourRows.Length; i++)
                {
                    int counter = 0;

                    for (int j = 0; j < PowerHourForecast.Rows.Count; j++)
                    {
                        if (Convert.ToString(PowerHourForecast.Rows[j][i]) == Convert.ToString(HourOfWorkDay.lowHours[lowHourIterator])
                            || Convert.ToString(PowerHourForecast.Rows[j][i]) == Convert.ToString(HourOfWorkDay.lowHours[lowHourIterator + 1])
                            || Convert.ToString(PowerHourForecast.Rows[j][i]) == Convert.ToString(HourOfWorkDay.lowHours[lowHourIterator + 2]))
                        {
                            HourOfWorkDay.lowHourRows[lowCount] = Convert.ToString(PowerHourForecast.Rows[j][0]);
                            counter++;
                            lowCount++;
                        }
                        if (counter == 3)
                        {
                            lowHourIterator += 3;
                            break;
                        }
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public DataTable FillCurrentWeekHoursTable()
        //{
        //    DateTime sundayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Sunday);
        //    DateTime mondayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
        //    DateTime tuesdayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Tuesday);
        //    DateTime wednesdayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Wednesday);
        //    DateTime thursdayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Thursday);
        //    DateTime fridayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Friday);
        //    DateTime saturdayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday);

        //    CurrentWeekHours.Columns.Add("HourOfDay", typeof(string));
        //    CurrentWeekHours.Columns.Add("Sunday " + Environment.NewLine + String.Format("{0:M/dd}", sundayDate), typeof(string));
        //    CurrentWeekHours.Columns.Add("Monday " + Environment.NewLine + String.Format("{0:M/dd}", mondayDate), typeof(string));
        //    CurrentWeekHours.Columns.Add("Tuesday " + Environment.NewLine + String.Format("{0:M/dd}", tuesdayDate), typeof(string));
        //    CurrentWeekHours.Columns.Add("Wednesday " + Environment.NewLine + String.Format("{0:M/dd}", wednesdayDate), typeof(string));
        //    CurrentWeekHours.Columns.Add("Thursday " + Environment.NewLine + String.Format("{0:M/dd}", thursdayDate), typeof(string));
        //    CurrentWeekHours.Columns.Add("Friday " + Environment.NewLine + String.Format("{0:M/dd}", fridayDate), typeof(string));
        //    CurrentWeekHours.Columns.Add("Saturday " + Environment.NewLine + String.Format("{0:M/dd}", saturdayDate), typeof(string));

        //    for (int i = 0; i < tableRows.Length - 1; i++)
        //    {
        //        CurrentWeekHours.Rows.Add(tableRows[i]);
        //    }

        //    for (int col = 1; col < 8; col++)
        //    {
        //        for (int row = 0; row < tableRows.Length - 1; row++)
        //        {
        //            CurrentWeekHours.Rows[row][col] = "";
        //        }
        //    }

        //    return CurrentWeekHours;

        //}

        public DataTable GenerateAllocatedHoursDisplay()
        {

            #region Table Setup
            DateTime[] dates = new DateTime[7];

            DateTime sundayDate = DateTime.Today.AddDays((-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Sunday) + 7);
            DateTime mondayDate = DateTime.Today.AddDays((-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday) + 7);
            DateTime tuesdayDate = DateTime.Today.AddDays((-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Tuesday) + 7);
            DateTime wednesdayDate = DateTime.Today.AddDays((-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Wednesday) + 7);
            DateTime thursdayDate = DateTime.Today.AddDays((-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Thursday) + 7);
            DateTime fridayDate = DateTime.Today.AddDays((-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Friday) + 7);
            DateTime saturdayDate = DateTime.Today.AddDays((-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday) + 7);

            AllocatedHoursDisplay.Columns.Add("HourOfDay", typeof(string));

            AllocatedHoursDisplay.Columns.Add("Sunday " + Environment.NewLine + String.Format("{0:M/dd}", sundayDate), typeof(string));
            AllocatedHoursDisplay.Columns.Add("Monday " + Environment.NewLine + String.Format("{0:M/dd}", mondayDate), typeof(string));
            AllocatedHoursDisplay.Columns.Add("Tuesday " + Environment.NewLine + String.Format("{0:M/dd}", tuesdayDate), typeof(string));
            AllocatedHoursDisplay.Columns.Add("Wednesday " + Environment.NewLine + String.Format("{0:M/dd}", wednesdayDate), typeof(string));
            AllocatedHoursDisplay.Columns.Add("Thursday " + Environment.NewLine + String.Format("{0:M/dd}", thursdayDate), typeof(string));
            AllocatedHoursDisplay.Columns.Add("Friday " + Environment.NewLine + String.Format("{0:M/dd}", fridayDate), typeof(string));
            AllocatedHoursDisplay.Columns.Add("Saturday " + Environment.NewLine + String.Format("{0:M/dd}", saturdayDate), typeof(string));

            for (int i = 0; i < WeekHourSchedule.Count; i++)
            {
                AllocatedHoursDisplay.Rows.Add(WeekHourSchedule[i]);
            }

            #endregion
            int rows = WeekHourSchedule.Count;
            int count = 0;
            int day = 1;
            bool powerCheck;

            //iterating through columns
            for (int i = 0; i < WeekHourSchedule.Count; i++)
            {
                powerCheck = false;
                count = 0;

                //iterating through each cell in the column
                if (i == 0)
                {
                    for (int j = 0; j < AllocatedHours.Rows.Count; j++)
                    {

                        if (AllocatedHours.Rows[j][0] == AllocatedHoursDisplay.Rows[i][0])
                        {
                            AllocatedHoursDisplay.Rows.Remove(AllocatedHoursDisplay.Rows[i]);
                            AllocatedHoursDisplay.ImportRow(AllocatedHours.Rows[j]);

                            //if (count < rows && powerCheck == false)
                            //{
                            //    if (Convert.ToString(j) == HourOfWorkDay.powerHourRows[day])
                            //    {
                            //        powerHoursDisplay.Add(Convert.ToString(j));
                            //        powerCheck = true;
                            //        count = 0;
                            //    }
                            //}

                            //if (count == rows && powerCheck == false)
                            //{
                            //    powerHoursDisplay.Add("-");
                            //    powerCheck = true;
                            //    count = 0;
                            //}
                        }
                        count++;
                    }
                }
            }

            //for (int i = 1; i < 8; i++)
            //{
            //    int counter = 0;

            //    for (int j = 0; j < AllocatedHoursDisplay.Rows.Count; j++)
            //    {
            //        if (Convert.ToString(AllocatedHoursDisplay.Rows[j][0]) == HourOfWorkDay.powerHourRows[i - 1])
            //        {
            //            powerHoursDisplay.Add(Convert.ToString(j));
            //            counter = 0;
            //        }
            //        counter++;
            //        if (counter == AllocatedHoursDisplay.Rows.Count)
            //        {
            //            powerHoursDisplay.Add(Convert.ToString("-"));
            //        }
            //    }
            //}

            return AllocatedHoursDisplay;
        }

        public void FillAssignmentTable(int column)
        {

            AssignmentTable.Columns.Add(" ", typeof(string));
            for (int i = 0; i < WeekHourSchedule.Count; i++)
            {
                AssignmentTable.Columns.Add(WeekHourSchedule[i], typeof(string));
            }

            string[] dateColumns = new string[8];

            for (int i = 0; i < 8; i++)
            {
                DataColumn c = AllocatedHoursDisplay.Columns[i];
                dateColumns[i] = c.Caption;
            }

            for (int j = 0; j < AllocatedHoursDisplay.Columns.Count; j++)
            {
                if (j == 0)
                {
                    for (int n = 1; n < dateColumns.Length; n++)
                    {
                        AssignmentTable.Rows.Add(dateColumns[n]);
                    }
                }
                else
                {
                    for (int h = 0; h < AllocatedHoursDisplay.Rows.Count; h++)
                    {
                        AssignmentTable.Rows[j - 1][h + 1] = AllocatedHoursDisplay.Rows[h][j];
                    }
                }
            }


            AssignmentView.Columns.Add(" ", typeof(string));
            for (int i = 0; i < WeekHourSchedule.Count; i++)
            {
                AssignmentView.Columns.Add(WeekHourSchedule[i], typeof(string));
            }

            var row = AssignmentTable.Rows[column];
            AssignmentView.ImportRow(row);

            DataRow dr = AssignmentView.NewRow();

            for (int i = 0; i <= WeekHourSchedule.Count; i++)
            {
                if (i == 0)
                {
                    AssignmentView.Rows.Add("Still Need");
                }
                else
                // change the display for employees still needed here
                {
                    AssignmentView.Rows[1][i] = AssignmentView.Rows[0][i];
                }
            }

        }

        /// <summary>
        /// Update an employee's schedule information for the week (when they are assigned)
        /// </summary>
        public void UpdateEmployees()
        {
            //LaborScheduling scheduling = new LaborScheduling();

            List<Employees> scheduling = LaborScheduling.EmployeeList;
            //if (employeeHourSchedule == null)
            //{
            //    employeeHourSchedule = new Dictionary<string, bool[]>();
            //}

            //if (employeeHourSchedule.Count == 0)
            //{
            //    // fill it up with default values - employee ids and empty bool arrays
            //    bool[] employeeHours = new bool[ScheduleHours.Keys.Count * 7];

            //    for (int i = 0; i < scheduling.Count; i++)
            //    {
            //        employeeHourSchedule.Add(scheduling[i].id, employeeHours);
            //    }
            //}

            // update the specified employee's array
            // get employee id input as a parameter


                bool[] employeeHours = new bool[ScheduleHours.Keys.Count * 7];
                int weekday = (selectedWeekday * (ScheduleHourSlots.Length - 1));
                int start = 0;
                int end = 0;
                int employeeId = selectedEmployeeId;
                int emp = 0;

                // match the employee ID with the correct one in the list
                for (int n = 0; n < scheduling.Count; n++)
                {
                    if (employeeId == Convert.ToInt32(scheduling[n].id))
                    {
                        emp = Convert.ToInt32(scheduling[n].id);
                    }
                }

                for (int n = 0; n < ScheduleHours.Keys.Count; n++)
                {
                    // check when the selected start time is
                    if (startHour == ScheduleHourSlots[n])
                    {
                        employeeHours[n + weekday] = true;
                        start = n;
                    }

                    // check when the selected end time is
                    if (endHour == ScheduleHourSlots[n])
                    {
                        employeeHours[(n - 1) + weekday] = true;
                        end = (n - 1);
                    }
                }

                for (int n = 0; n < ScheduleHours.Keys.Count; n++)
                {
                    // set everything true between the start and end hours
                    if (n > start && n < end)
                    {
                        employeeHours[n + weekday] = true;
                    }
                }



                string strSQLCon = @"Data Source=AFORTINE\SQLEXPRESS;Initial Catalog=LaborNeedsScheduling;Integrated Security=True; MultipleActiveResultSets=True;";

                using (SqlConnection conn = new SqlConnection(strSQLCon))
                {

                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();

                for (int n = 0; n < ((ScheduleHours.Keys.Count) * 7); n++)
                {
                    if (employeeHours[n] == true)
                    {
                        string day;
                        int offset = 0;

                        if (n > 107)
                        {
                            day = "Saturday";
                            offset = n - 108;
                        }
                        else if (n > 89)
                        {
                            day = "Friday";
                            offset = n - 90;
                        }
                        else if (n > 71)
                        {
                            day = "Thursday";
                            offset = n - 72;
                        }
                        else if (n > 53)
                        {
                            day = "Wednesday";
                            offset = n - 54;
                        }
                        else if (n > 35)
                        {
                            day = "Tuesday";
                            offset = n - 36;
                        }
                        else if (n > 17)
                        {
                            day = "Monday";
                            offset = n - 18;
                        }
                        else
                        {
                            day = "Sunday";
                            offset = n;
                        }

                        cmd.CommandText =
                        "update dbo.emp_" + employeeId
                        + " set " + day + " = '" + employeeHours[n] + "' "
                        + "where Hour = '" + ScheduleHourSlots[offset] + "'";

                        cmd.ExecuteNonQuery();

                        Debug.WriteLine(day + " + " + ScheduleHourSlots[offset]);
                    }
                }

                //if this doesn't end up working then edit the one below to only change if the value is false

                    //int i = 0;

                    //cmd.CommandText =
                    //  "DECLARE @i int = 0 "
                    //+ "WHILE(" + i + " < " + ScheduleHours.Keys.Count + ")"
                    //+ " BEGIN "
                    //    + "update dbo.emp_" + employeeId
                    //    + " set Sunday = '" + employeeHours[i] + "', "
                    //    + "Monday = '" + employeeHours[i + 19] + "', "
                    //    + "Tuesday = '" + employeeHours[i + 38] + "', "
                    //    + "WednesDay = '" + employeeHours[i + 57] + "', "
                    //    + "Thursday = '" + employeeHours[i + 76] + "', "
                    //    + "Friday = '" + employeeHours[i + 95] + "', "
                    //    + "Saturday = '" + employeeHours[i + 114] + "' "
                    //    + "set @i = @i + 1"
                    //+ " END";


                    // add the employee's id and their schedule array to the list
                    //employeeHourSchedule.Add(scheduling[emp].id, employeeHours);
                    employeeHourSchedule[scheduling[emp].id] = employeeHours;

                conn.Close();

            }


            //public void UpdateEmployee(int employeeID)
            //{

            //}
        }
    }

        public class DayOfWorkWeek
        {

            public string Day { get; set; }

            public int Week { get; set; }

            public List<int> HoursOfDay { get; set; }

        }

        public class HourOfWorkDay
        {
            public bool IsPowerHour { get; set; }

            public bool IsLowHour { get; set; }

            public bool IsExcludedHour { get; set; }

            public string DisplayLabel { get; set; }

            public List<Employee> ScheduledEmployees { get; set; }

            public static double[] powerHours = new double[7];

            public static double[] lowHours = new double[21];

            //public bool[,] hourChecks = new bool[7, 15];

            public static string[] powerHourRows = new string[7];

            public static string[] lowHourRows = new string[21];

        }

        public class Employee
        {
            string name { get; set; }

        }
    
}