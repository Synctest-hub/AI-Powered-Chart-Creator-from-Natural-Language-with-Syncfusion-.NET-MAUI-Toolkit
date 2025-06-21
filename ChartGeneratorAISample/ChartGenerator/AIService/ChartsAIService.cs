﻿using Azure;
using Azure.AI.OpenAI;
using ChartGenerator.AIService;
using Microsoft.Extensions.AI;

namespace ChartGenerator
{
    internal class ChartAIService : AICredentials
    {
        #region Fields

        /// <summary>
        /// The already credential validated field
        /// </summary>
        private static bool isAlreadyValidated;

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

            if (isAlreadyValidated)
            {
                return;
            }

            try
            {
                if (Client != null)
                {
                    await Client!.CompleteAsync("Hello, Test Check");
                    ChatHistory = string.Empty;
                    IsCredentialValid = true;
                    isAlreadyValidated = true;
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
        /// <returns>The AI response.</returns>
        internal async Task<string> GetAnswerFromGPT(string userPrompt)
        {
            try
            {
                if (IsCredentialValid && Client != null)
                {
                    ChatHistory = string.Empty;
                    // Add the system message and user message to the options
                    ChatHistory = ChatHistory + userPrompt;
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
                isAlreadyValidated = true;
                await page.DisplayAlert("Alert", "The Azure API key or endpoint is missing or incorrect. Please verify your credentials. You can also continue with the offline data.", "OK");
            }
        }
    }

    #endregion
}
