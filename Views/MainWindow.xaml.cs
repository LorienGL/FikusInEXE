using FikusIn.Model.Documents;
using FikusIn.ViewModel;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FikusIn.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var mainViewModel = new MainViewModel();
            DataContext = mainViewModel;
            messageLabel.DataContext = mainViewModel.MessagesViewModel;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Normal)
                WindowState = System.Windows.WindowState.Maximized;
            else
                WindowState = System.Windows.WindowState.Normal;
        }

        private void popupDocumentListButton_Click(object sender, RoutedEventArgs e)
        {
            popupDocumentList.IsOpen = false;
            //Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));

            for (int i = 0; i < tabsItemsControl.Items.Count; i++)
            {
                UIElement tabButtonUIE = (UIElement)tabsItemsControl.ItemContainerGenerator.ContainerFromIndex(i);

                if(sender is Button popupButton && popupButton.CommandParameter != null 
                    && tabButtonUIE is ContentPresenter tabButton && tabButton.Content != null
                    && ((Document)popupButton.CommandParameter).Id == ((Document)tabButton.Content).Id) 
                {
                    tabButton.BringIntoView();

                    return;
                }
            }
        }

        private void popupSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            popupSettings.IsOpen = false;
        }

        private void popupSaveButton_Click(object sender, RoutedEventArgs e)
        {
            popupSave.IsOpen = false;
        }

        private void wMain_Loaded(object sender, RoutedEventArgs e)
        {
            DocumentManager.NewDocument(((MainViewModel)DataContext).WindowScale);
        }
    }
}