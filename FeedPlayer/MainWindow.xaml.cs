using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Security.Cryptography;



namespace FeedPlayer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		//"http://localhost:13080/get", "http://arctic-sentry.appspot.com/get"
		private const String GetAddress = "http://arctic-sentry.appspot.com/get";
		private const String UpdateAddress = "http://arctic-sentry.appspot.com/update";

		private ShowInfo[] _currentShowList;
		private Listings[] _storage;

		private Boolean _inList = false;
		private Boolean _paused = false;
		private Boolean _currentlyDownloadingFile = false;

		private String _prevSelection = "";

		public MainWindow()
		{
			InitializeComponent();

			button1.IsEnabled = false;
			button4.IsEnabled = false;

			var client = new WebClient();
			client.Proxy = null;
			client.DownloadStringCompleted += client_DownloadString;

			client.DownloadStringAsync(new Uri(GetAddress));

			mediaElement1.MediaEnded += PlaybackEnded;
			
		}

		private void PlaybackEnded(object sender, RoutedEventArgs e)
		{
			button4.IsEnabled = false;
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			InitialLoad();
			_inList = false;
		}

		void InitialLoad()
		{
			button1.IsEnabled = false;


			listBox1.Items.Clear();

			foreach (var hord in _storage)
			{
				listBox1.Items.Add(hord.name);
			}

		}


		void client_DownloadString(object sender, DownloadStringCompletedEventArgs e)
		{
			if (e.Error != null)
				return;


			XElement feed = XElement.Parse(e.Result);


			var stored = new List<Listings>();


			var store = new List<Listings>();

			foreach (var item in feed.Descendants("item"))
			{
				var temps = new Listings();
				temps.name = item.Element("name").Value;
				foreach (var other in item.Descendants("episode"))
				{
					temps.Add(new ShowInfo
					{
						Title = other.Element("title").Value,
						Url = other.Element("guid").Value
					});
				}
				store.Add(temps);
			}

			_storage = store.OrderBy(i => i.name).ToArray();

			foreach (var item in _storage)
			{
				listBox1.Items.Add(item.name);
			}

		}

		

		private void listBox1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (listBox1.Items.IsEmpty != true)
			{
				foreach (var item in _storage)
				{
					if (item.name == listBox1.SelectedItem.ToString())
					{
						_currentShowList = item.Get().ToArray();
					}
				}
				
				var selected = from title in _currentShowList where title.Title == listBox1.SelectedItem.ToString() select title.Url;

				string[] url = selected.ToArray<string>();


				button1.IsEnabled = true;
				if (!_inList)
				{
					listBox1.Items.Clear();
					foreach (var show in _currentShowList)
					{
						listBox1.Items.Add(show.Title);

					}
					_inList = true;
				}
				else
				{
					if (url[0] != _prevSelection)
					{
						_prevSelection = url[0];

						button4.Content = "Pause";

						_paused = false;

						button4.IsEnabled = true;

						mediaElement1.Source = new Uri(url[0]);
						mediaElement1.Play();

					}
				}

			}
		}

		private void DlFile(string inp, string title)
		{
			if (!File.Exists(title + ".mp3"))
			{
				var temp = new WebClient();
				temp.Proxy = null;
				temp.DownloadFileCompleted += DlFileComplete;

				_currentlyDownloadingFile = true;
				temp.DownloadFileAsync(new Uri(inp), ValidifyFileName(title) + ".mp3");
			}


		}

		private void DlFileComplete(object sender, AsyncCompletedEventArgs e)
		{
			_currentlyDownloadingFile = false;
		}


		private static string ValidifyFileName( string name )
		{
			var invalidChars = new List<char>(System.IO.Path.GetInvalidFileNameChars());
			var temp = new StringBuilder();

			foreach (var item in name)
			{
				bool valid = true;

				if (!invalidChars.Contains(item))
				{
					temp.Append(item);
				}
				else
				{
					temp.Append('_');
				}
				//foreach (var invalidChar in invalidChars)
				//{
				//	if (item == invalidChar)
				//		valid = false;
				//}

				//temp.Append(valid ? item : '_');
			}

			return temp.ToString();
		}


		/*
		 * Changes the test diplayed in volumelabel to represent what value between 0 and 100
		 * mediaElement1's volume is set to
		 */

		private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			Double volume = slider1.Value;

			mediaElement1.Volume = volume;

			try
			{
				volumelabel.Content = "Volume: " + (volume * 100).ToString("#0");
			}
			catch
			{
				
			}
		}

		

		private void button4_Click(object sender, RoutedEventArgs e)
		{
			
			if (!_paused)
			{
				mediaElement1.Pause();

				_paused = true;

				button4.Content = "Play";
			}
			else
			{
				mediaElement1.Play();

				button4.Content = "Pause";

				_paused = false;
			}
		}

		public void Update_Click(object sender, RoutedEventArgs routedEventArgs)
		{
			var client = new WebClient();
			client.Proxy = null;

			client.DownloadDataCompleted += client_DownloadDataCompleted;
			client.DownloadDataAsync(new Uri(UpdateAddress));
			
			
			//listBox1.Items.Clear();
			//client.DownloadStringAsync(new Uri(GetAddress));
		}

		public void client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs downloadDataCompletedEventArgs)
		{
			var client = new WebClient();
			client.Proxy = null;
			client.DownloadStringCompleted += client_DownloadString;

			listBox1.Items.Clear();
			client.DownloadStringAsync(new Uri(GetAddress));
		}

		private void Fuck_Click(object sender, RoutedEventArgs e)
		{
			if (_inList)
			{
				var temp = listBox1.SelectedItem.ToString();

				foreach (var show in _currentShowList)
				{
					if (temp == show.Title)
					{
						DlFile(show.Url,show.Title);
					}
				}
			}
			


		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (_currentlyDownloadingFile)
			{
				if (MessageBox.Show("A file is currently downloading. Cancel?", "Cancel Download?", MessageBoxButton.YesNo) == MessageBoxResult.No)
				{
					e.Cancel = true;
				}
			}
		}
	}

	class Listings
	{
		public string name;
		public List<ShowInfo> shows = new List<ShowInfo>();

		public void Add(ShowInfo inp)
		{
			shows.Add(inp);
		}

		public List<ShowInfo> Get()
		{
			return shows;
		} 
	}
}
