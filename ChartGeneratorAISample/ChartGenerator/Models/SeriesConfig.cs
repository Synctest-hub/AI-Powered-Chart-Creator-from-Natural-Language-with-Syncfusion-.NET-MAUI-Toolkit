using Syncfusion.Maui.Toolkit.Charts;
using System.Collections.ObjectModel;

namespace ChartGenerator;


public class SeriesConfig
{
    public SeriesType Type
    {
        get;
        set;
    }

    public string Name
    {
        get;
        set;
    }

    public ObservableCollection<DataModel> DataSource
    {
        get;
        set;
    }

    public bool Tooltip
    {
        get;
        set;
    }
    public string? XPath { get; internal set; }
}

public class AxisConfig
{
    public string Title { get; set; }
    public string Type { get; set; } = "Numerical"; // default to Numerical
    public double? Minimum { get; set; }
    public double? Maximum { get; set; }

    public ChartAxis GetXAxis()
    {
        var title = new ChartAxisTitle() { Text = Title };
        return Type.ToLower() switch
        {

            "datetime" => new DateTimeAxis { Title = title },
            "category" => new CategoryAxis { Title = title },
            "log" or "logarithmic" => new LogarithmicAxis { Title = title },
            "numerical" or "linear" => new NumericalAxis { Title = title },
            _ => new NumericalAxis { Title = title }
        };
    }
    public ChartAxis GetAxis()
    {
        var title = new ChartAxisTitle() { Text = Title };
        switch (Type?.ToLower())
        {
            case "category":
                return new CategoryAxis { Title = title };
            case "numerical":
                return new NumericalAxis { Title = title };
            case "datetime":
                return new DateTimeAxis { Title = title };
            case "logarithmic":
                return new LogarithmicAxis { Title = title };
            default:
                return new NumericalAxis(); // fallback
        }
    }

    public RangeAxisBase? GetYAxis()
    {
        var title = new ChartAxisTitle() { Text = Title };
        return Type.ToLower() switch
        {
            "datetime" => new DateTimeAxis { Title = title },
            "log" or "logarithmic" => new LogarithmicAxis { Title = title },
            "numerical" or "linear" => new NumericalAxis { Title = title },
            _ => null // CategoryAxis is not valid for YAxes (not RangeAxisBase)
        };
    }

}


public class DataModel
{
    public object xvalue
    {
        get;
        set;
    }

    public double yvalue
    {
        get;
        set;
    }

    public DateTime? date
    {
        get;
        set;
    }

    public double? xval
    {
        get;
        set;
    }

    public string Category
    {
        get;
        set;
    }

    public double Value
    {
        get;
        set;
    }
}