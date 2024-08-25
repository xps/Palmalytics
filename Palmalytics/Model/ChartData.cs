using System;
using System.Collections.Generic;

namespace Palmalytics.Model
{
    public class TopData
    {
        public int TotalSessions { get; set; }
        public int TotalPageViews { get; set; }
        public int AverageBounceRate { get; set; }
        public int AverageSessionDuration { get; set; }     // in seconds. TODO: TimeSpan?
        public float AveragePagesPerSession { get; set; }
        public int SamplingFactor { get; set; }
    }

    public class ChartData
    {
        public int? TotalDays { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<ChartDataItem> Data { get; set; }
        public int SamplingFactor { get; set; }
    }

    public class ChartDataItem
    {
        public DateTime Date { get; set; }
        public float Value { get; set; }
    }

    public class TableData
    {
        public int TotalRows { get; set; }
        public int PageCount { get; set; }
        public List<TableDataItem> Rows { get; set; }
        public int SamplingFactor { get; set; }
    }

    public class TableDataItem
    {
        public string Label { get; set; }
        public int Value { get; set; }
        public double Percentage { get; set; }
    }
}
