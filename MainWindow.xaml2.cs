using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace StarWarsBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private ObservableCollection<StarWarsPeople> peopleList = new ObservableCollection<StarWarsPeople>();
        private ObservableCollection<Starships> starshipList = new ObservableCollection<Starships>();

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Icon = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "../../../walter.ico"));
                Background = new ImageBrush(new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "../../../background.jpg")));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            fillList();

        }


        public async void fillList()
        {
            starshipDataGrid.Visibility = Visibility.Hidden;
            dataGrid.ItemsSource = peopleList;


            string strPage = await getRequestAsync("/api/people");

            Page? page = JsonConvert.DeserializeObject<Page>(strPage);

            string addUrl = "/?page=1";
            List<char> charUrlList = new List<char> { '/', '?', 'p', 'a', 'g', 'e', '=', '1' };

            for (int i = 0; page.next != null; i++)
            {
                strPage = await getRequestAsync("/api/people" + addUrl);
                page = JsonConvert.DeserializeObject<Page>(strPage);

                foreach (StarWarsPeople people in page.results)
                {
                    peopleList.Add(people);
                }

                char[] charArr = addUrl.ToCharArray();

                for (int j = 0; j < charUrlList.Count; j++)
                {
                    if (char.IsDigit(charUrlList[j]))
                    {
                        charUrlList.RemoveAt(j);
                    }
                }
                addUrl = new string(charUrlList.ToArray()) + (i + 2);

            }
        }

        private async Task<string> getRequestAsync(string url)
        {
            HttpClient client = new HttpClient();
            Uri baseUri = new Uri("https://swapi.dev");
            client.BaseAddress = baseUri;

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            var task = await client.SendAsync(requestMessage);
            string responsebody = await task.Content.ReadAsStringAsync();

            return responsebody;
        }



        private async void DataGrid_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            starshipList = new ObservableCollection<Starships>();
            Starships? ship;
            starshipDataGrid.ItemsSource = starshipList;
            string strStarship;

            starshipDataGrid.Visibility = Visibility.Visible;
            StarWarsPeople people = (StarWarsPeople)dataGrid.SelectedItem;

            if (peopleList.Contains(people))
            {
                foreach (string str in people.starships)
                {
                    strStarship = await getRequestAsync(str);

                    ship = JsonConvert.DeserializeObject<Starships>(strStarship);

                    starshipList.Add(ship);
                }

                if (people.starships.Count == 0)
                {
                    starshipList.Add(new Starships("Keine Schiffe", "", ""));
                }

            }
        }

        private void DataGrid_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
