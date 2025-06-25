using Newtonsoft.Json;
using Syncfusion.Maui.AIAssistView;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using Application = Microsoft.Maui.Controls.Application;

namespace ChartGenerator
{
    public partial class ChatViewModel
    {

        #region Configure AI
        private ChartAIService openAIService = new();

        #endregion

        private ChartConfig chartData;

        public ChartConfig ChartData
        {
            get => chartData;
            set
            {
                chartData = value;
                OnPropertyChanged(nameof(ChartData));
            }
        }

        private void OnButtonClicked(string buttonText)
        {
            EntryText = buttonText.Replace("\"", "").Replace("&quot;", "");
        }

        private async void OnCreateButtonClicked(object obj)
        {
            if (!string.IsNullOrEmpty(EntryText) || ImageSourceCollection.Count != 0)
            {
                IsLoading = true;
                ShowAssistView = false;
                Messages.Clear();
                showHeader = true;
                if (ChartAIService.IsCredentialValid)
                {
                    EnableAssistant = true;
                    await GetDataFromAI(EntryText);
                }
                else
                {
                    await Task.Delay(500);
                    AssistItem message = new() { Text = "Currently in offline mode...", ShowAssistItemFooter = false };
                    Messages.Add(message);
                }

                Application.Current.MainPage.Navigation.PushAsync(new ChartView(this));
                IsLoading = false;
            }
        }

        
        private string GetChartUserPrompt(string userPrompt)
        {
            string userQuery = @"
As an AI service, your task is to convert user inputs describing chart specifications into JSON formatted strings. Each user input will describe a specific chart type and its configurations, including axes titles, legend visibility, series configurations, etc. You will structure the output in JSON format accordingly.

Example user input: ""Sales by region column chart.""
Expected JSON output:
{
  ""chartType"": ""cartesian"",
  ""title"": ""Revenue by Region"",
  ""showLegend"": true,
""sideBySidePlacement"": ""true/false"",
  ""xAxis"":[
{
    ""type"": ""category"",
    ""title"": ""Region""
  }
],
  ""yAxis"": [
{
    ""title"": ""Revenue"",
    ""type"": ""numerical"",
    ""min"": 0
  }
],
  ""series"": [
    {
      ""type"": ""column"",
      ""xpath"": ""region"",
      ""name"": ""Revenue"",
      ""dataSource"": [
        { ""xvalue"": ""North America"", ""yvalue"": 120000 },
        { ""xvalue"": ""Europe"", ""yvalue"": 90000 },
        { ""xvalue"": ""Asia"", ""yvalue"": 70000 },
        { ""xvalue"": ""South America"", ""yvalue"": 45000 },
        { ""xvalue"": ""Australia"", ""yvalue"": 30000 }
      ],
      ""tooltip"": true
    }
  ]
}

When generating the JSON output, take into account the following:

1. **Chart Type**: Determine the type of chart based on keywords in the user query. and it should be only circular or cartesian
2. **Chart Title**: Craft an appropriate title using key elements of the query.
3. **Axis Information**: Define the x-axis and y-axis with relevant titles and types. Use categories for discrete data and numerical for continuous data.
4. **Series Configuration**: Include details about the series type and data points as mentioned in the query. it supports only  Line, Column, Spline, Area, Pie, Doughnut, RadialBar.
5. **Name in the Series**: The name of the series should be represent category of the series.
6. **Data Source**: Provide a sample data source for the series, it should only name as ""dataSource"" and include ""xvalue"" and ""yvalue"".
7. **Show Legend**: Default as `true` unless specified otherwise.
8.  **SideBySidePlacement**: Default to 'false' and return bool value based on multiple column series placement and column placed side by side being true, column back to back being false, or Bottom/Top being false, or one column being positive and another being negative values.
     

Generate appropriate configurations according to these guidelines, and return the result as a JSON formatted string for any query shared with you." +

  $"User Request: " + userPrompt;
            return userQuery;
        }

