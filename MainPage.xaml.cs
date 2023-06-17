using Newtonsoft.Json;
using NFCProj.DTOs;
using NFCProj.Helpers;
using Plugin.NFC;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;

namespace NFCProj;

public partial class MainPage : ContentPage
{
    //public bool IsLoading { get; set; } = false;
    public MainPage()
    {
        InitializeComponent();

        //BindingContext = this;
    }


    private async void RegisterTagButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterTagPage());
    }

    private async void LogTagButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LogTagPage());
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            string token = null;
            var httpClient = new HttpClient();
            var loginRequest = new LoginRequest()
            {
                Email = usernameEntry.Text,
                Password = passwordEntry.Text
            };

            var validationResult = ValidationHelper.Validate(loginRequest);

            if(validationResult.isValid == false)
            {
                var stringBuilder = new StringBuilder();

                foreach(var error in validationResult.errors)
                {
                    stringBuilder.AppendLine(error);
                }

                await DisplayAlert("Validation error", stringBuilder.ToString(), "Cancel");

                stringBuilder.Clear();
                return;
            }

            LoadingIndicator.IsRunning = true;

            var jsonContent = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.PostAsync("https://parvigateapi.azurewebsites.net/Users/login", jsonContent);

            var responseAsString = await response.Content.ReadAsStringAsync();

            var responseData = JsonConvert.DeserializeObject<GlobalResponse<LoginResponse>>(responseAsString);

            LoadingIndicator.IsRunning = false;

            if (response.IsSuccessStatusCode)
            {
                token = responseData.Data.Token;
                var tokenToSave = token.Replace("\"", "");

                httpClient.DefaultRequestHeaders.Authorization =
                         new AuthenticationHeaderValue("Bearer", tokenToSave);

                // Save the token in local storage
                Preferences.Set("Token", tokenToSave);

                // Navigate to the next page
                await Navigation.PushAsync(new HomePage());
            }
            else
            {
                await DisplayAlert(responseData.Errors.FirstOrDefault().Key,
                    responseData.Errors.FirstOrDefault().ErrorMessages.FirstOrDefault(), "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Cancel");
            return;
        }
    }

}