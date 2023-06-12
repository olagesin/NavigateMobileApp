using Newtonsoft.Json;
using NFCProj.DTOs;
using Plugin.NFC;
using System.Net.Http.Headers;
using System.Text;

namespace NFCProj;

public partial class MainPage : ContentPage
{

    public MainPage()
	{
		InitializeComponent();
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
        // Perform authentication logic and get the token
        //string token = await Authenticate(usernameEntry.Text, passwordEntry.Text);
        string token = null;

        var httpClient = new HttpClient();
        var loginRequest = new LoginRequest()
        {
            Email = usernameEntry.Text,
            Password = passwordEntry.Text
        };

        var jsonContent = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await httpClient.PostAsync("https://parvigateapi.azurewebsites.net/Users/login", jsonContent);

        var responseAsString = await response.Content.ReadAsStringAsync();

        var responseData = JsonConvert.DeserializeObject<GlobalResponse<LoginResponse>>(responseAsString);

        if (response.IsSuccessStatusCode)
        {
            token = responseData.Data.Token;
            httpClient.DefaultRequestHeaders.Authorization =
                     new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
        }
        else
        {
            await DisplayAlert(responseData.Errors.FirstOrDefault().Key,
                responseData.Errors.FirstOrDefault().ErrorMessages.FirstOrDefault(), "OK");
        }

        // Save the token in local storage
        Preferences.Set("Token", token);

        // Navigate to the next page
        await Navigation.PushAsync(new LogTagPage());
    }
}