using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;

namespace MauiApp17
{
    public partial class Search : ContentPage
    {
        public ObservableCollection<City> Cities { get; set; }
        public ObservableCollection<City> FilteredCities { get; set; }

        public Search()
        {
            InitializeComponent();
            LoadCityData();
        }


        private async void OnFrameTapped(object sender, EventArgs e)
        {
            var frame = (Frame)sender;
            var city = (City)frame.BindingContext;

            if (city != null)
            {
                bool result = await DisplayAlert("提示", $"确定要{city.Name}？", "确定", "取消");

                if (result)
                {
                    Preferences.Set("latitude", city.latitude);
                    Preferences.Set("longitude", city.longitude);
                    Preferences.Set("cityName", city.Name);
                }

            }
        }


        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = citySearchBar.Text?.ToLower() ?? string.Empty;
            FilterCities(searchText);
        }
        private async Task LoadCityData()
        {
            try
            {

                using var stream = await FileSystem.OpenAppPackageFileAsync("city.json");
                using var reader = new StreamReader(stream);
                string json = await reader.ReadToEndAsync();
                Cities = JsonConvert.DeserializeObject<ObservableCollection<City>>(json);
                FilteredCities = new ObservableCollection<City>(Cities);
                cityCollectionView.ItemsSource = FilteredCities;
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error loading JSON: {ex.Message}");
            }
        }




        private void FilterCities(string searchText)
        {
            
            Dispatcher.Dispatch(() =>
            {
               
                if (cityCollectionView == null) return;

                FilteredCities.Clear();
                var filtered = Cities.Where(c => c.Name.ToLower().Contains(searchText)).ToList();
                foreach (var city in filtered)
                {
                    FilteredCities.Add(city);
                }
            });
        }

        public class City
        {
            public string Name { get; set; }

            public string ImagePath { get; set; }

            public double latitude { get; set; }

            public double longitude { get; set; }
        }


    }
}
