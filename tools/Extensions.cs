using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace tools
{
	public static class Extensions
	{
		public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj)
		where T : DependencyObject
		{
			bool flag;
			T t = default(T);
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject dependencyObject = VisualTreeHelper.GetChild(depObj, i);
					if (dependencyObject == null)
					{
						flag = false;
					}
					else
					{
						t = (T)(dependencyObject as T);
						flag = t != null;
					}
					if (flag)
					{
						yield return t;
					}
					foreach (T t1 in Extensions.FindVisualChildren<T>(dependencyObject))
					{
						yield return t1;
					}
					dependencyObject = null;
					t = default(T);
				}
			}
		}

		public static bool IsUserVisible(this UIElement element)
		{
			bool flag;
			if (element.IsVisible)
			{
				FrameworkElement parent = VisualTreeHelper.GetParent(element) as FrameworkElement;
				if (parent == null)
				{
					throw new ArgumentNullException("container");
				}
				GeneralTransform ancestor = element.TransformToAncestor(parent);
				double width = element.RenderSize.Width;
				Size renderSize = element.RenderSize;
				Rect rect = ancestor.TransformBounds(new Rect(0, 0, width, renderSize.Height));
				Rect rect1 = new Rect(0, 0, parent.ActualWidth, parent.ActualHeight);
				flag = rect1.IntersectsWith(rect);
			}
			else
			{
				flag = false;
			}
			return flag;
		}

		public static Task Start(this Storyboard sb)
		{
			TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
			EventHandler eventHandler = null;
			eventHandler = (object sender, EventArgs eeeeeeeeeeee) => {
				this.sb.Completed -= this.sbHandler;
				this.status.SetResult(true);
			};
			sb.Completed += eventHandler;
			sb.Begin();
			return taskCompletionSource.Task;
		}
	}
}