using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;

namespace tools.ProcessTools
{
	public class Watcher : IDisposable
	{
		public bool DisposedValue;

		private System.Timers.Timer _watcherTimer = new System.Timers.Timer();

		private string _processName;

		public bool _inited;

		public Watcher()
		{
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.DisposedValue)
			{
				if (disposing)
				{
					this._watcherTimer.Dispose();
				}
				this.DisposedValue = true;
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Initialize(string procName)
		{
			if (!this._inited)
			{
				this._watcherTimer.Interval = 2000;
				this._watcherTimer.Elapsed += new ElapsedEventHandler(this.OnTick);
				this._processName = procName;
				this._inited = true;
			}
		}

		private void OnTick(object sender, ElapsedEventArgs e)
		{
			Process[] processesByName = Process.GetProcessesByName(this._processName);
			if (processesByName.Length != 0)
			{
				processesByName[0].Exited += new EventHandler((object ee, EventArgs eee) => this._watcherTimer.Start());
				processesByName[0].EnableRaisingEvents = true;
				this._watcherTimer.Stop();
				this.OnProcessMade();
			}
		}

		public void Start()
		{
			if (this.OnProcessMade == null)
			{
				throw new Exception("Expected 'onProcessMade' Delegate. None was given");
			}
			if (!this._inited)
			{
				throw new Exception("Expected Processwatcher to be initialized");
			}
			this._watcherTimer.Start();
		}

		public void Stop()
		{
			if (this.OnProcessMade == null)
			{
				throw new Exception("Expected 'onProcessMade' Delegate. None was given");
			}
			if (!this._inited)
			{
				throw new Exception("Expected Processwatcher to be initialized");
			}
			this._watcherTimer.Stop();
		}

		public void SwitchProcess(string name)
		{
			this._processName = name;
		}

		public event Watcher.ProcessDelegate OnProcessMade;

		public delegate void ProcessDelegate();
	}
}