using Azure;
using Azure.AI.OpenAI;
using ChartGenerator.AIService;
using Microsoft.Extensions.AI;

namespace ChartGenerator
{
    internal class ChartAIService : AICredentials
    {
        #region Fields

        /// <summary>
        /// The uri result field
        /// </summary>
        private Uri? uriResult;

        #endregion

        public ChartAIService()
        {
            ValidateCredential();
        }

        #region Properties

        internal IChatClient? Client { get; set; }

        internal string? ChatHistory { get; set; }

        internal static bool IsCredentialValid { get; set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validate Azure Credentials
        /// </summary>
        private async void ValidateCredential()
        {
            this.GetAzureOpenAIKernal();

            try
            {
                if (Client != null)
                {
                    await Client!.CompleteAsync("Hello, Test Check");
                    ChatHistory = string.Empty;
                    IsCredentialValid = true;
                }
                else
                {
                    ShowAlertAsync();
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        #region Azure OpenAI
        /// <summary>
        /// To get the Azure open ai kernal method
        /// </summary>
        private void GetAzureOpenAIKernal()
        {
            try
            {
                var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key)).AsChatClient(modelId: deploymentName);
                this.Client = client;
            }
            catch (Exception)
            {
            }
        }
        #endregion

        /// <summary>
        /// Retrieves an answer from the deployment name model using the provided user prompt.
        /// </summary>
        /// <param name="userPrompt">The user prompt.</param>
        /// <param name="includeClassContext">Whether to include class structure context.</param>
        /// <returns>The AI response.</returns>
        internal async Task<string> GetAnswerFromGPT(string userPrompt, bool includeClassContext = false)
        {
            try
            {
                if (IsCredentialValid && Client != null)
                {
                    ChatHistory = string.Empty;

                    // Only include class context if specifically requested
                    if (includeClassContext)
                    {
                        var classContext = GetChartClassStructureContext();
                        ChatHistory = classContext + "\n\n" + userPrompt;
                    }
                    else
                    {
                        ChatHistory = userPrompt;
                    }

                    var response = await Client.CompleteAsync(ChatHistory);
                    return response.ToString();
                }
            }
            catch
            {
                // If an exception occurs (e.g., network issues, API errors), return an empty string.
                return "";
            }

            return "";
        }

        /// <summary>
        /// Show Alert Popup
        /// </summary>
        private async void ShowAlertAsync()
        {
            var page = Application.Current?.Windows[0].Page;
            if (page != null && !IsCredentialValid)
            {
                await page.DisplayAlert("Alert", "The Azure API key or endpoint is missing or incorrect. Please verify your credentials. You can also continue with the offline data.", "OK");
            }
        }

        /// <summary>
        /// Creates a concise context string that explains the structure of chart-related classes
        /// </summary>
        /// <returns>A string containing class structure information</returns>
        private string GetChartClassStructureContext()
        {
            return @"
            Chart class model reference:
            ChartConfig{ChartType:enum, Title:string, Series:Collection<SeriesConfig>, XAxis/YAxis:Collection<AxisConfig>, ShowLegend:bool, SideBySidePlacement:bool}
            SeriesConfig{Type:enum, Name:string, DataSource:Collection<DataModel>, Tooltip:bool}
            AxisConfig{Title:string, Type:string('Numerical'|'DateTime'|'Category'|'Logarithmic'), Minimum/Maximum:double?}";
        }

        /// <summary>
        /// Gets specific context about a particular chart class based on need
        /// </summary>
        /// <param name="classType">The class type to get context for ("ChartConfig", "SeriesConfig", or "AxisConfig")</param>
        /// <returns>Specific context about the requested class</returns>
        internal string GetSpecificClassContext(string classType)
        {
            return classType.ToLower() switch
            {
                "chartconfig" => "ChartConfig: Controls overall chart appearance with properties for chart type, title, axis collections, series data, and display options like legends.",
                "seriesconfig" => "SeriesConfig: Defines a data series with properties for series type, name, data source collection, and tooltip visibility.",
                "axisconfig" => "AxisConfig: Configures chart axes with title, axis type (Numerical, DateTime, Category, Logarithmic), and optional min/max range values.",
                _ => "Unknown class type. Available classes: ChartConfig, SeriesConfig, AxisConfig"
            };
        }
    }
    #endregion
}