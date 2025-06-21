
namespace ChartGenerator
{
    public class SeriesTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LineSeriesTemplate { get; set; }
        public DataTemplate SplineSeriesTemplate { get; set; }
        public DataTemplate ColumnSeriesTemplate { get; set; }
        public DataTemplate AreaSeriesTemplate { get; set; }
        public DataTemplate PieSeriesTemplate { get; set; }
        public DataTemplate DoughnutSeriesTemplate { get; set; }
        public DataTemplate RadialBarSeriesTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is SeriesConfig config)
            {
                switch (config.Type)
                {
                    case SeriesType.Line:
                        return LineSeriesTemplate;
                    case SeriesType.Spline:
                        return SplineSeriesTemplate;
                    case SeriesType.Column:
                        return ColumnSeriesTemplate;
                    case SeriesType.Area:
                        return AreaSeriesTemplate;
                    case SeriesType.Pie:
                        return PieSeriesTemplate;
                    case SeriesType.Doughnut:
                        return DoughnutSeriesTemplate;
                    case SeriesType.RadialBar:
                        return RadialBarSeriesTemplate;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return null;
        }
    }
}
