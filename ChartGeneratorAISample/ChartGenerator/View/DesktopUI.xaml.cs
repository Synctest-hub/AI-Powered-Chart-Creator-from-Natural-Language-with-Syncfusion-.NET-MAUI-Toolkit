using Syncfusion.Maui.Core;

namespace ChartGenerator;

public partial class DesktopUI : ContentPage
{
    public DesktopUI()
    {
        InitializeComponent();
    }
    private void OnChipClicked(object sender, EventArgs e)
    {
        var viewmodel = this.BindingContext as ChatViewModel;
        var chip = (sender as SfChip);
        var layout = chip.Children[0] as HorizontalStackLayout;

        Option option;
        if (layout != null)
        {
            option = layout.BindingContext as Option;
        }
        else
        {
            var label = chip.Children[0] as Grid;
            option = label.BindingContext as Option;
        }

        if (string.IsNullOrEmpty(option.Name) || !option.IsEnabled)
            return;

        viewmodel.EditorOptionsComamnd.Execute(option);
    }
}