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
    /// Interaction logic for WindowRestockLevel.xaml
    /// </summary>
    public partial class WindowRestockLevel : Window
    {
        public WindowRestockLevel()
        {
            InitializeComponent();
            listBoxStockList.DataContext = Calculation.ListOfStock();
            listBoxStockList.DisplayMemberPath = "StockName";
            labelNoSetting.Visibility = Visibility.Hidden;
        }

        private void listBoxStockList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StorageStock tmpss = listBoxStockList.SelectedItem as StorageStock;
            int iRestockLevel = Calculation.GetRestockLevel(tmpss.IDStock);

            if (iRestockLevel.Equals(-1))
            {
                textBoxQty.Clear();
                labelNoSetting.Visibility = Visibility.Visible;
            }
            else
            {
                labelNoSetting.Visibility = Visibility.Hidden;
                textBoxQty.Text = iRestockLevel.ToString();
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void textBoxQty_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                char c = e.Text.ToCharArray().First();
                e.Handled = !(char.IsNumber(c) || char.IsControl(c));
            }
            catch { } // do not need report error
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listBoxStockList.SelectedIndex.Equals(-1)) { throw new Exception("Nothing is selected."); }
                if (string.IsNullOrEmpty(textBoxQty.Text)) { throw new Exception("Nothing is entered."); }
                StorageStock tmpss = listBoxStockList.SelectedItem as StorageStock;
                if (Calculation.SaveRestockLevel(tmpss.IDStock, Convert.ToInt32(textBoxQty.Text)).Equals(true))
                {
                    MessageBox.Show("Update successfully");
                }
                else
                {
                    MessageBox.Show("Update fail, please retry");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
