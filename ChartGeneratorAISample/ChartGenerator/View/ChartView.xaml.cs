using Syncfusion.Maui.AIAssistView;
using Syncfusion.Maui.Core;
using Syncfusion.Maui.Core.Internals;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;

namespace ChartGenerator;

public partial class ChartView : ContentPage
{
    int count = 0;
    ChatViewModel ViewModel;
    public ChartView(ChatViewModel viewModel)
    {
        BindingContext = ViewModel = viewModel;
        InitializeComponent();
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        var previousData = (sender as Border)?.BindingContext as string;
        AssistItem botMessage = new AssistItem() { Text = previousData, IsRequested = true, ShowAssistItemFooter = false };
        ViewModel.Messages.Add(botMessage);
        ViewModel.ShowHeader = false;
        ViewModel.OnRequest(previousData);
    }

    private void close_Clicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is ChatViewModel model)
        {
            model.ShowAssistView = false;
            headerView.IsVisible = false;
        }
    }

    private async void Exportchartbutton_Clicked(object sender, EventArgs e)
    {
        ActivityIndicator? indicator = null;
        PdfDocument? document = null;
        MemoryStream? stream = null;
        Stream? imageStream = null;

        try
        {
            System.Diagnostics.Debug.WriteLine("Starting PDF export process...");
            
            // Display activity indicator
            indicator = new ActivityIndicator
            {
                IsRunning = true, 
                Color = Color.FromArgb("#6750a4"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            
            Grid parentGrid = (Grid)this.Content;
            parentGrid.Children.Add(indicator);
            
            System.Diagnostics.Debug.WriteLine("Creating PDF document...");
            // Create PDF document
            document = new PdfDocument();
            PdfPage page = document.Pages.Add();
            PdfGraphics graphics = page.Graphics;

            // Calculate dimensions
            float width = (float)templatedItemView.Width + 75;
            float height = (float)templatedItemView.Height + 125;

            //To reduce the width and height of the Windows and MAC platform
#if !IOS && !ANDROID
            width = (float)templatedItemView.Width / 2.5f;
            height = (float)templatedItemView.Height / 1.5f;
#endif
            System.Diagnostics.Debug.WriteLine($"PDF dimensions: {width}x{height}");

            // Get the image stream
            System.Diagnostics.Debug.WriteLine("Getting chart image stream...");
            imageStream = await templatedItemView.GetStreamAsync(ImageFileFormat.Png);
            if (imageStream == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to get chart image stream - it's null");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to get chart image", "OK");
                parentGrid.Children.Remove(indicator);
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"Chart image stream obtained. Length: {imageStream.Length} bytes");
            
            // Create PDF image
            System.Diagnostics.Debug.WriteLine("Creating PDF image from bitmap...");
            PdfImage img = new PdfBitmap(imageStream);
            graphics.DrawImage(img, 0, 0, width, height);
            
            // Save to memory stream
            System.Diagnostics.Debug.WriteLine("Saving PDF to memory stream...");
            stream = new MemoryStream();
            document.Save(stream);
            document.Close(true);
            stream.Position = 0;
            
            System.Diagnostics.Debug.WriteLine($"PDF created in memory. Size: {stream.Length} bytes");
            
            // Save to file
            System.Diagnostics.Debug.WriteLine("Saving PDF to file...");
            SavePDF("ChartAsPDF.pdf", stream);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PDF Export Exception: {ex}");
            await Application.Current.MainPage.DisplayAlert("Error", $"Exception during PDF export: {ex.Message}", "OK");
        }
        finally
        {
            // Clean up resources
            if (stream != null)
            {
                stream.Dispose();
                System.Diagnostics.Debug.WriteLine("Memory stream disposed");
            }
            
            if (imageStream != null)
            {
                imageStream.Dispose();
                System.Diagnostics.Debug.WriteLine("Image stream disposed");
            }
            
            if (document != null)
            {
                document = null;
                System.Diagnostics.Debug.WriteLine("PDF document nullified");
            }
            
            // Remove activity indicator
            if (indicator != null && this.Content is Grid parentGrid)
            {
                parentGrid.Children.Remove(indicator);
                System.Diagnostics.Debug.WriteLine("Activity indicator removed");
            }
            
            System.Diagnostics.Debug.WriteLine("PDF export process completed");
        }
    }

    private async void SavePDF(string fileName, Stream stream)
    {
        try 
        {
            fileName = Path.GetFileNameWithoutExtension(fileName) + ".pdf";
            string filePath;

#if ANDROID || IOS
            // For mobile platforms, use the app's cache directory
            string folderPath = Path.Combine(FileSystem.CacheDirectory, "PDFExports");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            filePath = Path.Combine(folderPath, fileName);
#else
            // For desktop platforms, save to bin/exports
            string binPath = AppDomain.CurrentDomain.BaseDirectory;
            string exportsFolder = Path.Combine(binPath, "exports");
            if (!Directory.Exists(exportsFolder))
            {
                Directory.CreateDirectory(exportsFolder);
            }
            filePath = Path.Combine(exportsFolder, fileName);
#endif
            
            using (FileStream fileStream = new(filePath, FileMode.Create, FileAccess.ReadWrite))
            {
                await stream.CopyToAsync(fileStream);
                fileStream.Flush();
            }
            
            await Application.Current.MainPage.DisplayAlert("Success", $"PDF saved to {filePath}", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save PDF: {ex.Message}", "OK");
            // Log more detailed error information
            System.Diagnostics.Debug.WriteLine($"PDF Export Error: {ex.ToString()}");
        }
    }

    private void Saveasimage_Clicked(object sender, EventArgs e)
    {
        templatedItemView.SaveAsImage("AiBlog");
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
                ChartTypeEnum.Cartesian => CartesianChartTemplate,
                ChartTypeEnum.Circular => CircularChartTemplate,
                _ => null
            };
        }
        return null;
    }
}