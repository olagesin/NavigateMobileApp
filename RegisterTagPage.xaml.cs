using Newtonsoft.Json;
using NFCProj.DTOs;
using Plugin.NFC;
using System.Text;
using System.Threading;

namespace NFCProj;

public partial class RegisterTagPage : ContentPage
{
    private const string RegisterTagEndpoint = "https://parvigateapi.azurewebsites.net/Tags/add-tag";
    private Button scanButton;


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

    public RegisterTagPage()
    {
        InitializeComponent();

        Title = "Register Tag";
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

        CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;
        CrossNFC.Current.OnMessagePublished -= Current_OnMessagePublished;
        CrossNFC.Current.OnTagDiscovered -= Current_OnTagDiscovered;
        CrossNFC.Current.OnTagListeningStatusChanged -= Current_OnTagListeningStatusChanged;
        CrossNFC.Current.OnNfcStatusChanged -= Current_OnNfcStatusChanged;

    }

    /// <summary>
    /// Event fired when the "Clear" button is clicked.
    /// </summary>
    private void ClearClicked(object sender, EventArgs e)
    {
        // Empty the saved tag info
        readInfo = null;
        // Hide the tag info group
        tagData.IsVisible = false;
    }


    /// <summary>
    /// Event fires when the "Write" button is clicked.
    /// </summary>
    private void WriteClicked(object sender, EventArgs e)
    {
        if (readInfo != null)
        {
            // Check if NFC is available/ready
            if (!GetNFCStatus())
            {
                DisplayAlert("NFC: Invalid State", "NFC is either not supported or turned off.", "OK");
                return;
            }

            if (!writing)
            {
                // Start publishing NFC messages
                CrossNFC.Current.StartPublishing();

                btnWrite.Text = "Stop Writing";
                lblStatus.Text = "NFC Status: Writing...";
                writing = true;
            }
            else
            {
                // Stop publishing NFC messages
                CrossNFC.Current.StopPublishing();

                // Update control texts to reflect write status
                btnWrite.Text = "Write";
                lblStatus.Text = "NFC Status: Ready";
                writing = false;
            }
        }
        else
            DisplayAlert("NFC Error", "Please read an NFC tag before writing.", "OK");
    }

    /// <summary>
    /// Event fired when the "Read" button is clicked.
    /// </summary>
    private void ReadClicked(object sender, EventArgs e)
    {
        // Check if NFC is available/ready
        if (!GetNFCStatus())
        {
            DisplayAlert("NFC: Invalid State", "NFC is either not supported or turned off.", "OK");
            return;
        }

        if (!reading)
        {
            // Start listening
            CrossNFC.Current.StartListening();


            // Attach NFC events
            CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
            CrossNFC.Current.OnMessagePublished += Current_OnMessagePublished;
            CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscovered;
            CrossNFC.Current.OnTagListeningStatusChanged += Current_OnTagListeningStatusChanged;
            CrossNFC.Current.OnNfcStatusChanged += Current_OnNfcStatusChanged;


            // Update button text and read status
            btnRead.Text = "Turn off NFC Scanner";
            reading = true;
            //saveTag.IsVisible = true;
        }
        else
        {
            // Stop listening
            CrossNFC.Current.StopListening();

            // Detach NFC events
            CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;
            CrossNFC.Current.OnMessagePublished -= Current_OnMessagePublished;
            CrossNFC.Current.OnTagDiscovered -= Current_OnTagDiscovered;
            CrossNFC.Current.OnTagListeningStatusChanged -= Current_OnTagListeningStatusChanged;
            CrossNFC.Current.OnNfcStatusChanged -= Current_OnNfcStatusChanged;

            // Update button text and read status
            btnRead.Text = "Turn on NFC scanner";
            reading = false;
            //saveTag.IsVisible = false;
        }
    }

    private async void ScanButton_Clicked(object sender, EventArgs e)
    {

        // Simulating tag details
        var tagToAdd = new AddTagDto()
        {
            SerialNumber = readInfo.SerialNumber
        };

        // Posting tag details to the endpoint
        var httpClient = new HttpClient();
        var bearerToken = Preferences.Get("Token", null);

        var jsonContent = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(tagToAdd),
            Encoding.UTF8,
            "application/json"
        );

        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

        var response = await httpClient.PostAsync(RegisterTagEndpoint, jsonContent);

        var responseAsString = await response.Content.ReadAsStringAsync();

        var responseData = JsonConvert.DeserializeObject<GlobalResponse<GetTagDto>>(responseAsString);


        if (response.IsSuccessStatusCode)
        {
            await DisplayAlert("Success", "Tag registered successfully", "OK");
        }
        else
        {
            await DisplayAlert(responseData.Errors.FirstOrDefault().Key, 
                responseData.Errors.FirstOrDefault().ErrorMessages.FirstOrDefault(), "OK");
        }

        await Navigation.PopAsync();
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

    /// <summary>
    /// Event fired when an NFC tag is discovered while writing.
    /// </summary>
    /// <param name="tagInfo">The NFC tag's info.</param>
    /// <param name="format">Indicates whether the NFC tag is being formatted.</param>
    private void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
    {
        try
        {
            //Create a new text record
            readInfo.Records = new NFCNdefRecord[] {
                new NFCNdefRecord() {
                    TypeFormat = NFCNdefTypeFormat.Uri,
                    Payload = System.Text.Encoding.UTF8.GetBytes("https://www.nuget.org/packages/Plugin.NFC"),
                    LanguageCode = "en"
                }
            };


            // Attempt to write text record to NFC tag
            CrossNFC.Current.PublishMessage(readInfo);

            CrossNFC.Current.OnTagDiscovered -= Current_OnTagDiscovered;
        }
        catch
        {
            // Writing not supported, dispatch alert to the UI
            Dispatcher.Dispatch(() => DisplayAlert("NFC Error", $"Failed to write tag.", "OK"));
        }
        finally
        {
            // Stop writing
            WriteClicked(null, null);
        }
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

    /// <summary>
    /// Event fired when an NFC message is received (read) from a tag.
    /// </summary>
    /// <param name="tagInfo">The tag information which was read.</param>
    private void Current_OnMessageReceived(ITagInfo tagInfo)
    {
        // Save tag info
        readInfo = tagInfo;

        saveTag.IsVisible = true;

        // Dispatch UI update to display tag info
        Dispatcher.Dispatch(() =>
        {
            lblData.Text = $"Device ID: {Convert.ToHexString(tagInfo.Identifier)}" +
            $"\nSerial Number: {tagInfo.SerialNumber}" +
            $"\nIs Empty: {tagInfo.IsEmpty}" +
            $"\nIs Supported: {tagInfo.IsSupported}" +
            $"\nIs Writable: {tagInfo.IsWritable}" +
            $"\nCapacity: {tagInfo.Capacity}" +
            $"\nRecords: {(tagInfo.Records == null ? "null" : tagInfo.Records.FirstOrDefault().Message.ToString())}";
            tagData.IsVisible = true;
        });
    }

}