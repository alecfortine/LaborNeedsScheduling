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

    public class LaborScheduling
    {
        /// <summary>
        /// The current week the schedule is being generated for
        /// </summary>
        public WorkWeek ThisWeek { get; set; }

        public AvailabilityViewModel Employee { get; set; }

        /// <summary>
        /// Set of previous work weeks of data
        /// </summary>
        public Dictionary<int, WorkWeek> PastWorkWeeks { get; set; }

        public LaborScheduling()
        {
            ThisWeek = new WorkWeek();

        }

    }
}