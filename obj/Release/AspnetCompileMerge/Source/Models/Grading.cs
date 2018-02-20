using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LaborNeedsScheduling.Models
{
    public class Grading
    {
        DayOfWorkWeek yesterday;
        List<int> Grades;
        int GradeForDay;
        string ReasonText;
    }
}