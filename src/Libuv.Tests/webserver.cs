using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using Libuv;

namespace Libuv.Tests {
	class webserver {
		[DllImport ("uv")]
		public static extern void uv_init ();
		[DllImport ("uv")]
		public static extern void uv_run ();
		static int clientcount = 0;
		static void Main ()
		{
			var endpoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1}), 8080);
			uv_init();

			var watch = new PrepareWatcher(() => {
				//Console.WriteLine("Prepare Watcher Called");
			});
			watch.Start();
			var server = new TcpServer((socket) => {
				clientcount++;
				socket.Write(System.Text.Encoding.ASCII.GetBytes(clientcount.ToString()), 1);
				if (clientcount > 5) {
					socket.Close();
				}
				Console.WriteLine("Client Connected");
				socket.OnData += (data) => {
					Console.WriteLine("Data Recieved: {0}", System.Text.Encoding.ASCII.GetString(data, 0, data.Length));
					socket.Write(data, data.Length);
				};
				//socket.OnClose += () => {
				//	Console.WriteLine("Client Disconnected");
				//};
			});
			server.Listen(endpoint);
			var client = new TcpSocket();
			client.OnData += (data) => {
				Console.WriteLine("Client Recieved: {0}", System.Text.Encoding.ASCII.GetString(data, 0, data.Length));
				watch.Stop();
				watch.Dispose();
				client.Close();
			};
			client.Connect(endpoint, () => {
				byte[] message = System.Text.Encoding.ASCII.GetBytes("Hello World\n");
				client.Write(message, message.Length);
			});
			var pipeserver = new PipeServer((socket) => {
				clientcount++;
				socket.Write(System.Text.Encoding.ASCII.GetBytes(clientcount.ToString()), 1);
				if (clientcount > 5) {
					socket.Close();
				}
				Console.WriteLine("Pipe Client Connected");
				socket.OnData += (data) => {
					Console.WriteLine("Pipe Data Recieved: {0}", System.Text.Encoding.ASCII.GetString(data, 0, data.Length));
					socket.Write(data, data.Length);
				};
				//socket.OnClose += () => {
				//	Console.WriteLine("Client Disconnected");
				//};
			});
			pipeserver.Listen("libuv-csharp");
			var pipeclient = new PipeSocket();
			pipeclient.OnData += (data) => {
				Console.WriteLine("Pipe Client Recieved: {0}", System.Text.Encoding.ASCII.GetString(data, 0, data.Length));
				watch.Stop();
				watch.Dispose();
				pipeclient.Close();
			};
			pipeclient.Connect("libuv-csharp", () => {
				byte[] message = System.Text.Encoding.ASCII.GetBytes("Hello World\n");
				pipeclient.Write(message, message.Length);
			});
			var watch2 = new PrepareWatcher(() => {
				//Console.WriteLine("Prepare Watcher 2 Called");
			});
			watch2.Start();
			var check = new CheckWatcher(() => {
				//Console.WriteLine("Check Watcher Called");
			});
			check.Start();
			var idle = new IdleWatcher(() => {
				//Console.WriteLine("Idle Watcher Called");
			});
			idle.Start();
			var after = new TimerWatcher(new TimeSpan(0,0,5), new TimeSpan(1,0,0), () => {
				//Console.WriteLine("After 5 Seconds");
			});
			after.Start();
			var every = new TimerWatcher(new TimeSpan(0,0,5), () => {
				//Console.WriteLine("Every 5 Seconds");
			//	after.Stop();
			});
			every.Start();
			uv_run();
		}
	}
}
