using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for the Statusbar
    /// </summary>
    public partial class StatusbarControl : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private string _loadingText = "";
        private string _savingPath = "Not saved!";
        private int _loadingValue;
        private int _loadingMaxValue = 100;

        public StatusbarControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string LoadingText
        {
            get { return _loadingText; }
            set
            {
                _loadingText = value;
                OnPropertyChanged();
                if (value == null || value.Equals(""))
                {
                    Loading.Visibility = Visibility.Hidden;
                }
                else
                {
                    Loading.Visibility = Visibility.Visible;
                }
            }
        }

        public int LoadingValue
        {
            get { return _loadingValue; }
            set
            {
                _loadingValue = value;
                OnPropertyChanged();
            }
        }

        public int LoadingMaxValue
        {
            get { return _loadingMaxValue; }
            set
            {
                _loadingMaxValue = value;
                OnPropertyChanged();
            }
        }

        public string SavingPath
        {
            get { return _savingPath; }
            set
            {
                _savingPath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Setting new progress to the status
        /// </summary>
        /// <param name="description">describing loading text</param>
        /// <param name="maxValue">max progress value</param>
        public void AddLoadingProgress(string description, int maxValue)
        {
            LoadingMaxValue = maxValue;
            LoadingText = description;
        }

        /// <summary>
        /// Property change for binding
        /// </summary>
        /// <param name="name">name of the property</param>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
