using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;

namespace Temperature
{
    partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private FirebaseClient _firebaseClient;

        // Individual properties for each piece of data
        private double _temperature;
        private double _humidity;
        private double _fridgeScore;

        public double Temperature
        {
            get => _temperature;
            set { _temperature = value; OnPropertyChanged(); }
        }

        public double Humidity
        {
            get => _humidity;
            set { _humidity = value; OnPropertyChanged(); }
        }

        public double FridgeScore
        {
            get => _fridgeScore;
            set
            {
                _fridgeScore = value;
                OnPropertyChanged(nameof(FridgeScore));
                OnPropertyChanged(nameof(FridgeScoreColor)); // Notify that the color needs to be updated as well
            }
        }


        public Color FridgeScoreColor
        {
            get
            {
                if (_fridgeScore >= 90.0) return Colors.Green;
                if (_fridgeScore >= 80.0) return Colors.Yellow;
                return Colors.Red;
            }
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            _firebaseClient = new FirebaseClient("https://fridge-database-e362b-default-rtdb.europe-west1.firebasedatabase.app");
            FetchTemperatureData();

            Device.StartTimer(TimeSpan.FromSeconds(1), () => // Refresh every 5 minutes
            {
                FetchTemperatureData();
                return true; // Return true to keep the timer running, false to stop
            });
        }

        private async void FetchTemperatureData()
        {
            Temperature = await _firebaseClient
                .Child("temperature_data")
                .Child("tempLive")
                .Child("temperature")
                .OnceSingleAsync<double>();

            Humidity = await _firebaseClient
                .Child("temperature_data")
                .Child("tempLive")
                .Child("humidity")
                .OnceSingleAsync<double>();

            FridgeScore = await _firebaseClient
                .Child("temperature_data")
                .Child("fridgeScore")
                .Child("score")
                .OnceSingleAsync<double>();
        }

        private void FridgeScoreTapped(object sender, EventArgs e)
        {
            DisplayAlert("Fridge Score", $"The current fridge score is {_fridgeScore}.", "OK");
        }



        // ... existing INotifyPropertyChanged implementation ...

        public class TemperatureModel
        {
            public double Temperature { get; set; }
            public double Humidity { get; set; }
            public double Score { get; set; }
        }


    }
}