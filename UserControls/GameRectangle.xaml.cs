using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Evon.UserControls
{
	public partial class GameRectangle : UserControl
	{
		public WrapPanel items;

		public int id;

		public GameRectangle()
		{
			this.InitializeComponent();
		}

		private void doRightClick(object sender, MouseButtonEventArgs e)
		{
		}
	}
}