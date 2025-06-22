using Newtonsoft.Json;
using Syncfusion.Maui.AIAssistView;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

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
            OnCreateButtonClicked();
        }

        private async void OnCreateButtonClicked()
        {
            if (!string.IsNullOrEmpty(EntryText))
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
You are a chart data generator API. Your task is to convert user inputs describing chart specifications into clean, properly formatted JSON strings. Respond ONLY with valid JSON - no explanations, markdown formatting, or code blocks.

Example user input: ""Sales by region column chart.""

Required JSON format:
{
  ""chartType"": ""cartesian"",
  ""title"": ""Revenue by Region"",
  ""showLegend"": true,
  ""sideBySidePlacement"": false,
  ""xAxis"": [
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

JSON Requirements:
1. ""chartType"": Must be either ""cartesian"" or ""circular"" (lowercase)
2. ""title"": Create an appropriate title based on the query
3. ""showLegend"": Boolean value, default to true
4. ""sideBySidePlacement"": Boolean value, default to false for single series
5. ""xAxis"" and ""yAxis"": Must be arrays of objects, even for single axis
6. ""xAxis"" can be ""datetime"", ""category"", ""logarithmic"" or ""numerical"", ""yAxis"" can only be ""numerical"" or ""logarithmic"", based on the user requested query. Proper capitalization will be handled.
7. ""series"": Must be an array containing series objects, and can have ""name"" based on its use case, if not have name you can do showlegend false.
8. ""type"": Must be one of: ""line"", ""column"", ""area"", ""pie"", ""doughnut"", ""radialbar"" (lowercase)
9. ""dataSource"": Array of objects with x and y values for the series, y value always should double and mention it in ""yvalue"" and x value can be any type mention it ""xvalue"" properties
10. ""tooltip"": Boolean value, default to true

IMPORTANT:
- Respond ONLY with valid, properly formatted JSON
- Use lowercase for all enum values (chartType, series type, axis type)
- Include all required properties
- Always format the ""xAxis"" and ""yAxis"" as arrays
- Always choose correct axis types based on the data points
- Do not include any explanations or comments in your response
- Do not use markdown code blocks - just output raw JSON
- Ensure all property names are consistent with the format provided

User Request: " + userPrompt;
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
