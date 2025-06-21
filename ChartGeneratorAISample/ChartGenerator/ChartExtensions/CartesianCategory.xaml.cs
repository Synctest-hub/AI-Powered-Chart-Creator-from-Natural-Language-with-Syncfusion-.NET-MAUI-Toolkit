using Syncfusion.Maui.Toolkit.Charts;
using System.Collections.ObjectModel;

namespace ChartGenerator;

public partial class CartesianCategory : SfCartesianChart
{
    public CartesianCategory()
    {
        InitializeComponent();
    }

    // BindableProperty for the series source
    public static readonly BindableProperty SourceProperty = BindableProperty.Create(
        nameof(Source),
        typeof(ObservableCollection<SeriesConfig>),
        typeof(CartesianCategory),
        null,
        BindingMode.Default,
        propertyChanged: OnPropertyChanged);

    public ObservableCollection<SeriesConfig> Source
    {
        get => (ObservableCollection<SeriesConfig>)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly BindableProperty XAxisProperty = BindableProperty.Create(
        nameof(XAxis),
        typeof(ObservableCollection<AxisConfig>),
        typeof(CartesianCategory),
        null,
        BindingMode.Default,
        propertyChanged: XAxisChanged);

    public ObservableCollection<SeriesConfig> XAxis
    {
        get => (ObservableCollection<SeriesConfig>)GetValue(XAxisProperty);
        set => SetValue(XAxisProperty, value);
    }

    public static readonly BindableProperty YAxisProperty = BindableProperty.Create(
        nameof(YAxis),
        typeof(ObservableCollection<AxisConfig>),
        typeof(CartesianCategory),
         null,
        BindingMode.Default,
        propertyChanged: YAxisChanged);

    public ObservableCollection<SeriesConfig> YAxis
    {
        get => (ObservableCollection<SeriesConfig>)GetValue(YAxisProperty);
        set => SetValue(YAxisProperty, value);
    }

    private static void XAxisChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SfCartesianChart cartesianChart && newValue is ObservableCollection<AxisConfig> xAxisConfigs)
        {
            // Clear current axes
            cartesianChart.XAxes.Clear();

            // Add new axes based on the new configuration
            foreach (var axisConfig in xAxisConfigs)
            {
                ChartAxis axis = axisConfig.GetXAxis();

                if (axis is CategoryAxis category)
                {
                    category.LabelRotation = 45;
                }

                axis.AxisLineStyle = SetLineStyle();
                axis.MajorGridLineStyle = SetLineStyle();
                axis.MajorTickStyle = SetTickStyle();
                axis.ShowMajorGridLines = false;
                cartesianChart.XAxes.Add(axis);
            }
        }
    }

    private static void YAxisChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SfCartesianChart cartesianChart && newValue is ObservableCollection<AxisConfig> yAxisConfigs)
        {
            cartesianChart.YAxes.Clear();

            foreach (var axisConfig in yAxisConfigs)
            {
                var axis = axisConfig.GetYAxis();
                if (axis != null)
                {
                    axis.AxisLineStyle = SetLineStyle();
                    axis.MajorGridLineStyle = SetLineStyle();
                    axis.MajorTickStyle = SetTickStyle();
                    axis.ShowMajorGridLines = false;
                    cartesianChart.YAxes.Add(axis);
                }
            }
        }
    }

    private static ChartLineStyle SetLineStyle()
    {
        ChartLineStyle axisLineStyle = new ChartLineStyle()
        {
            Stroke = Colors.Transparent,
            StrokeWidth = 0,
        };

        return axisLineStyle;
    }

    private static ChartAxisTickStyle SetTickStyle()
    {
        ChartAxisTickStyle tickStyle = new ChartAxisTickStyle()
        {
            TickSize = 0
        };

        return tickStyle;
    }

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CartesianCategory chart)
        {
            chart.GenerateSeries(newValue as ObservableCollection<SeriesConfig>);
        }
    }

    private void GenerateSeries(ObservableCollection<SeriesConfig> configs)
    {
        Series.Clear();
        if (configs != null)
        {
            foreach (var config in configs)
            {
                CreateSeriesFromTemplate(config);
                var paletteBrush = GetPaletteBrushes();

                if (Series.Count == 1 && Series[0] is ColumnSeries series)
                {
                    series.PaletteBrushes = paletteBrush;
                }
                else
                {
                    this.PaletteBrushes = paletteBrush;
                }
            }
        }
    }

    private Brush[] GetPaletteBrushes()
    {
        var random = new Random();
        switch (random.Next(1, 6))
        {
            case 1:
                return Resources["Pallet1"] as Brush[];
            case 2:
                return Resources["Pallet2"] as Brush[];
            case 3:
                return Resources["Pallet3"] as Brush[];
            case 5:
                return Resources["Pallet5"] as Brush[];
            default:
                return Resources["Pallet6"] as Brush[];
        }
    }

    private void CreateSeriesFromTemplate(SeriesConfig seriesConfig)
    {
        var templateSelector = (SeriesTemplateSelector)Resources["seriesTemplateSelector"];
        var template = templateSelector.SelectTemplate(seriesConfig, null);

        if (template != null)
        {
            ChartSeries series = (ChartSeries)template.CreateContent();

            if (series != null)
            {
                series.BindingContext = seriesConfig;
                this.Series.Add(series);
            }
        }
    }
}
