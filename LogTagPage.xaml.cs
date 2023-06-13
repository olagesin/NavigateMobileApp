using Microsoft.Maui.Devices.Sensors;
using Newtonsoft.Json;
using NFCProj.DTOs;
//using NFCProj.Models;
using Plugin.NFC;
using System.Net.Http.Headers;
using System.Text;

namespace NFCProj;

public partial class LogTagPage : ContentPage
{
    private const string LogTagEndpoint = "https://parvigateapi.azurewebsites.net/Tags/log-tag";
    private const string AssignToTagEndpoint = "https://parvigateapi.azurewebsites.net/LocationTags/assign-location-to-tag";
    private const string GetLocationsEndpoint = "https://parvigateapi.azurewebsites.net/Locations/list-all-locations";

    private Button calculateRouteButton;
    private Button scanButton;
    //private Picker sourcePicker;
    //private Picker destinationPicker;

    private List<GetLocationDto> Locations;

    /// <summary>
    /// Indicates whether an NFC read is taking place.
    /// </summary>
    private bool reading = false;
    /// <summary>
    /// Indicates whether an NFC write is taking place.
    /// </summary>
    private bool writing = false;
    /// <summary>
    /// The most recently saved NFC tag info.
    /// </summary>
    private ITagInfo readInfo = null;

    private string locationId = string.Empty;


    public LogTagPage()
	{
        InitializeComponent();
    }

    private bool GetNFCStatus()
    {
        if (!CrossNFC.IsSupported)
            NfcStatus.Text = "NFC Status: Not Supported";
        else if (!CrossNFC.Current.IsAvailable)
            NfcStatus.Text = "NFC Status: Not Available";
        else if (!CrossNFC.Current.IsEnabled)
            NfcStatus.Text = "NFC Status: Not Enabled";
        else
        {
            NfcStatus.Text = "NFC Status: Ready";
            return true;
        }
        return false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        await LoadLocations();

        GetNFCStatus();
    }

    private async Task LoadLocations()
    {
        var httpClient = new HttpClient();
        var bearertoken = Preferences.Get("Token", "Invalid token");

        httpClient.DefaultRequestHeaders.Authorization =
                      new AuthenticationHeaderValue("Bearer", bearertoken.Replace("\"", ""));

        var response = await httpClient.GetAsync(GetLocationsEndpoint);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<GlobalResponse<List<GetLocationDto>>>(content);

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

    private GetLocationDto GetSelectedLocation(string locationName)
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

        CrossNFC.Current.StartListening();

        CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;

        if(readInfo is null)
        {
            await DisplayAlert("Error", "Please place an NFC device to be scanned", "OK");
            return;
        }
        else
        {
            var routeToCalculate = new AddLocationTagDto()
            {
                SourceLocationId = sourceLocation.Id,
                DestinationLocationId = destinationLocation.Id,
                SerialNumber = readInfo.SerialNumber,
                UserName = "Sample user"
            };

            CrossNFC.Current.StopListening();

            // Posting assigned route details to the endpoint
            var httpClient = new HttpClient();
            var bearerToken = Preferences.Get("Token", null);


            var jsonContent = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(routeToCalculate),
                Encoding.UTF8,
                "application/json"
            );

            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

            var response = await httpClient.PostAsync(AssignToTagEndpoint, jsonContent);

            var responseAsString = await response.Content.ReadAsStringAsync();

            var responseData = JsonConvert.DeserializeObject<GlobalResponse<GetLocationTagDto>>(responseAsString);

            if (response.IsSuccessStatusCode)
            {
                CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscoveredInLog;

                CrossNFC.Current.StartPublishing();

                await DisplayAlert("Success", $"User assigned to {responseData.Data.Location.Name}", "OK");

                CrossNFC.Current.OnTagDiscovered -= Current_OnTagDiscoveredInLog;

                CrossNFC.Current.StopPublishing();
            }
            else
            {
                //await DisplayAlert("Error", "Failed to assign tag", "OK");
                await DisplayAlert(responseData.Errors.FirstOrDefault().Key,
                responseData.Errors.FirstOrDefault().ErrorMessages.FirstOrDefault(), "OK");
            }
        }
    }

    private async void ScanButton_Clicked(object sender, EventArgs e)
    {
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

    /// <summary>
    /// Event fired when an NFC message is received (read) from a tag.
    /// </summary>
    /// <param name="tagInfo">The tag information which was read.</param>
    private void Current_OnMessageReceived(ITagInfo tagInfo)
    {
        // update taginfo
        readInfo = tagInfo;
    }


    /// <summary>
    /// Event fired when an NFC tag is discovered while writing.
    /// </summary>
    /// <param name="tagInfo">The NFC tag's info.</param>
    /// <param name="format">Indicates whether the NFC tag is being formatted.</param>
    private void Current_OnTagDiscoveredInLog(ITagInfo tagInfo, bool format)
    {
        try
        {
            // Create a new text record 
            readInfo.Records = new NFCNdefRecord[] {
                new NFCNdefRecord() {
                    TypeFormat = NFCNdefTypeFormat.Uri,
                    Payload = System.Text.Encoding.UTF8.GetBytes($"https://parvigateapi.azurewebsites.net/Locations/get-location?locationId={locationId}"),
                    LanguageCode = "en"
                }
            };

            // Attempt to write text record to NFC tag
            CrossNFC.Current.PublishMessage(readInfo);
        }
        catch
        {
            // Writing not supported, dispatch alert to the UI
            Dispatcher.Dispatch(() => DisplayAlert("NFC Error", $"Failed to write tag.", "OK"));
        }
        finally
        {
            // Stop writing
            //WriteClicked(null, null);
            CrossNFC.Current.StopPublishing();
        }
    }
}