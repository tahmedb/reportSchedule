// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

#nullable disable

namespace ReportScheduler
{
    public partial class CompanyReportFilterConfiguration
    {
        public int Id { get; set; }
        public int CompanyReportId { get; set; }
        public int ReportFilterId { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }

        public virtual CompanyReport CompanyReport { get; set; }
        public virtual ReportFilter ReportFilter { get; set; }
    }
}