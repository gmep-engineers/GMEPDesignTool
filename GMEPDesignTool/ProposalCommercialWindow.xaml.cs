﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GMEPDesignTool
{
    /// <summary>
    /// Interaction logic for ProposalWindow.xaml
    /// </summary>
    public partial class ProposalCommercialWindow : Window
    {
        ProposalCommercialViewModel ProposalCommercialViewModel { get; set; }

        public ProposalCommercialWindow()
        {
            //InitializeComponent();
            ProposalCommercialViewModel = new ProposalCommercialViewModel();
            this.DataContext = ProposalCommercialViewModel;
        }
    }
}
