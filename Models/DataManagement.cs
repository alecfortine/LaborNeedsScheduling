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
        #region Pending Variables

        #region Selected Employee Scheduling
        //public List<Employees> schedulin = LaborScheduling.EmployeeList;

        public List<string> ErrorMessages { get; set; }
        public int selectedWeekday { get; set; }
        public string selectedHour { get; set; }
        public string selectedEmployee { get; set; }
        public string selectedEmployeeId { get; set; }
        public string startHour { get; set; }
        public string endHour { get; set; }
        public List<Employees> employeeListAll { get; set; }
        public List<Employees> employeeListStore { get; set; }
        public string currentStoreCode { get; set; }
        public bool employeeStatus { get; set; }
        public Dictionary<string, bool[]> employeeHourSchedule = new Dictionary<string, bool[]>();
        public int[] employeesStillNeeded { get; set; }
        public DataTable BlackoutTimes { get; set; }
        public Dictionary<string, Dictionary<string, string[]>> EmployeeAvailableTimes = new Dictionary<string, Dictionary<string, string[]>>();
        public Dictionary<string, Dictionary<string, string[]>> EmployeeScheduledTimes = new Dictionary<string, Dictionary<string, string[]>>();
        public List<string> ScheduleConflicts { get; set; }
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
        public DataTable BlackoutAssignmentView = new DataTable();
        public DataTable EmployeeAvailabilityView = new DataTable();
        public DataTable EmployeeScheduledView = new DataTable();
        public DataTable HourView = new DataTable();

        public DataTable EmployeesScheduledPartial = new DataTable();
        public DataTable EmployeesAvailablePartial = new DataTable();

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
        /// Set of possible hours for the schedule, hours shown in each table are based on selections of the ScheduleStartHours dictionary
        /// </summary>
        public string[] ScheduleHourSlots = { "6AM-7AM", "7AM-8AM","8AM-9AM","9AM-10AM","10AM-11AM","11AM-12PM","12PM-1PM", "1PM-2PM", "2PM-3PM",
                                              "3PM-4PM","4PM-5PM", "5PM-6PM", "6PM-7PM", "7PM-8PM", "8PM-9PM", "9PM-10PM", "10PM-11PM", "11PM-12AM", "12AM-1AM"};

        public string[] replaceRows = { "6:00AM", "7:00AM","8:00AM","9:00AM","10:00AM","11:00AM","12:00PM", "1:00PM", "2:00PM", "3:00PM",
                                        "4:00PM", "5:00PM", "6:00PM", "7:00PM", "8:00PM", "9:00PM", "10:00PM", "11:00PM", "12:00AM"};

        /// <summary>
        /// List of times for dropdown lists and start/end times for the week
        /// </summary>
        public Dictionary<string, string> ScheduleStartHours = new Dictionary<string, string>()
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
            //{"12AM", "12AM-1AM"},
        };

        /// <summary>
        /// List of hour end times for dropdown lists
        /// </summary>
        public Dictionary<string, string> ScheduleEndHours = new Dictionary<string, string>()
        {
            {"7AM", "6AM-7AM"},
            {"8AM", "7AM-8AM"},
            {"9AM", "8AM-9AM"},
            {"10AM", "9AM-10AM"},
            {"11AM", "10AM-11AM"},
            {"12PM", "11AM-12PM"},
            {"1PM", "12PM-1PM"},
            {"2PM", "1PM-2PM"},
            {"3PM", "2PM-3PM"},
            {"4PM", "3PM-4PM"},
            {"5PM", "4PM-5PM"},
            {"6PM", "5PM-6PM"},
            {"7PM", "6PM-7PM"},
            {"8PM", "7PM-8PM"},
            {"9PM", "8PM-9PM"},
            {"10PM", "9PM-10PM"},
            {"11PM", "10PM-11PM"},
            {"12AM", "11PM-12AM"},
        };

        /// <summary>
        /// Beginning hours for each day
        /// </summary>
        public string WeekStartHour { get; set; }

        /// <summary>
        /// End hours for each day
        /// </summary>
        public string WeekEndHour { get; set; }

        public string[] WeekStartHours { get; set; }

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
        public string[] excludedDates { get; set; }

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

        /// <summary>
        /// Name or Id of the employee to search for in the employees list
        /// </summary>
        public string EmployeeSearch { get; set; }

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

        /// <summary>
        /// Dates for the current week
        /// </summary>
        public DateTime[] CurrentWeekDates = new DateTime[7];

        /// <summary>
        /// The number of weeks to use for calculations
        /// </summary>
        public int NumberHistoricalWeeks { get; set; }

        /// <summary>
        /// The default number of weeks back to base calculations on
        /// </summary>
        public int DefaultHistoricalWeeks = 6;

        /// <summary>
        /// Default weighting values for the past six weeks
        /// </summary>
        public double[] getDefaultWeights(int numberOfWeeks)
        {
            double[] defaultWeightingValues = { 36, 24, 16, 12, 8, 4 };

            return defaultWeightingValues;
        }

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
        /// Runs all the table procedures and fills the tables with data
        /// </summary>
        public void FillDatatables()
        {
            try
            {
                //FakeAPI.dothething();

                // weighting input
                for (int i = 0; i < weekWeighting.Length; i++)
                {
                    if (weekWeighting[i] == 0)
                    {
                        weekWeighting = getDefaultWeights(weekWeighting.Length);
                    }
                }

                employeeListAll = FakeAPI.GetAllEmployees();
                if (employeeListStore.Count <= FakeAPI.GetEmployeesForStore(currentStoreCode).Count)
                {
                    employeeListStore = FakeAPI.GetEmployeesForStore(currentStoreCode);
                }

                //GetEmployeesForStore();
                GenerateCurrentWeekDates();
                FakeAPI.AddNewWeekDates(CurrentWeekDates);

                GenerateWeeklyTimeSlots();
                WTGTrafficPercent = FakeAPI.FillWTGTable(weekWeighting, NumberHistoricalWeeks, DateTime.Parse("2015-05-10"));

                FillExcludedDatesTable();
                FillWeightedAverageTrafficTable();
                FillPercentWeeklyTable();
                FillPercentDailyTable();
                FillAllocatedHoursTable();
                //FillCurrentWeekHoursTable();
                FillPowerHourForecastTable();
                GenerateAllocatedHoursDisplay();
                GeneratePowerHourCells();

                if (selectedEmployeeId != null)
                {
                    UpdateEmployees(employeeListStore);
                }

                //GenerateNumEmployeesNeeded(selectedWeekday, scheduling);
                FillAssignmentTable(selectedWeekday);

                //CheckSchedulingRules(selectedWeekday, scheduling);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Set the dates for the current week
        /// </summary>
        public void GenerateCurrentWeekDates()
        {
            for (int i = 1; i <= 7; i++)
            {
                CurrentWeekDates[i - 1] = weekMarker.AddDays(i);
            }
        }

        /// <summary>
        /// Get the employees for the current store
        /// </summary>
        public void GetEmployeesForStore()
        {
            List<Employees> storeEmployees = new List<Employees>();
            foreach (Employees emp in employeeListStore)
            {
                if (emp.storeCode == currentStoreCode)
                {
                    storeEmployees.Add(emp);
                }
            }
            employeeListStore = storeEmployees;
        }

        /// <summary>
        /// Get the availability hours of all employees for each day
        /// </summary>
        public void CreateEmpAvailabilityText()
        {
            GenerateCurrentWeekDates();
            string[] weekdays = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            string[] employeeIdsAll = new string[LaborScheduling.EmployeeListAll.Count];
            string[] employeeIdsStore = new string[employeeListStore.Count];

            // creating each day for an employee
            for (int i = 0; i < employeeIdsStore.Length; i++)
            {
                employeeIdsStore[i] = employeeListStore[i].id;
            }

            EmployeeAvailableTimes = FakeAPI.GetEmployeeAvailableTimes(employeeIdsStore);
            EmployeeScheduledTimes = FakeAPI.GetEmployeeScheduledTimes(employeeIdsStore, CurrentWeekDates);
        }

        /// <summary>
        /// Chec the selected employee's schedule for conflicts with their availability
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="weekday"></param>
        public void CheckScheduleConflicts(string employeeId, int weekday)
        {
            List<Employees> EmployeeList = FakeAPI.GetAllEmployees();
            string employeeName = "";
            foreach (var emp in EmployeeList)
            {
                if (emp.id == employeeId)
                {
                    employeeName = emp.firstName;
                }
            }
            string[] weekdays = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            string day = weekdays[weekday];
            Dictionary<string, string[]> available = EmployeeAvailableTimes[employeeId];
            Dictionary<string, string[]> scheduled = EmployeeScheduledTimes[employeeId];
            bool check = false;
            List<string> Conflicts = new List<string>();

            foreach (string scheduledHour in scheduled[day])
            {
                check = false;
                foreach (string availableHour in available[day])
                {
                    if (scheduledHour == availableHour)
                    {
                        check = true;
                        break;
                    }
                }
                if (check == false)
                {
                    Conflicts.Add(employeeName + " is assigned from " + scheduledHour + " but is not available.");
                }
            }
            ScheduleConflicts = Conflicts;
        }

        public DataTable FindEmployee(string EmployeeInfo)
        {
            DataTable FoundEmployee = new DataTable();
            List<Employees> EmployeeList = FakeAPI.GetAllEmployees();
            foreach (Employees emp in EmployeeList)
            {
                if (emp.storeCode != currentStoreCode && (/*emp.firstName == EmployeeInfo ||*/ emp.id == EmployeeInfo))
                {
                    FoundEmployee.Columns.Add("Name");
                    FoundEmployee.Columns.Add("Id");
                    FoundEmployee.Columns.Add("Store Code");
                    FoundEmployee.Columns.Add("Role");
                    FoundEmployee.Columns.Add("Level");

                    DataRow row = FoundEmployee.NewRow();
                    row["Name"] = emp.firstName + " " + emp.lastName;
                    row["Id"] = emp.id;
                    row["Store Code"] = emp.storeCode;
                    row["Role"] = emp.role;
                    row["Level"] = emp.rank;
                    FoundEmployee.Rows.Add(row);
                }
            }

            return FoundEmployee;
        }

        /// <summary>
        /// Add an employee from another store to the current store's roster
        /// </summary>
        /// <param name="employeeId"></param>
        public void AddEmployeeToList(string employeeId)
        {
            Employees addedEmployee = new Employees();

            foreach (Employees emp in employeeListAll)
            {
                if (emp.id == employeeId)
                {
                    addedEmployee = emp;
                    break;
                }
            }

            bool employeeExists = false;
            foreach (Employees emp in employeeListStore)
            {
                if (emp.id == addedEmployee.id)
                {
                    employeeExists = true;
                }
            }

            if (employeeExists == false)
            {
                employeeListStore.Add(addedEmployee);
            }
        }

        /// <summary>
        /// Determine which cells should be blacked out on the ain labor schedule based on individual start and end times
        /// </summary>
        /// <param name="AllocatedHours"></param>
        public void GetBlackoutCells(DataTable AllocatedHours)
        {
            Dictionary<int, string[]> blackoutTimes = new Dictionary<int, string[]>();
            DataTable BlackoutTable = AllocatedHours.Copy();

            for (int weekday = 0; weekday < 7; weekday++)
            {
                List<string> times = new List<string>();
                if (WeekStartHours[weekday] == "")
                {
                    WeekStartHours[weekday] = Convert.ToString(AllocatedHours.Rows[0][0]);
                }
                if (WeekEndHours[weekday] == "")
                {
                    int lastTime = AllocatedHours.Rows.Count - 1;
                    WeekEndHours[weekday] = Convert.ToString(AllocatedHours.Rows[lastTime][0]);
                }

                for (int i = 0; i < AllocatedHours.Rows.Count; i++)
                {
                    if (AllocatedHours.Rows[i][0].ToString() == WeekStartHours[weekday])
                    {
                        for (int n = i - 1; n >= 0; n--)
                        {
                            times.Add(AllocatedHours.Rows[n][0].ToString());

                            if (n == 0)
                            {
                                times.Reverse();
                            }
                        }
                    }

                    if (AllocatedHours.Rows[i][0].ToString() == WeekEndHours[weekday])
                    {
                        for (int n = i + 1; n < AllocatedHours.Rows.Count; n++)
                        {
                            times.Add(AllocatedHours.Rows[n][0].ToString());
                        }
                    }
                }
                string[] blackouts = new string[times.Count];

                for (int j = 0; j < times.Count; j++)
                {
                    blackouts[j] = times[j].ToString();
                }

                blackoutTimes[weekday] = blackouts;

            }

            for (int i = 0; i < AllocatedHours.Columns.Count; i++)
            {
                for (int n = 0; n < AllocatedHours.Rows.Count; n++)
                {
                    if (i == 0)
                    {
                        BlackoutTable.Rows[n][i] = AllocatedHours.Rows[n][i];
                    }
                    else
                    {
                        string[] hours = new string[blackoutTimes[i - 1].Length];
                        hours = blackoutTimes[i - 1];

                        for (int h = 0; h < BlackoutTable.Rows.Count; h++)
                        {
                            BlackoutTable.Rows[n][i] = "False";
                        }
                        for (int j = 0; j < hours.Length; j++)
                        {
                            if (Convert.ToString(BlackoutTable.Rows[n][0]) == hours[j])
                            {
                                BlackoutTable.Rows[n][i] = "True";
                                break;
                            }
                        }
                    }
                }
            }
            BlackoutTimes = BlackoutTable;
        }

        /// <summary>
        /// Get all employees available for a specific hour of the day
        /// </summary>
        /// <param name="weekday"></param>
        /// <param name="hour"></param>
        /// <returns></returns>
        public void GetEmployeesForHour(List<Employees> storeEmployeeList, int weekday, string hour)
        {
            EmployeesAvailablePartial.Clear();
            EmployeesAvailablePartial.Columns.Clear();
            EmployeesAvailablePartial.Columns.Add(hour, typeof(string));

            EmployeesScheduledPartial.Clear();
            EmployeesScheduledPartial.Columns.Clear();
            EmployeesScheduledPartial.Columns.Add(hour, typeof(string));

            //List<Employees> EmployeeList = FakeAPI.CreateEmployees();
            string[] weekdays = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            selectedHour = hour;

            foreach (Employees emp in storeEmployeeList)
            {
                Dictionary<string, string[]> available = EmployeeAvailableTimes[emp.id];
                Dictionary<string, string[]> scheduled = EmployeeScheduledTimes[emp.id];

                string[] availableHours = available[weekdays[weekday]];
                string[] scheduledHours = scheduled[weekdays[weekday]];

                for (int i = 0; i < availableHours.Length; i++)
                {
                    if (availableHours[i] == hour)
                    {
                        bool avail = true;
                        for (int n = 0; n < scheduledHours.Length; n++)
                        {
                            if (availableHours[i] == scheduledHours[n])
                            {
                                avail = false;
                                break;
                            }
                        }
                        if (avail == true)
                        {
                            EmployeesAvailablePartial.Rows.Add(emp.firstName);
                        }
                    }
                }

                for (int i = 0; i < scheduledHours.Length; i++)
                {
                    if (scheduledHours[i] == hour)
                    {
                        EmployeesScheduledPartial.Rows.Add(emp.firstName);
                    }
                }
            }
        }

        /// <summary>
        /// Generate future tables based on time selection inputs
        /// </summary>
        public void GenerateWeeklyTimeSlots()
        {
            int counter = 0;
            bool startCheck = false;
            WeekHourSchedule.Clear();

            for (int i = 0; i < 1; i++)
            {
                string start = WeekStartHour;
                string end = WeekEndHour;

                for (int j = 0; j < ScheduleHourSlots.Length; j++)
                {
                    if (ScheduleHourSlots[j] == end)
                    {
                        WeekHourSchedule.Add(ScheduleHourSlots[j]);
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
            int powerHourAmount = WeekdayPowerHours;
            if (WeekdayPowerHours == 0)
            {
                powerHourAmount = 3;
            }

            int powerHourAmountWeekend = WeekendPowerHours;
            if (WeekendPowerHours == 0)
            {
                powerHourAmountWeekend = 4;
            }

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
                DateTime[] days = new DateTime[ExclusionDates.Count];
                int counter = 0;
                foreach (DateTime date in ExclusionDates.Keys)
                {
                    days[counter] = date;

                    //if (excludedDates[counter] == "True")
                    //{
                    //    ExclusionDates[date] = true;
                    //}
                    //else
                    //{
                    //    ExclusionDates[date] = false;
                    //}
                    counter++;
                }

                for (int i = 0; i < days.Length; i++)
                {
                    if (excludedDates[i] == "True")
                    {
                        ExclusionDates[days[i]] = true;
                    }
                    else
                    {
                        ExclusionDates[days[i]] = false;
                    }
                }

                ExcludedDates = WTGTrafficPercent.Clone();

                #region Exclusion Check
                foreach (DataRow row in WTGTrafficPercent.Rows)
                {
                    // if the date in the dictionary is true don't add it to the table
                    if (!ExclusionDates[DateTime.Parse(row["Date"].ToString())])
                    {
                        ExcludedDates.Rows.Add(row.ItemArray);
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
            WeightedAverageTraffic.Columns.Clear();
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
            WeightedAverageTrafficTotal.Clear();
            WeightedAverageTrafficTotal.Columns.Clear();
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
            PercentWeeklyTotal.Clear();
            PercentWeeklyTotal.Columns.Clear();
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
            PercentDailyTotal.Clear();
            PercentDailyTotal.Columns.Clear();
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

            AllocatedHours.Clear();
            AllocatedHours.Columns.Clear();
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
        /// Determine which cells contain power hours and low hours
        /// </summary>
        public void FillPowerHourForecastTable()
        {
            #region Table Setup
            PowerHourForecast.Clear();
            PowerHourForecast.Columns.Clear();
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

                    // power hours
                    for (int row = 0; row < PowerHourForecast.Rows.Count; row++)
                    {
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

                    // low hours
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

        /// <summary>
        /// Generate the table for the labor schedule with the specified hours 
        /// </summary>
        /// <returns></returns>
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

            AllocatedHoursDisplay.Clear();
            AllocatedHoursDisplay.Columns.Clear();
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
                        }
                        count++;
                    }
                }
            }
            return AllocatedHoursDisplay;
        }

        /// <summary>
        /// Find the amount of employees still needed for each hour and update each employee's [hours remaining]
        /// </summary>
        /// <param name="weekday"></param>
        /// <param name="scheduling"></param>
        public void GenerateNumEmployeesNeeded(int weekday, List<Employees> scheduling)
        {
            //int[] stillNeeded = new int[ScheduleHourSlots.Length];
            string[] weekdays = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            string day = weekdays[weekday];
            employeesStillNeeded = new int[ScheduleHourSlots.Length];
            string[] employeeIds = new string[scheduling.Count];
            for (int x = 0; x < employeeIds.Length; x++)
            {
                employeeIds[x] = scheduling[x].id;
            }
            EmployeeScheduledTimes = FakeAPI.GetEmployeeScheduledTimes(employeeIds, CurrentWeekDates);

            //loop over each employee's (weekday) and check the hours that are selected - or all of them?
            foreach (Employees emp in scheduling)
            {
                Dictionary<string, string[]> innerDict = EmployeeScheduledTimes[emp.id];
                string[] hoursForDay = innerDict[day];
                for (int i = 0; i < ScheduleHourSlots.Length; i++)
                {
                    for (int n = 0; n < hoursForDay.Length; n++)
                    {
                        if (hoursForDay[n] == ScheduleHourSlots[i])
                        {
                            employeesStillNeeded[i] += 1;
                        }
                    }
                }
            }

            // update employee hours remaining
            foreach (Employees emp in scheduling)
            {
                //DataTable empSchedule = new DataTable();
                int hoursRemaining = emp.hours;

                Dictionary<string, string[]> innerDict = EmployeeScheduledTimes[emp.id];
                int hourCount = 0;
                for (int n = 0; n < weekdays.Length; n++)
                {
                    for (int j = 0; j < innerDict[weekdays[n]].Length; j++)
                    {
                        hourCount++;
                    }
                }
                emp.hoursRemaining = hoursRemaining - hourCount;
            }
            LaborScheduling.EmployeeListStore = scheduling;
        }

        /// <summary>
        /// Fill the horizontal assignment table with hours and employees needed
        /// </summary>
        /// <param name="column"></param>
        public void FillAssignmentTable(int column)
        {

            AssignmentTable.Clear();
            AssignmentTable.Columns.Clear();
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
        }

        /// <summary>
        /// Generate the assignment partial view based on the column selected
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public DataTable GenerateAssignmentView(int column)
        {
            AssignmentView.Clear();
            AssignmentView.Columns.Clear();
            AssignmentView.Columns.Add(" ", typeof(string));
            for (int i = 0; i < WeekHourSchedule.Count; i++)
            {
                AssignmentView.Columns.Add(WeekHourSchedule[i], typeof(string));
            }

            var row = AssignmentTable.Rows[column];
            AssignmentView.ImportRow(row);

            DataRow dr = AssignmentView.NewRow();
            int hourIterator = 0;
            bool iteratorFound = false;

            for (int i = 0; i < ScheduleHourSlots.Length; i++)
            {
                if (iteratorFound == false)
                {
                    for (int n = 0; n < WeekHourSchedule.Count; n++)
                    {
                        if (ScheduleHourSlots[i] == WeekHourSchedule[n])
                        {
                            hourIterator = i;
                            iteratorFound = true;
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            for (int i = 0; i <= WeekHourSchedule.Count; i++)
            {
                if (i == 0)
                {
                    AssignmentView.Rows.Add("Needed");
                }
                else
                {
                    //AssignmentView.Rows[1][i] = AssignmentView.Rows[0][i];
                    AssignmentView.Rows[1][i] = Convert.ToInt32(AssignmentView.Rows[0][i]) - employeesStillNeeded[(i - 1) + hourIterator];
                }
            }
            return AssignmentView;
        }

        /// <summary>
        /// Generate the times to exclude for the assignment partial view
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public DataTable GenerateBlackoutAssignmentView(int column)
        {
            BlackoutAssignmentView.Clear();
            BlackoutAssignmentView.Columns.Clear();
            BlackoutAssignmentView.Columns.Add(" ", typeof(string));
            for (int i = 0; i < WeekHourSchedule.Count; i++)
            {
                BlackoutAssignmentView.Columns.Add(WeekHourSchedule[i], typeof(string));
            }

            var row = BlackoutTimes.Rows[column];
            BlackoutAssignmentView.ImportRow(row);
            BlackoutAssignmentView.ImportRow(row);

            for (int i = 0; i < WeekHourSchedule.Count; i++)
            {
                for (int n = 0; n < BlackoutTimes.Rows.Count; n++)
                {
                    if (BlackoutTimes.Rows[n][0].ToString() == WeekHourSchedule[i])
                    {
                        BlackoutAssignmentView.Rows[0][i + 1] = BlackoutTimes.Rows[i][column + 1];
                        BlackoutAssignmentView.Rows[1][i + 1] = BlackoutTimes.Rows[i][column + 1];
                    }
                }
            }

            return BlackoutAssignmentView;
        }

        /// <summary>
        /// Create a set of times that an employee is available for a day of the week
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="employeeName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public DataTable GenerateEmployeeAvailabilityView(int employeeId, int column)
        {
            EmployeeAvailabilityView.Clear();
            EmployeeAvailabilityView.Columns.Clear();
            EmployeeAvailabilityView.Columns.Add("" + employeeId + "");

            string[] weekdays = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            Dictionary<string, string[]> innerDict = EmployeeAvailableTimes[Convert.ToString(employeeId)];
            List<string> hours = new List<string>();
            string[] hourValues = innerDict[weekdays[column]];

            foreach (string val in hourValues)
            {
                hours.Add(val);
            }

            // put each hour string in the datatable
            if (hours.Count == 0)
            {
                EmployeeAvailabilityView.Rows.Add("Not Available");
            }
            else
            {
                for (int i = 0; i < hours.Count; i++)
                {
                    //for (int n = 0; n < LaborSchedule.Rows.Count; n++)
                    //{
                    //    if (hours[i] == LaborSchedule.Rows[n][0].ToString())
                    //    {
                    EmployeeAvailabilityView.Rows.Add(hours[i]);
                    //    }
                    //}
                }
            }

            return EmployeeAvailabilityView;
        }

        /// <summary>
        /// Create a set of times that an employee is scheduled for a day of the week
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public DataTable GenerateEmployeeScheduledView(int employeeId, int column)
        {
            EmployeeScheduledView.Clear();
            EmployeeScheduledView.Columns.Clear();
            EmployeeScheduledView.Columns.Add("" + employeeId + "");

            string[] weekdays = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            Dictionary<string, string[]> innerDict = EmployeeScheduledTimes[Convert.ToString(employeeId)];
            List<string> hours = new List<string>();
            string[] hourValues = innerDict[weekdays[column]];

            foreach (string val in hourValues)
            {
                hours.Add(val);
            }

            // put each hour string in the datatable
            if (hours.Count == 0)
            {
                EmployeeScheduledView.Rows.Add("Not Scheduled");
            }
            else
            {
                for (int i = 0; i < hours.Count; i++)
                {
                    EmployeeScheduledView.Rows.Add(hours[i]);
                }
            }

            return EmployeeScheduledView;
        }

        /// <summary>
        /// Update an employee's schedule information for the week (when they are assigned)
        /// </summary>
        public Dictionary<string, Dictionary<string, string[]>> UpdateEmployees(List<Employees> scheduling)
        {
            bool[] employeeHours = new bool[ScheduleStartHours.Keys.Count * 7];
            int weekday = (selectedWeekday * (ScheduleHourSlots.Length - 1));
            int start = 0;
            int end = 0;
            string employeeId = selectedEmployeeId;
            int empid = 0;
            int emppos = 0;
            string[] daysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

            // match the employee ID with the correct one in the list
            for (int n = 0; n < scheduling.Count; n++)
            {
                if (employeeId == (scheduling[n].id))
                {
                    empid = Convert.ToInt32(scheduling[n].id);
                    emppos = n;
                }
            }

            for (int n = 0; n < ScheduleStartHours.Keys.Count; n++)
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
                    employeeHours[(n) + weekday] = true;
                    end = (n);
                }
            }

            for (int n = 0; n < ScheduleStartHours.Keys.Count; n++)
            {
                // set everything true between the start and end hours
                if (n > start && n < end)
                {
                    employeeHours[n + weekday] = true;
                }
            }

            string[] updatedHours = FakeAPI.UpdateEmployeeSchedule(ScheduleStartHours.Keys.Count, employeeHours,
                                    employeeId, CurrentWeekDates, EmployeeScheduledTimes[employeeId], selectedWeekday);

            Dictionary<string, string[]> employeeSchedule = EmployeeScheduledTimes[employeeId];
            employeeSchedule[daysOfWeek[selectedWeekday]] = updatedHours;
            EmployeeScheduledTimes[employeeId] = employeeSchedule;
            Dictionary<string, Dictionary<string, string[]>> updatedSchedule = EmployeeScheduledTimes;

            foreach (Employees emp in scheduling)
            {
                //DataTable empSchedule = new DataTable();
                int hoursRemaining = emp.hours;

                Dictionary<string, string[]> innerDict = EmployeeScheduledTimes[emp.id];
                int hourCount = 0;
                for (int n = 0; n < daysOfWeek.Length; n++)
                {
                    for (int j = 0; j < innerDict[daysOfWeek[n]].Length; j++)
                    {
                        hourCount++;
                    }
                }
                emp.hoursRemaining = hoursRemaining - hourCount;
            }
            LaborScheduling.EmployeeListStore = scheduling;

            return updatedSchedule;
        }

        /// <summary>
        /// Check if all scheduling rules are satisfied, create a list of any that aren't satisfied
        /// </summary>
        /// <param name="weekday"></param>
        /// <param name="scheduling"></param>
        public void CheckSchedulingRules(int weekday, List<Employees> scheduling)
        {
            //Dictionary<string, int[]> ScheduledEmployees = new Dictionary<string, int[]>();

            ErrorMessages = new List<string>();
            Dictionary<string, Dictionary<string, List<int>>> ScheduledEmployees = new Dictionary<string, Dictionary<string, List<int>>>();
            string[] daysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            string[] hourSlots = ScheduleHourSlots;
            string[] selectedHours = new string[AllocatedHoursDisplay.Rows.Count];
            for (int i = 0; i < AllocatedHoursDisplay.Rows.Count; i++)
            {
                selectedHours[i] = Convert.ToString(AllocatedHoursDisplay.Rows[i][0]);
            }
            string openHour = WeekStartHour;
            string closeHour = WeekEndHour;

            // Level 10 : Sales Associate
            // Level 20 : Stock
            // Level 30 : Cashier
            // Level 40 : Key Holder/ MIT / Stock Manager
            // Level 50 : Assistant Manager
            // Level 60 : Store Manager/ SMIT

            // Level 40, 50, or 60 must be scheduled at all times
            //bool condition1Checks;
            Dictionary<string, bool> condition1Checks = new Dictionary<string, bool>();
            bool condition1 = false;
            List<string> UnfilledHours = new List<string>();
            // Level 60 must work one closing shift on Friday or Saturday
            bool condition2 = false;
            // Level 60 must work every 3rd Sunday
            bool condition3 = false;
            // Stores must open with at least 2 associates
            bool condition4 = false;
            int openAssociates = 0;
            // Stores must close with at least 3 associates
            bool condition5 = false;
            int closeAssociates = 0;
            // Level 50 and 60 should work every Friday and Saturday. One should open and one should close
            bool condition6 = false;
            // Level 60 should work every Monday
            bool condition7 = false;

            //string strSQLCon = @"Data Source=AFORTINE\SQLEXPRESS;Initial Catalog=LaborNeedsScheduling;Integrated Security=True; MultipleActiveResultSets=True;";

            //using (SqlConnection conn = new SqlConnection(strSQLCon))
            //{

            //    SqlCommand cmd = new SqlCommand();
            //    cmd.CommandType = CommandType.Text;
            //    cmd.Connection = conn;
            //    conn.Open();

            //    // this slows things down
            //    // get the employee ranks for each hour of the week to check conditions
            //    foreach (string day in daysOfWeek)
            //    {
            //        Dictionary<string, List<int>> EmployeeData = new Dictionary<string, List<int>>();

            //        foreach (string hour in selectedHours)
            //        {
            //            List<int> hourRanks = new List<int>();

            //            foreach (Employees emp in scheduling)
            //            {
            //                DataTable DaySchedule = new DataTable();

            //                cmd.CommandText =
            //                "select Hour, " + day + " from dbo.emp_" + emp.id + " where Hour = '" + hour + "'";

            //                SqlDataAdapter da = new SqlDataAdapter(cmd);
            //                da.Fill(DaySchedule);

            //                if (Convert.ToString(DaySchedule.Rows[0][1]) == "True")
            //                {
            //                    hourRanks.Add(emp.rank);
            //                }
            //            }
            //            // inner dictionary
            //            EmployeeData.Add(hour, hourRanks);
            //        }
            //        // outer dictionary
            //        ScheduledEmployees.Add(day, EmployeeData);
            //    }

            Dictionary<string, List<int>> innerDict = new Dictionary<string, List<int>>();


            //Dictionary<string, List<int>> innerDict = new Dictionary<string, List<int>>();
            for (int n = 0; n < selectedHours.Length; n++)
            {
                List<int> employeeRanks = new List<int>();

                for (int m = 0; m < scheduling.Count; m++)
                {
                    Dictionary<string, string[]> employeeDayHours = EmployeeScheduledTimes[scheduling[m].id];

                    string[] hoursForDay = employeeDayHours[daysOfWeek[weekday]];

                    for (int h = 0; h < hoursForDay.Length; h++)
                    {
                        if (hoursForDay[h].Contains(selectedHours[n]))
                        {
                            employeeRanks.Add(scheduling[m].rank);
                        }
                    }
                }
                //innerDict.Add(selectedHours[n], employeeRanks);
                innerDict[selectedHours[n]] = employeeRanks;
            }
            ScheduledEmployees.Add(daysOfWeek[weekday], innerDict);

            innerDict = ScheduledEmployees[daysOfWeek[weekday]];

            // Level 40, 50, or 60 must be scheduled at all times
            #region Condition 1

            if (BlackoutTimes != null && BlackoutTimes.Rows.Count > 0)
            {
                List<string> includedHours = new List<string>();
                for (int i = 0; i < BlackoutTimes.Rows.Count; i++)
                {
                    if (BlackoutTimes.Rows[i][weekday + 1].ToString() == "False")
                        includedHours.Add(BlackoutTimes.Rows[i][0].ToString());
                }

                for (int i = 0; i < BlackoutTimes.Rows.Count; i++)
                {
                    if (BlackoutTimes.Rows[i][weekday + 1].ToString() == "True")
                    {
                        innerDict.Remove(BlackoutTimes.Rows[i][0].ToString());
                    }
                }
            }
            foreach (string hour in innerDict.Keys)
            {
                condition1Checks.Add(hour, false);
                foreach (int empRank in innerDict[hour])
                {
                    if (empRank == 40)
                    {
                        condition1Checks[hour] = true;
                        break;
                    }
                    else if (empRank == 50)
                    {
                        condition1Checks[hour] = true;
                        break;
                    }
                    else if (empRank == 60)
                    {
                        condition1Checks[hour] = true;
                        break;
                    }
                }

                if (condition1Checks[hour] == false)
                {
                    UnfilledHours.Add(hour);
                }
            }

            foreach (string check in condition1Checks.Keys)
            {
                if (condition1Checks[check] == false)
                {
                    condition1 = false;
                }
                else
                {
                    condition1 = true;
                }
            }
            #endregion

            // Level 60 must work one closing shift on Friday or Saturday
            #region Condition 2
            if (weekday == 5 || weekday == 6)
            {
                foreach (string day in ScheduledEmployees.Keys)
                {
                    if (day == daysOfWeek[weekday])
                    {
                        //var innerDict = ScheduledEmployees[day];
                        foreach (string hour in innerDict.Keys)
                        {
                            openHour = innerDict.Keys.First();
                            closeHour = innerDict.Keys.Last();
                            if (hour == closeHour)
                            {
                                foreach (int empRank in innerDict[hour])
                                {
                                    if (empRank == 60)
                                    {
                                        condition2 = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            // Level 60 must work every 3rd Sunday
            /* come back to this */
            #region Condition 3
            #endregion

            // Stores must open with at least 2 associates
            #region Condition 4
            foreach (string day in ScheduledEmployees.Keys)
            {
                bool flag1 = false;
                bool flag2 = false;
                if (day == daysOfWeek[weekday])
                {
                    //var innerDict = ScheduledEmployees[day];
                    foreach (string hour in innerDict.Keys)
                    {
                        if (hour == openHour)
                        {
                            foreach (int empRank in innerDict[hour])
                            {
                                if (empRank == 10)
                                {
                                    if (flag1 == false || flag2 == false)
                                    {
                                        if (flag1 == true)
                                        {
                                            flag2 = true;
                                        }
                                        else
                                        {
                                            flag1 = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (flag1 == true && flag2 == true)
                {
                    condition4 = true;
                }
                else
                {
                    if (flag1 == true)
                    {
                        openAssociates = 1;
                    }
                }
            }
            #endregion

            // Stores must close with at least 3 associates
            #region Condition 5
            foreach (string day in ScheduledEmployees.Keys)
            {
                bool flag1 = false;
                bool flag2 = false;
                bool flag3 = false;
                if (day == daysOfWeek[weekday])
                {
                    //var innerDict = ScheduledEmployees[day];
                    foreach (string hour in innerDict.Keys)
                    {
                        openHour = innerDict.Keys.First();
                        closeHour = innerDict.Keys.Last();
                        if (hour == closeHour)
                        {
                            foreach (int empRank in innerDict[hour])
                            {
                                if (empRank == 10)
                                {
                                    if (flag1 == false || flag2 == false || flag3 == false)
                                    {
                                        if (flag1 == true && flag2 == true)
                                        {
                                            flag3 = true;
                                        }
                                        else if (flag1 == true)
                                        {
                                            flag2 = true;
                                        }
                                        else
                                        {
                                            flag1 = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (flag1 == true && flag2 == true && flag3 == true)
                {
                    condition5 = true;
                }
                else
                {
                    if (flag2 == true && flag1 == true)
                    {
                        closeAssociates = 2;
                    }
                    else if (flag1 == true)
                    {
                        closeAssociates = 1;
                    }
                }
            }
            #endregion

            // Level 50 and 60 should work every Friday and Saturday. One should open and one should close
            #region Condition 6
            if (weekday == 5 || weekday == 6)
            {
                bool flag1 = false;
                bool flag2 = false;
                bool openTaken = false;
                bool closeTaken = false;

                foreach (string day in ScheduledEmployees.Keys)
                {
                    if (day == daysOfWeek[weekday])
                    {
                        //var innerDict = ScheduledEmployees[day];
                        int rankTaken = 0;
                        foreach (string hour in innerDict.Keys)
                        {
                            openHour = innerDict.Keys.First();
                            closeHour = innerDict.Keys.Last();
                            // check open hour
                            if (hour == openHour)
                            {
                                foreach (int empRank in innerDict[hour])
                                {
                                    if (empRank == 60 && rankTaken != 50)
                                    {
                                        flag1 = true;
                                        rankTaken = 60;
                                    }
                                    else if (empRank == 50 && rankTaken != 60)
                                    {
                                        flag1 = true;
                                        rankTaken = 50;
                                    }
                                }
                            }

                            // check close hour
                            if (hour == closeHour)
                            {
                                foreach (int empRank in innerDict[hour])
                                {
                                    if (empRank == 60)
                                    {
                                        if (rankTaken == 60)
                                        {
                                            // error message
                                        }
                                        else
                                        {
                                            flag2 = true;
                                        }
                                    }
                                    if (empRank == 50)
                                    {
                                        if (rankTaken == 50)
                                        {
                                            // error message
                                        }
                                        else
                                        {
                                            flag2 = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (flag1 == true && flag2 == true)
                {
                    condition6 = true;
                }
            }
            #endregion

            // Level 60 should work every Monday
            #region Condition 7
            if (weekday == 1)
            {
                foreach (string day in ScheduledEmployees.Keys)
                {
                    if (day == daysOfWeek[weekday])
                    {
                        //var innerDict = ScheduledEmployees[day];
                        foreach (string hour in innerDict.Keys)
                        {
                            foreach (int empRank in innerDict[hour])
                            {
                                if (empRank == 60)
                                {
                                    condition7 = true;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            //conn.Close();
            //}

            // Level 10 : Sales Associate
            // Level 20 : Stock
            // Level 30 : Cashier
            // Level 40 : Key Holder/ MIT / Stock Manager
            // Level 50 : Assistant Manager
            // Level 60 : Store Manager/ SMIT

            string numeric = ". ";
            int count = 1;
            if (condition1 == false)
            {
                List<string> includedHours = new List<string>();

                foreach (string hour in UnfilledHours)
                {
                    bool include = true;

                    if (includedHours.Count == 0)
                    {
                        include = true;
                    }
                    else
                    {
                        for (int i = 0; i < includedHours.Count; i++)
                        {
                            if (hour == includedHours[i])
                            {
                                include = false;
                            }
                        }
                    }

                    if (include == true)
                    {
                        includedHours.Add(hour);
                    }
                    //hours += (hour + ", ");
                }
                string unfilledHours = "";
                if (includedHours.Count == 0)
                {
                    ErrorMessages.Add(count + numeric + "Level 40, 50, or 60 must be scheduled at all times. No current hours contain these employees.");
                }
                else
                {
                    ErrorMessages.Add(count + numeric + "Level 40, 50, or 60 must be scheduled at all times. Current unscheduled hours are:");

                    for (int i = 0; i < includedHours.Count; i++)
                    {
                        if (i == includedHours.Count - 1)
                        {
                            unfilledHours += includedHours[i];
                        }
                        else
                        {
                            unfilledHours += includedHours[i] + ", ";
                        }
                    }
                    ErrorMessages.Add(unfilledHours);
                }
                count++;
            }
            if (condition2 == false && (weekday == 5 || weekday == 6))
            {
                ErrorMessages.Add(count + numeric + "Level 60 must work one closing shift on Friday or Saturday");
                count++;
            }
            if (condition3 == false && weekday == 0)
            {
                ErrorMessages.Add(count + numeric + "Level 60 must work every 3rd Sunday");
                count++;
            }
            if (condition4 == false)
            {
                ErrorMessages.Add(count + numeric + "Store must open with at least 2 associates");
                ErrorMessages.Add("Current associates: " + openAssociates);
                count++;
            }
            if (condition5 == false)
            {
                ErrorMessages.Add(count + numeric + "Store must close with at least 3 associates");
                ErrorMessages.Add("Current associates: " + closeAssociates);
                count++;
            }
            if (condition6 == false && (weekday == 5 || weekday == 6))
            {
                ErrorMessages.Add(count + numeric + "Level 50 and level 60 must work every Friday and Saturday. One must open and one must close.");
                count++;
            }
            if (condition7 == false && weekday == 1)
            {
                ErrorMessages.Add(count + numeric + "Level 60 must work every Monday");
                count++;
            }

        }

        public string CreateTimeOffResponse(string name, bool approved, string date, string startTime, string endTime)
        {
            string message = "";
            string weekday = Convert.ToDateTime(date).ToString("dddd");

            if (approved = true && startTime != null)
            {
                message = name + " approved your time off request for " + date + " from " + startTime + "-" + endTime + ".";
            }
            else if (approved = true && startTime == null)
            {
                message = name + " approved your time off request for " + date + ".";
            }
            else if (approved = false && startTime != null)
            {
                message = name + " denied your time off request for " + date + " from " + startTime + "-" + endTime + ".";
            }
            else if (approved = false && startTime == null)
            {
                message = name + " denied your time off request for " + date + ".";
            }

            return message;
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

        public static string[] powerHourRows = new string[7];

        public static string[] lowHourRows = new string[21];

    }

    public class Employee
    {
        string name { get; set; }

    }

}