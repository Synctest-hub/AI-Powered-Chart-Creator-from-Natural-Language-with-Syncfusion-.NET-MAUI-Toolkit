
using Newtonsoft.Json;
using Syncfusion.Maui.AIAssistView;
using Syncfusion.Maui.Popup;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
namespace ChartGenerator
{
    public partial class ChatViewModel : INotifyPropertyChanged
    {

        private readonly ImagePickerHelper _imagePickerHelper;
        public event PropertyChangedEventHandler? PropertyChanged;
        private bool canStopResponse;
        private string sendIconText;
        private bool autoSuggestionPopupIsOpen;
        private string inputText;
        private ObservableCollection<ImageSource> imageSourceCollection; 
        private int imageNo;
        private string? entryText;
        private bool showAssistView;
        private bool showHeader = true;
        private bool showIndicator = false;
        private string oldJson;
        private string newJson;
        private bool enableAssistant;
        private bool isLoading;
        private ObservableCollection<IAssistItem> messages = new(); 
        private bool isTemporaryChatEnabled;
        private SfPopup OptionsPopup; 
        private bool isSendIconEnabled;
        internal bool isResponseStreaming;
        private double sendIconWidth;   
        private SfPopup ExpandedEditorPopup;
        private ObservableCollection<Option> optionsContextMenu;  
        private bool isNewChatEnabled;
        private bool isHeaderVisible = true;
        public bool IsNewChatEnabled
        {
            get
            {
                return isNewChatEnabled;
            }
            set
            {
                isNewChatEnabled = value;
                OnPropertyChanged(nameof(IsNewChatEnabled));
            }
        }
        public bool IsHeaderVisible
        {
            get
            {
                return isHeaderVisible;
            }
            set
            {
                isHeaderVisible = value;
                if (!value)
                {
                    this.IsNewChatEnabled = true;
                }
                else
                {
                    this.IsNewChatEnabled = false;
                }
                OnPropertyChanged(nameof(IsHeaderVisible));
            }
        } 
        public ObservableCollection<Option> OptionsContextMenu
        {
            get
            {
                return optionsContextMenu;
            }
            set
            {
                optionsContextMenu = value;
                OnPropertyChanged(nameof(OptionsContextMenu));
            }
        }
         
        public bool IsTemporaryChatEnabled
        {
            get
            {
                return isTemporaryChatEnabled;
            }
            set
            {
                isTemporaryChatEnabled = value;
                OnPropertyChanged(nameof(IsTemporaryChatEnabled));
            }
        }
        public double SendIconWidth
        {
            get { return sendIconWidth; }
            set
            {
                sendIconWidth = value;
                OnPropertyChanged(nameof(SendIconWidth));
            }
        }
        public bool IsSendIconEnabled
        {
            get
            {
                return isSendIconEnabled;
            }
            set
            {
                isSendIconEnabled = value;
                OnPropertyChanged(nameof(IsSendIconEnabled));
            }
        }
        public string SendIconText
        {
            get
            {
                return sendIconText;
            }
            set
            {
                sendIconText = value;
                OnPropertyChanged(nameof(SendIconText));
            }
        }

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

        public Command<object> SendButtonCommand { get; set; }
        public ICommand ButtonClicked { get; }
        public Command<object> CreateButtonClicked { get; }
        public ICommand AiButtonClicked { get; }
        public ICommand CloseButtonClicked { get; }
        public ICommand RefreshButtonClicked { get; }
        public ICommand RequestCommand { get; }
        public Command<object> EditorOptionsComamnd { get; set; }
        public ObservableCollection<Option> EditorOptions { get; set; }
        public Command<object> AttachmentContextMenuCommand { get; set; }
        public Command<object> EditorExpandCollapseCommand { get; set; }
        public Command<object> CancelImageSelected { get; set; }
        public ObservableCollection<ImageSource> ImageSourceCollection
        {
            get
            {
                return imageSourceCollection;
            }
            set
            {
                imageSourceCollection = value;
                OnPropertyChanged(nameof(ImageSourceCollection));
            }
        }
        public bool AutoSuggestionPopupIsOpen
        {
            get { return autoSuggestionPopupIsOpen; }
            set
            {
                autoSuggestionPopupIsOpen = value;
                OnPropertyChanged(nameof(AutoSuggestionPopupIsOpen));
            }
        }
        public bool CanStopResponse
        {
            get
            {
                return canStopResponse;
            }
            set
            {
                canStopResponse = value;
                OnPropertyChanged(nameof(CanStopResponse));
            }
        }

