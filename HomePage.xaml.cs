namespace NFCProj;

public partial class HomePage : ContentPage
{
	public HomePage()
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

    private async void LogArrivalsButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LogArrivals());
    }
}