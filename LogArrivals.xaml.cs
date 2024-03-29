using NFCProj.DTOs;
using Plugin.NFC;
using Newtonsoft.Json;
using System.Text;

namespace NFCProj;

public partial class LogArrivals : ContentPage
{
    /// <summary>
    /// The most recently saved NFC tag info.
    /// </summary>
    private ITagInfo readInfo = null;

    private bool isRadioEnabled = true;

    private CheckInType CheckInType = CheckInType.Arrival;

    public bool IsRadioEnabled
    {
        get { return isRadioEnabled; }
        set
        {
            isRadioEnabled = value;
            OnPropertyChanged(nameof(IsRadioEnabled));
        }
    }


    public LogArrivals()
	{
		InitializeComponent();
	}

    private bool GetNFCStatus()
    {
        if (!CrossNFC.IsSupported)
            lblStatus.Text = "NFC Status: Not Supported";
        else if (!CrossNFC.Current.IsAvailable)
            lblStatus.Text = "NFC Status: Not Available";
        else if (!CrossNFC.Current.IsEnabled)
            lblStatus.Text = "NFC Status: Not Enabled";
        else
        {
            lblStatus.Text = "NFC Status: Ready";
            return true;
        }
        return false;
    }

    /// <summary>
    /// Overridden event which is fired when the app is appearing or reappearing.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Refresh NFC status
        GetNFCStatus();

        CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
        CrossNFC.Current.OnMessagePublished += Current_OnMessagePublished;
        CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscovered;
        CrossNFC.Current.OnTagListeningStatusChanged += Current_OnTagListeningStatusChanged;
        CrossNFC.Current.OnNfcStatusChanged += Current_OnNfcStatusChanged;
    }

    /// <summary>
    /// Event fired when NFC status has changed.
    /// </summary>
    /// <param name="isEnabled">Indicates whether NFC is enabled.</param>
    private void Current_OnNfcStatusChanged(bool isEnabled)
    {
        // Dispatch the status label change
        Dispatcher.Dispatch(() => lblStatus.Text = $"NFC Status: {(isEnabled ? "Enabled" : "Disabled")}");
    }

    /// <summary>
    /// Event fired when the NFC listening status has changed,
    /// </summary>
    /// <param name="isListening">Indicates whether NFC is listening.</param>
    private void Current_OnTagListeningStatusChanged(bool isListening)
    {
        // Dispatch the status label change
        Dispatcher.Dispatch(() => lblStatus.Text = $"NFC Status: {(isListening ? "Listening..." : "Ready")}");
    }



    private async void ClosePage_Clicked(object sender, EventArgs e)
    {
        CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;
        CrossNFC.Current.OnMessagePublished -= Current_OnMessagePublished;
        CrossNFC.Current.OnTagDiscovered -= Current_OnTagDiscovered;
        CrossNFC.Current.OnTagListeningStatusChanged -= Current_OnTagListeningStatusChanged;
        CrossNFC.Current.OnNfcStatusChanged -= Current_OnNfcStatusChanged;

        await Navigation.PopAsync();
    }

    /// <summary>
    /// Event fired when an NFC message is successfully written (published) to an NFC tag.
    /// </summary>
    /// <param name="tagInfo">The tag information which was written.</param>
    private void Current_OnMessagePublished(ITagInfo tagInfo)
    {
        // Dispatch alert to the UI
        Dispatcher.Dispatch(() => DisplayAlert("NFC Event", $"Write successful.", "OK"));
    }

    private void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
    {
        readInfo = tagInfo;
    }

    /// <summary>
    /// Event fired when an NFC message is received (read) from a tag.
    /// </summary>
    /// <param name="tagInfo">The tag information which was read.</param>
    private async void Current_OnMessageReceived(ITagInfo tagInfo)
    {
        // Save tag info
        readInfo = tagInfo;

        var storedTagValue = tagInfo.Records == null ? null : tagInfo.Records.FirstOrDefault().Message.ToString();

        if(storedTagValue is null)
        {
            await DisplayAlert("Invalid Tag", "Tag holds no records", "Ok");
            return;
        }

        Uri uri = new Uri(storedTagValue);

        string locationId = System.Web.HttpUtility.ParseQueryString(uri.Query)["locationId"];
        string eventId = System.Web.HttpUtility.ParseQueryString(uri.Query)["eventId"];

        var httpClient = new HttpClient();
        var bearerToken = Preferences.Get("Token", null);

        // Perform the operation based on the selected radio button
        if (ArrivalButton.IsChecked)
        {
            CheckInType = CheckInType.Arrival;
        }
        else if (DepartureButton.IsChecked)
        {
            CheckInType = CheckInType.Departure;
        }

        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

        var requestToSend = new AddParkingRecordDto()
        {
            EventId = eventId,
            CheckInType = CheckInType,
            CheckInLocation = CheckInLocation.MainEntrance,
            TagSerialNumber = readInfo.SerialNumber
        };

        var jsonContent = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(requestToSend),
            Encoding.UTF8,
            "application/json"
        );


        var response = await httpClient.PostAsync("https://parvigateapi.azurewebsites.net/LocationTags/log-tag-arrival", jsonContent);

        var responseAsString = await response.Content.ReadAsStringAsync();

        var responseData = JsonConvert.DeserializeObject<GlobalResponse<GetParkingRecordDto>>(responseAsString);


        if (response.IsSuccessStatusCode)
        {
            await DisplayAlert("Success", "User arrival logged!!", "OK");
        }
        else
        {
            await DisplayAlert(responseData.Errors.FirstOrDefault().Key,
                responseData.Errors.FirstOrDefault().ErrorMessages.FirstOrDefault(), "OK");
        }
    }
}