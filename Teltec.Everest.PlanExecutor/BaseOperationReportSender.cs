using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Teltec.Everest.PlanExecutor
{
	public abstract class BaseOperationReportSender<TReport> where TReport : BaseOperationReport
	{
		protected readonly string URL = "https://8nzutclnyk.execute-api.us-east-1.amazonaws.com/prod/TeltecBackupReportSendEmail";

		public string ReasonMessage { get; protected set; }

		protected string RecipientName;
		protected string RecipientAddress;
		protected string MailSubject;

		protected readonly Dictionary<string, object> RequestBody;
		protected readonly TReport Report;

		public BaseOperationReportSender(TReport report)
		{
			Report = report;
			RequestBody = new Dictionary<string, object>();
		}

		protected void BuildBaseRequestBody()
		{
			// Email settings
			RequestBody.Add("name", RecipientName);
			RequestBody.Add("email", RecipientAddress);
			RequestBody.Add("subject", MailSubject);

			// Operation
			RequestBody.Add("PlanType", Report.PlanType);
			RequestBody.Add("PlanName", Report.PlanName);
			RequestBody.Add("BucketName", Report.BucketName);
			RequestBody.Add("HostName", Report.HostName);
			RequestBody.Add("StartedAt", Report.StartedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss K"));
			RequestBody.Add("FinishedAt", Report.FinishedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss K"));
			RequestBody.Add("Duration", (Report.FinishedAt - Report.StartedAt).ToString(@"hh\:mm\:ss"));

			// Errors
			RequestBody.Add("ErrorMessages", Report.ErrorMessages);
		}

		protected abstract void BuildRequestBody();

		public virtual Task<bool> Send(string recipientName, string recipientAddress, string mailSubject)
		{
			return SendAsync(URL, recipientName, recipientAddress, mailSubject);
		}

		protected virtual async Task<bool> SendAsync(string envUrl, string recipientName, string recipientAddress, string mailSubject)
		{
			RecipientName = recipientName;
			RecipientAddress = recipientAddress;
			MailSubject = mailSubject;

			using (var client = new HttpClient())
			{
				BuildBaseRequestBody();
				BuildRequestBody();

				string json = JsonConvert.SerializeObject(RequestBody);
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
