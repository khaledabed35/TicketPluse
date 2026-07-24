using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Specification
{
    public class EventQueryParameters
    {
        public string? Search { get; set; }
        public string? Sort { get; set; }
        public string? Place { get; set; }
        public string? Name { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
