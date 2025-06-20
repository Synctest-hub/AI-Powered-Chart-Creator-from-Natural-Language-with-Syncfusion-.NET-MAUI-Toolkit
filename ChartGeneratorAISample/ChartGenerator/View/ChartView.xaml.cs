using Syncfusion.Maui.PdfViewer;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Syncfusion.Maui.AIAssistView;
using System.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Maui.Core;
using Syncfusion.Maui.Core.Internals;

namespace ChartGenerator;

public partial class ChartView : ContentPage
{
    int count = 0;
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

    private async void Exportchartbutton_Clicked(object sender, EventArgs e)
    {
        PdfDocument document = new PdfDocument();
        PdfPage page = document.Pages.Add();
        PdfGraphics graphics = page.Graphics;

        float width = (float)templatedItemView.Width + 75;
        float height = (float)templatedItemView.Height + 125;

        //To reduce the width and height of the Windows and MAC platform
#if !IOS && !ANDROID
        width = (float)templatedItemView.Width / 2.5f;
        height = (float)templatedItemView.Height / 1.5f;
#endif

        PdfImage img = new PdfBitmap((await templatedItemView.GetStreamAsync(ImageFileFormat.Png)));
        graphics.DrawImage(img, 0, 0, width, height);
        MemoryStream stream = new MemoryStream();
        document.Save(stream);
        document.Close(true);
        stream.Position = 0;
        SavePDF("ChartAsPDF.pdf", stream);
        stream.Flush();
        stream.Dispose();
    }
    private async void SavePDF(string fileName, Stream stream)
    {
        fileName = Path.GetFileNameWithoutExtension(fileName) + ".pdf";

#if ANDROID
        string path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).ToString();
#else
        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
        string filePath = Path.Combine(path, fileName);
        using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.ReadWrite);
        await stream.CopyToAsync(fileStream);
        fileStream.Flush();
        fileStream.Dispose();
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