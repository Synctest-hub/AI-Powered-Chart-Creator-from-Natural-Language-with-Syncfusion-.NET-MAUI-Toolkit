using Syncfusion.Maui.AIAssistView;

namespace ChartGenerator;

public partial class ChartView : ContentPage
{
    ChartViewModel ViewModel;
    public ChartView(ChartViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = ViewModel = viewModel;
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        var appointmentData = (sender as Border)?.BindingContext as string;
        AssistItem botMessage = new AssistItem() { Text = appointmentData, IsRequested = true, ShowAssistItemFooter = false };
        ViewModel.Messages.Add(botMessage);
        ViewModel.ShowHeader = false;
        ViewModel.OnRequest(appointmentData);
    }

    private void close_Clicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is ChartViewModel model)
        {
            model.ShowAssistView = false;
            headerView.IsVisible = false;
        }
    }
}

public class ChartTemplateSelector : DataTemplateSelector
{
    public DataTemplate CartesianChartTemplate { get; set; }
    public DataTemplate CircularChartTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is ChartConfig chartConfig)
        {
            return chartConfig.ChartType switch
            {
                ChartEnums.ChartTypeEnum.Cartesian => CartesianChartTemplate,
                ChartEnums.ChartTypeEnum.Circular => CircularChartTemplate,
                _ => null
            };
        }
        return null;
    }
}