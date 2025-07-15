using System.Windows;

namespace GMEPDesignTool
{
    public partial class ProposalCommercialWindow : Window
    {
        public ProposalCommercialWindow()
        {
            InitializeComponent();
            this.DataContext = new ProposalCommercialViewModel();
        }

        public ProposalCommercialWindow(ProposalCommercialViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}
