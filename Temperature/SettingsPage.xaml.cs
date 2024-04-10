// SettingsPage.xaml.cs
using Firebase.Database;
using Firebase.Database.Query;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;

namespace Temperature
{
    public partial class SettingsPage : ContentPage
    {
        private FirebaseClient _firebaseClient;
        private ContactInfo _contactInfo;

        public ContactInfo ContactInfo
        {
            get => _contactInfo;
            set
            {
                _contactInfo = value;
                OnPropertyChanged(nameof(ContactInfo));
            }
        }

        public SettingsPage()
        {
            InitializeComponent();
            _firebaseClient = new FirebaseClient("https://fridge-database-e362b-default-rtdb.europe-west1.firebasedatabase.app");
            ContactInfo = new ContactInfo();
            BindingContext = this;
            LoadContactInfo();
        }

        private async void LoadContactInfo()
        {
            ContactInfo = await GetContactInfoAsync();
        }

        private async void OnUpdateClicked(object sender, EventArgs e)
        {
            // Update the Firebase database with the new values
            await _firebaseClient
                .Child("temperature_data")
                .Child("contactInfo")
                .PutAsync(new ContactInfo { Email = emailEntry.Text, PhoneNumber = phoneEntry.Text });

            // Confirm to the user that the update is successful
            await DisplayAlert("Success", "Your settings have been updated.", "OK");
        }

        public async Task<ContactInfo> GetContactInfoAsync()
        {
            return await _firebaseClient
                .Child("temperature_data")
                .Child("contactInfo")
                .OnceSingleAsync<ContactInfo>();
        }
    }

    public class ContactInfo
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
