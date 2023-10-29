using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INF272ASSIGNMENT3.Models
{
    public static class DataService
    {
        public static List<DataPoint> GetData(List<borrows> borrows)
        {
            var chartData = borrows
                .Where(b => b.takenDate.HasValue)
                .GroupBy(l => new { Month = l.takenDate.Value.Month })
                .Select(g => new DataPoint(g.Count(), "Month" + g.Key.Month, g.Key.Month))
                .OrderBy(g => g.Month)
                .ToList();

            return chartData;
        }

    }
}