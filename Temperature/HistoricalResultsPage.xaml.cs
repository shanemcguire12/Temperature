﻿using Firebase.Database;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Temperature
{
    public partial class HistoricalResultsPage : ContentPage
    {
        private FirebaseClient _firebaseClient;
        public ObservableCollection<HistoricalDataModel> HistoricalData { get; set; }

        public HistoricalResultsPage()
        {
            InitializeComponent();
            HistoricalData = new ObservableCollection<HistoricalDataModel>();

            // Initialize FirebaseClient with your Firebase URL
            _firebaseClient = new FirebaseClient("https://fridge-database-e362b-default-rtdb.europe-west1.firebasedatabase.app");


            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                await PopulateHistoricalData();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load historical data: " + ex.Message, "OK");
            }
        }

        private async Task PopulateHistoricalData()
        {
            HistoricalData.Clear();

            var historicalItems = await _firebaseClient
                .Child("temperature_data/historical")
                .OnceAsync<HistoricalDataModel>();

            foreach (var item in historicalItems)
            {
                HistoricalData.Add(item.Object);
            }
        }
        public string GenerateFileName()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"data_{timestamp}.pdf";
            return fileName;
        }
        private void OnDownloadPdfClicked(object sender, EventArgs e)
        {
            string fileName = GenerateFileName();
            var filePath = GetAppSpecificDownloadPath(fileName);

            GeneratePdf(filePath, HistoricalData);

            DisplayAlert("PDF Downloaded", $"PDF has been saved to {filePath}", "OK");
        }

        public void GeneratePdf(string filePath, ObservableCollection<HistoricalDataModel> items)
        {
            using (var document = SKDocument.CreatePdf(filePath))
            {
                var paint = new SKPaint
                {
                    Typeface = SKTypeface.FromFamilyName("Arial"),
                    TextSize = 12,
                    IsAntialias = true,
                };

                var canvas = document.BeginPage(612, 792); // A4 size paper

                float margin = 20;
                float x = margin;
                float y = margin;
                float lineHeight = 20;

                canvas.DrawText("Temperature", x, y, paint);
                canvas.DrawText("Humidity", x + 200, y, paint);
                canvas.DrawText("Timestamp", x + 400, y, paint);

                y += lineHeight;

                foreach (var item in items)
                {
                    canvas.DrawText(item.FormattedTemperature, x, y, paint);
                    canvas.DrawText(item.FormattedHumidity, x + 200, y, paint);
                    canvas.DrawText(item.FormattedTimestamp, x + 400, y, paint);
                    y += lineHeight;
                }

                document.EndPage();
            }
        }

        private string GetAppSpecificDownloadPath(string fileName)
        {
#if __ANDROID__
    // This code will only compile for Android.
    var context = Android.App.Application.Context;
    var path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
#else
            // This code will compile for other platforms.
            var path = FileSystem.AppDataDirectory;
#endif
            return Path.Combine(path ?? string.Empty, fileName);
        }


        public class HistoricalDataModel
        {
            public double Temperature { get; set; }
            public double Humidity { get; set; }
            public string Timestamp { get; set; }

           

            public string FormattedTimestamp
            {
                get
                {
                    if (DateTime.TryParseExact(Timestamp, "ddMMyyyy_HHmm", null, System.Globalization.DateTimeStyles.None, out DateTime dateTime))
                    {
                        // If parsing is successful, return the formatted timestamp
                        return dateTime.ToString("HH:mm dd/MM/yyyy");
                    }
                    else
                    {
                        // Return the original string or some error indicator if parsing fails
                        return Timestamp;
                    }
                }
            }


            public string FormattedTemperature => $"{Temperature:0.0} °C";
            public string FormattedHumidity => $"{Humidity:0.0}%";
        }
    }
}
