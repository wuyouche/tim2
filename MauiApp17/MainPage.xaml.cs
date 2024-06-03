using Plugin.LocalNotification;
using Newtonsoft.Json;
using System.Globalization;

namespace MauiApp17
{


    public partial class MainPage : ContentPage
    {
        private DateTime[] dateArray;
        private List<WeatherCodeDescription> weatherCodeDescriptions;
        private Label[] dayFrames;
        private Frame[] dayFramesParent;
        private List<WeatherData> weatherDataList = new List<WeatherData>();
        private async Task<string> whatIsayAsync()
        {
            string weatherSay = "";
            double latitude = 25.1276;
            double longitude = 121.7392;
            latitude = Preferences.Get("latitude", latitude);
            longitude = Preferences.Get("longitude", longitude);
            
           string url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,is_day,rain&hourly=temperature_2m,weather_code,uv_index&daily=weather_code,temperature_2m_max,temperature_2m_min";

            HttpClient client = new HttpClient();
             
            
                string json = await client.GetStringAsync(url);

                ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(json);

                string maxTempString= apiResponse.Daily.Temperature_2m_max[0].ToString();
                string minTempString = apiResponse.Daily.Temperature_2m_min[0].ToString();
                string weathercode = GetDescriptionByCode(apiResponse.Daily.Weather_code[0]);

            if (Preferences.Get("maxSwitch", true))
            {
                weatherSay += $"最高溫度是{maxTempString}\n";
            }
            if (Preferences.Get("minSwitch", true))
            {
                weatherSay += $"最低溫度是{minTempString}\n";
            }
            if (Preferences.Get("deSwitch", true))
            {
                weatherSay += $"天氣狀況為{weathercode}\n";
            }



            return weatherSay;
          
        }
        private async void CheckLocationPermission()
        {
            
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

               
                if (status != PermissionStatus.Granted)
                {
                    Console.WriteLine("Location permission denied.");
                    
                    return;
                }
            }

            
            await LoadWeatherData();
            if (Preferences.Get("SwitchOpen", true)==true)
            {
                testOne();
            }
        }
        private async void testOne()
        {

           
            string notificationTimeString = Preferences.Get("talkTime", "8:25 PM");

            if (DateTime.TryParseExact(notificationTimeString, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime notificationTime))
            {
                DateTime today = DateTime.Today;
                DateTime notifyTime = new DateTime(today.Year, today.Month, today.Day, notificationTime.Hour, notificationTime.Minute, 0);

                if (DateTime.Now > notifyTime)
                {
                    notifyTime = notifyTime.AddDays(1);
                }

                
                string notificationDescription = await whatIsayAsync();

                var r = new NotificationRequest
                {
                    NotificationId = 1,
                    Title = $"{Preferences.Get("choseCity", "基隆市")}",
                    Subtitle = $"親愛的{Preferences.Get("Name",string.Empty)}您好",
                    Description = notificationDescription,
                    BadgeNumber = 42,
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = notifyTime
                    }
                };

               
                LocalNotificationCenter.Current.Show(r);
            }
            else
            {
                Console.WriteLine("Error parsing notification time.");
            }
        }




        protected override void OnAppearing()
        {
            base.OnAppearing();
            CheckLocationPermission();
            
        }

        public MainPage()
        {
            InitializeComponent();
            LoadWeatherCodeDescriptions(); 
        }




        private async Task LoadWeatherData()
        {
            loadingIndicator.IsRunning = true;
            loadingIndicator.IsVisible = true;

            double latitude = 25.1276;
            double longitude = 121.7392;
            if (Preferences.Get("cityName", "No") != "No")
            {
                local.Text = Preferences.Get("cityName", "").ToString();
                latitude = Preferences.Get("latitude", latitude);
                longitude = Preferences.Get("longitude", longitude);
            }
            maxChinese.Text = "最\n高\n溫\n度\n";
            minChinese.Text = "最\n低\n溫\n度\n";
            dateArray = new DateTime[5];
            dayFrames = new Label[] { one, two, three, four, five };
            dayFramesParent = new Frame[] { day1, day2, day3, day4, day5 };
            for (int i = 0; i < 5; i++)
            {

                dateArray[i] = DateTime.Now.AddDays(i);

                dayFrames[i].Text = dateArray[i].ToString("MM/dd");
            }

            dayFramesParent = new Frame[] { day1, day2, day3, day4, day5 };


            string url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,is_day,rain&hourly=temperature_2m,weather_code,uv_index&daily=weather_code,temperature_2m_max,temperature_2m_min";

            HttpClient client = new HttpClient();
            try
            {
                string json = await client.GetStringAsync(url);

                ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(json);

                for (int i = 0; i < 168; i++)
                {
                    WeatherData data = new WeatherData
                    {
                        Time = apiResponse.Hourly.Time[i],
                        Temp = apiResponse.Hourly.Temperature_2m[i],


                        Description = GetDescriptionByCode(apiResponse.Hourly.Weather_code[i]),

                        BackgroundColor = DecideUvColor(apiResponse.Hourly.Uv_index[i]),


                    };
                    weatherDataList.Add(data);
                }
                weatherListView.ItemsSource = weatherDataList.Take(24).Skip(0);

                for (int i = 0; i < 24; i++)
                {
                    if (apiResponse.Hourly.Time[i].ToString("HH") == DateTime.Now.ToString("HH"))
                    {
                        nowTemp.Text = apiResponse.Hourly.Temperature_2m[i].ToString() + "°C";
                        break;
                    }
                }
                max.Text = apiResponse.Daily.Temperature_2m_max[0].ToString() + "°C";
                min.Text = apiResponse.Daily.Temperature_2m_min[0].ToString() + "°C";

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading weather data: " + ex.Message);
            }
      
            loadingIndicator.IsRunning = false;
            loadingIndicator.IsVisible = false;
        }

        private async Task LoadWeatherCodeDescriptions()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("foo.json");
                using var reader = new StreamReader(stream);

                string json = await reader.ReadToEndAsync();
                weatherCodeDescriptions = JsonConvert.DeserializeObject<List<WeatherCodeDescription>>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading weather code data: " + ex.Message);
                weatherCodeDescriptions = new List<WeatherCodeDescription>();
            }
        }

        private string GetDescriptionByCode(int code)
        {
            var description = weatherCodeDescriptions.Find(w => w.Code == code.ToString());
            return description != null ? description.Description : "未知";
        }

        private void PutData(object sender, EventArgs e)
        {
            loadingIndicator.IsVisible = true;
            loadingIndicator.IsRunning = true;
           
            int start = 0;

            Frame selectedFrame = sender as Frame;


            foreach (var frame in dayFramesParent)
            {
                frame.BackgroundColor = Colors.White;
                ((Label)frame.Content).TextColor = Colors.Black;
            }


            if (selectedFrame != null)
            {
                selectedFrame.BackgroundColor = Color.FromArgb("#100C82");
                ((Label)selectedFrame.Content).TextColor = Colors.White;
            }

            if (sender == day1)
            {
                start = 0;
            }
            else if (sender == day2)
            {
                start = 24;
            }
            else if (sender == day3)
            {
                start = 48;
            }
            else if (sender == day4)
            {
                start = 72;
            }
            else if (sender == day5)
            {
                start = 96;
            }

            weatherListView.ItemsSource = weatherDataList.Take(start + 24).Skip(start);
            loadingIndicator.IsRunning = false;
            loadingIndicator.IsVisible = false;
        }

        private Color DecideUvColor(double uv_index)
        {
            switch (uv_index)
            {
                case double n when (n <= 2):
                    return Colors.Green;
                case double n when (n > 2 && n <= 5):
                    return Colors.Yellow;
                case double n when (n > 5 && n <= 7):
                    return Colors.Orange;
                case double n when (n > 7 && n <= 10):
                    return Colors.Red;
                default:
                    return Colors.Purple;
            }
        }

        private async void OnMapImageTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NewPage1()); 
        }


    }

    public class WeatherData
    {
        public DateTime Time { get; set; }
        public double Temp { get; set; }
        public double Uv { get; set; }
        public string Description { get; set; }

        public Color BackgroundColor { get; set; }
    }

    public class WeatherCodeDescription
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string ChineseDescription { get; set; }
    }

    public class ApiResponse
    {
        public HourlyData Hourly { get; set; }

        public DailyData Daily { get; set; }
    }

    public class HourlyData
    {
        public List<DateTime> Time { get; set; }
        public List<double> Temperature_2m { get; set; }
        public List<int> Weather_code { get; set; }
        public List<double> Uv_index { get; set; }
    }

    public class DailyData
    {
        public List<int> Weather_code { get; set; }
        public List<double> Temperature_2m_max { get; set; }
        public List<double> Temperature_2m_min { get; set; }
    }
}
