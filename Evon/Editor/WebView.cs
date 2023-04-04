using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace Evon.Editor
{
	public class WebView : WebView2
	{
		public bool IsLoaded;

		public string Text = "";

		public string scr;

		public WebView(string script = "")
		{
			EventHandler<CoreWebView2WebMessageReceivedEventArgs> eventHandler3 = null;
			EventHandler<CoreWebView2DOMContentLoadedEventArgs> eventHandler4 = null;
			WebView webView = this;
			this.scr = script;
			this.BeginInit();
			base.set_Source(new Uri(string.Concat(Directory.GetCurrentDirectory(), "\\bin\\Monaco.html")));
			base.set_DefaultBackgroundColor(Color.FromArgb(20, 20, 20));
			base.EnsureCoreWebView2Async(null);
			base.add_CoreWebView2InitializationCompleted((object s, CoreWebView2InitializationCompletedEventArgs e) => {
				CoreWebView2 coreWebView2 = webView.get_CoreWebView2();
				EventHandler<CoreWebView2WebMessageReceivedEventArgs> u003cu003e9_1 = eventHandler3;
				if (u003cu003e9_1 == null)
				{
					EventHandler<CoreWebView2WebMessageReceivedEventArgs> text = (object __, CoreWebView2WebMessageReceivedEventArgs args) => {
						webView.Text = args.TryGetWebMessageAsString();
						WebView.TextChanged onTextChanged = webView.OnTextChanged;
						if (onTextChanged != null)
						{
							onTextChanged(webView.Text);
						}
						else
						{
						}
					};
					EventHandler<CoreWebView2WebMessageReceivedEventArgs> eventHandler = text;
					eventHandler3 = text;
					u003cu003e9_1 = eventHandler;
				}
				coreWebView2.add_WebMessageReceived(u003cu003e9_1);
				CoreWebView2 coreWebView21 = webView.get_CoreWebView2();
				EventHandler<CoreWebView2DOMContentLoadedEventArgs> u003cu003e9_2 = eventHandler4;
				if (u003cu003e9_2 == null)
				{
					EventHandler<CoreWebView2DOMContentLoadedEventArgs> eventHandler1 = async (object sender, CoreWebView2DOMContentLoadedEventArgs args) => {
						await Task.Delay(500);
						webView.IsLoaded = true;
						if (script != "")
						{
							webView.SetText(script);
						}
						webView.antiskid();
						webView.minimap();
					};
					EventHandler<CoreWebView2DOMContentLoadedEventArgs> eventHandler2 = eventHandler1;
					eventHandler4 = eventHandler1;
					u003cu003e9_2 = eventHandler2;
				}
				coreWebView21.add_DOMContentLoaded(u003cu003e9_2);
				webView.get_CoreWebView2().get_Settings().set_AreDefaultContextMenusEnabled(false);
				webView.get_CoreWebView2().get_Settings().set_AreDevToolsEnabled(true);
			});
			this.EndInit();
		}

		public void antiskid()
		{
			dynamic obj = JsonConvert.DeserializeObject(File.ReadAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon")));
			if (obj.antiskid != (dynamic)null)
			{
				if (obj.antiskid != true)
				{
					base.get_CoreWebView2().ExecuteScriptAsync("disableAntiSkid()");
				}
				else
				{
					base.get_CoreWebView2().ExecuteScriptAsync("enableAntiSkid()");
				}
			}
		}

		public void minimap()
		{
			dynamic obj = JsonConvert.DeserializeObject(File.ReadAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\settings.evon")));
			if (obj.minimap != (dynamic)null)
			{
				if (obj.minimap != true)
				{
					base.get_CoreWebView2().ExecuteScriptAsync("HideMinimap()");
				}
				else
				{
					base.get_CoreWebView2().ExecuteScriptAsync("ShowMinimap()");
				}
			}
		}

		public void SetText(string text)
		{
			if (this.IsLoaded)
			{
				base.get_CoreWebView2().ExecuteScriptAsync(string.Concat("SetText(\"", HttpUtility.JavaScriptStringEncode(text), "\")"));
			}
		}

		public void Undo()
		{
			if (this.IsLoaded)
			{
				base.get_CoreWebView2().ExecuteScriptAsync("Undo()");
			}
		}

		public event WebView.TextChanged OnTextChanged;

		public delegate void TextChanged(string Text);
	}
}