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
        public DataTable TimeSelectionTable = new DataTable();

        public DataTable WTGTrafficPercent = new DataTable();
        public DataTable ExcludedDates = new DataTable();
        public DataTable WeightedAverageTraffic = new DataTable();
        public DataTable PercentWeeklyTotal = new DataTable();
        public DataTable PercentDailyTotal = new DataTable();
        public DataTable AllocatedHours = new DataTable();
        public DataTable PowerHourForecast = new DataTable();
        public DataTable CurrentWeekHours = new DataTable();


        /// <summary>
        /// Rows added to tables for times/totals
        /// </summary>
        public string[] tableRows = { "9AM-10AM","10AM-11AM","11AM-12PM","12PM-1PM", "1PM-2PM", "2PM-3PM", "3PM-4PM",
                                     "4PM-5PM", "5PM-6PM", "6PM-7PM", "7PM-8PM", "8PM-9PM", "9PM-10PM", "10PM-11PM", "Total" };

        /// <summary>
        /// Three letter abbreviations for each day of the week
        /// </summary>
        public string[] weekdayAbv = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

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
        public double[] weekWeighting = LSData.getDefaultWeights(6);

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
        public double  PayrollWeeklyHours { get; set; }

        /// <summary>
        /// Set the minimum number of employees that should be on the floor
        /// </summary>
        public int MinEmployees { get; set; }

        /// <summary>
        /// Set the maximum number of employees that can be on the floor
        /// </summary>
        public int MaxEmployees { get; set; }

        /// <summary>
        /// Number of sequential power hours for weekdays
        /// </summary>
        public int WeekdayPowerHours { get; set; }

        /// <summary>
        /// Number of sequential power hours for weekends
        /// </summary>
        public int WeekendPowerHours { get; set; }

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

        /// <summary>
        /// Runs all the table procedures and fills the final table with data
        /// </summary>
        public void FillDatatables()
        {
            try
            {

                GenerateTimeSelectionTable();

                // weighting input
                if (weekWeighting[0] == 0 && weekWeighting[1] == 0 && weekWeighting[2] == 0 &&
                    weekWeighting[3] == 0 && weekWeighting[4] == 0 && weekWeighting[5] == 0)
                {
                    weekWeighting = LSData.getDefaultWeights(weekWeighting.Length);
                }

                if (NumberHistoricalWeeks == 0)
                {
                    NumberHistoricalWeeks = DefaultHistoricalWeeks;
                }

                // procedures
                WTGTrafficPercent = LSData.FillWTGTable(weekWeighting, DateTime.Parse("2015-05-10"));

                FillExcludedDatesTable();

                FillWeightedAverageTrafficTable();

                FillPercentWeeklyTable();

                FillPercentDailyTable();

                bool[,] testHours = new bool[7, 14];

                FillAllocatedHoursTable(testHours);

                FillCurrentWeekHoursTable();

                FillPowerHourForecastTable(testHours);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public DataTable GenerateTimeSelectionTable()
        {
            TimeSelectionTable.Columns.Add("HourOfDay");
            TimeSelectionTable.Columns.Add("Sunday");
            TimeSelectionTable.Columns.Add("Monday");
            TimeSelectionTable.Columns.Add("Tuesday");
            TimeSelectionTable.Columns.Add("Wednesday");
            TimeSelectionTable.Columns.Add("Thursday");
            TimeSelectionTable.Columns.Add("Friday");
            TimeSelectionTable.Columns.Add("Saturday");

            for (int i = 0; i < tableRows.Length - 1; i++)
            {
                TimeSelectionTable.Rows.Add(tableRows[i]);
            }
            
            for (int i = 1; i < 7; i++)
            {
                for (int n = 0; n < 14; n++)
                {
                    TimeSelectionTable.Rows[n][i] = "";
                }
            }
            return TimeSelectionTable;
        }

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

            for (int i = 0; i < tableRows.Length; i++)
            {
                WeightedAverageTraffic.Rows.Add(tableRows[i]);
            }
            #endregion

            try
            {

                #region Calculations

                double total = 0;
                int col = 1;
                int row = 0;

                string[] hours = { "9AM-10AM","10AM-11AM","11AM-12PM","12PM-1PM", "1PM-2PM", "2PM-3PM", "3PM-4PM",
                                   "4PM-5PM", "5PM-6PM", "6PM-7PM", "7PM-8PM", "8PM-9PM", "9PM-10PM", "10PM-11PM" };

                for (int i = 0; i < 7; i++)
                {
                    DataRow[] datarow;

                    for (int n = 0; n <= hours.Length - 1; n++)
                    {
                        datarow = ExcludedDates.Select("WeekDay = '" + weekdayAbv[col - 1] + "' AND HourOfDay = '" + hours[n] + "'");

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

            for (int i = 0; i < tableRows.Length; i++)
            {
                PercentWeeklyTotal.Rows.Add(tableRows[i]);
            }
            #endregion

            try
            {

                #region Calculations
                double weeklyPercent = 0;
                double weeklyTotal = Convert.ToDouble(WeightedAverageTraffic.Rows[WeightedAverageTraffic.Rows.Count - 1]["Total"]);

                for (int col = 1; col < 8; col++)
                {
                    for (int row = 0; row < WeightedAverageTraffic.Rows.Count - 1; row++)
                    {
                        double hourAvg = Convert.ToDouble(WeightedAverageTraffic.Rows[row][col]);

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

            for (int i = 0; i < tableRows.Length; i++)
            {
                PercentDailyTotal.Rows.Add(tableRows[i]);
            }
            #endregion

            try
            {

                #region Calculations
                double dailyPercent = 0;

                for (int col = 1; col < 8; col++)
                {
                    double dailyTotal = Convert.ToDouble(WeightedAverageTraffic.Rows[WeightedAverageTraffic.Rows.Count - 1][col]);

                    for (int row = 0; row < WeightedAverageTraffic.Rows.Count - 1; row++)
                    {
                        double hourAvg = Convert.ToDouble(WeightedAverageTraffic.Rows[row][col]);

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
        public DataTable FillAllocatedHoursTable(bool[,] hourChecks)
        {
            // in progress
            #region Table Setup
            DateTime[] dates = new DateTime[7];
            string[] daysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };


            //for (int i = 0; i < 7; i++)
            //{
            //    dates[i] = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.daysOfWeek[i]);
            //}
            DateTime sundayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Sunday);
            DateTime mondayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime tuesdayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Tuesday);
            DateTime wednesdayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Wednesday);
            DateTime thursdayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Thursday);
            DateTime fridayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Friday);
            DateTime saturdayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday);

            AllocatedHours.Columns.Add("HourOfDay", typeof(string));
            //for (int i = 0; i < 7; i++)
            //{
            //    AllocatedHours.Columns.Add(daysOfWeek[i] + " " + Environment.NewLine + String.Format("{0:M/dd}", dates[i]), typeof(string));
            //}
            AllocatedHours.Columns.Add("Sunday " + Environment.NewLine + String.Format("{0:M/dd}", sundayDate), typeof(string));
            AllocatedHours.Columns.Add("Monday " + Environment.NewLine + String.Format("{0:M/dd}", mondayDate), typeof(string));
            AllocatedHours.Columns.Add("Tuesday " + Environment.NewLine + String.Format("{0:M/dd}", tuesdayDate), typeof(string));
            AllocatedHours.Columns.Add("Wednesday " + Environment.NewLine + String.Format("{0:M/dd}", wednesdayDate), typeof(string));
            AllocatedHours.Columns.Add("Thursday " + Environment.NewLine + String.Format("{0:M/dd}", thursdayDate), typeof(string));
            AllocatedHours.Columns.Add("Friday " + Environment.NewLine + String.Format("{0:M/dd}", fridayDate), typeof(string));
            AllocatedHours.Columns.Add("Saturday " + Environment.NewLine + String.Format("{0:M/dd}", saturdayDate), typeof(string));

            for (int i = 0; i < tableRows.Length - 1; i++)
            {
                AllocatedHours.Rows.Add(tableRows[i]);
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
                        if (hourChecks[col - 1, row] == false)
                        {

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
                        }
                        else
                        {

                            AllocatedHours.Rows[row][col] = blank;
                        }
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
        public void FillPowerHourForecastTable(bool[,] hourChecks)
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

            for (int i = 0; i < tableRows.Length - 1; i++)
            {
                PowerHourForecast.Rows.Add(tableRows[i]);
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
                        if (hourChecks[col - 1, row] == false)
                        {
                            if (row <= PowerHourForecast.Rows.Count - 3)
                            {
                                double currentHour = Convert.ToDouble(WeightedAverageTraffic.Rows[row][col]);
                                forecast = currentHour + Convert.ToDouble(WeightedAverageTraffic.Rows[row + 1][col]) + Convert.ToDouble(WeightedAverageTraffic.Rows[row + 2][col]);

                                dayNumbers[row] = forecast;
                            }
                            else if (row == PowerHourForecast.Rows.Count - 2)
                            {
                                double currentHour = Convert.ToDouble(WeightedAverageTraffic.Rows[row][col]);
                                forecast = currentHour + Convert.ToDouble(WeightedAverageTraffic.Rows[row + 1][col]);

                                dayNumbers[row] = forecast;
                            }
                            else
                            {
                                double currentHour = Convert.ToDouble(WeightedAverageTraffic.Rows[row][col]);
                                forecast = currentHour;

                                dayNumbers[row] = forecast;
                            }


                            PowerHourForecast.Rows[row][col] = Math.Round(forecast, 0);

                        }
                        else
                        {
                            PowerHourForecast.Rows[row][col] = 0;
                        }
                    }

                    dayHighs[col - 1] = dayNumbers.Max();

                    n += 3;
                    int smallest = 9999;
                    int second = 9999;
                    int third = 9999;

                    foreach (int i in dayNumbers)
                    {
                        if (i < smallest && i != 0)
                        {
                            third = second;
                            second = smallest;
                            smallest = i;
                        }
                        else if (i < second && i != 0)
                        {
                            third = second;
                            second = i;
                        }
                        else if (i < third && i != 0)
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

                //powerHours = new double[7];
                //lowHours = new double[21];

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

                //foreach (var item in dayHighs)
                //{
                //    Debug.WriteLine(item.ToString());
                //}

                //foreach (var item in dayLows)
                //{
                //    Debug.WriteLine(item.ToString());
                //}

                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public DataTable FillCurrentWeekHoursTable()
        {
            DateTime sundayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Sunday);
            DateTime mondayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime tuesdayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Tuesday);
            DateTime wednesdayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Wednesday);
            DateTime thursdayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Thursday);
            DateTime fridayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Friday);
            DateTime saturdayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday);

            CurrentWeekHours.Columns.Add("HourOfDay", typeof(string));
            CurrentWeekHours.Columns.Add("Sunday " + Environment.NewLine + String.Format("{0:M/dd}", sundayDate), typeof(string));
            CurrentWeekHours.Columns.Add("Monday " + Environment.NewLine + String.Format("{0:M/dd}", mondayDate), typeof(string));
            CurrentWeekHours.Columns.Add("Tuesday " + Environment.NewLine + String.Format("{0:M/dd}", tuesdayDate), typeof(string));
            CurrentWeekHours.Columns.Add("Wednesday " + Environment.NewLine + String.Format("{0:M/dd}", wednesdayDate), typeof(string));
            CurrentWeekHours.Columns.Add("Thursday " + Environment.NewLine + String.Format("{0:M/dd}", thursdayDate), typeof(string));
            CurrentWeekHours.Columns.Add("Friday " + Environment.NewLine + String.Format("{0:M/dd}", fridayDate), typeof(string));
            CurrentWeekHours.Columns.Add("Saturday " + Environment.NewLine + String.Format("{0:M/dd}", saturdayDate), typeof(string));

            for (int i = 0; i < tableRows.Length - 1; i++)
            {
                CurrentWeekHours.Rows.Add(tableRows[i]);
            }

            for (int col = 1; col < 8; col++)
            {
                for (int row = 0; row < tableRows.Length - 1; row++)
                {
                    CurrentWeekHours.Rows[row][col] = "";
                }
            }

            return CurrentWeekHours;

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

        public bool[,] hourChecks = new bool[7, 15];


    }

    public class Employee
    {
        string name { get; set; }

    }
}