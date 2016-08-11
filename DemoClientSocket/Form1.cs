using System;
using System.Text;
using System.Windows.Forms;
using Teltec.Backup.Ipc.TcpSocket;

namespace ClientSocketSim
{
	public partial class Form1 : Form
	{
		private Client Client;

		public Form1()
		{
			InitializeComponent();

			Client = new Client(this);
			Client.MessageReceived += Client_MessageReceived;
			Client.MessageSent += Client_MessageSent;
			Client.Connected += Client_Connected;
			Client.Disconnected += Client_Disconnected;
			Client.ConnectionFailed += Client_ConnectionFailed;
		}

		void Client_ConnectionFailed(object sender, ClientErrorEventArgs e)
		{
			Client cli = (Client)sender;
			AppendToHistory("CONNECTION FAILED TO {0}:{1}: {2}", cli.RemoteIP, cli.RemotePort, e.Reason);

			btnConnect.Enabled = true;
			btnConnect.Text = "connect";
		}

		void Client_Disconnected(object sender, ClientConnectedEventArgs e)
		{
			Client cli = (Client)sender;
			AppendToHistory("DISCONNECTED");

			btnConnect.Enabled = true;
			btnConnect.Text = "connect";
		}

		void Client_Connected(object sender, ClientConnectedEventArgs e)
		{
			lbxHistory.Items.Clear();

			Client cli = (Client)sender;
			AppendToHistory("CONNECTED TO {0}:{1}", cli.RemoteIP, cli.RemotePort);

			btnConnect.Enabled = true;
			btnConnect.Text = "disconnect";
		}

		void Client_MessageSent(object sender, ClientSendEventArgs e)
		{
			AppendToHistory("SENT ({0} bytes)", e.BytesSent);
		}

		void Client_MessageReceived(object sender, ClientReceiveEventArgs e)
		{
			string data = Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);
			string[] lines = data.Split('\n');
			foreach (string line in lines)
			{
				if (!string.IsNullOrEmpty(line))
					AppendToHistory("RECEIVED ({0} bytes): {1}", e.BytesReceived, line);
			}
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{
			if (!Client.IsConnected)
			{
				btnConnect.Enabled = false;
				Client.Connect("127.0.0.1", 8000, true);
			}
			else
			{
				btnConnect.Enabled = false;
				Client.Disconnect();
			}
		}

		private void txtInput_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
			{
				if (!Client.IsConnected)
				{
					AppendToHistory("NOT CONNECTED");
					return;
				}

				string data = txtInput.Text;
				txtInput.Clear();

				AppendToHistory("SENDING: {0}", data);

				byte[] message = Encoding.UTF8.GetBytes(data + "\n");
				Client.Send(message);
			}
		}

		private void AppendToHistory(string format, params object[] args)
		{
			lbxHistory.Items.Add(string.Format(format, args));
			lbxHistory.SelectedIndex = lbxHistory.Items.Count - 1;
		}

		#region Dispose Pattern Implementation

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
				Client.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
