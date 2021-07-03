﻿using Hangfire;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReportScheduler.Data;
using ReportScheduler.EmailService;
using ReportScheduler.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportScheduler.BackgroundJobs
{
    public class ScheduledReports
    {
        private readonly Hs_LiveContext _hs_LiveContext;
        private readonly ILogger<ScheduledReports> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ConnectionString _connectionString;
        private readonly IEmailService _emailService;
        public ScheduledReports(Hs_LiveContext hs_LiveContext,IEmailService emailService, IOptions<ConnectionString> connectionString, ILogger<ScheduledReports> logger, IBackgroundJobClient backgroundJobClient)
        {
            _hs_LiveContext = hs_LiveContext;
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
            _connectionString = connectionString.Value;
            _emailService = emailService;
        }

        public async Task SendScheduledReportsAsync()
        {
            var reports = await CompanyReportsAsync();
            reports.ForEach(x =>
            {

                int day = (int)DateTime.Now.DayOfWeek;
                _backgroundJobClient.Enqueue(() => RunJob(x));
                if (x.ScheduleDay == day)
                {
                    if (x.ScheduleTime.Value.Hour == DateTime.Now.Hour && x.ScheduleTime.Value.Minute == 0)
                    {
                        _backgroundJobClient.Enqueue(() => RunJob(x));
                    }
                    else if (x.ScheduleTime.Value.Hour == DateTime.Now.Hour && x.ScheduleTime.Value.Minute == 0)
                    {
                        _backgroundJobClient.Schedule(() => RunJob(x), TimeSpan.FromMinutes(x.ScheduleTime.Value.Minute));
                    }
                }

                _logger.LogInformation(x.Name);

            });
        }

        private async Task<List<CompanyReport>> CompanyReportsAsync()
        {
            return await _hs_LiveContext.CompanyReport
                .Include(x => x.ReportDataSet)
                .Include(x => x.CompanyReportColumnConfiguration)
                .Include(x => x.CompanyReportFilterConfiguration).Where(x => x.IsSchedule).ToListAsync();
        }
        private void RunJob(CompanyReport companyReport)
        {
            string sqlQuery = $"select * from ${companyReport.ReportDataSet.Name}";
            var reportData = DataExecuteReturn(sqlQuery);
            StringBuilder sb = new StringBuilder();

            foreach (DataColumn col in reportData.Tables[0].Columns)
            {
                sb.Append(string.Format("{0},", col.ColumnName));
            }
            foreach (DataRow row in reportData.Tables[0].Rows)
            {
                sb.AppendLine();
                foreach (DataColumn col in reportData.Tables[0].Columns)
                {
                    sb.Append(string.Format("{0},", row[col.ColumnName]));
                }
            }

            byte[] bytes = Encoding.ASCII.GetBytes(sb.ToString());
            _emailService.Send("mabbass@gmail.com", $"Scheduled report for company ${companyReport.Name}", "Csv file is attached",bytes);
        }
        private DataSet DataExecuteReturn(string strSQL)
        {

            SqlCommand oCommand = new SqlCommand(strSQL, new SqlConnection(_connectionString.database));
            SqlDataAdapter oDataAdapter = new SqlDataAdapter(strSQL, _connectionString.database);

            oDataAdapter.SelectCommand.CommandTimeout = 1000;

            DataSet oDataSet = new DataSet();
            oDataAdapter.Fill(oDataSet);

            return oDataSet;
        }
    }
}