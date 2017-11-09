using LaborNeedsScheduling.Models;
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
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-1 * diff).Date;
        }
    }

    public class WorkWeek
    {
        #region Pending Variables
        public DataTable TrafficData = new DataTable();
        public DataTable BlackoutTableView = new DataTable();

        public double minhours = 0;
        public double maxhours = 0;
        public int openHourSlots = 2;
        public double TotalScheduledHours { get; set; }

        #region Selected Employee Scheduling
        //public string currentStoreCode = "1008";
        public List<string> ErrorMessages { get; set; }
        public int selectedWeekday { get; set; }
        public string selectedHour { get; set; }
        public string selectedEmployee { get; set; }
        public string selectedEmployeeId { get; set; }
        public string startHour { get; set; }
        public string endHour { get; set; }
        //public List<Employees> employeeListAll { get; set; }
        public List<Employees> employeeListStore { get; set; }
        public string currentStoreCode { get; set; }
        public bool employeeStatus { get; set; }
        public Dictionary<string, bool[]> employeeHourSchedule = new Dictionary<string, bool[]>();
        public int[] employeesStillNeeded { get; set; }
        public DataTable BlackoutTimes { get; set; }
        public Dictionary<string, Dictionary<string, string[]>> EmployeeAvailableTimes = new Dictionary<string, Dictionary<string, string[]>>();
        public Dictionary<string, Dictionary<string, string[]>> EmployeeScheduledTimes = new Dictionary<string, Dictionary<string, string[]>>();
        public Dictionary<string, Dictionary<DateTime, string[]>> EmployeeTimeOffRequests = new Dictionary<string, Dictionary<DateTime, string[]>>();
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

        public DataTable AssignedEmployeesRequestedWeek = new DataTable();
        #endregion

        public string[] weekdayNames = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        /// <summary>
        /// Three letter abbreviations for each day of the week
        /// </summary>
        public string[] weekdayAbv = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

        /// <summary>
        /// Set of possible hours for the schedule, hours shown in each table are based on selections of the ScheduleStartHours dictionary
        /// </summary>
        public string[] ScheduleHourSlots = { "12AM-1AM","1AM-2AM","2AM-3AM","3AM-4AM","4AM-5AM","5AM-6AM","6AM-7AM", "7AM-8AM","8AM-9AM",
                                              "9AM-10AM","10AM-11AM","11AM-12PM","12PM-1PM","1PM-2PM","2PM-3PM","3PM-4PM","4PM-5PM","5PM-6PM",
                                              "6PM-7PM","7PM-8PM","8PM-9PM","9PM-10PM","10PM-11PM","11PM-12AM"};

        public string[] ScheduleHalfHourSlots = { "12:00AM","12:30AM","1:00AM","1:30AM","2:00AM","2:30AM","3:00AM","3:30AM","4:00AM","4:30AM","5:00AM","5:30AM",
                                                  "6:00AM", "6:30AM", "7:00AM", "7:30AM","8:00AM", "8:30AM","9:00AM", "9:30AM","10:00AM", "10:30AM",
                                                  "11:00AM", "11:30AM","12:00PM","12:30PM", "1:00PM", "1:30PM", "2:00PM", "2:30PM", "3:00PM", "3:30PM",
                                                  "4:00PM", "4:30PM", "5:00PM", "5:30PM", "6:00PM", "6:30PM", "7:00PM", "7:30PM", "8:00PM", "8:30PM",
                                                  "9:00PM", "9:30PM", "10:00PM", "10:30PM", "11:00PM", "11:30PM"};

        public string[] SQLHours = {"00:00:00","00:30:00","01:00:00","01:30:00","02:00:00","02:30:00","03:00:00","03:30:00","04:00:00",
                                    "04:30:00","05:00:00","05:30:00","06:00:00", "06:30:00", "07:00:00", "07:30:00", "08:00:00", "08:30:00",
                                    "09:00:00", "09:30:00", "10:00:00", "10:30:00", "11:00:00", "11:30:00", "12:00:00", "12:30:00",
                                    "13:00:00", "13:30:00", "14:00:00", "14:30:00","15:00:00", "15:30:00", "16:00:00", "16:30:00", "17:00:00",
                                    "17:30:00", "18:00:00", "18:30:00", "19:00:00", "19:30:00", "20:00:00", "20:30:00", "21:00:00", "21:30:00",
                                    "22:00:00", "22:30:00", "23:00:00", "23:30:00"};

        /// <summary>
        /// List of times for dropdown lists and start times for the week
        /// </summary>
        public Dictionary<string, string> ScheduleStartHours = new Dictionary<string, string>()
        {
            {"12:00AM", "12:00AM"},{"12:30AM", "12:30AM"},{"1:00AM", "1:00AM"},{"1:30AM", "1:30AM"},{"2:00AM", "2:00AM"},
            { "2:30AM", "2:30AM"},{"3:00AM", "3:00AM"},{"3:30AM", "3:30AM"},{"4:00AM", "4:00AM"},{"4:30AM", "4:30AM"},
            { "5:00AM", "5:00AM"}, {"5:30AM", "5:30AM"},{"6:00AM", "6:00AM"}, {"6:30AM", "6:30AM"},{"7:00AM", "7:00AM"},
            { "7:30AM", "7:30AM"},{"8:00AM", "8:00AM"},{"8:30AM", "8:30AM"},{"9:00AM", "9:00AM"},{"9:30AM", "9:30AM"},
            { "10:00AM", "10:00AM"},{"10:30AM", "10:30AM"}, {"11:00AM", "11:00AM"},{"11:30AM", "11:30AM"},{"12:00PM", "12:00PM"},
            { "12:30PM", "12:30PM"},{"1:00PM", "1:00PM"},{"1:30PM", "1:30PM"},{"2:00PM", "2:00PM"},{"2:30PM", "2:30PM"},
            { "3:00PM", "3:00PM"},{"3:30PM", "3:30PM"},{"4:00PM", "4:00PM"},{"4:30PM", "4:30PM"},{"5:00PM", "5:00PM"},
            { "5:30PM", "5:30PM"},{"6:00PM", "6:00PM"},{"6:30PM", "6:30PM"},{"7:00PM", "7:00PM"},{"7:30PM", "7:30PM"},
            { "8:00PM", "8:00PM"},{"8:30PM", "8:30PM"},{"9:00PM", "9:00PM"},{"9:30PM", "9:30PM"},{"10:00PM", "10:00PM"},
            { "10:30PM", "10:30PM"},{"11:00PM", "11:00PM"},{"11:30PM", "11:30PM"}
        };

        /// <summary>
        /// List of hour end times for dropdown lists
        /// </summary>
        public Dictionary<string, string> ScheduleEndHours = new Dictionary<string, string>()
        {
            {"12:30AM", "12:00AM"},{"1:00AM", "12:30AM"},{"1:30AM", "1:00AM"},{"2:00AM", "1:30AM"},{"2:30AM", "2:00AM"},
            { "3:00AM", "2:30AM"},{"3:30AM", "3:00AM"},{"4:00AM", "3:30AM"},{"4:30AM", "4:00AM"},{"5:00AM", "4:30AM"},
            { "5:30AM", "5:00AM"},{"6:00AM", "5:30AM"},{"6:30AM", "6:00AM"},{"7:00AM", "6:30AM"},{"7:30AM", "7:00AM"},
            { "8:00AM", "7:30AM"},{"8:30AM", "8:00AM"},{"9:00AM", "8:30AM"},{"9:30AM", "9:00AM"}, { "10:00AM", "9:30AM"},
            { "10:30AM", "10:00AM"},{"11:00AM", "10:30AM"},{"11:30AM", "11:00AM"},{"12:00PM", "11:30AM"},{"12:30PM", "12:00PM"},
            { "1:00PM", "12:30PM"},{"1:30PM", "1:00PM"},{"2:00PM", "1:30PM"},{"2:30PM", "2:00PM"},{"3:00PM", "2:30PM"},
            { "3:30PM", "3:00PM"},{"4:00PM", "3:30PM"},{"4:30PM", "4:00PM"},{"5:00PM", "4:30PM"},{"5:30PM", "5:00PM"},
            { "6:00PM", "5:30PM"},{"6:30PM", "6:00PM"}, {"7:00PM", "6:30PM"},{"7:30PM", "7:00PM"},{"8:00PM", "7:30PM"},
            { "8:30PM", "8:00PM"},{"9:00PM", "8:30PM"},{"9:30PM", "9:00PM"},{"10:00PM", "9:30PM"},{"10:30PM", "10:00PM"},
            { "11:00PM", "10:30PM"},{"11:30PM", "11:00PM"},{"12:00AM", "11:30AM"}
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

        public DataTable WeekStartEndHours { get; set; }

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
        public int[] weekWeighting { get; set; }

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
        public int MinEmployeesEarly { get; set; }
        public int MinEmployeesLater { get; set; }

        /// <summary>
        /// Set the maximum number of employees that can be on the floor
        /// </summary>
        public int MaxEmployees { get; set; }

        //public int MinEmployeesDefault { get; set; }

        //public int MaxEmployeesDefault { get; set; }

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
        /// The most recent Sunday
        /// </summary>
        public static DateTime weekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
        //public static DateTime currentWeekMarker = DateTime.Today.AddDays(-1 * (int)DayOfWeek.Saturday);

        DateTime currentWeekMarker = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);

        /// <summary>
        /// Dates for the current week
        /// </summary>
        public DateTime[] CurrentWeekDates = new DateTime[7];
        public DateTime[] OneWeekFromNowDates = new DateTime[7];
        public DateTime[] TwoWeeksFromNowDates = new DateTime[7];
        public DateTime[] ThreeWeeksFromNowDates = new DateTime[7];
        public DateTime[] NextFourWeeksDates = new DateTime[28];
        public DateTime[] RequestedDates = new DateTime[7];
        public string startdateCurrentWeek { get; set; }
        public string enddateCurrentWeek { get; set; }
        public string startdateOneWeek { get; set; }
        public string enddateOneWeek { get; set; }
        public string startdateTwoWeeks { get; set; }
        public string enddateTwoWeeks { get; set; }
        public string startdateThreeWeeks { get; set; }
        public string enddateThreeWeeks { get; set; }
        public string startdateRequested { get; set; }
        public string enddateRequested { get; set; }


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
        public int[] getDefaultWeights(int numberOfWeeks)
        {
            //int[] defaultWeightingValues = { 36, 24, 16, 12, 8, 4 };
            int[] WeightingValues = FakeAPI.GetWeekWeighting(numberOfWeeks, currentStoreCode);

            return WeightingValues;
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
                    for (int j = 1; j < 8; j++)
                    {
                        ExclusionDates.Add(weekMarker.AddDays(-j - (7 * i)), false);
                    }
                }
            }
            else
            {
                for (int i = 0; i < DefaultHistoricalWeeks; i++)
                {
                    for (int j = 1; j < 8; j++)
                    {
                        ExclusionDates.Add(weekMarker.AddDays(-j - (7 * i)), false);
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
                WeekStartEndHours = FakeAPI.GetStoreHours(currentStoreCode);

                // weighting input
                for (int i = 0; i < weekWeighting.Length; i++)
                {
                    if (weekWeighting[i] == 0)
                    {
                        weekWeighting = getDefaultWeights(weekWeighting.Length);
                    }
                }

                //employeeListAll = FakeAPI.GetAllEmployees();
                if (employeeListStore.Count <= FakeAPI.GetEmployeesForStore(currentStoreCode).Count)
                {
                    employeeListStore = FakeAPI.GetEmployeesForStore(currentStoreCode);
                }

                //GenerateCurrentWeekDates();
                //FakeAPI.AddNewWeekDates(OneWeekFromNowDates);
                //FakeAPI.dothething(OneWeekFromNowDates);

                GenerateWeeklyTimeSlots();
                WTGTrafficPercent = TrafficData;/*FakeAPI.FillWTGTable(weekWeighting, NumberHistoricalWeeks, DateTime.Parse("2015-05-10"));*/

                GenerateBlackoutTable();
                FillExcludedDatesTable();
                FillWeightedAverageTrafficTable();
                FillPercentWeeklyTable();
                FillPercentDailyTable();
                FillAllocatedHoursTable(RequestedDates);
                FillPowerHourForecastTable();
                RedistributeHours();
                GenerateAllocatedHoursDisplay(RequestedDates);
                GeneratePowerHourCells();
                GetBlackoutCells(AllocatedHours);

                if (selectedEmployeeId != null)
                {
                    UpdateEmployees(employeeListStore);
                }

                FillAssignmentTable(selectedWeekday);
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
            for (int i = 0; i < 7; i++)
            {
                CurrentWeekDates[i] = currentWeekMarker.AddDays(i);
                OneWeekFromNowDates[i] = currentWeekMarker.AddDays(i + 7);
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
        public void GenerateEmployeeAvailability()
        {
            //GenerateCurrentWeekDates();
            string[] weekdays = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            //string[] employeeIdsAll = new string[LaborScheduling.EmployeeListAll.Count];
            string[] employeeIdsStore = new string[employeeListStore.Count];

            // creating each day for an employee
            for (int i = 0; i < employeeIdsStore.Length; i++)
            {
                employeeIdsStore[i] = employeeListStore[i].id;
            }

            for (int i = 0; i < 28; i++)
            {
                NextFourWeeksDates[i] = currentWeekMarker.AddDays(i);
            }

            //FakeAPI.CheckForCurrentWeek(employeeListStore, RequestedDates);
            EmployeeAvailableTimes = FakeAPI.GetEmployeeAvailableTimes(employeeIdsStore);
            EmployeeTimeOffRequests = FakeAPI.GetEmployeeTimeOff(currentStoreCode, NextFourWeeksDates);
            List<string> idList = new List<string>(EmployeeTimeOffRequests.Keys);

            for (int i = 0; i < employeeListStore.Count; i++)
            {
                for (int n = 0; n < idList.Count; n++)
                {
                    if (employeeListStore[i].id == idList[n])
                    {
                        Dictionary<string, string[]> employeeAvailability = EmployeeAvailableTimes[idList[n]];
                        Dictionary<DateTime, string[]> employeeTimeOff = EmployeeTimeOffRequests[idList[n]];

                        for (int j = 0; j < RequestedDates.Length; j++)
                        {
                            string currentDay = weekdays[j];
                            DateTime currentDate = RequestedDates[j];

                            string[] dayAvailability = employeeAvailability[currentDay];
                            string[] dayTimeOff = employeeTimeOff[currentDate];

                            for (int h = 0; h < dayAvailability.Length; h++)
                            {
                                for (int m = 0; m < dayTimeOff.Length; m++)
                                {
                                    if (dayAvailability[h] == dayTimeOff[m])
                                    {
                                        dayAvailability[h] = "";
                                    }
                                }
                            }

                            var updatedTimes = new List<string>(dayAvailability);

                            for (int k = dayAvailability.Length - 1; k >= 0; k--)
                            {
                                if (dayAvailability[k] == "")
                                {
                                    updatedTimes.RemoveAt(k);
                                }
                            }

                            employeeAvailability[currentDay] = updatedTimes.ToArray();
                        }
                        EmployeeAvailableTimes[idList[n]] = employeeAvailability;
                    }
                }
            }

            EmployeeScheduledTimes = FakeAPI.GetEmployeeScheduledTimes(employeeIdsStore, currentStoreCode, RequestedDates);
        }

        /// <summary>
        /// Chec the selected employee's schedule for conflicts with their availability
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="weekday"></param>
        public void CheckScheduleConflicts(string employeeId, int weekday)
        {
            List<Employees> EmployeeList = FakeAPI.GetEmployeesForStore(currentStoreCode);
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
                    Conflicts.Add(employeeName + " is assigned at " + scheduledHour + " but is not available.");
                }
            }
            ScheduleConflicts = Conflicts;
        }

        public DataTable FindEmployee(string EmployeeId)
        {
            Employees FoundEmployee = FakeAPI.FindEmployee(EmployeeId);

            DataTable EmployeeInfo = new DataTable();

            if (FoundEmployee.id == "0")
            {
                EmployeeInfo.Columns.Add("Employee not found");
            }
            else
            {
                EmployeeInfo.Columns.Add("Name");
                EmployeeInfo.Columns.Add("Id");
                EmployeeInfo.Columns.Add("Store Code");
                EmployeeInfo.Columns.Add("Role");
                EmployeeInfo.Columns.Add("Level");

                DataRow row = EmployeeInfo.NewRow();
                row["Name"] = FoundEmployee.firstName + " " + FoundEmployee.lastName;
                row["Id"] = FoundEmployee.id;
                row["Store Code"] = FoundEmployee.storeCode;
                row["Role"] = FoundEmployee.role;
                row["Level"] = FoundEmployee.rank;
                EmployeeInfo.Rows.Add(row);
            }

            return EmployeeInfo;
        }

        /// <summary>
        /// Add an employee from another store to the current store's roster
        /// </summary>
        /// <param name="employeeId"></param>
        public void AddEmployeeToList(string employeeId)
        {
            employeeListStore = FakeAPI.GetEmployeesForStore(currentStoreCode);
            bool employeeExists = false;
            foreach (Employees emp in employeeListStore)
            {
                if (emp.id == employeeId)
                {
                    employeeExists = true;
                }
            }

            for (int i = 0; i < 7; i++)
            {
                NextFourWeeksDates[i] = CurrentWeekDates[i];
                NextFourWeeksDates[i + 7] = OneWeekFromNowDates[i];
                NextFourWeeksDates[i + 14] = TwoWeeksFromNowDates[i];
                NextFourWeeksDates[i + 21] = ThreeWeeksFromNowDates[i];
            }

            if (employeeExists == false)
            {
                FakeAPI.AddBorrowedEmployee(employeeId, currentStoreCode, NextFourWeeksDates);
            }
        }


        public void GenerateBlackoutTable()
        {
            BlackoutTableView.Clear();
            BlackoutTableView.Columns.Clear();
            BlackoutTableView.Columns.Add("HourOfDay", typeof(string));
            BlackoutTableView.Columns.Add("Sunday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[0]), typeof(string));
            BlackoutTableView.Columns.Add("Monday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[1]), typeof(string));
            BlackoutTableView.Columns.Add("Tuesday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[2]), typeof(string));
            BlackoutTableView.Columns.Add("Wednesday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[3]), typeof(string));
            BlackoutTableView.Columns.Add("Thursday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[4]), typeof(string));
            BlackoutTableView.Columns.Add("Friday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[5]), typeof(string));
            BlackoutTableView.Columns.Add("Saturday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[6]), typeof(string));

            List<string[]> preOpenSlots = new List<string[]>();
            List<string[]> postCloseSlots = new List<string[]>();

            string[] openAddedSlots = new string[openHourSlots];
            string[] closeAddedSlots = new string[2];

            List<string> UpdatedHourSchedule = new List<string>();
            foreach (string s in WeekHourSchedule)
            {
                UpdatedHourSchedule.Add(s);
            }

            for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
            {
                if (ScheduleHalfHourSlots[i] == UpdatedHourSchedule[0])
                {
                    for (int n = 0; n < openHourSlots; n++)
                    {
                        openAddedSlots[n] = ScheduleHalfHourSlots[i - (n + 1)];
                    }
                }
                if (ScheduleHalfHourSlots[i] == UpdatedHourSchedule[UpdatedHourSchedule.Count - 1])
                {
                    for (int n = 0; n < 2; n++)
                    {
                        closeAddedSlots[n] = ScheduleHalfHourSlots[i + (n + 1)];
                    }
                }
            }

            for (int i = 0; i < openAddedSlots.Length; i++)
            {
                UpdatedHourSchedule.Insert(0, openAddedSlots[i]);
            }
            for (int i = 0; i < closeAddedSlots.Length; i++)
            {
                UpdatedHourSchedule.Insert(UpdatedHourSchedule.Count, closeAddedSlots[i]);
            }

            for (int i = 0; i < UpdatedHourSchedule.Count; i++)
            {
                BlackoutTableView.Rows.Add(UpdatedHourSchedule[i]);
            }

            for (int i = 1; i < 8; i++)
            {
                int startPosition = 0;
                int endPosition = 0;
                for (int n = 0; n < BlackoutTableView.Rows.Count; n++)
                {
                    if (BlackoutTableView.Rows[n][0].ToString() == WeekStartEndHours.Rows[0][i - 1].ToString())
                    {
                        startPosition = n;
                    }
                    if (BlackoutTableView.Rows[n][0].ToString() == WeekStartEndHours.Rows[1][i - 1].ToString())
                    {
                        endPosition = n;
                    }
                }
                if (startPosition - openHourSlots >= 0)
                {
                    startPosition -= openHourSlots;
                }
                if (endPosition < BlackoutTableView.Rows.Count - 2)
                {
                    endPosition += 2;
                }

                for (int n = 0; n < BlackoutTableView.Rows.Count; n++)
                {
                    if (n < startPosition || n > endPosition)
                    {
                        BlackoutTableView.Rows[n][i] = "True";
                    }
                    else
                    {
                        BlackoutTableView.Rows[n][i] = "False";
                    }
                }
            }
        }


        /// <summary>
        /// Determine which cells should be blacked out on the main labor schedule based on individual start and end times
        /// </summary>
        /// <param name="AllocatedHours"></param>
        public void GetBlackoutCells(DataTable AllocatedHours)
        {
            Dictionary<int, string[]> blackoutTimes = new Dictionary<int, string[]>();
            DataTable BlackoutTable = AllocatedHours.Copy();

            for (int weekday = 0; weekday < 7; weekday++)
            {
                string starthour = "";
                string endhour = "";
                if (WeekStartEndHours != null)
                {
                    starthour = WeekStartEndHours.Rows[0][weekday].ToString();
                    endhour = WeekStartEndHours.Rows[1][weekday].ToString();
                }
                else
                {
                    starthour = WeekStartHours[weekday];
                    endhour = WeekEndHours[weekday];
                }

                List<string> times = new List<string>();

                string OpenHour = starthour;
                string CloseHour = endhour;
                string StartHour = "";
                string EndHour = "";
                for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
                {
                    if (OpenHour == ScheduleHalfHourSlots[i] && i > openHourSlots)
                    {
                        StartHour = ScheduleHalfHourSlots[i - openHourSlots];
                    }
                    if (CloseHour == ScheduleHalfHourSlots[i] && i < ScheduleHalfHourSlots.Length - 2)
                    {
                        EndHour = ScheduleHalfHourSlots[i + 2];
                    }
                }

                for (int i = 0; i < AllocatedHours.Rows.Count; i++)
                {
                    if (AllocatedHours.Rows[i][0].ToString() == StartHour)
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

                    if (AllocatedHours.Rows[i][0].ToString() == EndHour)
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
                //if (WeekStartHour == null && WeekEndHour == null)
                //{
                //    WeekStartHour = ScheduleHalfHourSlots[0];
                //    WeekEndHour = ScheduleHalfHourSlots[ScheduleHalfHourSlots.Length - 1];
                //}
                List<int> OpenHourPositions = new List<int>();
                List<int> CloseHourPositions = new List<int>();
                for (int n = 0; n < 7; n++)
                {
                    for (int j = 0; j < ScheduleHalfHourSlots.Length; j++)
                    {
                        if (WeekStartEndHours.Rows[0][n].ToString() == ScheduleHalfHourSlots[j])
                        {
                            OpenHourPositions.Add(j);
                        }
                        if (WeekStartEndHours.Rows[1][n].ToString() == ScheduleHalfHourSlots[j])
                        {
                            CloseHourPositions.Add(j);
                        }
                    }
                }
                int[] opens = OpenHourPositions.ToArray();
                int[] closes = CloseHourPositions.ToArray();

                int openhour = opens.Min();
                int closehour = closes.Max();

                string start = ScheduleHalfHourSlots[openhour];
                string end = ScheduleHalfHourSlots[closehour];

                for (int j = 0; j < ScheduleHalfHourSlots.Length; j++)
                {
                    if (ScheduleHalfHourSlots[j] == end)
                    {
                        WeekHourSchedule.Add(ScheduleHalfHourSlots[j]);
                        break;
                    }

                    if (ScheduleHalfHourSlots[j] == start)
                    {
                        startCheck = true;
                    }

                    if (startCheck == true)
                    {
                        //WeekHourSchedule[counter] = ScheduleHourSlots[j];
                        WeekHourSchedule.Add(ScheduleHalfHourSlots[j]);
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
            PowerHourCells = new bool[400];
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
                                    for (int j = 1; j < powerHourAmount * 2; j++)
                                    {
                                        PowerHourCells[y + (j * 8)] = true;
                                    }
                                }
                                else
                                {
                                    for (int j = 1; j < powerHourAmountWeekend * 2; j++)
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

            int[] columnCount = new int[42];
            for (int i = 0; i < columnCount.Length; i++)
            {
                if (i == 0 || i == 1 || i == 2 || i == 3 || i == 4 || i == 5) columnCount[i] = 1;
                if (i == 6 || i == 7 || i == 8 || i == 9 || i == 10 || i == 11) columnCount[i] = 2;
                if (i == 12 || i == 13 || i == 14 || i == 15 || i == 16 || i == 17) columnCount[i] = 3;
                if (i == 18 || i == 19 || i == 20 || i == 21 || i == 22 || i == 23) columnCount[i] = 4;
                if (i == 24 || i == 25 || i == 26 || i == 27 || i == 28 || i == 29) columnCount[i] = 5;
                if (i == 30 || i == 31 || i == 32 || i == 33 || i == 34 || i == 35) columnCount[i] = 6;
                if (i == 36 || i == 37 || i == 38 || i == 39 || i == 40 || i == 41) columnCount[i] = 7;
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
                                //LowHourCells[y + (8)] = true;
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

        /// <summary>
        /// Uses the data from the WTGTrafficPercent table and excludes the dates specified by the manager. 
        /// These dates will be ignored by calculations so they do not affect any weighting.
        /// </summary>
        private void FillExcludedDatesTable()
        {
            try
            {
                List<DateTime> DatesToExclude = FakeAPI.GetExcludedDates(currentStoreCode);

                DateTime[] days = new DateTime[ExclusionDates.Count];
                int counter = 0;
                foreach (DateTime date in ExclusionDates.Keys)
                {
                    days[counter] = date;
                    counter++;
                }

                for (int i = 0; i < ExclusionDates.Keys.Count; i++)
                {
                    ExclusionDates[days[i]] = false;

                    for (int n = 0; n < DatesToExclude.Count; n++)
                    {
                        if (DatesToExclude[n] == days[i])
                        {
                            ExclusionDates[days[i]] = true;
                        }
                    }
                }

                ExcludedDates = WTGTrafficPercent.Clone();

                #region Exclusion Check
                foreach (DataRow row in WTGTrafficPercent.Rows)
                {
                    // if the date in the dictionary is true don't add it to the table
                    DateTime date = DateTime.Parse(row["TrafficDate"].ToString()).Date;
                    if (!ExclusionDates[DateTime.Parse(row["TrafficDate"].ToString()).Date])
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
            WeightedAverageTraffic.Rows.Clear();

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

                //string[] hours = { "9AM-10AM","10AM-11AM","11AM-12PM","12PM-1PM", "1PM-2PM", "2PM-3PM", "3PM-4PM",
                //                   "4PM-5PM", "5PM-6PM", "6PM-7PM", "7PM-8PM", "8PM-9PM", "9PM-10PM", "10PM-11PM" };

                // get the dates for each weekday from weeks past depending on historical weeks variable
                DateTime[] Sundays = new DateTime[NumberHistoricalWeeks];
                DateTime[] Mondays = new DateTime[NumberHistoricalWeeks];
                DateTime[] Tuesdays = new DateTime[NumberHistoricalWeeks];
                DateTime[] Wednesdays = new DateTime[NumberHistoricalWeeks];
                DateTime[] Thursdays = new DateTime[NumberHistoricalWeeks];
                DateTime[] Fridays = new DateTime[NumberHistoricalWeeks];
                DateTime[] Saturdays = new DateTime[NumberHistoricalWeeks];
                for (int i = 1; i <= NumberHistoricalWeeks; i++)
                {
                    Sundays[i - 1] = weekMarker.AddDays(-(i * 7));
                    Mondays[i - 1] = weekMarker.AddDays(-(i * 7 - 1));
                    Tuesdays[i - 1] = weekMarker.AddDays(-(i * 7 - 2));
                    Wednesdays[i - 1] = weekMarker.AddDays(-(i * 7 - 3));
                    Thursdays[i - 1] = weekMarker.AddDays(-(i * 7 - 4));
                    Fridays[i - 1] = weekMarker.AddDays(-(i * 7 - 5));
                    Saturdays[i - 1] = weekMarker.AddDays(-(i * 7 - 6));
                }
                List<DateTime[]> WeekdayDates = new List<DateTime[]>();
                WeekdayDates.Add(Sundays);
                WeekdayDates.Add(Mondays);
                WeekdayDates.Add(Tuesdays);
                WeekdayDates.Add(Wednesdays);
                WeekdayDates.Add(Thursdays);
                WeekdayDates.Add(Fridays);
                WeekdayDates.Add(Saturdays);

                for (int i = 0; i < 7; i++)
                {
                    DataRow[] datarow;
                    DateTime[] Dates = WeekdayDates[i];
                    double DayTotalHours = 0;

                    int startslot = 0;
                    for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                    {
                        if (WeekHourSchedule[0] == ScheduleHalfHourSlots[n])
                        {
                            startslot = n / 2;
                        }
                    }


                    for (int n = 0; n <= WeekHourSchedule.Count - 1; n++)
                    {
                        string SQLTime = "";
                        for (int m = 0; m < ScheduleHalfHourSlots.Length; m++)
                        {
                            if (WeekHourSchedule[n] == ScheduleHalfHourSlots[m])
                            {
                                SQLTime = SQLHours[m];
                                break;
                            }
                        }

                        double HourTotal = 0;

                        for (int m = 0; m < NumberHistoricalWeeks; m++)
                        {
                            //datarow = ExcludedDates.Select("WeekDay = '" + weekdayAbv[col - 1] + "' AND HourOfDay = '" + ScheduleHourSlots[n] + "'");
                            datarow = ExcludedDates.Select("TrafficDate = '" + Dates[m] + "' and TrafficTime = '" + SQLTime + "'");
                            if (datarow.Length != 0)
                            {
                                HourTotal += Convert.ToDouble(datarow[0]["TrafficCount"]);
                            }
                        }

                        int RowPosition = 0;
                        for (int m = 0; m < WeightedAverageTraffic.Rows.Count; m++)
                        {
                            if (WeightedAverageTraffic.Rows[m][0].ToString() == WeekHourSchedule[n])
                            {
                                RowPosition = m;
                            }
                        }
                        WeightedAverageTraffic.Rows[RowPosition][i + 1] = HourTotal;

                        //if (datarow.Length == 0)
                        //{
                        //    WeightedAverageTraffic.Rows[row][col] = 0;
                        //    row++;
                        //}

                        //for (int j = 0; j < datarow.Length; j++)
                        //{
                        //    if (j < datarow.Length - 1)
                        //    {
                        //        total += Convert.ToDouble(datarow[j]["TrafficCount"]);
                        //    }
                        //    else
                        //    {
                        //        total += Convert.ToDouble(datarow[j]["TrafficCount"]);
                        //        Math.Round(total, MidpointRounding.AwayFromZero);
                        //        WeightedAverageTraffic.Rows[row][col] = (Math.Round(total, MidpointRounding.AwayFromZero));
                        //        total = 0;
                        //        row++;
                        //    }
                        //}

                        DayTotalHours += HourTotal;
                    }
                    //row = 0;
                    //col++;
                    WeightedAverageTraffic.Rows[WeightedAverageTraffic.Rows.Count - 1][i + 1] = DayTotalHours;
                }

                for (int i = 0; i < WeightedAverageTraffic.Rows.Count; i++)
                {
                    double HourTotal = 0;
                    for (int n = 1; n < WeightedAverageTraffic.Columns.Count - 1; n++)
                    {
                        HourTotal += Convert.ToDouble(WeightedAverageTraffic.Rows[i][n]);
                    }
                    WeightedAverageTraffic.Rows[i][8] = HourTotal;
                }

                //#region Totals
                //int totalCol = 1;
                //double[] daySums = new double[7];

                //for (int i = 0; i < 7; i++)
                //{
                //    daySums[i] = (double)WeightedAverageTraffic.Compute("Sum(" + weekdayAbv[i] + "WeightedAverage)", "");
                //    WeightedAverageTraffic.Rows[WeightedAverageTraffic.Rows.Count - 1][totalCol] = daySums[i];
                //    totalCol++;
                //}

                //foreach (DataRow trow in WeightedAverageTraffic.Rows)
                //{
                //    double rowSum = 0;
                //    foreach (DataColumn tcol in WeightedAverageTraffic.Columns)
                //    {
                //        if (!trow.IsNull(tcol))
                //        {
                //            string stringValue = trow[tcol].ToString();
                //            double d;
                //            if (double.TryParse(stringValue, out d))
                //                rowSum += d;
                //        }
                //    }
                //    trow.SetField("Total", rowSum);
                //}

                //#endregion
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
            //#region Total WTG Table
            //#region Table Setup
            //WeightedAverageTrafficTotal.Clear();
            //WeightedAverageTrafficTotal.Columns.Clear();
            WeightedAverageTrafficTotal = WeightedAverageTraffic;

            //WeightedAverageTrafficTotal.Columns.Add("HourOfDay", typeof(string));
            //WeightedAverageTrafficTotal.Columns.Add("SunWeightedAverage", typeof(double));
            //WeightedAverageTrafficTotal.Columns.Add("MonWeightedAverage", typeof(double));
            //WeightedAverageTrafficTotal.Columns.Add("TueWeightedAverage", typeof(double));
            //WeightedAverageTrafficTotal.Columns.Add("WedWeightedAverage", typeof(double));
            //WeightedAverageTrafficTotal.Columns.Add("ThuWeightedAverage", typeof(double));
            //WeightedAverageTrafficTotal.Columns.Add("FriWeightedAverage", typeof(double));
            //WeightedAverageTrafficTotal.Columns.Add("SatWeightedAverage", typeof(double));
            //WeightedAverageTrafficTotal.Columns.Add("Total", typeof(double));


            //for (int i = 0; i < WeekHourSchedule.Count; i++)
            //{
            //    WeightedAverageTrafficTotal.Rows.Add(WeekHourSchedule[i]);
            //}
            //WeightedAverageTrafficTotal.Rows.Add("Total");
            //#endregion

            //try
            //{

            //    #region Calculations

            //    double total = 0;
            //    int col = 1;
            //    int row = 0;

            //    for (int i = 0; i < 7; i++)
            //    {
            //        DataRow[] datarow;
            //        int startslot = 0;
            //        for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
            //        {
            //            if (WeekHourSchedule[0] == ScheduleHalfHourSlots[n])
            //            {
            //                startslot = n / 2;
            //            }
            //        }
            //        for (int n = 0; n < WeekHourSchedule.Count; n++)
            //        {
            //            int wronghourcount = (n / 2);
            //            datarow = ExcludedDates.Select("WeekDay = '" + weekdayAbv[col - 1] + "' AND HourOfDay = '" + ScheduleHourSlots[startslot + wronghourcount] + "'");

            //            if (datarow.Length == 0)
            //            {
            //                WeightedAverageTrafficTotal.Rows[row][col] = 0;
            //                WeightedAverageTrafficTotal.Rows[row][col] = 0;
            //                row += 1;
            //            }

            //            for (int j = 0; j < datarow.Length; j++)
            //            {
            //                if (j < datarow.Length - 1)
            //                {
            //                    total += Convert.ToDouble(datarow[j]["TrafficOut"]);
            //                }
            //                else
            //                {
            //                    total += Convert.ToDouble(datarow[j]["TrafficOut"]);
            //                    Math.Round(total, MidpointRounding.AwayFromZero);
            //                    WeightedAverageTrafficTotal.Rows[row][col] = Math.Round((Math.Round(total, MidpointRounding.AwayFromZero) / 2));
            //                    total = 0;
            //                    row += 1;
            //                }
            //            }
            //        }

            //        row = 0;
            //        col++;
            //    }

            //    #region Totals
            //    int totalCol = 1;
            //    double[] daySums = new double[7];

            //    for (int i = 0; i < 7; i++)
            //    {
            //        daySums[i] = (double)WeightedAverageTrafficTotal.Compute("Sum(" + weekdayAbv[i] + "WeightedAverage)", "");
            //        WeightedAverageTrafficTotal.Rows[WeightedAverageTrafficTotal.Rows.Count - 1][totalCol] = daySums[i];
            //        totalCol++;
            //    }

            //    foreach (DataRow trow in WeightedAverageTrafficTotal.Rows)
            //    {
            //        double rowSum = 0;
            //        foreach (DataColumn tcol in WeightedAverageTrafficTotal.Columns)
            //        {
            //            if (!trow.IsNull(tcol))
            //            {
            //                string stringValue = trow[tcol].ToString();
            //                double d;
            //                if (double.TryParse(stringValue, out d))
            //                    rowSum += d;
            //            }
            //        }
            //        trow.SetField("Total", rowSum);
            //    }

            //    #endregion
            //    #endregion

            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            //#endregion

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


            for (int i = 0; i < WeekHourSchedule.Count; i++)
            {
                PercentWeeklyTotal.Rows.Add(WeekHourSchedule[i]);
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
                        if (WeightedAverageTrafficTotal.Rows[row][col].ToString() != "")
                        {
                            double hourAvg = Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row][col]);

                            weeklyPercent = (hourAvg / weeklyTotal) * 100;
                            PercentWeeklyTotal.Rows[row][col] = Math.Round(weeklyPercent, 1, MidpointRounding.AwayFromZero);
                        }
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

            for (int i = 0; i < WeekHourSchedule.Count; i++)
            {
                PercentDailyTotal.Rows.Add(WeekHourSchedule[i]);
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
                        //if (WeightedAverageTrafficTotal.Rows[row][col].ToString() != "")
                        //{
                        double hourAvg = Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row][col]);

                        dailyPercent = (hourAvg / dailyTotal) * 100;
                        PercentDailyTotal.Rows[row][col] = Math.Round(dailyPercent, 1, MidpointRounding.AwayFromZero);
                        //}
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
        public DataTable FillAllocatedHoursTable(DateTime[] RequestedDates)
        {
            #region Table Setup
            AllocatedHours.Clear();
            AllocatedHours.Columns.Clear();
            AllocatedHours.Columns.Add("HourOfDay", typeof(string));
            AllocatedHours.Columns.Add("Sunday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[0]), typeof(string));
            AllocatedHours.Columns.Add("Monday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[1]), typeof(string));
            AllocatedHours.Columns.Add("Tuesday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[2]), typeof(string));
            AllocatedHours.Columns.Add("Wednesday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[3]), typeof(string));
            AllocatedHours.Columns.Add("Thursday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[4]), typeof(string));
            AllocatedHours.Columns.Add("Friday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[5]), typeof(string));
            AllocatedHours.Columns.Add("Saturday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[6]), typeof(string));

            List<string[]> preOpenSlots = new List<string[]>();
            List<string[]> postCloseSlots = new List<string[]>();

            string[] openAddedSlots = new string[openHourSlots];
            string[] closeAddedSlots = new string[2];

            List<string> UpdatedHourSchedule = new List<string>();
            foreach (string s in WeekHourSchedule)
            {
                UpdatedHourSchedule.Add(s);
            }

            for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
            {
                if (ScheduleHalfHourSlots[i] == UpdatedHourSchedule[0])
                {
                    for (int n = 0; n < openHourSlots; n++)
                    {
                        openAddedSlots[n] = ScheduleHalfHourSlots[i - (n + 1)];
                    }
                }
                if (ScheduleHalfHourSlots[i] == UpdatedHourSchedule[UpdatedHourSchedule.Count - 1])
                {
                    for (int n = 0; n < 2; n++)
                    {
                        closeAddedSlots[n] = ScheduleHalfHourSlots[i + (n + 1)];
                    }
                }
            }

            for (int i = 0; i < openAddedSlots.Length; i++)
            {
                UpdatedHourSchedule.Insert(0, openAddedSlots[i]);
            }
            for (int i = 0; i < closeAddedSlots.Length; i++)
            {
                UpdatedHourSchedule.Insert(UpdatedHourSchedule.Count, closeAddedSlots[i]);
            }

            for (int i = 0; i < UpdatedHourSchedule.Count; i++)
            {
                AllocatedHours.Rows.Add(UpdatedHourSchedule[i]);
            }

            #endregion

            try
            {

                #region Calculations

                double OriginalPayrollWeeklyHours = PayrollWeeklyHours;
                double[] allocatedHours = new double[7];
                minhours = 0;
                maxhours = 0;

                double hourtotal = 0;

                for (int i = 0; i < WeekStartEndHours.Columns.Count; i++)
                {
                    for (int n = 0; n < AllocatedHours.Rows.Count; n++)
                    {
                        if (AllocatedHours.Rows[n][0].ToString() == WeekStartEndHours.Rows[0][i].ToString() &&
                            n > openHourSlots - 1)
                        {
                            for (int m = 1; m <= openHourSlots; m++)
                            {
                                AllocatedHours.Rows[n - m][i + 1] = MinEmployeesEarly;
                                PayrollWeeklyHours -= Convert.ToDouble(MinEmployeesEarly) / 2;
                            }
                        }
                    }
                }
                for (int i = 0; i < WeekStartEndHours.Columns.Count; i++)
                {
                    for (int n = 0; n < AllocatedHours.Rows.Count; n++)
                    {
                        if (AllocatedHours.Rows[n][0].ToString() == WeekStartEndHours.Rows[1][i].ToString() &&
                            n < AllocatedHours.Rows.Count - 2)
                        {
                            for (int m = 1; m <= 2; m++)
                            {
                                AllocatedHours.Rows[n + m][i + 1] = MinEmployeesLater;
                                PayrollWeeklyHours -= Convert.ToDouble(MinEmployeesLater) / 2;
                            }
                        }
                    }
                }

                for (int i = 0; i < 7; i++)
                {
                    double percentWeeklyTotal = (Convert.ToDouble(PercentWeeklyTotal.Rows[PercentWeeklyTotal.Rows.Count - 1][weekdayAbv[i] + "PercentOfWeeklyTotal"]) / 100);
                    allocatedHours[i] = Math.Round((PayrollWeeklyHours * percentWeeklyTotal) * 2);
                    hourtotal += allocatedHours[i];
                }

                for (int col = 1; col < 8; col++)
                {
                    List<string> addedhours = new List<string>();
                    for (int row = 0; row < PercentWeeklyTotal.Rows.Count - 1; row++)
                    {
                        if (PercentDailyTotal.Rows[row][col].ToString() != "" && BlackoutTableView.Rows[row + openHourSlots][col].ToString() != "True")
                        {
                            double hourPercentage = (Convert.ToDouble(PercentDailyTotal.Rows[row][col]) / 100);
                            double employees = hourPercentage * allocatedHours[col - 1];

                            string time = PercentDailyTotal.Rows[row][0].ToString();
                            int hourposition = 0;
                            for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
                            {
                                if (time == ScheduleHalfHourSlots[i])
                                {
                                    hourposition = i;
                                    break;
                                }
                            }

                            // set min employees based on time of day (currently changes at 1:00pm)
                            if (employees < MinEmployeesEarly - .5 && hourposition < 26)
                            {
                                int f = Convert.ToInt32(employees);
                                minhours += (MinEmployeesEarly - Convert.ToDouble(f));
                                addedhours.Add(AllocatedHours.Rows[row][0].ToString() + " - " + (MinEmployeesEarly - Convert.ToDouble(f)) / 2);

                                employees = MinEmployeesEarly;
                            }
                            else if (employees < MinEmployeesLater - .5 && hourposition >= 26)
                            {
                                int f = Convert.ToInt32(employees);
                                minhours += (MinEmployeesLater - Convert.ToDouble(f));
                                addedhours.Add(AllocatedHours.Rows[row][0].ToString() + " - " + (MinEmployeesEarly - Convert.ToDouble(f)) / 2);

                                employees = MinEmployeesLater;
                            }
                            if (employees > MaxEmployees)
                            {
                                maxhours += (Convert.ToInt32(employees) - MaxEmployees);

                                employees = MaxEmployees;
                            }

                            AllocatedHours.Rows[row + openHourSlots][col] = Math.Round(employees, 0);
                        }
                    }

                    #endregion
                }
                PayrollWeeklyHours = OriginalPayrollWeeklyHours;
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

            for (int i = 0; i < WeekHourSchedule.Count; i++)
            {
                PowerHourForecast.Rows.Add(WeekHourSchedule[i]);
            }
            #endregion

            try
            {
                #region Calculations
                double forecast;
                double[] dayHighs = new double[7];
                double[] dayLows = new double[42];
                int n = 0;

                for (int col = 1; col < PowerHourForecast.Columns.Count; col++)
                {
                    double[] dayNumbers = new double[PowerHourForecast.Rows.Count];

                    // power hours
                    for (int row = 0; row < PowerHourForecast.Rows.Count; row++)
                    {
                        if (WeightedAverageTrafficTotal.Rows[row][col].ToString() != "" && WeightedAverageTrafficTotal.Rows[row][col].ToString() != "0")
                        {
                            if (row <= PowerHourForecast.Rows.Count - 6)
                            {
                                double currentHour = Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row][col]);
                                forecast = currentHour + Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row + 2][col]) + Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row + 4][col]);

                                dayNumbers[row] = forecast;
                            }
                            else if (row == PowerHourForecast.Rows.Count - 4)
                            {
                                double currentHour = Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row][col]);
                                forecast = currentHour + Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row + 2][col]);

                                dayNumbers[row] = forecast;
                            }
                            else
                            {
                                double currentHour = Convert.ToDouble(WeightedAverageTrafficTotal.Rows[row][col]);
                                forecast = currentHour;

                                dayNumbers[row] = forecast;
                            }


                            PowerHourForecast.Rows[row][col] = Math.Round(forecast, 0);
                        }
                        //}
                        //else
                        //{
                        //    PowerHourForecast.Rows[row][col] = 0;
                        //}
                    }

                    dayHighs[col - 1] = dayNumbers.Max();

                    // low hours
                    n += 6;
                    int smallest = 9999;
                    int second = 9999;
                    int third = 9999;
                    int fourth = 9999;
                    int fifth = 9999;
                    int sixth = 9999;

                    foreach (int i in dayNumbers)
                    {
                        if (i < smallest && i != 0 && i != 1)
                        {
                            sixth = fifth;
                            fifth = fourth;
                            fourth = third;
                            third = second;
                            second = smallest;
                            smallest = i;
                        }
                        else if (i < second && i != 0 && i != 1)
                        {
                            sixth = fifth;
                            fifth = fourth;
                            fourth = third;
                            third = second;
                            second = i;
                        }
                        else if (i < third && i != 0 && i != 1)
                        {
                            sixth = fifth;
                            fifth = fourth;
                            fourth = third;
                            third = i;
                        }
                        else if (i < fourth && i != 0 && i != 1)
                        {
                            sixth = fifth;
                            fifth = fourth;
                            fourth = i;
                        }
                        else if (i < fifth && i != 0 && i != 1)
                        {
                            sixth = fifth;
                            fifth = i;
                        }
                        else if (i < sixth && i != 0 && i != 1)
                        {
                            sixth = i;
                        }
                    }
                    dayLows[n - 6] = smallest;
                    dayLows[n - 5] = second;
                    dayLows[n - 4] = third;
                    dayLows[n - 3] = fourth;
                    dayLows[n - 2] = fifth;
                    dayLows[n - 1] = sixth;

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
                            break;
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
                        int count = j / 2;
                        if (Convert.ToString(PowerHourForecast.Rows[j][i]) == Convert.ToString(HourOfWorkDay.lowHours[lowHourIterator])
                            || Convert.ToString(PowerHourForecast.Rows[j][i]) == Convert.ToString(HourOfWorkDay.lowHours[lowHourIterator + 1])
                            || Convert.ToString(PowerHourForecast.Rows[j][i]) == Convert.ToString(HourOfWorkDay.lowHours[lowHourIterator + 2])
                            || Convert.ToString(PowerHourForecast.Rows[j][i]) == Convert.ToString(HourOfWorkDay.lowHours[lowHourIterator + 3])
                            || Convert.ToString(PowerHourForecast.Rows[j][i]) == Convert.ToString(HourOfWorkDay.lowHours[lowHourIterator + 4])
                            || Convert.ToString(PowerHourForecast.Rows[j][i]) == Convert.ToString(HourOfWorkDay.lowHours[lowHourIterator + 5]))
                        {
                            HourOfWorkDay.lowHourRows[lowCount] = Convert.ToString(PowerHourForecast.Rows[j][0]);
                            counter++;
                            lowCount++;
                        }
                        if (counter == 6)
                        {
                            lowHourIterator += 6;
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

        public void RedistributeHours()
        {
            DataTable PowerHours = new DataTable();
            PowerHours.Columns.Add("Time", typeof(string));
            for (int n = 0; n < AllocatedHours.Rows.Count; n++)
            {
                PowerHours.Rows.Add(AllocatedHours.Rows[n][0]);
            }
            for (int i = 1; i < 8; i++)
            {
                PowerHours.Columns.Add(weekdayNames[i - 1], typeof(string));
                string powerHour = HourOfWorkDay.powerHourRows[i - 1];
                List<string> powerHours = new List<string>();
                powerHours.Add(powerHour);

                for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                {
                    if (ScheduleHalfHourSlots[n] == powerHour)
                    {
                        if (i == 1 || i == 7)
                        {
                            for (int m = 1; m < WeekendPowerHours * 2; m++)
                            {
                                powerHours.Add(ScheduleHalfHourSlots[n + m]);
                            }
                            break;
                        }
                        else
                        {
                            for (int m = 1; m < WeekdayPowerHours * 2; m++)
                            {
                                powerHours.Add(ScheduleHalfHourSlots[n + m]);
                            }
                            break;
                        }
                    }
                }

                int powerhourcounter = 0;
                for (int n = 0; n < PowerHours.Rows.Count; n++)
                {
                    if (i == 1 || i == 7)
                    {
                        if (PowerHours.Rows[n][0].ToString() == powerHours[powerhourcounter])
                        {
                            PowerHours.Rows[n][i] = "True";
                            if (powerhourcounter < WeekendPowerHours * 2 - 1)
                            {
                                powerhourcounter++;
                            }
                        }
                        else
                        {
                            PowerHours.Rows[n][i] = "False";
                        }
                    }
                    else
                    {
                        if (PowerHours.Rows[n][0].ToString() == powerHours[powerhourcounter])
                        {
                            PowerHours.Rows[n][i] = "True";
                            if (powerhourcounter < WeekdayPowerHours * 2 - 1)
                            {
                                powerhourcounter++;
                            }
                        }
                        else
                        {
                            PowerHours.Rows[n][i] = "False";
                        }
                    }
                }
            }

            DataTable DistributionDaysTake = new DataTable();
            DistributionDaysTake.Columns.Add("Time", typeof(string));
            for (int n = 0; n < AllocatedHours.Rows.Count; n++)
            {
                DistributionDaysTake.Rows.Add(AllocatedHours.Rows[n][0]);
            }
            for (int i = 1; i < 8; i++)
            {
                DistributionDaysTake.Columns.Add(weekdayNames[i - 1], typeof(string));
                for (int n = 0; n < AllocatedHours.Rows.Count; n++)
                {
                    DistributionDaysTake.Rows[n][i] = AllocatedHours.Rows[n][i];
                }
            }

            DataTable DistributionDaysGive = new DataTable();
            DistributionDaysGive.Columns.Add("Time", typeof(string));
            for (int n = 0; n < AllocatedHours.Rows.Count; n++)
            {
                DistributionDaysGive.Rows.Add(AllocatedHours.Rows[n][0]);
            }
            for (int i = 1; i < 5; i++)
            {
                DistributionDaysGive.Columns.Add(weekdayNames[i], typeof(string));
                for (int n = 0; n < AllocatedHours.Rows.Count; n++)
                {
                    DistributionDaysGive.Rows[n][i] = AllocatedHours.Rows[n][i + 1];
                }
            }

            double totalhours = 0;
            for (int i = 1; i < AllocatedHours.Columns.Count; i++)
            {
                for (int n = 0; n < AllocatedHours.Rows.Count; n++)
                {
                    if (AllocatedHours.Rows[n][i].ToString() != "")
                    {
                        double emps = Convert.ToDouble(AllocatedHours.Rows[n][i]);
                        totalhours += emps;
                    }
                }
            }
            totalhours = totalhours / 2;

            //still 1 off I guess
            double HalfHoursToTake = minhours - maxhours;
            HalfHoursToTake = totalhours * 2 - PayrollWeeklyHours * 2;

            int minChange = 0;
            for (int i = 0; i < DistributionDaysTake.Rows.Count; i++)
            {
                if (DistributionDaysTake.Rows[i][0].ToString() == "1:00PM")
                {
                    minChange = i;
                    break;
                }
            }

            // take hours if necessary
            if (HalfHoursToTake > 0)
            {
                // take hours from non-power hours
                while (HalfHoursToTake != 0)
                {
                    int loopCounter = 0;
                    // take the hours from other slots
                    for (int i = 0; i < DistributionDaysTake.Rows.Count; i++)
                    {
                        for (int n = 1; n < DistributionDaysTake.Columns.Count; n++)
                        {
                            // check if the hour is in the day's schedule, if the number is above the minimum,
                            // and if the hour isn't a power hour
                            // if time is before 1pm
                            if (i < minChange && DistributionDaysTake.Rows[i][n].ToString() != ""
                                && Convert.ToInt32(DistributionDaysTake.Rows[i][n]) > MinEmployeesEarly
                                && PowerHours.Rows[i][n].ToString() != "True" && HalfHoursToTake != 0)
                            {
                                int newValue = Convert.ToInt32(DistributionDaysTake.Rows[i][n]) - 1;
                                DistributionDaysTake.Rows[i][n] = newValue;
                                HalfHoursToTake--;
                                totalhours--;
                                loopCounter++;
                            }
                            // if time is after 1pm
                            else if (i >= minChange && DistributionDaysTake.Rows[i][n].ToString() != ""
                                && Convert.ToInt32(DistributionDaysTake.Rows[i][n]) > MinEmployeesLater
                                && PowerHours.Rows[i][n].ToString() != "True" && HalfHoursToTake != 0)
                            {
                                int newValue = Convert.ToInt32(DistributionDaysTake.Rows[i][n]) - 1;
                                DistributionDaysTake.Rows[i][n] = newValue;
                                HalfHoursToTake--;
                                totalhours--;
                                loopCounter++;
                            }
                        }
                    }
                    //stop from infinitely looping if there are no more hours to take
                    if (loopCounter == 0)
                    {
                        break;
                    }
                }

                // take hours from power hours if still necessary
                while (HalfHoursToTake != 0)
                {
                    int loopCounter = 0;
                    // take the hours from other slots
                    for (int i = 0; i < DistributionDaysTake.Rows.Count; i++)
                    {
                        for (int n = 1; n < DistributionDaysTake.Columns.Count; n++)
                        {
                            // check if the hour is in the day's schedule, if the number is above the minimum,
                            // and if the hour is a power hour
                            // if time is before 1pm
                            if (i < minChange && DistributionDaysTake.Rows[i][n].ToString() != ""
                                && Convert.ToInt32(DistributionDaysTake.Rows[i][n]) > MinEmployeesEarly
                                && PowerHours.Rows[i][n].ToString() != "False" && HalfHoursToTake != 0)
                            {
                                int newValue = Convert.ToInt32(DistributionDaysTake.Rows[i][n]) - 1;
                                DistributionDaysTake.Rows[i][n] = newValue;
                                HalfHoursToTake--;
                                totalhours--;
                                loopCounter++;
                            }
                            // if time is after 1pm
                            else if (i >= minChange && DistributionDaysTake.Rows[i][n].ToString() != ""
                                && Convert.ToInt32(DistributionDaysTake.Rows[i][n]) > MinEmployeesLater
                                && PowerHours.Rows[i][n].ToString() != "False" && HalfHoursToTake != 0)
                            {
                                int newValue = Convert.ToInt32(DistributionDaysTake.Rows[i][n]) - 1;
                                DistributionDaysTake.Rows[i][n] = newValue;
                                HalfHoursToTake--;
                                totalhours--;
                                loopCounter++;
                            }
                        }
                    }
                    //stop from infinitely looping if there are no more hours to take
                    if (loopCounter == 0)
                    {
                        break;
                    }
                }

                // add the updated day columns to the table
                for (int i = 1; i < 8; i++)
                {
                    for (int n = 0; n < AllocatedHours.Rows.Count; n++)
                    {
                        AllocatedHours.Rows[n][i] = DistributionDaysTake.Rows[n][i];
                    }
                }
            }
            // add hours if necessary
            else if (HalfHoursToTake < 0)
            {
                while (HalfHoursToTake != 0)
                {
                    int loopCounter = 0;

                    //add the hours to other slots
                    for (int i = 0; i < DistributionDaysGive.Rows.Count; i++)
                    {
                        for (int n = 1; n < DistributionDaysGive.Columns.Count; n++)
                        {
                            // check if the hour is in the day's schedule, if the number is below the maximum,
                            // and if the hour isn't a power hour
                            // if time is before 1pm
                            if (i < minChange && DistributionDaysGive.Rows[i][n].ToString() != ""
                                && Convert.ToInt32(DistributionDaysGive.Rows[i][n]) < MaxEmployees
                                && PowerHours.Rows[i][n].ToString() != "True" && HalfHoursToTake != 0)
                            {
                                int newValue = Convert.ToInt32(DistributionDaysGive.Rows[i][n]) + 1;
                                DistributionDaysGive.Rows[i][n] = newValue;
                                HalfHoursToTake++;
                                totalhours++;
                                loopCounter++;
                            }
                        }
                    }
                    //stop from infinitely looping if there are no more hours to take
                    if (loopCounter == 0)
                    {
                        break;
                    }
                }
                for (int i = 1; i < 5; i++)
                {
                    for (int n = 0; n < AllocatedHours.Rows.Count; n++)
                    {
                        AllocatedHours.Rows[n][i + 1] = DistributionDaysGive.Rows[n][i];
                    }
                }
            }
        }

        /// <summary>
        /// Generate the table for the labor schedule with the specified hours 
        /// </summary>
        /// <returns></returns>
        public DataTable GenerateAllocatedHoursDisplay(DateTime[] RequestedDates)
        {
            //#region Table Setup
            //AllocatedHoursDisplay.Clear();
            //AllocatedHoursDisplay.Columns.Clear();
            //AllocatedHoursDisplay.Columns.Add("HourOfDay", typeof(string));
            //AllocatedHoursDisplay.Columns.Add("Sunday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[0]), typeof(string));
            //AllocatedHoursDisplay.Columns.Add("Monday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[1]), typeof(string));
            //AllocatedHoursDisplay.Columns.Add("Tuesday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[2]), typeof(string));
            //AllocatedHoursDisplay.Columns.Add("Wednesday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[3]), typeof(string));
            //AllocatedHoursDisplay.Columns.Add("Thursday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[4]), typeof(string));
            //AllocatedHoursDisplay.Columns.Add("Friday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[5]), typeof(string));
            //AllocatedHoursDisplay.Columns.Add("Saturday " + Environment.NewLine + String.Format("{0:M/dd}", RequestedDates[6]), typeof(string));

            //for (int i = 0; i < WeekHourSchedule.Count; i++)
            //{
            //    AllocatedHoursDisplay.Rows.Add(WeekHourSchedule[i]);
            //}

            //#endregion
            //int rows = WeekHourSchedule.Count;
            //int count = 0;

            ////iterating through columns
            //for (int i = 0; i < WeekHourSchedule.Count; i++)
            //{
            //    count = 0;

            //    //iterating through each cell in the column
            //    if (i == 0)
            //    {
            //        for (int j = 0; j < AllocatedHours.Rows.Count; j++)
            //        {

            //            if (AllocatedHours.Rows[j][0] == AllocatedHoursDisplay.Rows[i][0])
            //            {
            //                AllocatedHoursDisplay.Rows.Remove(AllocatedHoursDisplay.Rows[i]);
            //                DataRow dr = AllocatedHours.Rows[j];
            //                AllocatedHoursDisplay.ImportRow(dr);
            //            }
            //            count++;
            //        }
            //    }
            //}

            //for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
            //{
            //    if (ScheduleHalfHourSlots[i] == WeekHourSchedule[0])
            //    {
            //        DataRow dr = AllocatedHoursDisplay.NewRow();
            //        dr[0] = ScheduleHalfHourSlots[i - 1];
            //        AllocatedHoursDisplay.Rows.InsertAt(dr, 0);
            //        DataRow dr2 = AllocatedHoursDisplay.NewRow();
            //        dr2[0] = ScheduleHalfHourSlots[i - 2];
            //        AllocatedHoursDisplay.Rows.InsertAt(dr2, 0);
            //        break;
            //    }
            //}

            //for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
            //{
            //    if (ScheduleHalfHourSlots[i] == WeekHourSchedule[WeekHourSchedule.Count - 1] && i < ScheduleHalfHourSlots.Length - 2)
            //    {
            //        AllocatedHoursDisplay.Rows.Add(ScheduleHalfHourSlots[i + 1]);
            //        AllocatedHoursDisplay.Rows.Add(ScheduleHalfHourSlots[i + 2]);
            //    }
            //}

            AllocatedHoursDisplay = AllocatedHours;

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
            employeesStillNeeded = new int[ScheduleHalfHourSlots.Length];
            string[] employeeIds = new string[scheduling.Count];
            for (int x = 0; x < employeeIds.Length; x++)
            {
                employeeIds[x] = scheduling[x].id;
            }

            //FakeAPI.CheckForCurrentWeek(scheduling, RequestedDates);
            EmployeeScheduledTimes = FakeAPI.GetEmployeeScheduledTimes(employeeIds, currentStoreCode, RequestedDates);

            //loop over each employee's (weekday) and check the hours that are selected - or all of them?
            foreach (Employees emp in scheduling)
            {
                Dictionary<string, string[]> innerDict = EmployeeScheduledTimes[emp.id];
                string[] hoursForDay = innerDict[day];
                for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
                {
                    for (int n = 0; n < hoursForDay.Length; n++)
                    {
                        if (hoursForDay[n] == ScheduleHalfHourSlots[i])
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
                double hoursRemaining = emp.hours;

                Dictionary<string, string[]> innerDict = EmployeeScheduledTimes[emp.id];
                DataTable AssignedHourBlocks = AssignedEmployeesRequestedWeek.Clone();
                for (int i = 0; i < AssignedEmployeesRequestedWeek.Rows.Count; i++)
                {
                    if (AssignedEmployeesRequestedWeek.Rows[i][1].ToString() == emp.id)
                    {
                        AssignedHourBlocks.ImportRow(AssignedEmployeesRequestedWeek.Rows[i]);
                    }
                }

                double hourCount = 0;
                for (int i = 0; i < AssignedHourBlocks.Rows.Count; i++)
                {
                    bool block = false;
                    double blockHours = 0;
                    for (int n = 0; n < ScheduleHalfHourSlots.Length; n++)
                    {
                        if (SQLHours[n] == AssignedHourBlocks.Rows[i][4].ToString())
                        {
                            block = true;
                        }
                        if (SQLHours[n] == AssignedHourBlocks.Rows[i][5].ToString())
                        {
                            block = false;
                        }
                        if (block == true)
                        {
                            blockHours += .5;
                        }
                    }
                    if (blockHours > 4)
                    {
                        blockHours -= .5;
                    }
                    hourCount += blockHours;
                }

                emp.hoursRemaining = hoursRemaining - hourCount;
                emp.hoursScheduled = hourCount;
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

            string starthour = WeekStartEndHours.Rows[0][column].ToString();
            string endhour = WeekStartEndHours.Rows[1][column].ToString();

            for (int i = openHourSlots; i > 0; i--)
            {
                DataColumn col = AssignmentTable.Columns.Add(AllocatedHoursDisplay.Rows[i - 1][0].ToString(), typeof(string));
                //AssignmentTable.Columns.Add(AllocatedHoursDisplay.Rows[i-1][0].ToString(), typeof(string));
                col.SetOrdinal(1);
            }
            for (int i = 0; i < WeekHourSchedule.Count; i++)
            {
                AssignmentTable.Columns.Add(WeekHourSchedule[i], typeof(string));
            }
            for (int i = 2; i > 0; i--)
            {
                AssignmentTable.Columns.Add(AllocatedHoursDisplay.Rows[AllocatedHoursDisplay.Rows.Count - i][0].ToString(), typeof(string));
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
                    for (int h = 1; h <= AllocatedHours.Rows.Count; h++)
                    {
                        AssignmentTable.Rows[j - 1][h] = AllocatedHours.Rows[h - 1][j];
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

            string startHour = WeekStartEndHours.Rows[0][column].ToString();
            string endHour = WeekStartEndHours.Rows[1][column].ToString();
            for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
            {
                if (ScheduleHalfHourSlots[i] == startHour && i > openHourSlots - 1)
                {
                    startHour = ScheduleHalfHourSlots[i - openHourSlots];
                }
                if (ScheduleHalfHourSlots[i] == endHour && i != ScheduleHalfHourSlots.Length - 2)
                {
                    endHour = ScheduleHalfHourSlots[i + 2];
                    break;
                }
            }

            bool block = false;
            for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
            {
                if (ScheduleHalfHourSlots[i] == startHour)
                {
                    block = true;
                }
                if (block == true)
                {
                    AssignmentView.Columns.Add(ScheduleHalfHourSlots[i], typeof(string));
                }
                if (ScheduleHalfHourSlots[i] == endHour)
                {
                    block = false;
                }
            }

            //for (int i = 0; i < WeekHourSchedule.Count; i++)
            //{
            //    AssignmentView.Columns.Add(WeekHourSchedule[i], typeof(string));
            //}

            var row = AssignmentTable.Rows[column];
            AssignmentView.ImportRow(row);

            DataRow dr = AssignmentView.NewRow();
            int hourIterator = 0;
            string starthour = WeekStartEndHours.Rows[0][column].ToString();
            for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
            {
                if (i < ScheduleHalfHourSlots.Length - openHourSlots && ScheduleHalfHourSlots[i + openHourSlots] == starthour)
                {
                    hourIterator = i;
                }
            }

            for (int i = 0; i < AssignmentView.Columns.Count; i++)
            {
                if (i == 0)
                {
                    AssignmentView.Rows.Add("Needed");
                }
                else
                {
                    if (AssignmentView.Rows[0][i].ToString() != "")
                    {
                        AssignmentView.Rows[1][i] = Convert.ToInt32(AssignmentView.Rows[0][i]) - employeesStillNeeded[(i - 1) + hourIterator];
                    }
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
            string startHour = WeekStartEndHours.Rows[0][column].ToString();
            string endHour = WeekStartEndHours.Rows[1][column].ToString();
            for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
            {
                if (ScheduleHalfHourSlots[i] == startHour && i > 1)
                {
                    startHour = ScheduleHalfHourSlots[i - 2];
                }
                if (ScheduleHalfHourSlots[i] == endHour && i != ScheduleHalfHourSlots.Length - 2)
                {
                    endHour = ScheduleHalfHourSlots[i + 2];
                    break;
                }
            }

            bool block = false;
            for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
            {
                if (ScheduleHalfHourSlots[i] == startHour)
                {
                    block = true;
                }
                if (block == true)
                {
                    BlackoutAssignmentView.Columns.Add(ScheduleHalfHourSlots[i], typeof(string));
                }
                if (ScheduleHalfHourSlots[i] == endHour)
                {
                    block = false;
                }
            }

            var row = BlackoutTimes.Rows[column];
            BlackoutAssignmentView.ImportRow(row);
            BlackoutAssignmentView.ImportRow(row);

            for (int i = 1; i < BlackoutAssignmentView.Columns.Count; i++)
            {
                for (int n = 0; n < BlackoutTimes.Rows.Count; n++)
                {
                    DataColumn c = BlackoutAssignmentView.Columns[i];

                    if (BlackoutTimes.Rows[n][0].ToString() == c.Caption)
                    {
                        BlackoutAssignmentView.Rows[0][i] = BlackoutTimes.Rows[n][column + 1];
                        BlackoutAssignmentView.Rows[1][i] = BlackoutTimes.Rows[n][column + 1];
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
            int weekday = (selectedWeekday * (ScheduleHalfHourSlots.Length));
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
                if (startHour == ScheduleHalfHourSlots[n])
                {
                    employeeHours[n + weekday] = true;
                    start = n;
                }

                // check when the selected end time is
                if (endHour == ScheduleHalfHourSlots[n])
                {
                    employeeHours[n + weekday] = true;
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
                                    employeeId, RequestedDates, EmployeeScheduledTimes[employeeId], selectedWeekday, currentStoreCode);

            Dictionary<string, string[]> employeeSchedule = EmployeeScheduledTimes[employeeId];
            employeeSchedule[daysOfWeek[selectedWeekday]] = updatedHours;
            EmployeeScheduledTimes[employeeId] = employeeSchedule;
            Dictionary<string, Dictionary<string, string[]>> updatedSchedule = EmployeeScheduledTimes;

            foreach (Employees emp in scheduling)
            {
                //DataTable empSchedule = new DataTable();
                double hoursRemaining = Convert.ToDouble(emp.hours);

                Dictionary<string, string[]> innerDict = EmployeeScheduledTimes[emp.id];
                double hourCount = 0;
                for (int n = 0; n < daysOfWeek.Length; n++)
                {
                    for (int j = 0; j < innerDict[daysOfWeek[n]].Length; j++)
                    {
                        hourCount += .5;
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
        /// <param name="EmployeeList"></param>
        public void CheckSchedulingRules(int weekday, List<Employees> EmployeeList)
        {
            //Dictionary<string, int[]> ScheduledEmployees = new Dictionary<string, int[]>();

            ErrorMessages = new List<string>();
            Dictionary<string, Dictionary<string, List<int>>> ScheduledEmployees = new Dictionary<string, Dictionary<string, List<int>>>();
            string[] daysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            string[] hourSlots = ScheduleHalfHourSlots;
            string[] selectedHours = new string[AllocatedHoursDisplay.Rows.Count];
            for (int i = 0; i < AllocatedHoursDisplay.Rows.Count; i++)
            {
                selectedHours[i] = Convert.ToString(AllocatedHoursDisplay.Rows[i][0]);
            }
            string openHour = WeekStartEndHours.Rows[0][weekday].ToString();
            string closeHour = WeekStartEndHours.Rows[1][weekday].ToString();

            for (int i = 0; i < ScheduleHalfHourSlots.Length; i++)
            {
                if (closeHour == ScheduleHalfHourSlots[i] && i < ScheduleHalfHourSlots.Length - 2 && i > 0)
                {
                    closeHour = ScheduleHalfHourSlots[i + 2];
                    break;
                }
            }

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


            for (int i = 0; i < 7; i++)
            {
                Dictionary<string, List<int>> EmployeeRanksForHour = new Dictionary<string, List<int>>();

                for (int n = 0; n < selectedHours.Length; n++)
                {
                    List<int> employeeRanks = new List<int>();

                    for (int m = 0; m < EmployeeList.Count; m++)
                    {
                        Dictionary<string, string[]> employeeDayHours = EmployeeScheduledTimes[EmployeeList[m].id];

                        string[] hoursForDay = employeeDayHours[daysOfWeek[i]];

                        for (int h = 0; h < hoursForDay.Length; h++)
                        {
                            if (hoursForDay[h] == selectedHours[n])
                            {
                                employeeRanks.Add(EmployeeList[m].rank);
                            }
                        }
                    }
                    //innerDict.Add(selectedHours[n], employeeRanks);
                    EmployeeRanksForHour[selectedHours[n]] = employeeRanks;
                }

                ScheduledEmployees.Add(daysOfWeek[i], EmployeeRanksForHour);
            }

            //ScheduledEmployees.Add(daysOfWeek[weekday], innerDict);

            Dictionary<string, List<int>> SelectedDayEmployees = ScheduledEmployees[daysOfWeek[weekday]];

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
                        SelectedDayEmployees.Remove(BlackoutTimes.Rows[i][0].ToString());
                    }
                }
            }
            int hourcount = 0;
            int openslots = 2;
            foreach (string hour in SelectedDayEmployees.Keys)
            {
                if (hourcount > openslots - 1 && hourcount != SelectedDayEmployees.Keys.Count)
                {
                    condition1Checks.Add(hour, false);
                    foreach (int empRank in SelectedDayEmployees[hour])
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
                hourcount++;
            }

            foreach (string check in condition1Checks.Keys)
            {
                if (condition1Checks[check] == false)
                {
                    condition1 = false;
                    break;
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
                Dictionary<string, List<int>> FridayEmployees = ScheduledEmployees[daysOfWeek[5]];
                Dictionary<string, List<int>> SaturdayEmployees = ScheduledEmployees[daysOfWeek[6]];

                List<Dictionary<string, List<int>>> FridayAndSaturday = new List<Dictionary<string, List<int>>>();
                FridayAndSaturday.Add(FridayEmployees);
                FridayAndSaturday.Add(SaturdayEmployees);

                for (int i = 0; i < FridayAndSaturday.Count; i++)
                {
                    Dictionary<string, List<int>> EmployeeRanks = FridayAndSaturday[i];

                    foreach (string hour in EmployeeRanks.Keys)
                    {
                        if (hour == closeHour)
                        {
                            foreach (int empRank in EmployeeRanks[hour])
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
            #endregion

            // Level 60 must work every 3rd Sunday
            #region Condition 3
            List<DateTime> Sundays = GetEveryThirdSunday();
            bool dateFound = false;
            for (int n = 0; n < Sundays.Count; n++)
            {
                if (RequestedDates[0] == Sundays[n])
                {
                    dateFound = true;
                }

            }
            if (dateFound == true)
            {
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
                            SelectedDayEmployees.Remove(BlackoutTimes.Rows[i][0].ToString());
                        }
                    }
                }
                foreach (string hour in SelectedDayEmployees.Keys)
                {
                    foreach (int empRank in SelectedDayEmployees[hour])
                    {
                        if (empRank == 60)
                        {
                            condition3 = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                condition3 = true;
            }

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
                    foreach (string hour in SelectedDayEmployees.Keys)
                    {
                        //openHour = innerDict.Keys.First();
                        //closeHour = innerDict.Keys.Last();
                        if (hour == openHour)
                        {
                            foreach (int empRank in SelectedDayEmployees[hour])
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
                    foreach (string hour in SelectedDayEmployees.Keys)
                    {
                        //openHour = innerDict.Keys.First();
                        //closeHour = innerDict.Keys.Last();
                        if (hour == closeHour)
                        {
                            foreach (int empRank in SelectedDayEmployees[hour])
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

                int rankTaken = 0;
                foreach (string hour in SelectedDayEmployees.Keys)
                {
                    // check open hour
                    if (hour == openHour)
                    {
                        foreach (int empRank in SelectedDayEmployees[hour])
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
                        foreach (int empRank in SelectedDayEmployees[hour])
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
                        foreach (string hour in SelectedDayEmployees.Keys)
                        {
                            foreach (int empRank in SelectedDayEmployees[hour])
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
                    ErrorMessages.Add(count + numeric + "Level 40, 50, or 60 must be scheduled at all times. No current slots contain these employees.");
                }
                else
                {
                    ErrorMessages.Add(count + numeric + "Level 40, 50, or 60 must be scheduled at all times. Current unscheduled slots are:");

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

        public static List<DateTime> GetEveryThirdSunday()
        {
            List<DateTime> SundaysOfYear = new List<DateTime>();
            List<DateTime> ThirdSundays = new List<DateTime>();
            for (int n = 1; n < 13; n++)
            {
                int intMonth = n;
                int intYear = DateTime.Now.Year;
                int intDaysThisMonth = DateTime.DaysInMonth(intYear, intMonth);
                DateTime oBeginnngOfThisMonth = new DateTime(intYear, intMonth, 1);
                for (int i = 0; i < intDaysThisMonth; i++)
                {
                    if (oBeginnngOfThisMonth.AddDays(i).DayOfWeek == DayOfWeek.Sunday)
                    {
                        SundaysOfYear.Add(new DateTime(intYear, intMonth, i + 1));
                    }
                }
            }

            int count = 0;
            while (count < SundaysOfYear.Count)
            {
                ThirdSundays.Add(SundaysOfYear[count]);
                count += 3;
            }

            return ThirdSundays;
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

        public void GenerateTotalHours()
        {
            double weekHourTotal = 0;
            double[] dayTotals = new double[7];
            foreach (Employees emp in LaborScheduling.EmployeeListStore)
            {
                //double[] employeeDayHourCount = new double[7];
                //double weekHourAmount = 0;

                //string name = emp.firstName + " " + emp.lastName;
                //for (int n = 0; n < 7; n++)
                //{
                //    List<string> startTime = new List<string>();
                //    List<string> endTime = new List<string>();
                //    List<double> dayHourCount = new List<double>();
                //    for (int i = 0; i < AssignedEmployeesRequestedWeek.Rows.Count; i++)
                //    {
                //        if (emp.id == AssignedEmployeesRequestedWeek.Rows[i][1].ToString() && RequestedDates[n] == Convert.ToDateTime(AssignedEmployeesRequestedWeek.Rows[i][3]))
                //        {
                //            bool block = false;
                //            double hourcount = 0;
                //            for (int m = 0; m < ScheduleHalfHourSlots.Length; m++)
                //            {
                //                if (AssignedEmployeesRequestedWeek.Rows[i][4].ToString() == SQLHours[m])
                //                {
                //                    startTime.Add(ScheduleHalfHourSlots[m]);
                //                    block = true;
                //                }
                //                if (AssignedEmployeesRequestedWeek.Rows[i][5].ToString() == SQLHours[m])
                //                {
                //                    endTime.Add(ScheduleHalfHourSlots[m]);
                //                    block = false;
                //                }
                //                if (block == true)
                //                {
                //                    weekHourAmount += .5;
                //                    hourcount += .5;
                //                }
                //            }

                //            if (hourcount > 4)
                //            {
                //                hourcount = hourcount - .5;
                //            }
                //            dayTotals[n] += hourcount;
                //        }
                //    }

                //    for (int m = 0; m < startTime.Count; m++)
                //    {

                //    }

                //    for (int m = 0; m < dayHourCount.Count; m++)
                //    {
                //        employeeDayHourCount[n] += dayHourCount[m];
                //    }

                //}

                //weekHourTotal += weekHourAmount;
                weekHourTotal += emp.hoursScheduled;
            }
            TotalScheduledHours = weekHourTotal;
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

        public static double[] lowHours = new double[42];

        public static string[] powerHourRows = new string[7];

        public static string[] lowHourRows = new string[42];

    }

    public class Employee
    {
        string name { get; set; }

    }

}