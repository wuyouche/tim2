using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Globalization;
using static MauiApp17.Search;

namespace MauiApp17;

public partial class Setting : ContentPage
{
    public ObservableCollection<City> Cities { get; set; }
    public ObservableCollection<City> FilteredCities { get; set; }

    public Setting()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadCityData();
        initData();
    }


    private async void LoadCityData()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("city.json");
            using var reader = new StreamReader(stream);
            string json = await reader.ReadToEndAsync();
            Cities = JsonConvert.DeserializeObject<ObservableCollection<City>>(json);
            FilteredCities = new ObservableCollection<City>(Cities);


         
          
            choseCity.ItemsSource = Cities.Select(c => c.Name).ToList();



          
          
            choseCity.SelectedIndex = Preferences.Get("cityIndexForPhone", 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading JSON: {ex.Message}");
        }
    }

    private void OnSwitchToggled(object sender, ToggledEventArgs e)
    {
      
       
        if (!e.Value)
        {
               choseCity.IsVisible = false;
                choseTime.IsVisible = false;
                timePicker2.IsVisible = false;
                name.IsVisible = false;
                maxSwitch.IsVisible = false;
                MaxLable.IsVisible = false;
                MinLable.IsVisible = false;
                minSwitch.IsVisible = false;
                choseDetail.IsVisible = false;
                choseLocal.IsVisible = false;
                deLable.IsVisible = false;
                deSwitch.IsVisible = false;
            entryName.IsVisible = false;
                
            }
            else
            {
            choseCity.IsVisible = true;
            choseTime.IsVisible = true;
            timePicker2.IsVisible = true;
            name.IsVisible = true;
            maxSwitch.IsVisible = true;
            MaxLable.IsVisible = true;
            MinLable.IsVisible = true;
            minSwitch.IsVisible = true;
            choseDetail.IsVisible = true;
            choseLocal.IsVisible = true;
            deLable.IsVisible = true;
            deSwitch.IsVisible = true;
            entryName.IsVisible = true;
        }
        

    }

    private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        if (choseCity.SelectedIndex != -1)
        {
            var selectedCityName = choseCity.Items[choseCity.SelectedIndex];
            var selectedCity = Cities.FirstOrDefault(c => c.Name == selectedCityName);

            if (selectedCity != null)
            {
                FilteredCities.Clear();
                FilteredCities.Add(selectedCity);
            }
        }
    }

    private void OnImageTapped(object sender, EventArgs e)
    {
        timePicker2.IsVisible = true;
        timePicker2.Focus();
    }

    private void OnButtonClick(object sender, EventArgs e)
    {
        SaveData();


    }
    private void SaveData()
    {
        Preferences.Set("choseCity", Cities[choseCity.SelectedIndex].Name);
        Preferences.Set("latitude", Cities[choseCity.SelectedIndex].latitude);
        Preferences.Set("longitude", Cities[choseCity.SelectedIndex].longitude);
        Preferences.Set("cityIndexForPhone", choseCity.SelectedIndex);
        Preferences.Set("talkTime", DateTime.Today.Add(timePicker2.Time).ToString("h:mm tt", CultureInfo.InvariantCulture).ToLower());
        Preferences.Set("Name", name.Text);
        Preferences.Set("SwitchOpen", notificationSwitch.IsToggled);
        Preferences.Set("maxSwitch", maxSwitch.IsToggled);
        Preferences.Set("minSwitch", minSwitch.IsToggled);
        Preferences.Set("deSwitch", deSwitch.IsToggled);
    }


    private void initData()
    {
        if (Preferences.Get("SwitchOpen", true))
        {
            choseCity.IsVisible = true;
            choseTime.IsVisible = true;
            timePicker2.IsVisible = true;
            name.IsVisible = true;
            maxSwitch.IsVisible = true;
            MaxLable.IsVisible = true;
            MinLable.IsVisible = true;
            minSwitch.IsVisible = true;
            choseDetail.IsVisible = true;
            choseLocal.IsVisible = true;
            deLable.IsVisible = true;
            deSwitch.IsVisible = true;
            entryName.IsVisible = true;
        }
        else
        {
            choseCity.IsVisible = false;
            choseTime.IsVisible = false;
            timePicker2.IsVisible = false;
            name.IsVisible = false;
            maxSwitch.IsVisible = false;
            MaxLable.IsVisible = false;
            MinLable.IsVisible = false;
            minSwitch.IsVisible = false;
            choseDetail.IsVisible = false;
            choseLocal.IsVisible = false;
            deLable.IsVisible = false;
            deSwitch.IsVisible = false;
            entryName.IsVisible = false;

        }
        name.Text = Preferences.Get("Name", string.Empty);
        notificationSwitch.IsToggled = Preferences.Get("SwitchOpen", true);
        maxSwitch.IsToggled = Preferences.Get("maxSwitch", true);
        minSwitch.IsToggled = Preferences.Get("minSwitch", true);
        deSwitch.IsToggled = Preferences.Get("deSwitch", true);
        string savedTime = Preferences.Get("talkTime", string.Empty);
        if (DateTime.TryParseExact(savedTime, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
        {
          
            timePicker2.Time = parsedTime.TimeOfDay;
        }

     
    }


}
