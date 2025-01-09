
namespace ChartGenerator
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
#if WINDOWS || MACCATALYST
            return new Window(new NavigationPage(new DesktopUI()));
# else
            return new Window(new NavigationPage(new MobileUI()));
#endif
        }
    }
}