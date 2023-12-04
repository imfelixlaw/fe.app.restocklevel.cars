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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InHouseReporting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<StorageCentre> lsc = Calculation.ListOfCentre();
        List<StorageStock> lss = Calculation.ListOfStock();
        List<StorageStock> sellss = new List<StorageStock>();
        public MainWindow()
        {
            InitializeComponent();
            Initializing();
        }

        private void Initializing()
        {
            try
            {
                comboBoxCentre.DataContext = lsc;
                comboBoxCentre.DisplayMemberPath = "CentreName";
                comboBoxStock.DataContext = lss;
                comboBoxStock.DisplayMemberPath = "StockName";
                comboBoxRptStock.DataContext = lss;
                comboBoxRptStock.DisplayMemberPath = "StockName";
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonInsert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (datePickerSelectedDate.SelectedDate.Equals(null)) { throw new Exception("No Date is select"); }
                if (comboBoxStock.SelectedIndex.Equals(-1)) { throw new Exception("No Stock is select"); }
                if (radioButtonIn.IsChecked.Equals(false) && radioButtonOut.IsChecked.Equals(false)) { throw new Exception("Please select either In or Out"); }
                if (comboBoxCentre.SelectedIndex.Equals(-1) && radioButtonOut.IsChecked.Equals(true)) { throw new Exception("No Centre is select"); }
                if (string.IsNullOrEmpty(textBoxQty.Text) || Convert.ToInt32(textBoxQty.Text).Equals(0)) { throw new Exception("Please enter a quantity value not equal to 0 (zero)"); }
                DateTime dtdate = (DateTime)datePickerSelectedDate.SelectedDate;
                string IO = "I";
                if (radioButtonIn.IsChecked.Equals(true))
                {
                    IO = "I";
                }
                if (radioButtonOut.IsChecked.Equals(true))
                {
                    IO = "O";
                }
                int IDCentre = 0; // if nothing wrong
                if (radioButtonOut.IsChecked.Equals(true))
                {
                    StorageCentre SelectedCentre = comboBoxCentre.SelectedItem as StorageCentre;
                    IDCentre = SelectedCentre.IDCentre;
                }
                StorageStock SelectedStock = comboBoxStock.SelectedItem as StorageStock;
                Calculation.UpdateInOut(IO, dtdate.ToString("yyyy-MM-dd"), IDCentre, SelectedStock.IDStock, Convert.ToInt32(textBoxQty.Text));
                int iRestockLevel = Calculation.GetRestockLevel(SelectedStock.IDStock);
                if (iRestockLevel.Equals(-1))
                {
                    MessageBox.Show("The selected stock do not have restocking level");
                }
                else if (iRestockLevel.Equals(0))
                {
                    MessageBox.Show("The selected stock have 0 restocking quantity level");
                }
                else
                {
                    int iSumOut = Calculation.GetStockOutSum(SelectedStock.IDStock),
                        iSumIn = Calculation.GetStockInSum(SelectedStock.IDStock),
                        iSumBalance = iSumIn - iSumOut;
                    //MessageBox.Show("i" + iSumIn.ToString());
                    //MessageBox.Show("o" + iSumOut.ToString());
                    //MessageBox.Show("b" + iSumBalance.ToString());
                    //MessageBox.Show("r" + GetRestockLevel.ToString());
                    if (iSumBalance < iRestockLevel)
                    {
                        MessageBox.Show("The selected stock have reached restocking level");
                    }
                    else
                    {
                        MessageBox.Show("Operation complete");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

        private void radioButtonIn_Checked(object sender, RoutedEventArgs e)
        {
            comboBoxCentre.IsEnabled = false;
            textBlockCentre.IsEnabled = false;
        }

        private void radioButtonOut_Checked(object sender, RoutedEventArgs e)
        {
            comboBoxCentre.IsEnabled = true;
            textBlockCentre.IsEnabled = true;
        }

        private void buttonViewReport_Click(object sender, RoutedEventArgs e)
        {
            Excel._Application appExcel = new Excel.Application(); // creating Excel Application
            try
            {
                if (datePickerRptFrom.SelectedDate.Equals(null) || datePickerRptTo.SelectedDate.Equals(null)) { throw new Exception("No Date is select"); }
                if (datePickerRptFrom.SelectedDate > datePickerRptTo.SelectedDate) { throw new Exception("Checking the date is selected, not accepted format"); }
                //if (comboBoxRptStock.SelectedIndex.Equals(-1)) { throw new Exception("Select a stock to view"); }
                if (sellss.Count.Equals(0)) { throw new Exception("Select a stock to view"); }
                string FileName = Calculation.SaveFilename();
                if (string.IsNullOrEmpty(FileName)) { throw new Exception("File name cannot be empty"); }
                int ExcelRow = 0;
                Excel._Workbook workbook = appExcel.Workbooks.Add(Type.Missing); // creating new WorkBook within Excel application
                Excel._Worksheet worksheet = null; // creating new Excelsheet in workbook
                appExcel.Visible = false; // Hiding Excel
                // get the reference of first sheet.
                worksheet = (Excel.Worksheet)workbook.Sheets["Sheet1"]; //  By default its name is Sheet1.
                worksheet = (Excel.Worksheet)workbook.ActiveSheet; // Active this Sheet
                worksheet.Name = "Stock In Out Report"; // changing the name of active sheet
                ExcelRow++;
                DateTime dtStartdate = (DateTime)datePickerRptFrom.SelectedDate, dtEnddate = (DateTime)datePickerRptTo.SelectedDate;
                worksheet.Cells[ExcelRow, 1].EntireRow.Font.Size = "18"; // font size
                worksheet.Cells[ExcelRow++, 1].Value = "Comprehensive Auto Restoration Service S/B";
                worksheet.Cells[ExcelRow, 1].EntireRow.Font.Size = "12"; // font size
                worksheet.Cells[ExcelRow++, 1].Value = "Stock In Out Report For " + dtStartdate.ToString("dd-MM-yyyy") + " to " + dtEnddate.ToString("dd-MM-yyyy");
                ExcelRow++;

                foreach (StorageStock tmpss in sellss)
                {

                    //StorageStock tmpss = comboBoxRptStock.SelectedItem as StorageStock;
                    worksheet.Cells[ExcelRow, 1].EntireRow.Font.Size = "12"; // font size
                    worksheet.Cells[ExcelRow++, 1].Value = "Item : " + tmpss.StockName;
                    // Get Restock Level
                    int iRestockLevel = Calculation.GetRestockLevel(tmpss.IDStock);
                    if (iRestockLevel.Equals(-1))
                    {
                        worksheet.Cells[ExcelRow++, 5].Value = "Restock Level : No Set";
                    }
                    else if (iRestockLevel.Equals(0))
                    {
                        worksheet.Cells[ExcelRow++, 5].Value = "Restock Level : 0";
                    }
                    else
                    {
                        worksheet.Cells[ExcelRow++, 5].Value = "Restock Level : " + iRestockLevel;
                    }
                    // End Get Restock Level
                    // Get Last Balance
                    int LastBalance = Calculation.GetStockInSum(tmpss.IDStock, dtStartdate.ToString("yyyy-MM-dd")) - Calculation.GetStockOutSum(tmpss.IDStock, dtStartdate.ToString("yyyy-MM-dd"));
                    int tmpBal = LastBalance;
                    // End Get Last Balance
                    // Print Header
                    ExcelRow += 2;
                    worksheet.Cells[ExcelRow, 1].EntireRow.Font.Bold = true; // Make Font Bold
                    worksheet.Cells[ExcelRow, 1].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    worksheet.Cells[ExcelRow, 2].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    worksheet.Cells[ExcelRow, 3].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    worksheet.Cells[ExcelRow, 4].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    worksheet.Cells[ExcelRow, 5].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    ((Excel.Range)worksheet.Columns["A", Type.Missing]).ColumnWidth = 11;
                    ((Excel.Range)worksheet.Columns["B", Type.Missing]).ColumnWidth = 13;
                    ((Excel.Range)worksheet.Columns["C", Type.Missing]).ColumnWidth = 13;
                    ((Excel.Range)worksheet.Columns["D", Type.Missing]).ColumnWidth = 18;
                    ((Excel.Range)worksheet.Columns["E", Type.Missing]).ColumnWidth = 20;
                    worksheet.Cells[ExcelRow, 1] = "Date";
                    worksheet.Cells[ExcelRow, 2] = "In";
                    worksheet.Cells[ExcelRow, 3] = "Out";
                    worksheet.Cells[ExcelRow, 4] = "Centre";
                    worksheet.Cells[ExcelRow, 5] = "Balance";
                    // End Header
                    ExcelRow++;
                    // Print Last Balance
                    if (LastBalance > 0)
                    {
                        //worksheet.Cells[ExcelRow, 1] = dtStartdate.ToString("dd-MM-yyyy");
                        worksheet.Cells[ExcelRow, 4] = "Last Balance";
                        worksheet.Cells[ExcelRow, 5] = LastBalance;
                        ExcelRow++;
                    }
                    // End Print Last Balance
                    foreach (StorageListOfData lod in Calculation.ListOfData(dtStartdate.ToString("yyyy-MM-dd"), dtEnddate.ToString("yyyy-MM-dd"), tmpss.IDStock))
                    {
                        worksheet.Cells[ExcelRow, 1] = lod.Date;
                        if (lod.Type.Equals("I"))
                        {
                            worksheet.Cells[ExcelRow, 2] = lod.Qty;
                            tmpBal += lod.Qty;
                            worksheet.Cells[ExcelRow, 5] = tmpBal;
                        }
                        else if (lod.Type.Equals("O"))
                        {
                            worksheet.Cells[ExcelRow, 3] = lod.Qty;
                            StorageCentre Centre = (from q in lsc
                                                    where q.IDCentre.Equals(lod.IDCentre)
                                                    select q).SingleOrDefault();
                            worksheet.Cells[ExcelRow, 4] = Centre.CentreName;
                            tmpBal -= lod.Qty;
                            worksheet.Cells[ExcelRow, 5] = tmpBal;
                        }
                        ExcelRow++;
                    }
                    if (tmpBal < iRestockLevel)
                    {
                        ExcelRow += 3;
                        worksheet.Cells[ExcelRow, 3] = "Please Re-stock this stock";
                    }
                    ExcelRow += 3;
                }
                // Save File
                workbook.SaveAs(FileName, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                // End Save File
                MessageBox.Show("Excel file is successfully created");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                appExcel.Quit();
            }
        }

        private void buttonReorderLevel_Click(object sender, RoutedEventArgs e)
        {
            WindowRestockLevel wrl = new WindowRestockLevel();
            wrl.Owner = this;
            wrl.ShowDialog();
        }

        private void buttonRawData_Click(object sender, RoutedEventArgs e)
        {
            WindowRawData wrd = new WindowRawData();
            wrd.Owner = this;
            wrd.ShowDialog();
        }

        private void UpdateListBox()
        {
            listBoxOutputList.DataContext = sellss;
            listBoxOutputList.DisplayMemberPath = "StockName";
            listBoxOutputList.Items.Refresh();
            int selectIndex = comboBoxRptStock.SelectedIndex;
            List<StorageStock> t = new List<StorageStock>();
            t.Clear();
            foreach (StorageStock diff in lss.Except(sellss)) // for each all item in the default list and it not include in the selected list then add to all centre list
            {
                t.Add(diff);
            }
            comboBoxRptStock.DataContext = t;
            comboBoxRptStock.DisplayMemberPath = "StockName";
            comboBoxRptStock.Items.Refresh();
            comboBoxRptStock.SelectedIndex = selectIndex;
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboBoxRptStock.SelectedIndex.Equals(-1))
                {
                    throw new Exception("Please select a stock to proceed.");
                }
                sellss.Add(comboBoxRptStock.SelectedItem as StorageStock);
                UpdateListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listBoxOutputList.SelectedIndex.Equals(-1))
                {
                    throw new Exception("Please select a stock to proceed.");
                }
                StorageStock p = listBoxOutputList.SelectedItem as StorageStock;
                sellss = (from k in sellss
                          where k.IDStock != p.IDStock
                          select k).ToList();
                UpdateListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
