using System;
using System.Collections.Generic;

namespace ReportScheduler.BackgroundJobs
{
    public class CompanyReportDto
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string ReportDataSetName { get; set; }
        public IEnumerable<CompanyReportColumnConfigurationDto> CompanyReportColumnConfiguration { get; internal set; }
        public IEnumerable<CompanyReportFilterConfigurationDto> CompanyReportFilterConfiguration { get; internal set; }
        public DateTime? ScheduleTime { get; internal set; }
        public int? ScheduleDay { get; internal set; }
    }
}