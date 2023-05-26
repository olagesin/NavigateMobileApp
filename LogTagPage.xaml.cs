using Microsoft.Maui.Devices.Sensors;
using Newtonsoft.Json;
using NFCProj.Models;
using System.Text;

namespace NFCProj;

public partial class LogTagPage : ContentPage
{
    private const string LogTagEndpoint = "https://parvigateapi.azurewebsites.net/Tags/log-tag";
    private const string AssignToTagEndpoint = "https://parvigateapi.azurewebsites.net/Tags/assign-to-tag";
    private const string GetLocationsEndpoint = "https://parvigateapi.azurewebsites.net/Locations/list-all-locations";

    private Button calculateRouteButton;
    private Button scanButton;
    private Picker sourcePicker;
    private Picker destinationPicker;

    private List<GetLocaationDto> Locations;

    public LogTagPage()
	{
        InitializeComponent();

        Title = "Log Tag";

        sourcePicker = new Picker
        {
            Title = "Select Source Location"
        };


        //sourcePicker.Items.Add("Location A");
        //sourcePicker.Items.Add("Location B");

        destinationPicker = new Picker
        {
            Title = "Select Destination Location"
        };

        //destinationPicker.Items.Add("Location X");
        //destinationPicker.Items.Add("Location Y");

        calculateRouteButton = new Button
        {
            Text = "Calculate Route",
            AutomationId = "CalculateRouteButton"
        };
        calculateRouteButton.Clicked += CalculateRouteButton_Clicked;

        scanButton = new Button
        {
            Text = "Scan NFC Tag",
            AutomationId = "ScanButton"
        };
        scanButton.Clicked += ScanButton_Clicked;

        Content = new StackLayout
        {
            Children =
            {
                sourcePicker,
                destinationPicker,
                calculateRouteButton,
                scanButton
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        //load locations from the endpoint
        await LoadLocations();
    }

    private async Task LoadLocations()
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(GetLocationsEndpoint);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<GlobalResponse<List<GetLocaationDto>>>(content);

            Locations = responseData.Data;

            // Update source and destination pickers with location names
            foreach (var location in Locations)
            {
                sourcePicker.Items.Add(location.Name);
                destinationPicker.Items.Add(location.Name);
            }
        }
        else
        {
            await DisplayAlert("Error", "Failed to load locations", "OK");
        }
    }

    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        var selectedLocationName = picker.SelectedItem?.ToString();

        if (!string.IsNullOrEmpty(selectedLocationName))
        {
            var selectedLocation = GetSelectedLocation(selectedLocationName);

            if (selectedLocation != null)
            {
                // Perform any required logic with the selected location
                // For example, you can access selectedLocation.Id or selectedLocation.Name
            }
        }
    }

    private GetLocaationDto GetSelectedLocation(string locationName)
    {
        return Locations.Find(location => location.Name == locationName);
    }

    private async void CalculateRouteButton_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(sourcePicker.SelectedItem.ToString()) || string.IsNullOrEmpty(destinationPicker.SelectedItem.ToString()))
        {
            await DisplayAlert("Error", "Please select both source and destination locations", "OK");
            return;
        }

        if (sourcePicker.SelectedItem.ToString() == destinationPicker.SelectedItem.ToString())
        {
            await DisplayAlert("Error", "Please select different source and destination locations", "OK");
            return;
        }

        // TODO: Implement route calculation logic

        var sourceLocation = GetSelectedLocation(sourcePicker.SelectedItem.ToString());
        var destinationLocation = GetSelectedLocation(destinationPicker.SelectedItem.ToString());

        // Simulating assigned route
        var assignedRoute = new
        {
            RouteId = "123",
            RouteDetails = "Route details"
        };

        // Posting assigned route details to the endpoint
        var httpClient = new HttpClient();
        var jsonContent = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(assignedRoute),
            Encoding.UTF8,
            "application/json"
        );
        var response = await httpClient.PostAsync(AssignToTagEndpoint, jsonContent);

        if (response.IsSuccessStatusCode)
        {
            await DisplayAlert("Success", "Tag assigned successfully", "OK");
        }
        else
        {
            await DisplayAlert("Error", "Failed to assign tag", "OK");
        }
    }

    private async void ScanButton_Clicked(object sender, EventArgs e)
    {
        // TODO: Implement NFC scanning logic

        // Simulating tag scanning
        var scannedTag = "123456";

        // Requesting tag assignment with the scanned tag
        var httpClient = new HttpClient();
        var jsonContent = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(scannedTag),
            Encoding.UTF8,
            "application/json"
        );
        var response = await httpClient.PostAsync(LogTagEndpoint, jsonContent);

        if (response.IsSuccessStatusCode)
        {
            await DisplayAlert("Success", "Tag logged successfully", "OK");
        }
        else
        {
            await DisplayAlert("Error", "Failed to log tag", "OK");
        }
    }
}