using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FeedPlayer
{
	/// <summary>
	/// Interaction logic for Login.xaml
	/// </summary>
	public partial class Login : Window
	{
		public Login()
		{
			InitializeComponent();
		}

		private void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			var username = UserNameBox.Text;
			var password = PasswordBox.Password;

			var client = new WebClient();
			client.Proxy = null;

			var response = client.DownloadString("http://arctic-sentry.appspot.com/login?u=" + username + "&p=" + password);


		}
	}
}