        /// <summary>
        /// Normalizes axis types to ensure they have the proper format
        /// </summary>
        /// <param name="chartData">The chart data to normalize</param>
        private void NormalizeAxisTypes(ChartConfig chartData)
        {
            // Normalize X-Axis types
            foreach (var axis in chartData.XAxis)
            {
                axis.Type = NormalizeAxisType(axis.Type);
            }

            // Normalize Y-Axis types
            foreach (var axis in chartData.YAxis)
            {
                axis.Type = NormalizeAxisType(axis.Type);
            }
        }

        /// <summary>
        /// Normalizes an individual axis type string
        /// </summary>
        /// <param name="axisType">The axis type to normalize</param>
        /// <returns>The normalized axis type string</returns>
        private string NormalizeAxisType(string axisType)
        {
            if (string.IsNullOrEmpty(axisType))
                return "Numerical"; // Default

            switch (axisType.ToLower())
            {
                case "numerical":
                case "linear":
                    return "Numerical";
                case "datetime":
                    return "DateTime";
                case "category":
                    return "Category";
                case "log":
                case "logarithmic":
                    return "Logarithmic";
                default:
                    return "Numerical"; // Default
            }
        }

        private string ExtractJsonContent(string responseString)
        {
            try
            {
                // First, check if the content is already valid JSON without code blocks
                try
                {
                    // Try to parse the whole string as JSON
                    var testJsonObject = JsonConvert.DeserializeObject(responseString);
                    if (testJsonObject != null)
                    {
                        return responseString; // Already valid JSON
                    }
                }
                catch
                {
                    // Not valid JSON, continue with extraction
                }

                // Regex pattern to extract content within <code> tags
                var codeBlockPattern = @"<code>(.*?)</code>";
                var regex = new Regex(codeBlockPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var match = regex.Match(responseString);

                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }

                // If no code blocks, try to extract JSON from markdown code blocks (```json...```)
                var markdownPattern = @"```(?:json)?\s*([\s\S]*?)```";
                regex = new Regex(markdownPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                match = regex.Match(responseString);

                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }

                // If still no match, look for content that looks like JSON (starts with { and ends with })
                var jsonObjectPattern = @"\{[\s\S]*\}";
                regex = new Regex(jsonObjectPattern, RegexOptions.Singleline);
                match = regex.Match(responseString);

                if (match.Success)
                {
                    return match.Value.Trim();
                }

                // If all extraction attempts fail, return the original string
                return responseString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting JSON content: {ex.Message}");
                return responseString; // Return original in case of error
            }
        }

        internal void DecryptJSON(string jsonData, bool fromChat)
        {
            try
            {
                // Extract JSON content from possible code blocks or formatted text
                string extractedJson = ExtractJsonContent(jsonData);
                Console.WriteLine($"Extracted JSON: {extractedJson}");

                // Deserialize the JSON to ChartConfig object
                var chartData = JsonConvert.DeserializeObject<ChartConfig>(extractedJson);

                // Validate and assign chart data
                if (chartData != null)
                {
                    // Ensure collections are initialized
                    if (chartData.Series == null)
                        chartData.Series = new ObservableCollection<SeriesConfig>();

                    if (chartData.XAxis == null)
                        chartData.XAxis = new ObservableCollection<AxisConfig>();

                    if (chartData.YAxis == null)
                        chartData.YAxis = new ObservableCollection<AxisConfig>();

                    // Normalize axis types
                    NormalizeAxisTypes(chartData);

                    ChartData = chartData;

                    if (fromChat)
                    {
                        AssistItem message = new() { Text = "Chart generated successfully...", ShowAssistItemFooter = false };
                        Messages.Add(message);
                    }
                }
                else
                {
                    throw new JsonException("Failed to deserialize chart data");
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Exception: {ex.Message}");
                AssistItem assistItem = new() { Text = "An error occurred while processing your JSON data. Please try again with a different request.", ShowAssistItemFooter = false };
                Messages.Add(assistItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                AssistItem assistItem = new() { Text = "An error occurred while processing your request. Please try again later.", ShowAssistItemFooter = false };
                Messages.Add(assistItem);
            }
        }

    }
}
