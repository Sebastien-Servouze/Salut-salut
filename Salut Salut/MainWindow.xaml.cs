using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;

namespace Salut_Salut
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string URL = "https://www.googleapis.com/customsearch/v1?key=AIzaSyCXAWYIrC4gI-e5MLTXLV3UBkYBVDYge5k&cx=004136826335110334001:w_pkcacv3hy&q=";
        private static HttpClient HttpClient = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();

            Question.Text = "Comment s'appelle la petite amie de Cédric dans la bande dessinée du même nom ?";
            Prop1.Text = "Chen";
            Prop2.Text = "Loïs";
            Prop3.Text = "Nadia";
        }

        private void OnFindAnswer(object sender, RoutedEventArgs e)
        {
            JArray searchResult = GetSearchResult(Question.Text);
            int[] countProp = CountPropOccurencesInSearchResult(searchResult, Prop1.Text, Prop2.Text, Prop3.Text);
            Debug.WriteLine(Prop1.Text + ": " + countProp[0]);
            Debug.WriteLine(Prop2.Text + ": " + countProp[1]);
            Debug.WriteLine(Prop3.Text + ": " + countProp[2]);

            int[] countProp2 = CountPropOccurencesInFirstLink(searchResult, countProp, Prop1.Text, Prop2.Text, Prop3.Text);
            Debug.WriteLine(Prop1.Text + ": " + countProp2[0]);
            Debug.WriteLine(Prop2.Text + ": " + countProp2[1]);
            Debug.WriteLine(Prop3.Text + ": " + countProp2[2]);

            int maxCountIndex = -1;
            int max = -1;
            for (int i = 0; i < countProp2.Length; i++)
            {
                if (Math.Max(max, countProp2[i]) == countProp2[i])
                {
                    max = countProp2[i];
                    maxCountIndex = i;
                }
            }

            TextBox responseTextBox = null;
            switch (maxCountIndex)
            {
                case 0:
                    responseTextBox = Prop1;
                    break;
                case 1:
                    responseTextBox = Prop2;
                    break;
                case 2:
                    responseTextBox = Prop3;
                    break;
            }

            responseTextBox.Background = new SolidColorBrush(Colors.LightGreen);
        }

        private JArray GetSearchResult(string search)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, URL + search);
            HttpResponseMessage response = HttpClient.SendAsync(request).Result;
            return (JArray) JObject.Parse(response.Content.ReadAsStringAsync().Result)["items"];
        }

        private int[] CountPropOccurencesInSearchResult(JArray searchResult, params string[] props)
        {
            int[] countProp = new int[3];

            for (int i = 0; i < searchResult.Count; i++)
            {
                Debug.WriteLine(searchResult[i]["title"]);
                if (searchResult[i]["title"].ToString().ToLower().Contains(props[0].ToLower()))
                    countProp[0]++;
                if (searchResult[i]["title"].ToString().ToLower().Contains(props[1].ToLower()))
                    countProp[1]++;
                if (searchResult[i]["title"].ToString().ToLower().Contains(props[2].ToLower()))
                    countProp[2]++;
            }

            return countProp;
        }

        private int[] CountPropOccurencesInFirstLink(JArray searchResult, int[] countProp, params string[] props)
        {
            int[] countProp2 = countProp;
            string html;

            using (WebClient client = new WebClient())
                html = client.DownloadString(searchResult[0]["link"].ToString());

            for (int i = 0; i < countProp2.Length; i++)
                countProp2[i] += Regex.Matches(html, props[i]).Count;

            return countProp2;
        }
    }
}
