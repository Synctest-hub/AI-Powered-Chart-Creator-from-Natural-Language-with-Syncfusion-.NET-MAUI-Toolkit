using Newtonsoft.Json;
using Syncfusion.Maui.AIAssistView;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ChartGenerator
{
    public class ChartViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string? entryText;
        private bool showAssistView;
        private bool showHeader = true;
        private bool showIndicator = false;
        private ChartConfig chartData;
        private string oldJson;
        private string newJson;
        private bool enableAssistant;
        private bool isLoading;
        private ObservableCollection<IAssistItem> messages = new();

        private ChartAIService semanticKernalService = new();

        public string? EntryText
        {
            get => entryText;
            set
            {
                entryText = value;
                OnPropertyChanged(nameof(EntryText));
            }
        }

        public bool ShowAssistView
        {
            get => showAssistView;
            set
            {
                showAssistView = value;
                OnPropertyChanged(nameof(ShowAssistView));
            }
        }

        public ChartConfig ChartData
        {
            get => chartData;
            set
            {
                chartData = value;
                OnPropertyChanged(nameof(ChartData));
            }
        }

        public ObservableCollection<IAssistItem> Messages
        {
            get => messages;
            set
            {
                messages = value;
                OnPropertyChanged(nameof(Messages));
            }
        }

        public bool EnableAssistant
        {
            get => enableAssistant;
            set
            {
                enableAssistant = value;
                OnPropertyChanged(nameof(EnableAssistant));
            }
        }

        private bool isViewDisable = true;

        public bool IsDisable
        {
            get => isViewDisable;
            set
            {
                isViewDisable = value;
                OnPropertyChanged(nameof(IsDisable));
            }
        }

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                IsDisable = !value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        /// <summary>
        /// Gets or sets the show header.
        /// </summary>
        public bool ShowHeader
        {
            get { return this.showHeader; }
            set { this.showHeader = value; OnPropertyChanged(nameof(ShowHeader)); }
        }

        /// <summary>
        /// Gets or sets the show activitity indicator.
        /// </summary>
        public bool ShowIndicator
        {
            get
            {
                return this.showIndicator;
            }
            set
            {
                this.showIndicator = value;
                OnPropertyChanged(nameof(ShowIndicator));
            }
        }

        private ObservableCollection<String>? modelPrompts;
        public ObservableCollection<String>? ModelPrompts
        {
            get { return modelPrompts; }
            set
            {
                this.modelPrompts = value;
                OnPropertyChanged(nameof(ModelPrompts));
            }
        }

        public ICommand ButtonClicked { get; }
        public ICommand CreateButtonClicked { get; }
        public ICommand AiButtonClicked { get; }
        public ICommand CloseButtonClicked { get; }
        public ICommand RefreshButtonClicked { get; }
        public ICommand RequestCommand { get; }

        public ChartViewModel()
        {
            ButtonClicked = new Command<string>(OnButtonClicked);
            CreateButtonClicked = new Command(OnCreateButtonClicked);
            AiButtonClicked = new Command(OnAiButtonClicked);
            RequestCommand = new Command<object>(OnRequest);
            CloseButtonClicked = new Command(OnCloseButtonClicked);
            RefreshButtonClicked = new Command(OnRefreshButtonClicked);

            modelPrompts = new ObservableCollection<String>()
            {
                "Remove legends from the chart",
                "Change the chart title to 'Sales by Region'",
            };
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
                if (ChartAIService.IsCredentialValid)
                {
                    IsLoading = true;
                    EnableAssistant = true;
                    await GetDataFromAI(EntryText);
                }
                else
                {
                    CreateOfflineChart(EntryText);
                    AssistItem message = new() { Text = "Currently in offline mode...", ShowAssistItemFooter = false };
                    Messages.Add(message);
                }

                Application.Current.MainPage.Navigation.PushAsync(new ChartView(this));
                IsLoading = false;
            }
        }

        internal void CreateOfflineChart(string entryText)
        {
            string response = string.Empty;

            if (entryText.Contains("column"))
            {
                response = "{\n  \"chartType\": \"cartesian\",\n  \"title\": \"Revenue by Region\",\n  \"xAxis\": {\n    \"type\": \"category\",\n    \"title\": \"Region\"\n  },\n  \"yAxis\": {\n    \"title\": \"Revenue\",\n    \"type\": \"numerical\",\n    \"min\": 0\n  },\n  \"series\": [\n    {\n      \"type\": \"column\",\n      \"xpath\": \"region\",\n      \"dataSource\": [\n        { \"xvalue\": \"North America\", \"yvalue\": 120000 },\n        { \"xvalue\": \"Europe\", \"yvalue\": 90000 },\n        { \"xvalue\": \"Asia\", \"yvalue\": 70000 },\n        { \"xvalue\": \"South America\", \"yvalue\": 45000 },\n        { \"xvalue\": \"Australia\", \"yvalue\": 30000 }\n      ],\n      \"tooltip\": true\n    }\n  ]\n}";
            }
            else if (entryText.Contains("area"))
            {
                response = "{\n  \"chartType\": \"cartesian\",\n  \"title\": \"Farm Productivity Over Different Seasons\",\n  \"xAxis\": {\n    \"type\": \"category\",\n    \"title\": \"Seasons\"\n  },\n  \"yAxis\": {\n    \"title\": \"Productivity (tons)\",\n    \"type\": \"numerical\",\n    \"min\": 0\n, \n    \"max\": 200\n   },\n  \"series\": [\n    {\n      \"type\": \"area\",\n      \"xpath\": \"season\",\n      \"dataSource\": [\n        { \"xvalue\": \"Spring\", \"yvalue\": 120 },\n        { \"xvalue\": \"Summer\", \"yvalue\": 150 },\n        { \"xvalue\": \"Autumn\", \"yvalue\": 130 },\n        { \"xvalue\": \"Winter\", \"yvalue\": 80 }\n      ],\n      \"tooltip\": true\n    }\n  ]\n}";
            }
            else if (entryText.Contains("pie"))
            {
                response = "{\n  \"chartType\": \"circular\",\n  \"title\": \"Monthly Sales Data\",\n  \"series\": [\n    {\n      \"type\": \"pie\",\n      \"xpath\": \"category\",\n      \"dataSource\": [\n        { \"xvalue\": \"January\", \"yvalue\": 5000 },\n        { \"xvalue\": \"February\", \"yvalue\": 6000 },\n        { \"xvalue\": \"March\", \"yvalue\": 7000 },\n        { \"xvalue\": \"April\", \"yvalue\": 8000 },\n        { \"xvalue\": \"May\", \"yvalue\": 9000 },\n        { \"xvalue\": \"June\", \"yvalue\": 10000 },\n        { \"xvalue\": \"July\", \"yvalue\": 11000 },\n        { \"xvalue\": \"August\", \"yvalue\": 12000 },\n        { \"xvalue\": \"September\", \"yvalue\": 13000 },\n        { \"xvalue\": \"October\", \"yvalue\": 14000 },\n        { \"xvalue\": \"November\", \"yvalue\": 15000 },\n        { \"xvalue\": \"December\", \"yvalue\": 16000 }\n      ],\n      \"tooltip\": true\n    }\n  ]\n}";
            }
            else if (entryText.Contains("doughnut"))
            {
                response = "{\n  \"chartType\": \"circular\",\n  \"title\": \"Task Completion Doughnut Chart\",\n  \"series\": [\n    {\n      \"type\": \"doughnut\",\n      \"xpath\": \"xvalue\",\n      \"dataSource\": [\n        { \"xvalue\": \"Completed\", \"yvalue\": 70 },\n        { \"xvalue\": \"In Progress\", \"yvalue\": 20 },\n        { \"xvalue\": \"Pending\", \"yvalue\": 10 }\n      ],\n      \"tooltip\": true\n    }\n  ],\n  \"xAxis\": {\n    \"type\": \"category\",\n    \"title\": \"Task Status\"\n  },\n  \"yAxis\": {\n    \"title\": \"Percentage\",\n    \"type\": \"numerical\",\n    \"min\": 0,\n    \"max\": 100\n  }\n}";
            }
            else if (entryText.Contains("spline"))
            {
                response = "{\n  \"chartType\": \"cartesian\",\n  \"title\": \"Daily Average Temperature for the Past Week\",\n  \"series\": [\n    {\n      \"type\": \"spline\",\n      \"xpath\": \"date\",\n      \"dataSource\": [\n        { \"xvalue\": \"2023-10-01\", \"yvalue\": 15 },\n        { \"xvalue\": \"2023-10-02\", \"yvalue\": 17 },\n        { \"xvalue\": \"2023-10-03\", \"yvalue\": 16 },\n        { \"xvalue\": \"2023-10-04\", \"yvalue\": 18 },\n        { \"xvalue\": \"2023-10-05\", \"yvalue\": 20 },\n        { \"xvalue\": \"2023-10-06\", \"yvalue\": 19 },\n        { \"xvalue\": \"2023-10-07\", \"yvalue\": 21 }\n      ],\n      \"tooltip\": true\n    }\n  ],\n  \"xAxis\": {\n    \"type\": \"category\",\n    \"title\": \"Date\"\n  },\n  \"yAxis\": {\n    \"title\": \"Temperature (°C)\",\n    \"type\": \"numerical\",\n    \"min\": 0,\n    \"max\": 30\n  }\n}";
            }
            else if (entryText.Contains("line"))
            {
                response = "\n{\n  \"chartType\": \"cartesian\",\n  \"title\": \"Product Sales Over Six Months\",\n  \"series\": [\n    {\n      \"type\": \"line\",\n      \"xpath\": \"month\",\n      \"dataSource\": [\n        { \"xvalue\": \"January\", \"yvalue\": 150 },\n        { \"xvalue\": \"February\", \"yvalue\": 200 },\n        { \"xvalue\": \"March\", \"yvalue\": 175 },\n        { \"xvalue\": \"April\", \"yvalue\": 220 },\n        { \"xvalue\": \"May\", \"yvalue\": 240 },\n        { \"xvalue\": \"June\", \"yvalue\": 210 }\n      ],\n      \"tooltip\": true\n    }\n  ],\n  \"xAxis\": {\n    \"type\": \"category\",\n    \"title\": \"Month\"\n  },\n  \"yAxis\": {\n    \"title\": \"Sales (Units)\",\n    \"type\": \"numerical\",\n    \"min\": 0,\n    \"max\": 300\n  }\n}\n";
            }
            else
            {
                response = "{\n  \"chartType\": \"cartesian\",\n  \"title\": \"Revenue by Region\",\n  \"xAxis\": {\n    \"type\": \"category\",\n    \"title\": \"Region\"\n  },\n  \"yAxis\": {\n    \"title\": \"Revenue\",\n    \"type\": \"numerical\",\n    \"min\": 0\n  },\n  \"series\": [\n    {\n      \"type\": \"column\",\n      \"xpath\": \"region\",\n      \"dataSource\": [\n        { \"xvalue\": \"North America\", \"yvalue\": 120000 },\n        { \"xvalue\": \"Europe\", \"yvalue\": 90000 },\n        { \"xvalue\": \"Asia\", \"yvalue\": 70000 },\n        { \"xvalue\": \"South America\", \"yvalue\": 45000 },\n        { \"xvalue\": \"Australia\", \"yvalue\": 30000 }\n      ],\n      \"tooltip\": true\n    }\n  ]\n}";
            }

            DecryptJSON(response, false);
        }

        private void OnAiButtonClicked()
        {
            ShowAssistView = true;
        }

        private void OnCloseButtonClicked()
        {
            ShowAssistView = false;
        }

        internal async void OnRequest(object requestText)
        {
            var value = ((RequestEventArgs)requestText).RequestItem.Text;
            OnRequest(value);
        }

        internal async void OnRequest(string requestText)
        {
            if (!string.IsNullOrEmpty(requestText))
            {
                newJson = await ProcessUserRequest(requestText);
            }

            if (!string.IsNullOrEmpty(newJson) && newJson != oldJson)
            {
                DecryptJSON(newJson, true);

                oldJson = newJson;
            }
            else
            {
                AssistItem assistItem = new() { Text = "Invalid request. Please try again!", ShowAssistItemFooter = false };
                Messages.Add(assistItem);
            }
        }

        public async Task<string> ProcessUserRequest(string request)
        {
            string prompt = $"Given the user's request: {request}, modify the following json data." +
                            $"json data: {oldJson}"
                            + "Instructions: "
                            + "- Accept only the possible modifications that can be made to the current json data; return empty string for unintended requests like changing the entire chart type or etc"
                            + "- Only plain text should be used; no need to specify 'json' above the data."
                            + "- No additional content other than json data should be included!";

            var response = await semanticKernalService.GetAnswerFromGPT(prompt);

            return response.ToString();
        }

        private void OnRefreshButtonClicked()
        {
            Messages.Clear();

            AssistItem message = new()
            {
                Text = ChartAIService.IsCredentialValid ? "Share your ideas to modify the chart." : "You are in offline mode...",
                ShowAssistItemFooter = false
            };

            Messages.Add(message);
        }

        private async Task GetDataFromAI(string text)
        {
            string response = await GetAIResponse(text);

            if (response != null)
            {
                oldJson = response;
                DecryptJSON(response, false);
            }
        }

        public async Task<string> GetAIResponse(string query)
        {
            string prompt = "Create a JSON configuration for a cartesian chart using the ChartConfig and SeriesConfig classes, "
                + $"based on the following query: {query}. The JSON should include: "
                + "1. The chart type (cartesian or circular). "
                + "2. Title of the chart. "
                + "3. X-axis and Y-axis specifications (for cartesian charts). "
                + "4. Series configurations, including type and data source. "
                + "5. A setting for whether tooltips are enabled. "
                + "Use exemplary data relevant to the query to fill in the values. "
                + "Example JSON Structure: "
                + "{ "
                + "  chartType: cartesian, // or circular"
                + "  title: {Chart Title}, // Replace with an appropriate title"
                + "  ShowLegend: true "
                + "  series: [ "
                + "    { "
                + "      type: line, // Specify type: line, area, column, pie, doughnut or radialbar etc."
                + "      xpath: xvalue, "
                + "      dataSource: [ "
                + "        { xvalue: {X1}, yvalue: {Y1} },    // Sample data points"
                + "        { xvalue: {X2}, yvalue: {Y2} },   //  Keys should always be xvalue and yvalue. All other keys are not allowed."
                + "        { xvalue: {X3}, yvalue: {Y3} }   //   Real-world data is preferred over sample data"
                + "      ], "
                + "      tooltip: true "
                + "    } "
                + "  ], "
                + "  xAxis: { "
                + "    type: category, // For cartesian charts"
                + "    title: {X Axis Title} // Optional: Replace with an appropriate title"
                + "  }, "
                + "  yAxis: { "
                + "    title: {Y Axis Title}, // Optional: Replace with an appropriate title"
                + "    type: numerical, // For cartesian charts"
                + "    min: {Min Value}, // Optional: set minimum value if relevant"
                + "    max: {Max Value} // Optional: set maximum value if relevant"
                + "  }, "
                + "} "
                + "Instructions: "
                + "- Replace placeholders such as `{query}`, `{Chart Title}`, `{X1}`, `{Y1}`, `{X Axis Title}`, and `{Y Axis Title}` with actual data relevant to the query. "
                + "- Choose the appropriate chart and series types based on the data. "
                + "- Ensure the data format matches the requirements for cartesian charts. "
                + "- Only plain text should be used; no need to specify 'json' above the data. "
                + "- No additional content other than json data should be included!";

            // Call the method to get the AI response
            var response = await semanticKernalService.GetAnswerFromGPT(prompt);

            // Convert the response to a string (assuming the response has a ToString method)
            return response.ToString();
        }

        internal void DecryptJSON(string jsonData, bool fromChat)
        {
            try
            {
                var chartData = JsonConvert.DeserializeObject<ChartConfig>(jsonData);

                ChartData = chartData!;

                if (fromChat)
                {
                    AssistItem message = new() { Text = "Chart generated successfully...", ShowAssistItemFooter = false };
                    Messages.Add(message);
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine(ex.Message);
                AssistItem assistItem = new() { Text = "An error occurred while processing your request. Please try again later.", ShowAssistItemFooter = false };
                Messages.Add(assistItem);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
