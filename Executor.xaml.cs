using ColorPicker;
using ColorPicker.Models;
using Evon.Classes.Apis;
using Evon.Editor;
using Evon.UserControls;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using tools;
using tools.Pipes;
using tools.ProcessTools;

namespace Evon
{
	public partial class Executor : Window
	{
		private bool showNotiff = false;

		private Dictionary<TabItem, string> Texts = new Dictionary<TabItem, string>();

		private string searchingsystem = "";

		private int pagenum = 1;

		private bool showScripts;

		private int pipeDelay;

		private bool doingDownload;

		private Brush DefaultBrush = new SolidColorBrush(Color.FromRgb(153, 0, 255));

		private WebView editor = new WebView("");

		private Watcher w = new Watcher();

		public List<GameRectangle> scriptItems = new List<GameRectangle>();

		public static bool IsAdministrator
		{
			get
			{
				return (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
			}
		}

		public Executor()
		{
			if (!Directory.Exists("bin"))
			{
				Directory.CreateDirectory("bin");
			}
			Directory.CreateDirectory("bin\\tabs");
			if (!Directory.Exists("scripts"))
			{
				Directory.CreateDirectory("scripts");
			}
			if (!File.Exists(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon")))
			{
				File.WriteAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon"), "{}");
			}
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows NT\\CurrentVersion");
			if (registryKey.GetValue("ProductName").ToString().Contains("Windows 7"))
			{
				MessageBox.Show("Windows 7 is not supported with EVON. We're terribly sorry for this inconvenience");
				base.Close();
			}
			registryKey.Dispose();
			this.InitializeComponent();
			this.editor.OnTextChanged += new WebView.TextChanged((string text) => {
				string str;
				if (this.tabs.SelectedItem != null)
				{
					TabItem selectedItem = (TabItem)this.tabs.SelectedItem;
					if (!this.Texts.TryGetValue(selectedItem, out str))
					{
						this.Texts.Add(selectedItem, text);
					}
					else
					{
						this.Texts[selectedItem] = text;
					}
				}
			});
			base.Hide();
			this.w.OnProcessMade += new Watcher.ProcessDelegate(() => base.Dispatcher.Invoke<Task>(async () => {
				await Task.Delay(5000);
				this.doInject(null, null);
			}));
			this.w.Initialize("RobloxPlayerBeta");
			this.setcolor();
			string str1 = null;
			if (!Evon.Executor.WebViewIsInstalled())
			{
				Evon.Executor.doDownload();
			}
			if (!Evon.Executor.isInstalled())
			{
				Evon.Executor.doRedistDownload();
			}
			if (!File.Exists("C:\\Windows\\System32\\msvcp140.dll"))
			{
				if (!Evon.Executor.IsAdministrator)
				{
					MessageBox.Show("Please restart evon as an admin so we can download missing files.");
					Process.GetCurrentProcess().Kill();
				}
				try
				{
					using (WebClient webClient = new WebClient())
					{
						webClient.DownloadFile(string.Concat("https://github.com/ahhh-ahhh/EVON-downloads/raw/main/msvcp", (Environment.Is64BitOperatingSystem ? "64" : "32"), ".dll"), "C:\\Windows\\System32\\msvcp140.dll");
					}
				}
				catch
				{
					Clipboard.SetText(string.Concat("https://github.com/ahhh-ahhh/EVON-downloads/raw/main/msvcp", (Environment.Is64BitOperatingSystem ? "64" : "32"), ".dll"));
					MessageBox.Show("Something happened while attempting to download msvcp140. We've copied the link to your clipboard.");
					base.Close();
				}
			}
			try
			{
				using (WebClient webClient1 = new WebClient())
				{
					dynamic obj1 = JsonConvert.DeserializeObject(webClient1.DownloadString("https://clientsettingscdn.roblox.com/v2/client-version/WindowsPlayer"));
					if (obj1.clientVersionUpload != (dynamic)null)
					{
						str1 = (string)obj1.clientVersionUpload.ToString();
					}
				}
			}
			catch
			{
				MessageBox.Show("Failed to get the ROBLOX Version! Please make sure you have a valid internet connection and your connection to roblox.com is correct.");
				base.Close();
			}
			if (!File.Exists("version.data"))
			{
				if (MessageBox.Show("Failed to find a vital file! this could be because your antivirus deleted it, or that you didnt download the right verion of EVON.\nPlease make sure you download from https://sakpot.com/evon-executor .\nWould you like us to open it in your default browser?", "EVON", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					try
					{
						Process.Start("https://sakpot.com/evon-executor");
					}
					catch
					{
						MessageBox.Show("Couldn't open the link in your default browser! Please make sure you have a default browser set in your settings. The URL has been copied to your clipboard for ease of use.", "EVON");
						Clipboard.SetText("https://sakpot.com/evon-executor");
					}
				}
				Process.GetCurrentProcess().Kill();
			}
			if (File.ReadAllText("version.data") != str1)
			{
				if (MessageBox.Show("The current version you're using is out of date, this could be because the exploit is currently patched, or you have downloaded an older verison of EVON. Please make sure you downloaded from https://sakpot.com/evon-executor .\nWould you like us to open it in your default browser?", "EVON", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					try
					{
						Process.Start("https://sakpot.com/evon-executor");
					}
					catch
					{
						MessageBox.Show("Couldn't open the link in your default browser! Please make sure you have a default browser set in your settings. The URL has been copied to your clipboard for ease of use.", "EVON");
						Clipboard.SetText("https://sakpot.com/evon-executor");
					}
				}
				Process.GetCurrentProcess().Kill();
			}
			if (!File.Exists("Evon.dll"))
			{
				if (MessageBox.Show("Failed to find Evon.DLL! this could be because your antivirus deleted it, or that you didnt download the right verion of EVON.\nPlease make sure you download from https://sakpot.com/evon-executor .\nWould you like us to open it in your default browser?", "EVON", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					try
					{
						Process.Start("https://sakpot.com/evon-executor");
					}
					catch
					{
						MessageBox.Show("Couldn't open the link in your default browser! Please make sure you have a default browser set in your settings. The URL has been copied to your clipboard for ease of use.", "EVON");
						Clipboard.SetText("https://sakpot.com/evon-executor");
					}
				}
				Process.GetCurrentProcess().Kill();
			}
			if (!File.Exists("Oxygen API.dll"))
			{
				if (MessageBox.Show("Failed to find Oxygen API.DLL! this could be because your antivirus deleted it, or that you didnt download the right verion of EVON.\nPlease make sure you download from https://sakpot.com/evon-executor .\nWould you like us to open it in your default browser?", "EVON", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					try
					{
						Process.Start("https://sakpot.com/evon-executor");
					}
					catch
					{
						MessageBox.Show("Couldn't open the link in your default browser! Please make sure you have a default browser set in your settings. The URL has been copied to your clipboard for ease of use.", "EVON");
						Clipboard.SetText("https://sakpot.com/evon-executor");
					}
				}
				Process.GetCurrentProcess().Kill();
			}
			if (!File.Exists("KrnlAPI.dll"))
			{
				if (MessageBox.Show("Failed to find KrnlAPI.DLL! this could be because your antivirus deleted it, or that you didnt download the right verion of EVON.\nPlease make sure you download from https://sakpot.com/evon-executor .\nWould you like us to open it in your default browser?", "EVON", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					try
					{
						Process.Start("https://sakpot.com/evon-executor");
					}
					catch
					{
						MessageBox.Show("Couldn't open the link in your default browser! Please make sure you have a default browser set in your settings. The URL has been copied to your clipboard for ease of use.", "EVON");
						Clipboard.SetText("https://sakpot.com/evon-executor");
					}
				}
				Process.GetCurrentProcess().Kill();
			}
			if (!File.Exists("FluxAPI.dll"))
			{
				if (MessageBox.Show("Failed to find FluxAPI.DLL! this could be because your antivirus deleted it, or that you didnt download the right verion of EVON.\nPlease make sure you download from https://sakpot.com/evon-executor .\nWould you like us to open it in your default browser?", "EVON", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					try
					{
						Process.Start("https://sakpot.com/evon-executor");
					}
					catch
					{
						MessageBox.Show("Couldn't open the link in your default browser! Please make sure you have a default browser set in your settings. The URL has been copied to your clipboard for ease of use.", "EVON");
						Clipboard.SetText("https://sakpot.com/evon-executor");
					}
				}
				Process.GetCurrentProcess().Kill();
			}
			FileSystemWatcher fileSystemWatcher = new FileSystemWatcher("scripts")
			{
				EnableRaisingEvents = true,
				IncludeSubdirectories = true,
				NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
			};
			fileSystemWatcher.Changed += new FileSystemEventHandler((object poop, FileSystemEventArgs pee) => this.reloadScripts(null, null));
			fileSystemWatcher.Created += new FileSystemEventHandler((object poop, FileSystemEventArgs pee) => this.reloadScripts(null, null));
			fileSystemWatcher.Deleted += new FileSystemEventHandler((object poop, FileSystemEventArgs pee) => this.reloadScripts(null, null));
			fileSystemWatcher.Renamed += new RenamedEventHandler((object poop, RenamedEventArgs pee) => this.reloadScripts(null, null));
			dynamic obj9 = JsonConvert.DeserializeObject(File.ReadAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon")));
			if (obj9.topmost == (dynamic)null)
			{
				obj9.topmost = false;
			}
			if (obj9.legacy == (dynamic)null)
			{
				obj9.legacy = false;
			}
			if (obj9.antiskid == (dynamic)null)
			{
				obj9.antiskid = false;
			}
			if (obj9.minimap == (dynamic)null)
			{
				obj9.minimap = false;
			}
			if (obj9.autoinject == (dynamic)null)
			{
				obj9.autoinject = false;
			}
			if (obj9.unlockfps == (dynamic)null)
			{
				obj9.unlockfps = false;
			}
			if (!File.Exists("bin\\info.evon"))
			{
				File.WriteAllText("bin\\info.evon", "true");
				if (MessageBox.Show("Would you like to join our discord server for constant updates about EVON? It may seem annoying that we make this request but do not worry, it's only a one-time request", "Evon - Invite", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					try
					{
						Process.Start("https://discord.gg/YpXFb3xUqz");
					}
					catch
					{
						Clipboard.SetText("https://discord.gg/YpXFb3xUqz");
						MessageBox.Show("Couldn't open your default browser, please make sure that you have one set. For now, we've copied our cute invite to your clipboard!");
					}
				}
			}
			if (!Directory.Exists("bin\\tabs"))
			{
				Directory.CreateDirectory("bin\\tabs");
			}
			base.Topmost = (bool)obj9.topmost;
			this.TopMostCheck.IsChecked = new bool?((bool)obj9.topmost);
			this.antiskidcheck.IsChecked = new bool?((bool)obj9.antiskid);
			this.minimapCheck.IsChecked = new bool?((bool)obj9.minimap);
			this.autoinjectcheck.IsChecked = new bool?((bool)obj9.autoinject);
			this.unlockfpscheck.IsChecked = new bool?((bool)obj9.unlockfps);
			if (obj9.defaultexec == (dynamic)null)
			{
				obj9.defaultexec = 1;
				Apis.selected = (int)obj9.defaultexec;
				this.api_evon.IsChecked = new bool?(true);
			}
			else
			{
				switch ((int)obj9.defaultexec)
				{
					case 0:
					{
						this.api_oxygen.IsChecked = new bool?(true);
						break;
					}
					case 1:
					{
						this.api_evon.IsChecked = new bool?(true);
						break;
					}
					case 2:
					{
						this.api_fluxus.IsChecked = new bool?(true);
						break;
					}
					case 3:
					{
						this.api_krnl.IsChecked = new bool?(true);
						break;
					}
					default:
					{
						this.api_evon.IsChecked = new bool?(true);
						break;
					}
				}
				Apis.selected = (int)obj9.defaultexec;
			}
			if (!this.autoinjectcheck.IsChecked.Value)
			{
				this.w.Stop();
			}
			else
			{
				this.w.Start();
			}
			typeof(File).WriteAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon"), JsonConvert.SerializeObject(obj9));
			base.Show();
			this.reloadrScripts();
			this.reloadScripts(null, null);
			if (!Directory.Exists("runtimes"))
			{
				using (WebClient webClient2 = new WebClient())
				{
					try
					{
						webClient2.DownloadFile("https://github.com/ahhh-ahhh/EVON-downloads/raw/main/runtimes.zip", string.Concat(Path.GetTempPath(), "\\runtimes.zip"));
						ZipFile.ExtractToDirectory(string.Concat(Path.GetTempPath(), "\\runtimes.zip"), "./");
						File.Delete(string.Concat(Path.GetTempPath(), "\\runtimes.zip"));
					}
					catch
					{
						MessageBox.Show("Something happened while downloading files required for EVON. Please make sure your Firewall isnt blocking https://www.github.com .", "EVON", MessageBoxButton.OK, MessageBoxImage.Hand);
						Process.GetCurrentProcess().Kill();
					}
				}
			}
			if (!File.Exists("bin\\Monaco.html"))
			{
				using (WebClient webClient3 = new WebClient())
				{
					try
					{
						webClient3.DownloadFile("https://raw.githubusercontent.com/ahhh-ahhh/EVON-downloads/main/Monaco.html", "bin\\Monaco.html");
					}
					catch
					{
						MessageBox.Show("Something happened while downloading files required for EVON. Please make sure your Firewall isnt blocking https://www.github.com .", "EVON", MessageBoxButton.OK, MessageBoxImage.Hand);
						Process.GetCurrentProcess().Kill();
					}
				}
			}
			if (!Directory.Exists("bin\\vs"))
			{
				using (WebClient webClient4 = new WebClient())
				{
					try
					{
						webClient4.DownloadFile("https://github.com/ahhh-ahhh/EVON-downloads/raw/main/vs.zip", string.Concat(Path.GetTempPath(), "\\vs.zip"));
						ZipFile.ExtractToDirectory(string.Concat(Path.GetTempPath(), "\\vs.zip"), "bin");
						File.Delete(string.Concat(Path.GetTempPath(), "\\vz.zip"));
					}
					catch
					{
						MessageBox.Show("Something happened while downloading files required for EVON. Please make sure your Firewall isnt blocking https://www.github.com .", "EVON", MessageBoxButton.OK, MessageBoxImage.Hand);
						Process.GetCurrentProcess().Kill();
					}
				}
			}
		}

		private void addTab(object sender, RoutedEventArgs e)
		{
			this.createTab("New Tab", "");
		}

		private void closeUI(object sender, RoutedEventArgs e)
		{
			base.Close();
		}

		private void colorbtnc(object sender, RoutedEventArgs e)
		{
			this.colrp(sender, null);
		}

		private void colrp(object sender, ContextMenuEventArgs e)
		{
			Evon.Executor.ThemeStrings themeString = new Evon.Executor.ThemeStrings()
			{
				Theme = new List<Evon.Executor.ThemeStrings.ThemeSystem>()
				{
					new Evon.Executor.ThemeStrings.ThemeSystem()
					{
						ThemeManufacturer = "Evon",
						Color1 = (new SolidColorBrush(Color.FromArgb(255, (byte)this.colr.get_Color().get_RGB_R(), (byte)this.colr.get_Color().get_RGB_G(), (byte)this.colr.get_Color().get_RGB_B()))).ToString(),
						TextboxImage = ""
					}
				}
			};
			try
			{
				File.WriteAllText("./bin/theme.evon", JsonConvert.SerializeObject(themeString, 1));
			}
			catch
			{
			}
			object obj1 = JsonConvert.DeserializeObject(File.ReadAllText("./bin/theme.evon"));
			obj1 = JsonConvert.DeserializeObject(File.ReadAllText("./bin/theme.evon"));
			this.setcolor();
		}

		private void createTab(string name = "New Tab", string script = "")
		{
			RoutedEventHandler routedEventHandler2 = null;
			try
			{
				name = (name == "New Tab" ? string.Format("New Tab {0}", this.tabs.Items.Count + 1) : name);
				TabItem tabItem = new TabItem()
				{
					Style = (System.Windows.Style)base.TryFindResource("Tab"),
					BorderBrush = this.DefaultBrush,
					Header = new TextBox()
					{
						Text = name,
						IsHitTestVisible = false,
						IsEnabled = false,
						TextWrapping = TextWrapping.NoWrap,
						Style = (System.Windows.Style)base.TryFindResource("InvisibleTextBox"),
						MaxLength = 26
					},
					Content = this.editor
				};
				tabItem.MouseLeftButtonDown += new MouseButtonEventHandler((object a, MouseButtonEventArgs e) => {
					((WebView)tabItem.Content).antiskid();
					((WebView)tabItem.Content).minimap();
				});
				tabItem.MouseRightButtonDown += new MouseButtonEventHandler((object _, MouseButtonEventArgs __) => {
					TextBox header = (TextBox)tabItem.Header;
					header.IsEnabled = true;
					header.Focus();
					header.SelectAll();
				});
				tabItem.Loaded += new RoutedEventHandler((object loaded, RoutedEventArgs yay) => {
					TextBox header = (TextBox)tabItem.Header;
					Button button = (Button)tabItem.Template.FindName("CloseButton", tabItem);
					RoutedEventHandler u003cu003e9_3 = routedEventHandler2;
					if (u003cu003e9_3 == null)
					{
						RoutedEventHandler routedEventHandler = (object _, RoutedEventArgs __) => this.tabs.Items.Remove(tabItem);
						RoutedEventHandler routedEventHandler1 = routedEventHandler;
						routedEventHandler2 = routedEventHandler;
						u003cu003e9_3 = routedEventHandler1;
					}
					button.Click += u003cu003e9_3;
					header.FocusableChanged += new DependencyPropertyChangedEventHandler((object _, DependencyPropertyChangedEventArgs e) => {
						if (!header.IsFocused)
						{
							header.IsEnabled = false;
						}
					});
					header.KeyDown += new KeyEventHandler((object _, KeyEventArgs e) => {
						if (e.Key == Key.Return)
						{
							header.IsEnabled = false;
						}
					});
				});
				this.tabs.Items.Add(tabItem);
				this.tabs.SelectedItem = tabItem;
				this.SetText(script);
			}
			catch
			{
			}
		}

		private void doAntiSkid(object sender, RoutedEventArgs e)
		{
			try
			{
				dynamic isChecked = JsonConvert.DeserializeObject(File.ReadAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon")));
				isChecked.antiskid = this.antiskidcheck.IsChecked;
				typeof(File).WriteAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon"), JsonConvert.SerializeObject(isChecked));
				foreach (TabItem item in (IEnumerable)this.tabs.Items)
				{
					((WebView)item.Content).antiskid();
				}
				WebView webView = this.editor;
				if (webView != null)
				{
					webView.antiskid();
				}
				else
				{
				}
			}
			catch
			{
				MessageBox.Show("there was an error while trying to set your settings, please make sure nothing is using the settings file.");
			}
		}

		private void doAutoInject(object sender, RoutedEventArgs e)
		{
			try
			{
				dynamic isChecked = JsonConvert.DeserializeObject(File.ReadAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon")));
				isChecked.autoinject = this.autoinjectcheck.IsChecked;
				typeof(File).WriteAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon"), JsonConvert.SerializeObject(isChecked));
			}
			catch
			{
				MessageBox.Show("there was an error while trying to set your settings, please make sure nothing is using the settings file.");
			}
			if (!this.autoinjectcheck.IsChecked.Value)
			{
				this.w.Stop();
			}
			else
			{
				this.w.Start();
			}
		}

		private void doClear(object sender, RoutedEventArgs e)
		{
			this.SetText("");
		}

		private void docpl(object sender, RoutedEventArgs e)
		{
			if (this.pagenum >= 1)
			{
				this.pagenum--;
				this.reloadrScripts();
			}
			else
			{
				MessageBox.Show("You cannot go back any further.");
			}
		}

		private void docpr(object sender, RoutedEventArgs e)
		{
			this.pagenum++;
			this.reloadrScripts();
		}

		public static async void doDownload()
		{
			Evon.Executor.<doDownload>d__16 variable = null;
			AsyncVoidMethodBuilder asyncVoidMethodBuilder = AsyncVoidMethodBuilder.Create();
			asyncVoidMethodBuilder.Start<Evon.Executor.<doDownload>d__16>(ref variable);
		}

		private void doDrag(object sender, MouseButtonEventArgs e)
		{
			base.DragMove();
		}

		private void doExecute(object sender, RoutedEventArgs e)
		{
			if (this.Texts[(TabItem)this.tabs.SelectedItem] != null)
			{
				Apis.Execute(this.GetText());
			}
		}

		private async void doInject(object sender, RoutedEventArgs e)
		{
			string str;
			this.pipeDelay = 0;
			this.injBtn.IsEnabled = false;
			Process[] processesByName = Process.GetProcessesByName("injector-evon.exe");
			for (int i = 0; i < (int)processesByName.Length; i++)
			{
				Process process = processesByName[i];
				process.Kill();
				process = null;
			}
			processesByName = null;
			if (await Apis.Inject())
			{
				if (Apis.selected == 1)
				{
					while (!Apis.p.Exists())
					{
						if (this.pipeDelay < 30000)
						{
							await Task.Delay(200);
							this.pipeDelay += 200;
						}
						else
						{
							MessageBox.Show("Couldn't find the pipe for EVON within the selected time! Maybe try setting your no rename mode to true?");
							this.pipeDelay = 0;
							this.injBtn.IsEnabled = true;
							return;
						}
					}
				}
				this.showNotif("Wohoo!", "Evon Injected.");
				Pipe pipe = Apis.p;
				str = (this.unlockfpscheck.IsChecked.Value ? "999" : "60");
				pipe.Write(string.Concat("setfpscap(", str, ")"));
				((Storyboard)base.TryFindResource("IconInjected")).Begin();
				while (Process.GetProcessesByName("RobloxPlayerBeta").Length != 0)
				{
					await Task.Delay(1000);
				}
				if (Process.GetProcessesByName("RobloxPlayerBeta").Length == 0)
				{
					((Storyboard)base.TryFindResource("IconNotInjected")).Begin();
				}
			}
			this.injBtn.IsEnabled = true;
		}

		private void doMinimap(object sender, RoutedEventArgs e)
		{
			try
			{
				dynamic isChecked = JsonConvert.DeserializeObject(File.ReadAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon")));
				isChecked.minimap = this.minimapCheck.IsChecked;
				typeof(File).WriteAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon"), JsonConvert.SerializeObject(isChecked));
				foreach (TabItem item in (IEnumerable)this.tabs.Items)
				{
					((WebView)item.Content).minimap();
				}
				WebView webView = this.editor;
				if (webView != null)
				{
					webView.minimap();
				}
				else
				{
				}
			}
			catch
			{
				MessageBox.Show("there was an error while trying to set your settings, please make sure nothing is using the settings file.");
			}
		}

		private void doOpen(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog()
			{
				Title = "EVON | Open Script",
				Filter = "Lua (*.lua) |*.lua|Text (*.txt) |*.txt",
				Multiselect = true
			};
			bool? nullable = openFileDialog.ShowDialog();
			if (nullable.GetValueOrDefault() & nullable.HasValue)
			{
				if ((int)openFileDialog.FileNames.Length > 1)
				{
					string[] fileNames = openFileDialog.FileNames;
					for (int i = 0; i < (int)fileNames.Length; i++)
					{
						string str = fileNames[i];
						this.createTab(Path.GetFileName(str), File.ReadAllText(str));
					}
				}
				else if ((this.tabs.SelectedItem == null ? true : this.Texts[(TabItem)this.tabs.SelectedItem] == null))
				{
					this.createTab(Path.GetFileName(openFileDialog.FileName), File.ReadAllText(openFileDialog.FileName));
				}
				else
				{
					this.SetText(File.ReadAllText(openFileDialog.FileName));
				}
			}
		}

		public static async void doRedistDownload()
		{
			Evon.Executor.<doRedistDownload>d__15 variable = null;
			AsyncVoidMethodBuilder asyncVoidMethodBuilder = AsyncVoidMethodBuilder.Create();
			asyncVoidMethodBuilder.Start<Evon.Executor.<doRedistDownload>d__15>(ref variable);
		}

		private void doSave(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog()
			{
				Title = "EVON | Save File",
				Filter = "Lua (*.lua) |*.lua | Text Files (*.txt) |*.txt"
			};
			bool? nullable = saveFileDialog.ShowDialog();
			if (nullable.GetValueOrDefault() & nullable.HasValue)
			{
				using (StreamWriter streamWriter = new StreamWriter(File.OpenWrite(saveFileDialog.FileName)))
				{
					if (this.tabs.SelectedItem != null)
					{
						streamWriter.Write(this.Texts[(TabItem)this.tabs.SelectedItem]);
					}
				}
			}
		}

		private void doScripts(object sender, RoutedEventArgs e)
		{
			this.showScripts = !this.showScripts;
			((Storyboard)base.TryFindResource((this.showScripts ? "showScripts" : "hideScripts"))).Begin();
		}

		private void doScriptSelection(object sender, SelectionChangedEventArgs e)
		{
			string selectedItem = (string)this.ScriptList.SelectedItem;
			if (File.Exists(string.Concat("scripts\\", selectedItem)))
			{
				this.createTab(selectedItem, File.ReadAllText(string.Concat("scripts\\", selectedItem)));
			}
		}

		private void doSearch(object sender, TextChangedEventArgs e)
		{
			this.ScriptList.Items.Clear();
			foreach (string str in 
				from script in Directory.EnumerateFiles("scripts", "*.*", SearchOption.AllDirectories)
				where script.ToLower().Contains(this.ListBoxSearch.Text.ToLower())
				select script)
			{
				this.ScriptList.Items.Add(Path.GetFileName(str));
			}
		}

		private void doSearchScripts(object sender, TextChangedEventArgs e)
		{
			this.pagenum = 1;
			this.searchingsystem = this.search.Text;
			this.reloadrScripts();
		}

		private void doTopMost(object sender, RoutedEventArgs e)
		{
			try
			{
				dynamic isChecked = JsonConvert.DeserializeObject(File.ReadAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon")));
				isChecked.topmost = this.TopMostCheck.IsChecked;
				base.Topmost = this.TopMostCheck.IsChecked.Value;
				typeof(File).WriteAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon"), JsonConvert.SerializeObject(isChecked));
			}
			catch
			{
				MessageBox.Show("there was an error while trying to set your settings, please make sure nothing is using the settings file.");
			}
		}

		private void doUnlockFPS(object sender, RoutedEventArgs e)
		{
			try
			{
				dynamic isChecked = JsonConvert.DeserializeObject(File.ReadAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon")));
				isChecked.unlockfps = this.unlockfpscheck.IsChecked;
				typeof(File).WriteAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon"), JsonConvert.SerializeObject(isChecked));
			}
			catch
			{
				MessageBox.Show("there was an error while trying to set your settings, please make sure nothing is using the settings file.");
			}
			if (Apis.p.Exists())
			{
				Apis.p.Write(string.Concat("setfpscap(", (this.unlockfpscheck.IsChecked.Value ? "999" : "60"), ")"));
			}
		}

		private async void fix268(object sender, RoutedEventArgs e)
		{
			this.fixbtn.IsEnabled = false;
			Process[] processesByName = Process.GetProcessesByName("RobloxPlayerBeta");
			for (int i = 0; i < (int)processesByName.Length; i++)
			{
				Process process = processesByName[i];
				process.Kill();
				process = null;
			}
			processesByName = null;
			this.showNotif("Wohoo!", "268 Fixes Applied.");
			this.fixbtn.IsEnabled = true;
		}

		private void GameRefreshB_Click(object sender, RoutedEventArgs e)
		{
		}

		private string GetText()
		{
			string str;
			string str1;
			TabItem selectedItem = (TabItem)this.tabs.SelectedItem;
			str1 = (!this.Texts.TryGetValue(selectedItem, out str) ? "" : str);
			return str1;
		}

		private async Task<string> getVersion()
		{
			Evon.Executor.<getVersion>d__49 variable = null;
			AsyncTaskMethodBuilder<string> asyncTaskMethodBuilder = AsyncTaskMethodBuilder<string>.Create();
			asyncTaskMethodBuilder.Start<Evon.Executor.<getVersion>d__49>(ref variable);
			return asyncTaskMethodBuilder.Task;
		}

		private void handleShortcuts(object sender, KeyEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.Control)
			{
				Key key = e.Key;
				if (key == Key.I)
				{
					this.doInject(sender, e);
				}
				else
				{
					switch (key)
					{
						case Key.N:
						{
							this.addTab(sender, e);
							break;
						}
						case Key.O:
						{
							this.doOpen(sender, e);
							break;
						}
						case Key.Q:
						{
							if (MessageBox.Show("Are you sure you want to close EVON?", "EVON", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								base.Close();
							}
							break;
						}
						case Key.S:
						{
							this.doSave(sender, e);
							break;
						}
					}
				}
			}
		}

		public static bool isInstalled()
		{
			bool flag;
			try
			{
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\DevDiv\\VC\\Servicing\\14.0\\RuntimeMinimum", false);
				if (registryKey != null)
				{
					object value = registryKey.GetValue("Version");
					flag = ((string)value == null ? false : ((string)value).StartsWith("14"));
				}
				else
				{
					flag = false;
				}
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		public static bool IsValidURI(string uri)
		{
			Uri uri1;
			bool flag;
			if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
			{
				flag = false;
			}
			else if (Uri.TryCreate(uri, UriKind.Absolute, out uri1))
			{
				flag = (uri1.Scheme == Uri.UriSchemeHttp ? true : uri1.Scheme == Uri.UriSchemeHttps);
			}
			else
			{
				flag = false;
			}
			return flag;
		}

		private void killRoblox(object sender, RoutedEventArgs e)
		{
			Process[] processesByName = Process.GetProcessesByName("RobloxPlayerBeta");
			for (int i = 0; i < (int)processesByName.Length; i++)
			{
				processesByName[i].Kill();
			}
			this.showNotif("Wohoo!", "Closed all open Roblox processes.");
		}

		private void minimizeUI(object sender, RoutedEventArgs e)
		{
			base.WindowState = System.Windows.WindowState.Minimized;
		}

		private void onClosing(object sender, CancelEventArgs e)
		{
			string str;
			TabItem selectedItem = (TabItem)this.tabs.SelectedItem;
			if (selectedItem != null)
			{
				if (!this.Texts.TryGetValue(selectedItem, out str))
				{
					this.Texts.Add(selectedItem, this.GetText());
				}
				else
				{
					this.Texts[selectedItem] = this.GetText();
				}
			}
			foreach (KeyValuePair<TabItem, string> text in this.Texts)
			{
				File.WriteAllText(string.Concat("./bin/tabs/", ((TextBox)text.Key.Header).Text), text.Value);
			}
			Process.GetCurrentProcess().Kill();
		}

		private void onDrop(object sender, DragEventArgs e)
		{
			try
			{
				string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
				for (int i = 0; i < (int)data.Length; i++)
				{
					string str = data[i];
					if (File.Exists(str))
					{
						if ((Path.GetExtension(str) == ".lua" ? false : Path.GetExtension(str) != ".txt"))
						{
							goto Label0;
						}
						this.createTab(Path.GetFileName(str), File.ReadAllText(str));
					}
				Label0:
				}
			}
			catch
			{
				MessageBox.Show("Something happened while attempting to handle your drag-drop.");
			}
		}

		private async void onLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				await ((Storyboard)base.TryFindResource("loaded")).Start();
				string[] files = Directory.GetFiles("bin\\tabs");
				for (int i = 0; i < (int)files.Length; i++)
				{
					string str = files[i];
					try
					{
						this.createTab(Path.GetFileName(str), File.ReadAllText(str));
					}
					catch
					{
					}
					str = null;
				}
				files = null;
				if (this.tabs.Items.Count < 1)
				{
					this.createTab("New Tab", "");
				}
			}
			catch
			{
			}
			this.reloadScripts(null, null);
		}

		private void onSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			string str;
			string str1;
			if (e.RemovedItems.Count > 0)
			{
				foreach (TabItem removedItem in e.RemovedItems)
				{
					if (!this.Texts.TryGetValue(removedItem, out str))
					{
						this.Texts.Add(removedItem, this.editor.Text);
					}
					else
					{
						this.Texts[removedItem] = this.editor.Text;
					}
				}
			}
			e.Handled = true;
			if (this.tabs.SelectedItem != null)
			{
				if (!this.Texts.TryGetValue((TabItem)this.tabs.SelectedItem, out str1))
				{
					this.editor.SetText("");
				}
				else
				{
					this.editor.SetText(str1);
				}
			}
		}

		private void reloadrScripts()
		{
			(new Thread(() => {
				RoutedEventHandler routedEventHandler2 = null;
				base.Dispatcher.Invoke(() => {
					this.ScriptList.Items.Clear();
					this.GameSys.Children.Clear();
				});
				try
				{
					if (this.searchingsystem == "")
					{
						this.searchingsystem = "a";
					}
					using (WebClient webClient = new WebClient())
					{
						JsonConvert.DeserializeObject(File.ReadAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon")));
						dynamic obj1 = JsonConvert.DeserializeObject(webClient.DownloadString(string.Concat("https://scriptblox.com/api/script/search?q=", this.searchingsystem, "&mode=free&max=100&page=", this.pagenum.ToString())));
						List<int> nums = new List<int>();
						BrushConverter brushConverter1 = new BrushConverter();
						foreach (object obj2 in (IEnumerable)obj1.result.scripts)
						{
							base.Dispatcher.Invoke(() => {
								if (Evon.Executor.<>o__21.<>p__5 == null)
								{
									Evon.Executor.<>o__21.<>p__5 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(string), typeof(Evon.Executor)));
								}
								!0 _u002101 = Evon.Executor.<>o__21.<>p__5.Target;
								CallSite<Func<CallSite, object, string>> u003cu003ep_5 = Evon.Executor.<>o__21.<>p__5;
								if (Evon.Executor.<>o__21.<>p__4 == null)
								{
									Evon.Executor.<>o__21.<>p__4 = CallSite<Func<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
								}
								!0 _u002102 = Evon.Executor.<>o__21.<>p__4.Target;
								CallSite<Func<CallSite, object, object>> u003cu003ep_4 = Evon.Executor.<>o__21.<>p__4;
								if (Evon.Executor.<>o__21.<>p__3 == null)
								{
									Evon.Executor.<>o__21.<>p__3 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "imageUrl", typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
								}
								!0 _u002103 = Evon.Executor.<>o__21.<>p__3.Target;
								CallSite<Func<CallSite, object, object>> u003cu003ep_3 = Evon.Executor.<>o__21.<>p__3;
								if (Evon.Executor.<>o__21.<>p__2 == null)
								{
									Evon.Executor.<>o__21.<>p__2 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "game", typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
								}
								string str = _u002101(u003cu003ep_5, _u002102(u003cu003ep_4, _u002103(u003cu003ep_3, Evon.Executor.<>o__21.<>p__2.Target(Evon.Executor.<>o__21.<>p__2, obj2))));
								if (!str.Contains("rbxcdn.com"))
								{
									if (Evon.Executor.<>o__21.<>p__10 == null)
									{
										Evon.Executor.<>o__21.<>p__10 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(string), typeof(Evon.Executor)));
									}
									!0 _u002104 = Evon.Executor.<>o__21.<>p__10.Target;
									CallSite<Func<CallSite, object, string>> u003cu003ep_10 = Evon.Executor.<>o__21.<>p__10;
									if (Evon.Executor.<>o__21.<>p__9 == null)
									{
										Evon.Executor.<>o__21.<>p__9 = CallSite<Func<CallSite, string, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Add, typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
									}
									!0 _u002105 = Evon.Executor.<>o__21.<>p__9.Target;
									CallSite<Func<CallSite, string, object, object>> u003cu003ep_9 = Evon.Executor.<>o__21.<>p__9;
									if (Evon.Executor.<>o__21.<>p__8 == null)
									{
										Evon.Executor.<>o__21.<>p__8 = CallSite<Func<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
									}
									!0 _u002106 = Evon.Executor.<>o__21.<>p__8.Target;
									CallSite<Func<CallSite, object, object>> u003cu003ep_8 = Evon.Executor.<>o__21.<>p__8;
									if (Evon.Executor.<>o__21.<>p__7 == null)
									{
										Evon.Executor.<>o__21.<>p__7 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "imageUrl", typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
									}
									!0 _u002107 = Evon.Executor.<>o__21.<>p__7.Target;
									CallSite<Func<CallSite, object, object>> u003cu003ep_7 = Evon.Executor.<>o__21.<>p__7;
									if (Evon.Executor.<>o__21.<>p__6 == null)
									{
										Evon.Executor.<>o__21.<>p__6 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "game", typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
									}
									str = _u002104(u003cu003ep_10, _u002105(u003cu003ep_9, "https://scriptblox.com", _u002106(u003cu003ep_8, _u002107(u003cu003ep_7, Evon.Executor.<>o__21.<>p__6.Target(Evon.Executor.<>o__21.<>p__6, obj2)))));
								}
								GameRectangle gameRectangle = new GameRectangle();
								gameRectangle.ScriptImage.ImageSource = (Evon.Executor.IsValidURI(str) ? new BitmapImage(new Uri(str)) : null);
								TextBlock scriptTitle = gameRectangle.ScriptTitle;
								if (Evon.Executor.<>o__21.<>p__13 == null)
								{
									Evon.Executor.<>o__21.<>p__13 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(string), typeof(Evon.Executor)));
								}
								!0 _u002108 = Evon.Executor.<>o__21.<>p__13.Target;
								CallSite<Func<CallSite, object, string>> u003cu003ep_13 = Evon.Executor.<>o__21.<>p__13;
								if (Evon.Executor.<>o__21.<>p__12 == null)
								{
									Evon.Executor.<>o__21.<>p__12 = CallSite<Func<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
								}
								!0 _u002109 = Evon.Executor.<>o__21.<>p__12.Target;
								CallSite<Func<CallSite, object, object>> u003cu003ep_12 = Evon.Executor.<>o__21.<>p__12;
								if (Evon.Executor.<>o__21.<>p__11 == null)
								{
									Evon.Executor.<>o__21.<>p__11 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "title", typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
								}
								scriptTitle.Text = _u002108(u003cu003ep_13, _u002109(u003cu003ep_12, Evon.Executor.<>o__21.<>p__11.Target(Evon.Executor.<>o__21.<>p__11, obj2)));
								GameRectangle gameSys = gameRectangle;
								gameSys.items = this.GameSys;
								Button executeB = gameSys.ExecuteB;
								RoutedEventHandler u003cu003e9_3 = routedEventHandler2;
								if (u003cu003e9_3 == null)
								{
									RoutedEventHandler routedEventHandler = (object ssss, RoutedEventArgs eeee) => {
										if (Evon.Executor.<>o__21.<>p__16 == null)
										{
											Evon.Executor.<>o__21.<>p__16 = CallSite<Action<CallSite, Type, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "Execute", null, typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
										}
										!0 target = Evon.Executor.<>o__21.<>p__16.Target;
										CallSite<Action<CallSite, Type, object>> u003cu003ep_16 = Evon.Executor.<>o__21.<>p__16;
										Type type = typeof(Apis);
										if (Evon.Executor.<>o__21.<>p__15 == null)
										{
											Evon.Executor.<>o__21.<>p__15 = CallSite<Func<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
										}
										!0 _u00210 = Evon.Executor.<>o__21.<>p__15.Target;
										CallSite<Func<CallSite, object, object>> u003cu003ep_15 = Evon.Executor.<>o__21.<>p__15;
										if (Evon.Executor.<>o__21.<>p__14 == null)
										{
											Evon.Executor.<>o__21.<>p__14 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "script", typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
										}
										target(u003cu003ep_16, type, _u00210(u003cu003ep_15, Evon.Executor.<>o__21.<>p__14.Target(Evon.Executor.<>o__21.<>p__14, obj2)));
									};
									RoutedEventHandler routedEventHandler1 = routedEventHandler;
									routedEventHandler2 = routedEventHandler;
									u003cu003e9_3 = routedEventHandler1;
								}
								executeB.Click += u003cu003e9_3;
								object obj = JsonConvert.DeserializeObject(File.ReadAllText("./bin/theme.evon"));
								Button button = gameSys.ExecuteB;
								if (Evon.Executor.<>o__21.<>p__22 == null)
								{
									Evon.Executor.<>o__21.<>p__22 = CallSite<Func<CallSite, object, Brush>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(Brush), typeof(Evon.Executor)));
								}
								!0 _u0021010 = Evon.Executor.<>o__21.<>p__22.Target;
								CallSite<Func<CallSite, object, Brush>> u003cu003ep_22 = Evon.Executor.<>o__21.<>p__22;
								if (Evon.Executor.<>o__21.<>p__21 == null)
								{
									Evon.Executor.<>o__21.<>p__21 = CallSite<Func<CallSite, BrushConverter, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ConvertFromString", null, typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
								}
								!0 _u0021011 = Evon.Executor.<>o__21.<>p__21.Target;
								CallSite<Func<CallSite, BrushConverter, object, object>> u003cu003ep_21 = Evon.Executor.<>o__21.<>p__21;
								BrushConverter brushConversion = brushConverter1;
								if (Evon.Executor.<>o__21.<>p__20 == null)
								{
									Evon.Executor.<>o__21.<>p__20 = CallSite<Func<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
								}
								!0 _u0021012 = Evon.Executor.<>o__21.<>p__20.Target;
								CallSite<Func<CallSite, object, object>> u003cu003ep_20 = Evon.Executor.<>o__21.<>p__20;
								if (Evon.Executor.<>o__21.<>p__19 == null)
								{
									Evon.Executor.<>o__21.<>p__19 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Color1", typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
								}
								!0 _u0021013 = Evon.Executor.<>o__21.<>p__19.Target;
								CallSite<Func<CallSite, object, object>> u003cu003ep_19 = Evon.Executor.<>o__21.<>p__19;
								if (Evon.Executor.<>o__21.<>p__18 == null)
								{
									Evon.Executor.<>o__21.<>p__18 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null) })));
								}
								!0 _u0021014 = Evon.Executor.<>o__21.<>p__18.Target;
								CallSite<Func<CallSite, object, int, object>> u003cu003ep_18 = Evon.Executor.<>o__21.<>p__18;
								if (Evon.Executor.<>o__21.<>p__17 == null)
								{
									Evon.Executor.<>o__21.<>p__17 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Theme", typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
								}
								button.Background = _u0021010(u003cu003ep_22, _u0021011(u003cu003ep_21, brushConversion, _u0021012(u003cu003ep_20, _u0021013(u003cu003ep_19, _u0021014(u003cu003ep_18, Evon.Executor.<>o__21.<>p__17.Target(Evon.Executor.<>o__21.<>p__17, obj), 0)))));
								DropShadowEffect buttonGlow = gameSys.ButtonGlow;
								if (Evon.Executor.<>o__21.<>p__28 == null)
								{
									Evon.Executor.<>o__21.<>p__28 = CallSite<Func<CallSite, object, SolidColorBrush>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(SolidColorBrush), typeof(Evon.Executor)));
								}
								!0 _u0021015 = Evon.Executor.<>o__21.<>p__28.Target;
								CallSite<Func<CallSite, object, SolidColorBrush>> u003cu003ep_28 = Evon.Executor.<>o__21.<>p__28;
								if (Evon.Executor.<>o__21.<>p__27 == null)
								{
									Evon.Executor.<>o__21.<>p__27 = CallSite<Func<CallSite, BrushConverter, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ConvertFromString", null, typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
								}
								!0 _u0021016 = Evon.Executor.<>o__21.<>p__27.Target;
								CallSite<Func<CallSite, BrushConverter, object, object>> u003cu003ep_27 = Evon.Executor.<>o__21.<>p__27;
								BrushConverter brushConverter = brushConverter1;
								if (Evon.Executor.<>o__21.<>p__26 == null)
								{
									Evon.Executor.<>o__21.<>p__26 = CallSite<Func<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
								}
								!0 _u0021017 = Evon.Executor.<>o__21.<>p__26.Target;
								CallSite<Func<CallSite, object, object>> u003cu003ep_26 = Evon.Executor.<>o__21.<>p__26;
								if (Evon.Executor.<>o__21.<>p__25 == null)
								{
									Evon.Executor.<>o__21.<>p__25 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Color1", typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
								}
								!0 _u0021018 = Evon.Executor.<>o__21.<>p__25.Target;
								CallSite<Func<CallSite, object, object>> u003cu003ep_25 = Evon.Executor.<>o__21.<>p__25;
								if (Evon.Executor.<>o__21.<>p__24 == null)
								{
									Evon.Executor.<>o__21.<>p__24 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null) })));
								}
								!0 _u0021019 = Evon.Executor.<>o__21.<>p__24.Target;
								CallSite<Func<CallSite, object, int, object>> u003cu003ep_24 = Evon.Executor.<>o__21.<>p__24;
								if (Evon.Executor.<>o__21.<>p__23 == null)
								{
									Evon.Executor.<>o__21.<>p__23 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "Theme", typeof(Evon.Executor), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
								}
								buttonGlow.Color = _u0021015(u003cu003ep_28, _u0021016(u003cu003ep_27, brushConverter, _u0021017(u003cu003ep_26, _u0021018(u003cu003ep_25, _u0021019(u003cu003ep_24, Evon.Executor.<>o__21.<>p__23.Target(Evon.Executor.<>o__21.<>p__23, obj), 0))))).Color;
								this.scriptItems.Add(gameSys);
								this.GameSys.Children.Add(gameSys);
							});
						}
						GC.Collect(2, GCCollectionMode.Forced);
					}
				}
				catch (Exception exception)
				{
					if (MessageBox.Show("Something happened while attempting to load scripts from https://www.rbxscripts.xyz . Please make sure your connection is valid. Would you like to retry fetching scripts?", "EVON", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					{
						this.reloadrScripts();
					}
				}
			})).Start();
		}

		public void reloadScripts(object sender = null, FileSystemEventArgs e = null)
		{
			base.Dispatcher.Invoke(() => {
				IEnumerable<string> strs = Directory.EnumerateFiles("scripts", "*.*", SearchOption.AllDirectories);
				this.ScriptList.Items.Clear();
				if (this.ListBoxSearch.Text == null)
				{
					foreach (string str in strs)
					{
						this.ScriptList.Items.Add(Path.GetFileName(str));
					}
				}
				else
				{
					foreach (string str1 in 
						from script in strs
						where script.ToLower().Contains(this.ListBoxSearch.Text.ToLower())
						select script)
					{
						this.ScriptList.Items.Add(Path.GetFileName(str1));
					}
				}
			});
		}

		private async void revert268(object sender, RoutedEventArgs e)
		{
			this.revertbtn.IsEnabled = false;
			this.showNotif("Wohoo!", "268 Fixes Reverted.");
			this.revertbtn.IsEnabled = true;
		}

		private void selectEvonAPI(object sender, RoutedEventArgs e)
		{
			Func<CheckBox, bool> func = null;
			Apis.selected = 1;
			IEnumerable<CheckBox> checkBoxes = Extensions.FindVisualChildren<CheckBox>(this);
			Func<CheckBox, bool> func1 = func;
			if (func1 == null)
			{
				Func<CheckBox, bool> func2 = (CheckBox b) => (b == (CheckBox)sender ? false : b.Name.Contains("api_"));
				Func<CheckBox, bool> func3 = func2;
				func = func2;
				func1 = func3;
			}
			foreach (CheckBox nullable in checkBoxes.Where<CheckBox>(func1))
			{
				nullable.IsChecked = new bool?(false);
			}
			try
			{
				dynamic obj = JsonConvert.DeserializeObject(File.ReadAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon")));
				obj.defaultexec = Apis.selected;
				typeof(File).WriteAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon"), JsonConvert.SerializeObject(obj));
			}
			catch
			{
				MessageBox.Show("there was an error while trying to set your settings, please make sure nothing is using the settings file.");
			}
		}

		private void selectExecutor(object sender, RoutedEventArgs e)
		{
			((Storyboard)base.TryFindResource("selectExecution")).Begin();
		}

		private void selectFluxus(object sender, RoutedEventArgs e)
		{
			Func<CheckBox, bool> func = null;
			Apis.selected = 2;
			IEnumerable<CheckBox> checkBoxes = Extensions.FindVisualChildren<CheckBox>(this);
			Func<CheckBox, bool> func1 = func;
			if (func1 == null)
			{
				Func<CheckBox, bool> func2 = (CheckBox b) => (b == (CheckBox)sender ? false : b.Name.Contains("api_"));
				Func<CheckBox, bool> func3 = func2;
				func = func2;
				func1 = func3;
			}
			foreach (CheckBox nullable in checkBoxes.Where<CheckBox>(func1))
			{
				nullable.IsChecked = new bool?(false);
			}
			try
			{
				dynamic obj = JsonConvert.DeserializeObject(File.ReadAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon")));
				obj.defaultexec = Apis.selected;
				typeof(File).WriteAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon"), JsonConvert.SerializeObject(obj));
			}
			catch
			{
				MessageBox.Show("there was an error while trying to set your settings, please make sure nothing is using the settings file.");
			}
		}

		private void selectKRNL(object sender, RoutedEventArgs e)
		{
			Func<CheckBox, bool> func = null;
			Apis.selected = 3;
			IEnumerable<CheckBox> checkBoxes = Extensions.FindVisualChildren<CheckBox>(this);
			Func<CheckBox, bool> func1 = func;
			if (func1 == null)
			{
				Func<CheckBox, bool> func2 = (CheckBox b) => (b == (CheckBox)sender ? false : b.Name.Contains("api_"));
				Func<CheckBox, bool> func3 = func2;
				func = func2;
				func1 = func3;
			}
			foreach (CheckBox nullable in checkBoxes.Where<CheckBox>(func1))
			{
				nullable.IsChecked = new bool?(false);
			}
			try
			{
				dynamic obj = JsonConvert.DeserializeObject(File.ReadAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon")));
				obj.defaultexec = Apis.selected;
				typeof(File).WriteAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon"), JsonConvert.SerializeObject(obj));
			}
			catch
			{
				MessageBox.Show("there was an error while trying to set your settings, please make sure nothing is using the settings file.");
			}
		}

		private void selectOxygenU(object sender, RoutedEventArgs e)
		{
			Func<CheckBox, bool> func = null;
			Apis.selected = 0;
			IEnumerable<CheckBox> checkBoxes = Extensions.FindVisualChildren<CheckBox>(this);
			Func<CheckBox, bool> func1 = func;
			if (func1 == null)
			{
				Func<CheckBox, bool> func2 = (CheckBox b) => (b == (CheckBox)sender ? false : b.Name.Contains("api_"));
				Func<CheckBox, bool> func3 = func2;
				func = func2;
				func1 = func3;
			}
			foreach (CheckBox nullable in checkBoxes.Where<CheckBox>(func1))
			{
				nullable.IsChecked = new bool?(false);
			}
			try
			{
				dynamic obj = JsonConvert.DeserializeObject(File.ReadAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon")));
				obj.defaultexec = Apis.selected;
				typeof(File).WriteAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon"), JsonConvert.SerializeObject(obj));
			}
			catch
			{
				MessageBox.Show("there was an error while trying to set your settings, please make sure nothing is using the settings file.");
			}
		}

		private void selectScripts(object sender, RoutedEventArgs e)
		{
			((Storyboard)base.TryFindResource("selectScripts")).Begin();
		}

		private void setcolor()
		{
			try
			{
				if (!File.Exists("./bin/theme.evon"))
				{
					Evon.Executor.ThemeStrings themeString = new Evon.Executor.ThemeStrings()
					{
						Theme = new List<Evon.Executor.ThemeStrings.ThemeSystem>()
						{
							new Evon.Executor.ThemeStrings.ThemeSystem()
							{
								ThemeManufacturer = "Evon",
								Color1 = "#FF9700FF",
								TextboxImage = ""
							}
						}
					};
					try
					{
						File.WriteAllText("./bin/theme.evon", JsonConvert.SerializeObject(themeString, 1));
					}
					catch
					{
					}
				}
				dynamic obj1 = JsonConvert.DeserializeObject(File.ReadAllText("./bin/theme.evon"));
				BrushConverter brushConverter = new BrushConverter();
				this.ShadowVibe.Background = (Brush)brushConverter.ConvertFromString(obj1.Theme[0].Color1.ToString());
				foreach (Border border in Extensions.FindVisualChildren<Border>(this.settingsViewWindow))
				{
					foreach (CheckBox checkBox in Extensions.FindVisualChildren<CheckBox>(border))
					{
						checkBox.Background = (Brush)brushConverter.ConvertFromString(obj1.Theme[0].Color1.ToString());
						checkBox.BorderBrush = (Brush)brushConverter.ConvertFromString(obj1.Theme[0].Color1.ToString());
					}
				}
				Evon.Executor.SetColor(this.colr, new SolidColorBrush((Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString())));
				foreach (Border border1 in Extensions.FindVisualChildren<Border>(this.ApiPanerl))
				{
					foreach (CheckBox checkBox1 in Extensions.FindVisualChildren<CheckBox>(border1))
					{
						checkBox1.Background = (Brush)brushConverter.ConvertFromString(obj1.Theme[0].Color1.ToString());
						checkBox1.BorderBrush = (Brush)brushConverter.ConvertFromString(obj1.Theme[0].Color1.ToString());
					}
				}
				this._1e.Color = (Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString());
				this._2e.Color = (Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString());
				this._3e.Color = (Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString());
				this._4e.Color = (Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString());
				this.ShadowNotifColor.Color = (Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString());
				this.notifBar.Background = new SolidColorBrush((Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString()));
				this.DSB1.Color = (Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString());
				this.DSB2.Color = (Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString());
				this.DSB3.Color = (Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString());
				this.DSB4.Color = (Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString());
				this.DSB5.Color = (Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString());
				this.DSB6.Color = (Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString());
				this.DSB7.Color = (Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString());
				this.DSB8.Color = (Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString());
				this.DSB9.Color = (Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString());
				this.ListBoxSearch.CaretBrush = (Brush)brushConverter.ConvertFromString(obj1.Theme[0].Color1.ToString());
				this.search.CaretBrush = this.ListBoxSearch.CaretBrush;
				this.search.BorderBrush = this.ListBoxSearch.CaretBrush;
				this.DefaultBrush = new SolidColorBrush((Color)typeof(ColorConverter).ConvertFromString(obj1.Theme[0].Color1.ToString()));
				foreach (TabItem item in (IEnumerable)this.tabs.Items)
				{
					item.BorderBrush = this.DefaultBrush;
				}
				this.reloadrScripts();
				this.UISettingsTab.BorderBrush = (Brush)brushConverter.ConvertFromString(obj1.Theme[0].Color1.ToString());
				this.ApiTab.BorderBrush = (Brush)brushConverter.ConvertFromString(obj1.Theme[0].Color1.ToString());
			}
			catch
			{
				File.Delete("./bin/theme.evon");
				this.setcolor();
			}
		}

		public static void SetColor(PortableColorPicker s, SolidColorBrush r)
		{
			NotifyableColor color = s.get_Color();
			Color color1 = r.Color;
			color.set_RGB_R((double)color1.R);
			NotifyableColor notifyableColor = s.get_Color();
			color1 = r.Color;
			notifyableColor.set_RGB_G((double)color1.G);
			NotifyableColor notifyableColor1 = s.get_Color();
			color1 = r.Color;
			notifyableColor1.set_RGB_B((double)color1.B);
			s.get_Color().set_A(255);
		}

		private void SetText(string text)
		{
			string str;
			this.editor.SetText(text);
			TabItem selectedItem = (TabItem)this.tabs.SelectedItem;
			if (!this.Texts.TryGetValue(selectedItem, out str))
			{
				this.Texts.Add(selectedItem, text);
			}
			else
			{
				this.Texts[selectedItem] = text;
			}
		}

		public async void showNotif(string title, string text)
		{
			if (!this.showNotiff)
			{
				if (!this.showNotiff)
				{
					this.showNotiff = true;
				}
				this.Notiftitle.Text = title;
				this.NotifSub.Text = text;
				base.Focus();
				await ((Storyboard)base.Resources["ShowNotif"]).Start();
				await ((Storyboard)base.Resources["HideNotif"]).Start();
				this.showNotiff = false;
			}
		}

		private void showSettings(object sender, RoutedEventArgs e)
		{
			((Storyboard)base.TryFindResource("selectSettings")).Begin();
		}

		private void sViewMouseMove(object sender, MouseEventArgs e)
		{
		}

		public static bool WebViewIsInstalled()
		{
			bool flag;
			try
			{
				if (!Environment.Is64BitOperatingSystem)
				{
					using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}"))
					{
						if (registryKey != null)
						{
							if (registryKey.GetValue("pv") != null)
							{
								flag = true;
								return flag;
							}
						}
						flag = false;
					}
				}
				else
				{
					using (RegistryKey registryKey1 = Registry.LocalMachine.OpenSubKey("Software\\WOW6432Node\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}"))
					{
						if (registryKey1 != null)
						{
							if (registryKey1.GetValue("pv") != null)
							{
								flag = true;
								return flag;
							}
						}
						flag = false;
					}
				}
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		public class ThemeStrings
		{
			public List<Evon.Executor.ThemeStrings.ThemeSystem> Theme;

			public ThemeStrings()
			{
			}

			public class ThemeSystem
			{
				public string Color1
				{
					get;
					set;
				}

				public string TextboxImage
				{
					get;
					set;
				}

				public string ThemeManufacturer
				{
					get;
					set;
				}

				public ThemeSystem()
				{
				}
			}
		}
	}
}