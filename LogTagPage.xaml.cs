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
    private const string GetEventsEndpoint = "https://parvigateapi.azurewebsites.net/Events/list-all-events";


    private List<GetLocationDto> Locations;

    private List<GetEventDto> Events;

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

        await LoadEvents();

        GetNFCStatus();
    }

    private async Task LoadEvents()
    {
        var httpClient = new HttpClient();
        var bearertoken = Preferences.Get("Token", "Invalid token");

        httpClient.DefaultRequestHeaders.Authorization =
                      new AuthenticationHeaderValue("Bearer", bearertoken.Replace("\"", ""));

        var eventsResponse = await httpClient.GetAsync(GetEventsEndpoint);

        var eventContent = await eventsResponse.Content.ReadAsStringAsync();

        var eventsResponseData = JsonConvert.DeserializeObject<GlobalResponse<List<GetEventDto>>>(eventContent);

        Events = eventsResponseData.Data;

        foreach (var eventToSelect in Events)
        {
            eventPicker.Items.Add(eventToSelect.EventName);
        }
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


            var locationResponseData = JsonConvert.DeserializeObject<GlobalResponse<List<GetLocationDto>>>(content);

            Locations = locationResponseData.Data;
            
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

    private GetEventDto GetSelectedEvents(string eventName)
    {
        return Events.Find(events => events.EventName == eventName);
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
        var eventChosen = GetSelectedEvents(eventPicker.SelectedItem.ToString());

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
                EventId = eventChosen.Id,
                UserName = "Sample user"
            };


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
                locationId = responseData.Data.Location.Id;
                var eventId = responseData.Data.Event.Id;

                CrossNFC.Current.StartPublishing();

                readInfo.Records = new NFCNdefRecord[] {
                new NFCNdefRecord() {
                    TypeFormat = NFCNdefTypeFormat.Uri,
                    Payload = System.Text.Encoding.UTF8.GetBytes($"https://parvigateapi.azurewebsites.net/Locations/get-location?locationId={locationId}&eventId={eventId}"),
                    LanguageCode = "en"
                }
            };

                // Attempt to write text record to NFC tag
                CrossNFC.Current.PublishMessage(readInfo);

                await DisplayAlert("Success", $"User assigned to {responseData.Data.Location.Name}", "OK");

                CrossNFC.Current.StopPublishing();
            }
            else
            {
                await DisplayAlert(responseData.Errors.FirstOrDefault().Key,
                responseData.Errors.FirstOrDefault().ErrorMessages.FirstOrDefault(), "OK");
            }
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


    private async void ClosePageButton_Clicked(object sender, EventArgs e)
    {
        CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;

        await Navigation.PopAsync();
    }

    /// <summary>
    /// Event fired when an NFC tag is discovered while writing.
    /// </summary>
    /// <param name="tagInfo">The NFC tag's info.</param>
    /// <param name="format">Indicates whether the NFC tag is being formatted.</param>
    //private void Current_OnTagDiscoveredInLog(ITagInfo tagInfo, bool format)
    //{
    //    try
    //    {
    //        // Create a new text record 
    //        readInfo.Records = new NFCNdefRecord[] {
    //            new NFCNdefRecord() {
    //                TypeFormat = NFCNdefTypeFormat.Uri,
    //                Payload = System.Text.Encoding.UTF8.GetBytes($"https://parvigateapi.azurewebsites.net/Locations/get-location?locationId={locationId}"),
    //                LanguageCode = "en"
    //            }
    //        };

    //        // Attempt to write text record to NFC tag
    //        CrossNFC.Current.PublishMessage(readInfo);
    //    }
    //    catch
    //    {
    //        // Writing not supported, dispatch alert to the UI
    //        Dispatcher.Dispatch(() => DisplayAlert("NFC Error", $"Failed to write tag.", "OK"));
    //    }
    //    finally
    //    {
    //        // Stop writing
    //        //WriteClicked(null, null);
    //        CrossNFC.Current.StopPublishing();
    //    }
    //}
}