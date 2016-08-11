using System;
using System.Text;
using System.Windows.Forms;
using Teltec.Backup.Ipc.TcpSocket;

namespace ServerSocketSim
{
	public partial class Form1 : Form, IDisposable
	{
		private ServerHandler Handler;

		public Form1()
		{
			InitializeComponent();

			Handler = new ServerHandler(this);
			Handler.Server.MessageReceived += Server_MessageReceived;
			Handler.Server.MessageSent += Server_MessageSent;
			Handler.Server.Connected += Server_Connected;
			Handler.Server.Disconnected += Server_Disconnected;
		}

		void Server_Disconnected(object sender, ServerConnectedEventArgs e)
		{
			AppendToHistory("CLIENT DISCONNECTED: {0}", e.Context.ClientKey);
		}

		void Server_Connected(object sender, ServerConnectedEventArgs e)
		{
			AppendToHistory("CLIENT CONNECTED: {0}", e.Context.ClientKey);
		}

		void Server_MessageSent(object sender, ServerSendEventArgs e)
		{
			AppendToHistory("SENT TO {0} ({1} bytes)", e.Context.ClientKey, e.BytesSent);
		}

		void Server_MessageReceived(object sender, ServerReceiveEventArgs e)
		{
			string data = Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);
			string[] lines = data.Split('\n');
			foreach (string line in lines)
			{
				if (!string.IsNullOrEmpty(line))
					AppendToHistory("RECEIVED FROM {0} ({1} bytes): {2}", e.Context.ClientKey, e.BytesReceived, line);
			}
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			if (!Handler.IsRunning)
			{
				lbxHistory.Items.Clear();

				btnStart.Enabled = false;
				Handler.Start("127.0.0.1", 8000);

				btnStart.Enabled = true;
				btnStart.Text = "stop";
			}
			else
			{
				btnStart.Enabled = false;
				Handler.RequestStop();
				Handler.Wait();

				btnStart.Enabled = true;
				btnStart.Text = "start";
			}
		}

		private void txtInput_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
			{
				if (!Handler.IsRunning)
				{
					AppendToHistory("NOT CONNECTED");
					return;
				}

				string data = txtInput.Text;
				txtInput.Clear();

				AppendToHistory("SENDING: {0}", data);

				Handler.Send(null, data);
			}
		}

		private void AppendToHistory(string format, params object[] args)
		{
			lbxHistory.Items.Add(string.Format(format, args));
			lbxHistory.SelectedIndex = lbxHistory.Items.Count - 1;
		}

		#region Dispose Pattern Implementation

		bool _shouldDispose = true;
		bool _isDisposed;

		/// <summary>
		/// Implements the Dispose pattern
		/// </summary>
		/// <param name="disposing">Whether this object is being disposed via a call to Dispose
		/// or garbage collected.</param>
		protected override void Dispose(bool disposing)
		{
			if (!this._isDisposed)
			{
				if (disposing && _shouldDispose)
				{
					if (components != null)
						components.Dispose();
					if (Handler != null)
						Handler.Dispose();
				}
				base.Dispose(disposing);
				this._isDisposed = true;
			}
		}

		#endregion
	}
}
