using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SlideshowCreator
{
    /// <summary>
    /// Interaction logic for StatusbarControl.xaml
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
                    loading.Visibility = Visibility.Hidden;
                }
                else
                {
                    loading.Visibility = Visibility.Visible;
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

        public void AddLoadingProgress(string description, int maxValue)
        {
            LoadingMaxValue = maxValue;
            LoadingText = description;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
