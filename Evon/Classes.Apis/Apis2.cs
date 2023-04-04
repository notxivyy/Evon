using FluxAPI;
using KrnlAPI;
using Oxygen;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using tools.Pipes;

namespace Evon.Classes.Apis
{
	public static class Apis
	{
		public static tools.Pipes.Pipe p;

		private static KrnlApi kAPI;

		private static FluxAPI.API fAPI;

		private static int injectionms;

		private static int pipems;

		public static int selected;

		static Apis()
		{
			Evon.Classes.Apis.Apis.p = new tools.Pipes.Pipe("EvonSakpot");
			Evon.Classes.Apis.Apis.kAPI = new KrnlApi();
			Evon.Classes.Apis.Apis.fAPI = new FluxAPI.API();
			Evon.Classes.Apis.Apis.injectionms = 0;
			Evon.Classes.Apis.Apis.pipems = 0;
			Evon.Classes.Apis.Apis.selected = 1;
		}

		public static bool Execute(string script)
		{
			bool flag;
			try
			{
				switch (Evon.Classes.Apis.Apis.selected)
				{
					case 0:
					{
						switch (Execution.Execute(script))
						{
							case Execution.ExecutionResult.Success:
							{
								flag = true;
								return flag;
							}
							case Execution.ExecutionResult.DLLNotFound:
							{
								MessageBox.Show("Couldn't find the Oxygen U dll, please make sure your anti-virus's exclusions has had the EVON folder added.\nFirst time executing? Make sure you're injected.");
								flag = false;
								return flag;
							}
							case Execution.ExecutionResult.PipeNotFound:
							{
								MessageBox.Show("Please inject EVON before trying to execute.");
								flag = false;
								return flag;
							}
							case Execution.ExecutionResult.Failed:
							{
								MessageBox.Show("Something unexpected happened while attempting to execute your script. Sorry!");
								flag = false;
								return flag;
							}
						}
						flag = false;
						return flag;
					}
					case 1:
					{
						if ((int)Process.GetProcessesByName("RobloxPlayerBeta").Length < 1)
						{
							MessageBox.Show("Please open ROBLOX before executing.");
							flag = false;
							return flag;
						}
						else if (Evon.Classes.Apis.Apis.p.Exists())
						{
							Evon.Classes.Apis.Apis.p.Write(script);
							flag = true;
							return flag;
						}
						else
						{
							MessageBox.Show("Please inject EVON.");
							flag = false;
							return flag;
						}
					}
					case 2:
					{
						if ((int)Process.GetProcessesByName("RobloxPlayerBeta").Length >= 1)
						{
							Evon.Classes.Apis.Apis.fAPI.Execute(script, true);
							flag = true;
							return flag;
						}
						else
						{
							MessageBox.Show("Please open ROBLOX before executing.");
							flag = false;
							return flag;
						}
					}
					case 3:
					{
						if (!Evon.Classes.Apis.Apis.kAPI.IsInitialized())
						{
							MessageBox.Show("Please try injecting first before executing.");
							flag = false;
							return flag;
						}
						else if ((int)Process.GetProcessesByName("RobloxPlayerBeta").Length >= 1)
						{
							flag = Evon.Classes.Apis.Apis.kAPI.Execute(script);
							return flag;
						}
						else
						{
							MessageBox.Show("Please open ROBLOX before executing.");
							flag = false;
							return flag;
						}
					}
				}
				flag = false;
				return flag;
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		public static Task<bool> Inject()
		{
			Task<bool> task;
			TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
			try
			{
				(new Thread(async () => {
					Evon.Classes.Apis.Apis.<>c__DisplayClass7_0.<<Inject>b__0>d _ = null;
					AsyncVoidMethodBuilder asyncVoidMethodBuilder = AsyncVoidMethodBuilder.Create();
					asyncVoidMethodBuilder.Start<Evon.Classes.Apis.Apis.<>c__DisplayClass7_0.<<Inject>b__0>d>(ref _);
				})).Start();
				task = taskCompletionSource.Task;
			}
			catch
			{
				taskCompletionSource.SetResult(false);
				task = taskCompletionSource.Task;
			}
			return task;
		}
	}
}