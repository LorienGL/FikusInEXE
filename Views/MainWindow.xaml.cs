using FikusIn.Model.Documents;
using FikusIn.ViewModel;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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
        Model.GraphicEngine.GraphicEngine gfxEngine;

        public MainWindow()
        {
            InitializeComponent();

            //pnlSubMenu.Visibility = Visibility.Collapsed;

            var mainViewModel = new MainViewModel();
            DataContext = mainViewModel;
            //messageLabel.DataContext = mainViewModel.MessagesViewModel;

            //gfxEngine = new Model.GraphicEngine.GraphicEngine(v3dMain, v3dCamera, [v3dLightTop, v3dLightRight, v3dLightLeft]);

            //gfxEngine.PaintCube(10);
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

        bool firstRender = true;
        private void wMain_ContentRendered(object sender, EventArgs e)
        {
            if (firstRender)
            {
                firstRender = false;

                double w = ActualWidth - 17; // 17 is the size of the border or something... but uless we substract it, the window is too big
                double h = ActualHeight - 16; // 16 is the size of the title bar+border

                //WindowStyle = WindowStyle.None;
                //ResizeMode = ResizeMode.CanResize;
                //WindowState = WindowState.Normal;
                //Height = h;
                //Width = w / 2.0;
                //Top = 0;
                //Left = w / 2.0;
                //btnMaximize.Visibility = Visibility.Collapsed;
            }
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
    }
}