        public string InputText
        {
            get { return inputText; }
            set
            {
                inputText = value;
                UpdateSendIcon(inputText);
                OnPropertyChanged(nameof(InputText));
                //UpdateFilter();
                AutoSuggestionPopupIsOpen = !string.IsNullOrEmpty(InputText) && this.Messages.Count == 0;
            }
        }

        public bool HasImageUploaded
        {
            get { return this.ImageSourceCollection.Count > 0; }
        }
        public ChatViewModel()
        { 
            _imagePickerHelper = new ImagePickerHelper();
            ButtonClicked = new Command<string>(OnButtonClicked);
            CreateButtonClicked = new Command<object>(OnCreateButtonClicked);
            AiButtonClicked = new Command(OnAiButtonClicked);
            RequestCommand = new Command<object>(OnRequest);
            CloseButtonClicked = new Command(OnCloseButtonClicked);
            RefreshButtonClicked = new Command(OnRefreshButtonClicked);
            EditorOptionsComamnd = new Command<object>(ExecuteEditorOptionsCommand);    
            CancelImageSelected = new Command<object>(ExecuteCancelImageSelectedCommand);
            SendButtonCommand = new Command<object>(ExecuteSendButtonCommand);
            EditorExpandCollapseCommand = new Command<object>(ExecuteEditorExpandCollapseCommand); 
            this.ImageSourceCollection = new ObservableCollection<ImageSource>();
            AttachmentContextMenuCommand = new Command<object>(OnAttachmentContextMenuTapCommand);
            EditorOptions = new ObservableCollection<Option>();
            EditorOptions.Add(new Option() { Name = "Attachment", Icon = "\ue754" }); 
        }
        public void ExecuteCancelImageSelectedCommand(object obj)
        {
            try
            {
                var imageToRemove = obj as ImageSource;
                if (imageToRemove != null)
                {
                    // Remove the image from collection
                    this.ImageSourceCollection.Remove(imageToRemove);

                    // Notify property changes
                    OnPropertyChanged(nameof(this.HasImageUploaded));

                    // Update send icon status
                    this.UpdateSendIcon(this.InputText);

                    // Clean up file if it's a file source
                    if (imageToRemove is FileImageSource fileSource)
                    {
                        try
                        {
                            string filePath = fileSource.File;
                            if (File.Exists(filePath) && filePath.Contains(FileSystem.AppDataDirectory))
                            {
                                File.Delete(filePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Just log and continue if we can't delete the file
                            Console.WriteLine($"Error deleting image file: {ex.Message}");
                        }
                    }

                    // Provide haptic feedback if available
                    try
                    {
                        HapticFeedback.Perform(HapticFeedbackType.Click);
                    }
                    catch
                    {
                        // Ignore if haptic feedback is not available
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing image: {ex.Message}");
            }
        }
        private void ExecuteSendButtonCommand(object obj)
        {
            var text = this.InputText;
            if (this.isResponseStreaming)
            {
                this.CanStopResponse = true;
            }
            else
            {
                this.CanStopResponse = false;
            }
            if (string.IsNullOrWhiteSpace(text) && !HasImageUploaded)
            {
                return;
            }

            if (this.ExpandedEditorPopup != null && this.ExpandedEditorPopup.IsOpen)
            {
                this.ExpandedEditorPopup.Dismiss();
            }

            if (!HasImageUploaded)
            {
                var requestItem = new AssistItem()
                {
                    Text = text,
                    IsRequested = true
                };
                this.SendRequest(requestItem, obj);
            }
            else
            {
                foreach (var imageSource in this.ImageSourceCollection)
                {
                    var requestItem = new AssistImageItem()
                    {
                        Source = imageSource,
                        IsRequested = true,
                        Text = text
                    };
                    this.SendRequest(requestItem, obj);
                }
                this.ImageSourceCollection.Clear();
            }
        }

       
        private async void SendRequest(AssistItem requestItem, object obj)
        {
            if (this.isResponseStreaming)
            {
                return;
            }
            this.isResponseStreaming = true;
            this.Messages.Add(requestItem);
             this.InputText = string.Empty;
            await this.GetResult(requestItem, obj).ConfigureAwait(true);
        }

        internal async Task GetResult(object inputQuery, object assistView)
        { 
        }
        
        private void ExecuteEditorExpandCollapseCommand(object obj)
        {
            var editor = obj as CustomEditor;
            this.ExpandedEditorPopup = App.Current.Resources["EditorExpandPopup"] as SfPopup;
            this.ExpandedEditorPopup.BindingContext = this;
            if (!this.ExpandedEditorPopup.IsOpen)
            {
                this.ExpandedEditorPopup.Show(true);
                editor.Focus();
            }
            else
            {
                this.ExpandedEditorPopup.Dismiss();
            }
        }

        private void OnAttachmentContextMenuTapCommand(object eventArgs)
        {
            var chipText = eventArgs as Option;

            EditorOptions[0].IsOpen = false;
            ShowImagePicker();

        }

        private async void ShowImagePicker()
        {
#if ANDROID || WINDOWS || (IOS && !MACCATALYST)
            try
            {
                string filePath = Path.Combine(FileSystem.AppDataDirectory, "image" + ++imageNo + ".jpg");

                // Get and store the image from gallery - the method returns void, not bool
                await _imagePickerHelper.SaveImageAsync(filePath);

                // Check if the file exists after the operation to determine success
                if (File.Exists(filePath))
                {
                    var imageSource = ImageSource.FromFile(filePath);

                    // Add the image to the collection
                    this.ImageSourceCollection.Add(imageSource);

                    // Notify property changes
                    OnPropertyChanged(nameof(this.HasImageUploaded));

                    // Update the send icon status based on whether we have images or text
                    this.UpdateSendIcon(this.InputText);

                    // Provide haptic feedback if available
                    try
                    {
                        HapticFeedback.Perform(HapticFeedbackType.Click);
                    }
                    catch
                    {
                        // Ignore if haptic feedback is not available
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error picking image: {ex.Message}");
                // Could show a toast or notification to user here
            }
            //#elif MACCATALYST

            //            // TODO: The file picker is not supported in Mac Catalyst. So, we have used the custom file picker.
            //            // https://github.com/dotnet/maui/issues/11088
            //            var result = await NativeHelper.ImagePickAsync();
            //            if (string.IsNullOrEmpty(result))
            //            {
            //                return;
            //            }

            //            var imageSource = ImageSource.FromFile(result);            
            //#else
            await Task.Delay(1);
#endif
        }
        private void UpdateSendIcon(string input)
        {
            // Update the send icon appearance based on input status
            if (string.IsNullOrWhiteSpace(input) && !this.HasImageUploaded)
            {
                // No text and no images - default state
                //SendIconText = "\ue7E8" + " Voice"; // TODO:Need to add voice input support
                SendIconText = "\ue710";
                SendIconWidth = 40;

                // Only disable the icon if we're not streaming a response
                if (!this.isResponseStreaming)
                {
                    this.IsSendIconEnabled = false;
                }
            }
            else if (!isResponseStreaming)
            {
                // We have either text or images - enable send button
                SendIconText = "\ue710";
                SendIconWidth = 40;
                this.IsSendIconEnabled = true;

                // If we have images, make the send icon more prominent
                if (this.HasImageUploaded)
                {
                    // Could set a different icon or style if desired
                }
            }

            // Update property notifications
            OnPropertyChanged(nameof(SendIconText));
            OnPropertyChanged(nameof(SendIconWidth));
            OnPropertyChanged(nameof(IsSendIconEnabled));
        }
        private void ExecuteEditorOptionsCommand(object obj)
        {
            var chipText = obj as Option;

            if (chipText.Name == "Attachment")
            {
                chipText.IsOpen = true;
            }
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

            var response = await openAIService.GetAnswerFromGPT(prompt);

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
            var request_query = GetChartUserPrompt(text);

            if (imageSourceCollection != null && imageSourceCollection.Count > 0)
            {
                foreach (var imageSource in imageSourceCollection)
                {
                    if (imageSource is FileImageSource fileImageSource)
                    {
                        request_query += text + openAIService.AnalyzeImageAzureAsync(fileImageSource.File, string.Empty);
                    }
                }
            }

            string response = await openAIService.GetAnswerFromGPT(request_query, true);

            if (response != null)
            {
                oldJson = response;
                DecryptJSON(response, false);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
