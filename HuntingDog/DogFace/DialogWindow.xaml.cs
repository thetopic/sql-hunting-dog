using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HuntingDog.Config;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid;
using HuntingDog;


namespace HuntingDog.DogFace
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();

        }

        public void ShowConfiguration(DogConfig cfg)
        {
            DogConfig = cfg.CloneMe();
            _propertyGrid.SelectedObject = DogConfig;
            _propertyGrid.ShowSearchBox = false;
            _propertyGrid.ShowSortOptions = false;
            _propertyGrid.ShowTitle = false;
        }

        public DogConfig DogConfig
        {
            get;
            private set;
        }


        private bool _refreshingGrid;

        private void OnPropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            if (DogConfig == null || _refreshingGrid) return;
            Loc.Load(DogConfig.Language);
            // Force the PropertyGrid to re-read DisplayName/Description from the new language.
            _refreshingGrid = true;
            try
            {
                var cfg = DogConfig;
                _propertyGrid.SelectedObject = null;
                _propertyGrid.SelectedObject = cfg;
            }
            finally
            {
                _refreshingGrid = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
