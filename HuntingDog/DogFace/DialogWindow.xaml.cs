using System.Windows;
using HuntingDog.Config;

namespace HuntingDog.DogFace {
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window {
        public DialogWindow() {
            InitializeComponent();

        }

        public void ShowConfiguration(DogConfig cfg) {
            DogConfig = cfg.CloneMe();
            _propertyGrid.SelectedObject = DogConfig;
            _propertyGrid.ShowSearchBox = false;
            _propertyGrid.ShowSortOptions = false;
            _propertyGrid.ShowTitle = false;
        }

        public DogConfig DogConfig {
            get;
            private set;
        }


        private void Button_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
