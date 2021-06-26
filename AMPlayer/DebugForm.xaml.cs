using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AMPlayer
{
    /// <summary>
    /// Interaction logic for DebugForm.xaml
    /// </summary>
    public partial class DebugForm : Window
    {
        private bool close = false;

        public DebugForm()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!close)
                e.Cancel = true;
        }

        public void CloseForm()
        {
            close = true;
            Close();
        }

        private void debugLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            debugLog.ScrollToEnd();
        }
    }
}
