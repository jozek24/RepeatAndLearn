using MahApps.Metro.Controls;
using RepeatAndLearn.ViewModel;

namespace RepeatAndLearn.View
{
    /// <summary>
    /// Interaction logic for MyWordsTab.xaml
    /// </summary>
    public partial class MyWordsTab
    {
        public MyWordsTab()
        {
            InitializeComponent();
            this.DataContext = new MyWordsVM();
        }
    }
}
