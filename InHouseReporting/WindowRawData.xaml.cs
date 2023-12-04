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
using System.Windows.Shapes;

namespace InHouseReporting
{
    /// <summary>
    /// Interaction logic for WindowRawData.xaml
    /// </summary>
    public partial class WindowRawData : Window
    {
        public WindowRawData()
        {
            InitializeComponent();
            LoadList();
        }
        
        private void LoadList()
        {
            try
            {
                dataGridResult.ItemsSource = Calculation.ListOfDataDetails();
                dataGridResult.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StorageListOfDataDetails sldd = dataGridResult.SelectedItem as StorageListOfDataDetails;
                if (MessageBox.Show("This delete the selected data, are you sure to proceed?", "Confirmation", MessageBoxButton.YesNo).Equals(MessageBoxResult.Yes))
                {
                    if (Calculation.RemoveRawData(sldd.IDInOut).Equals(true))
                    {
                        MessageBox.Show("Update successfully");
                    }
                    else
                    {
                        throw new Exception("Update fail");
                    }
                    LoadList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
