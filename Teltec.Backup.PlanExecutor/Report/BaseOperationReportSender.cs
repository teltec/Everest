using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teltec.Common.Utils;

namespace Teltec.Backup.PlanExecutor.Report
{
	public class BaseOperationReportSender
	{
		protected readonly string URL = "https://8nzutclnyk.execute-api.us-east-1.amazonaws.com/prod/TeltecBackupReportSendEmail";

		public string ReasonMessage { get; protected set; }

		BaseOperationReport Report;

		public BaseOperationReportSender(BaseOperationReport report)
		{
			Report = report;
		}

		public Task<bool> Send(string recipientName, string recipientAddress, string mailSubject)
		{
			return SendAsync(URL, recipientName, recipientAddress, mailSubject);
		}

		protected async Task<bool> SendAsync(string envUrl, string recipientName, string recipientAddress, string mailSubject)
		{
			using (var client = new HttpClient())
			{
				var values = new Dictionary<string, object>
				{
					// Email settings
					{ "name", recipientName },
					{ "email", recipientAddress },
					{ "subject", mailSubject },

					// Operation
					{ "PlanType", Report.PlanType },
					{ "PlanName", Report.PlanName },
					{ "BucketName", Report.BucketName },
					{ "HostName", Report.HostName },
					{ "StartedAt", Report.StartedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss K") },
					{ "FinishedAt", Report.FinishedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss K") },
					{ "Duration", (Report.FinishedAt - Report.StartedAt).ToString(@"hh\:mm\:ss") },

					// Status
					{ "Status", Report.TransferResults.OverallStatus.ToString() },

					// Transfers
					{ "Total", Report.TransferResults.Stats.Total },
					{ "Pending", Report.TransferResults.Stats.Pending },
					{ "Running", Report.TransferResults.Stats.Running },
					{ "Failed", Report.VersionerResults.Stats.Failed + Report.TransferResults.Stats.Failed },
					{ "Canceled", Report.TransferResults.Stats.Canceled },
					{ "Completed", Report.TransferResults.Stats.Completed },

					// Sizes
					{ "TotalSize", FileSizeUtils.FileSizeToString(Report.TransferResults.Stats.BytesTotal) },
					{ "PendingSize", FileSizeUtils.FileSizeToString(Report.TransferResults.Stats.BytesPending) },
					{ "FailedSize", FileSizeUtils.FileSizeToString(Report.TransferResults.Stats.BytesFailed) },
					{ "CanceledSize", FileSizeUtils.FileSizeToString(Report.TransferResults.Stats.BytesCanceled) },
					{ "CompletedSize", FileSizeUtils.FileSizeToString(Report.TransferResults.Stats.BytesCompleted) },

					// Errors
					{ "ErrorMessages", Report.ErrorMessages },
				};

				string json = JsonConvert.SerializeObject(values);
				StringContent content = new StringContent(json, Encoding.UTF8);

				using (HttpResponseMessage response = await client.PostAsync(envUrl, content))
				{
					ReasonMessage = response.ReasonPhrase;
					return response.IsSuccessStatusCode;
				}
			}
		}
	}
}
