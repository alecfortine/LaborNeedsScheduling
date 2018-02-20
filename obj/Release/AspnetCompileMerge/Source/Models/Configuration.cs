using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace LaborNeedsScheduling.Models
{
    public class Configuration
    {
        List<ResourceVariables> Resources { get; set; }
        DataTable StoreHours { get; set; }
    }
    public class ResourceVariables
    {
        int id { get; set; }
        string Name { get; set; }
        string Value { get; set; }
        string LocationCode { get; set; }
    }

}