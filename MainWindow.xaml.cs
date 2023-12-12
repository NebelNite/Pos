using System;
using System.Windows;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _01POS_PLF_Maier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public enum GetRequestAsycMethod : byte
    {
        BaseUri = 0,
        NoBaseUri = 1
    }



    public partial class MainWindow : Window
    {
        private ObservableCollection<User> users = new ObservableCollection<User>();
        private ObservableCollection<Album> albums = new ObservableCollection<Album>();
        private readonly string curDir = AppDomain.CurrentDomain.BaseDirectory;



        public MainWindow()
        {
            InitializeComponent();
            config();
        }

        private void config()
        {
            this.ResizeMode = ResizeMode.CanResize;
            this.Background = Brushes.PaleTurquoise;

            this.Title = "Json Placeholder Browser";


            this.Icon = new BitmapImage(new Uri(curDir + "Mucki.png"));
        }

        private async void AddToAlbumList(User u)
        {
            string album = await getRequestAsync("users/" + u.Id + "/albums");


            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(album));


            ObservableCollection<Album>? albumCollection = await JsonSerializer.DeserializeAsync<ObservableCollection<Album>>(stream);

            if (albumCollection is null)
            {
                albumList.Items.Add("No Items Found!");
                return;
            }

            albums = albumCollection;

            CopyDataAlbum();
        }


        private async void AddToUserList()
        {
            string usersStrings = await getRequestAsync("https://jsonplaceholder.typicode.com/users", GetRequestAsycMethod.BaseUri);

            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(usersStrings));

            ObservableCollection<User>? user = await JsonSerializer.DeserializeAsync<ObservableCollection<User>>(stream);

            if (user is null)
            {
                userList.Items.Add("No Users Found!");
                return;
            }

            users = user;

            CopyDataUser();

        }

        private void CopyDataUser()
        {
            userList.Items.Clear();

            foreach(User user in users)
            {
                userList.Items.Add(user.ToString());
            }
        }

        private void CopyDataAlbum()
        {
            albumList.Items.Clear();

            foreach(Album a in albums)
            {
                albumList.Items.Add(a.ToString());
            }
        }

        private async Task<string> getRequestAsync(string url, GetRequestAsycMethod method = GetRequestAsycMethod.BaseUri)
        {
            string output = String.Empty;
            HttpClient client = new();


            try
            {
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, url);

                if (method == GetRequestAsycMethod.BaseUri)
                    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");


                HttpResponseMessage responese = await client.SendAsync(message);

                responese.EnsureSuccessStatusCode();


                output = await responese.Content.ReadAsStringAsync();


            }
            catch(Exception ex)
            {
                userList.Items.Clear();
                userList.Items.Add(ex.Message);
            }

            return output;
        }

        

        private async void save_Click(object sender, RoutedEventArgs e)
        {
            AddToUserList();
        }

        private void albumList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string? selected = albumList.SelectedItem as string;

            if (selected is null)
                return;

           
            char[] ids = selected.ToCharArray();

            for (int i = 0; i < ids.Length; i++)
            {
                if (!char.IsDigit(ids[i])) ids[i] = ' ';
            }
            selected = new string(ids);
            selected.Replace(" ", "");

            foreach(Album a in albums)
            {
                if(uint.Parse(selected) == a.Id)
                {


                    addPicture(selected);
                }
            }
        }

        private async void addPicture(string path)
        {
            try
            {
                imageBox.Source = null;


                string content = await getRequestAsync("/albums/" + path + "/photos", GetRequestAsycMethod.BaseUri);

                MessageBox.Show(content);


                MemoryStream stream = new(Encoding.UTF8.GetBytes(content));


                Photo? p = await JsonSerializer.DeserializeAsync<Photo>(stream);

                if (p is null)
                    return;

                var uri = new Uri(p.ThumbnailUrl);
                var bitmap = new BitmapImage(uri);
                imageBox.Source = bitmap;

            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK);
                return;
            }


        }

        private void userList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            albumList.Items.Clear();
            string? selected = null;
            try
            {
                selected = userList.SelectedItem.ToString();
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK);
            }


            if (selected is null)
                return;

            int start = selected.IndexOf("ID");

            string id = selected.Substring(start, 7);

            char[] ids = id.ToCharArray();

            for (int i = 0; i < ids.Length; i++)
            {
                if (!char.IsDigit(ids[i])) ids[i] = ' ';
            }
            selected = new string(ids);
            selected.Replace(" ", "");


            foreach (User u in users)
            {

                if (uint.Parse(selected) == u.Id)
                {
                    AddToAlbumList(u);


                }
            }

            albumList.Items.Add(selected);
        }
    }
}
