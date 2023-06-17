//using CommunityToolkit.Mvvm.ComponentModel;
//using Newtonsoft.Json;
//using NFCProj.DTOs;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Threading.Tasks;

//namespace NFCProj.ViewModels
//{
//    public partial class MainViewModel : ObservableObject
//    {
//        public MainViewModel()
//        {
            
//        }

//        [ObservableProperty]
//        private bool isLoading;

//        [ObservableProperty]
//        private readonly string usernameEntry;

//        [ObservableProperty]
//        private readonly string passwordEntry;

//        private async void RegisterTagButton_Clicked(object sender, EventArgs e)
//        {
//            await Shell.Current.GoToAsync(nameof(RegisterTagPage));

//            //await Navigation.PushAsync(new RegisterTagPage());
//        }

//        private async void LogTagButton_Clicked(object sender, EventArgs e)
//        {
//            await Shell.Current.GoToAsync(nameof(LogTagPage));

//            //await Navigation.PushAsync(new LogTagPage());
//        }

//        //protected override void OnAppearing()
//        //{
//        //    base.OnAppearing();
//        //}

//        private async void LoginButton_Clicked(object sender, EventArgs e)
//        {
//            string token = null;

//            var httpClient = new HttpClient();
//            var loginRequest = new LoginRequest()
//            {
//                Email = usernameEntry,
//                Password = passwordEntry
//            };

//            if (string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
//            {
//                await Shell.Current.DisplayAlert("Error", "Please enter both Email and Password.", "OK");
//                return;
//            }

//            var jsonContent = new StringContent(
//                Newtonsoft.Json.JsonConvert.SerializeObject(loginRequest),
//                Encoding.UTF8,
//                "application/json"
//            );

//            IsLoading = true;

//            var response = await httpClient.PostAsync("https://parvigateapi.azurewebsites.net/Users/login", jsonContent);

//            var responseAsString = await response.Content.ReadAsStringAsync();

//            IsLoading = false;

//            var responseData = JsonConvert.DeserializeObject<GlobalResponse<LoginResponse>>(responseAsString);

//            if (response.IsSuccessStatusCode)
//            {
//                token = responseData.Data.Token;
//                var tokenToSave = token.Replace("\"", "");

//                httpClient.DefaultRequestHeaders.Authorization =
//                         new AuthenticationHeaderValue("Bearer", tokenToSave);

//                // Save the token in local storage
//                Preferences.Set("Token", tokenToSave);

//                // Navigate to the next page
//                await Shell.Current.GoToAsync(nameof(HomePage));
//            }
//            else
//            {
//                await Shell.Current.DisplayAlert(responseData.Errors.FirstOrDefault().Key,
//                    responseData.Errors.FirstOrDefault().ErrorMessages.FirstOrDefault(), "OK");
//            }
//        }
//    }
//}
