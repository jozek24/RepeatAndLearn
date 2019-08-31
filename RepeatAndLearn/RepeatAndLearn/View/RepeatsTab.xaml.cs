using MahApps.Metro.Controls;
using RepeatAndLearn.ViewModel;
using System.Windows.Controls;

namespace RepeatAndLearn.View
{
    /// <summary>
    /// Interaction logic for RepeansTab.xaml
    /// </summary>
    public partial class RepeatsTab
    {
        public RepeatsTab()
        {
            InitializeComponent();
            this.DataContext = new RepeatsVM();
        }
    }
}
