using System.ComponentModel;

namespace ChartGenerator
{
    public class Option : INotifyPropertyChanged
    {
        #region Private Fields

        private bool isEnabled;
        private bool isOpen;

        #endregion

        #region Public APIs

        public string Name { get; set; }
        public string Icon { get; set; }
        public bool CanShowSeparator { get; set; }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                RaisePropertyChanged(nameof(IsEnabled));
            }
        }
        public bool IsOpen
        {
            get { return isOpen; }
            set
            {
                isOpen = value;
                RaisePropertyChanged(nameof(IsOpen));
            }
        }

        #endregion

        #region Constructor
        public Option()
        {
            IsEnabled = true;
        }

        #endregion

        #region Property Changed

        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Occurs when property is changed.
        /// </summary>
        /// <param name="propName">changed property name</param>
        public void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        #endregion
    }
}
