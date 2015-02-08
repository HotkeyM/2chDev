using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.ComponentModel;


namespace WpfApplication1
{

    public class DvachStruct
{

    public class News
    {
        public string date { get; set; }
        public string num { get; set; }
        public string subject { get; set; }
    }

    public class File
    {
        public int height { get; set; }
        public string md5 { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public int size { get; set; }
        public string thumbnail { get; set; }
        public int tn_height { get; set; }
        public int tn_width { get; set; }
        public int type { get; set; }
        public int width { get; set; }
    }

    public class Post
    {
        public int banned { get; set; }
        public int closed { get; set; }
        public string comment { get; set; }
        public string date { get; set; }
        public string email { get; set; }
        public List<File> files { get; set; }
        public int lasthit { get; set; }
        public string name { get; set; }
        public int num { get; set; }
        public int number { get; set; }
        public int op { get; set; }
        public string parent { get; set; }
        public int sticky { get; set; }
        public string subject { get; set; }
        public int timestamp { get; set; }
        public string trip { get; set; }
    }

    public class Thread
    {
        public List<Post> posts { get; set; }
    }

    public class Top
    {
        public string board { get; set; }
        public string info { get; set; }
        public string name { get; set; }
        public int speed { get; set; }
    }

    public class RootObject
    {
        public string Board { get; set; }
        public string BoardInfo { get; set; }
        public string BoardName { get; set; }
        public string board_banner_image { get; set; }
        public string board_banner_link { get; set; }
        public string current_thread { get; set; }
        public int enable_dices { get; set; }
        public int enable_flags { get; set; }
        public int enable_icons { get; set; }
        public int enable_images { get; set; }
        public int enable_names { get; set; }
        public int enable_posting { get; set; }
        public int enable_sage { get; set; }
        public int enable_shield { get; set; }
        public int enable_subject { get; set; }
        public int enable_trips { get; set; }
        public int enable_video { get; set; }
        public int is_board { get; set; }
        public int is_closed { get; set; }
        public int max_comment { get; set; }
        public int max_num { get; set; }
        public List<News> news { get; set; }
        public List<Thread> threads { get; set; }
        public string title { get; set; }
        public List<Top> top { get; set; }
    }

}

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        class UrlFile
        {
            public string url {get;set;}
            public string file {get;set;}
        }

        string downloadDir;

        List<string> hashes = new List<string>();

        DvachStruct.RootObject rootObj;

        List <UrlFile> downloadList = new List<UrlFile>();

        List<UrlFile>.Enumerator enumerator;

        WebClient client;

        int counter;
        int numFiles;

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
            LoadHashes(); 
        }

        void LoadSettings()
        {
            if (!File.Exists("settings.json"))
            {
                using (var f = File.CreateText("settings.json"))
                {
                    f.WriteLine("{\"downloadDir\":\"" + Directory.GetCurrentDirectory().Replace("\\","/") + "\"}");

                }
            }
            using (var f = File.OpenText("settings.json"))
            {
                string data = f.ReadToEnd();
                
                dynamic d = JsonConvert.DeserializeObject(data);
                downloadDir = d.downloadDir;
            }
        }

        void LoadHashes()
        {
            foreach (string f in Directory.GetFiles(downloadDir, "*.webm"))
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(f))
                    {

                        hashes.Add(md5.ComputeHash(stream).ToString());
                      
                    }
                }
            }
        }

        public void FileDownloaded(object sender, AsyncCompletedEventArgs args)

        {
           // args.
            
            if(enumerator.MoveNext())
            {
                UrlFile u = enumerator.Current;
                client.DownloadFileAsync(new Uri(u.url), u.file);
            }

            counter++;
            UrlTextBox.Text = "[" + counter + "/" + numFiles + "]";

            if (counter == numFiles) Button1.IsEnabled = true;
        }

       


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            client = new WebClient();

            client.DownloadFileCompleted += new AsyncCompletedEventHandler(FileDownloaded);

            string url = UrlTextBox.Text;
            url = url.Replace("html", "json");
            try
            {

            string data = client.DownloadString(url);

            
                rootObj = JsonConvert.DeserializeObject<DvachStruct.RootObject>(data);
            }
            catch (Exception)
            {
                UrlTextBox.Text = "Ошибка";
                return;
            }
            if (rootObj == null)
            {
                UrlTextBox.Text = "Ошибка";
                return;
            }

            foreach (DvachStruct.Post p in rootObj.threads[0].posts)
            {
                if (p.files == null) continue;
                foreach (DvachStruct.File f in p.files)
                {
                    if (f.name.IndexOf("webm") != -1)
                    {
                        if (hashes.FindIndex(delegate(string str) { return (str == f.md5); }) == -1)
                        {
                            string uri = "https://2ch.hk/" + rootObj.Board + "/" + f.path;
                            string file = downloadDir + "/" + f.name;

                            downloadList.Add(new UrlFile(){url = uri, file = file});

                            //client.DownloadFile(uri, file);
                        }
                    }
                }
            }

            numFiles = downloadList.Count;
            counter = 0;

            enumerator = downloadList.GetEnumerator();

            if (enumerator.MoveNext())
            {
                UrlFile u = enumerator.Current;
                client.DownloadFileAsync(new Uri(u.url), u.file);
            }

            UrlTextBox.Text = "[" + counter + "/" + numFiles + "]";
            Button1.IsEnabled = false;
        }
    }
}
