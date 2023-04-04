using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace tools.Pipes
{
	public class Pipe
	{
		public string Name
		{
			get;
			set;
		}

		public Pipe(string n)
		{
			this.Name = n;
		}

		public bool Exists()
		{
			bool flag = Pipe.WaitNamedPipe(string.Concat("\\\\.\\pipe\\", this.Name), 10);
			return flag;
		}

		public string Read()
		{
			string end;
			if (this.Name == null)
			{
				throw new Exception("Pipe Name was not set.");
			}
			if (!this.Exists())
			{
				end = "";
			}
			else
			{
				using (NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(".", this.Name, PipeDirection.InOut))
				{
					namedPipeClientStream.Connect();
					using (StreamReader streamReader = new StreamReader(namedPipeClientStream))
					{
						end = streamReader.ReadToEnd();
					}
				}
			}
			return end;
		}

		[DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=false, SetLastError=true)]
		public static extern bool WaitNamedPipe(string pipe, int timeout = 10);

		public bool Write(string content)
		{
			bool flag;
			if (this.Name == null)
			{
				throw new Exception("Pipe Name was not set.");
			}
			if ((string.IsNullOrWhiteSpace(content) ? true : string.IsNullOrEmpty(content)))
			{
				flag = false;
			}
			else if (!this.Exists())
			{
				flag = false;
			}
			else
			{
				using (NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(".", this.Name, PipeDirection.InOut))
				{
					namedPipeClientStream.Connect();
					using (StreamWriter streamWriter = new StreamWriter(namedPipeClientStream))
					{
						streamWriter.Write(content);
					}
					flag = true;
				}
			}
			return flag;
		}
	}
}