using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Windows;
using Microsoft.Win32;

namespace InHouseReporting
{
    public static class myConnStr
    {
        public static string Setting // create MySQL Setting
        {
            get
            {
                return string.Format(@"SERVER={0};
                    DATABASE={1};
                    UID={2};
                    PASSWORD={3};
                    respect binary flags=false; Compress=true; Pooling=true; Min Pool Size=0; Max Pool Size=100; Connection Lifetime=0",
                        Properties.Settings.Default.myhost,
                        Properties.Settings.Default.mytable,
                        Properties.Settings.Default.myuser,
                        Properties.Settings.Default.mypass);
            }
        }
    }

    public static class Calculation
    {
        public static int GetStockInSum(int IDStock, string DateBefore = "")
        {
            int iQtySum = 0;
            try
            {
                using (MySqlConnection myConn = new MySqlConnection(myConnStr.Setting))
                {
                    myConn.Open();
                    if (string.IsNullOrEmpty(DateBefore).Equals(false))
                    {
                        DateBefore = string.Format(" AND `InOutDate` < '{0}'", DateBefore);
                    }
                    string sql = string.Format(@"SELECT IFNULL(SUM(`Qty`), 0)
                        FROM `cars_inhouse_stock_inout`
                        WHERE `InOutType` = 'I' AND `Status` = 'Y'
                        AND `FKIDStock` = {0} {1};", IDStock, DateBefore);
                    //MessageBox.Show(sql);
                    using (MySqlCommand myCmd = new MySqlCommand(sql, myConn))
                    {
                        using (MySqlDataReader myDr = myCmd.ExecuteReader())
                        {
                            if (myDr.Read())
                            {
                                iQtySum = myDr.GetInt32(0);
                            }
                        }
                    }
                    myConn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return iQtySum;
        }

        public static int GetStockOutSum(int IDStock, string DateBefore = "")
        {
            int iQtySum = 0;
            try
            {
                using (MySqlConnection myConn = new MySqlConnection(myConnStr.Setting))
                {
                    myConn.Open();
                    if (string.IsNullOrEmpty(DateBefore).Equals(false))
                    {
                        DateBefore = string.Format(" AND `InOutDate` < '{0}'", DateBefore);
                    }
                    string sql = string.Format(@"SELECT IFNULL(SUM(`Qty`), 0)
                        FROM `cars_inhouse_stock_inout`
                        WHERE `InOutType` = 'O' AND `Status` = 'Y'
                        AND `FKIDStock` = {0} {1};", IDStock, DateBefore);
                    //MessageBox.Show(sql); 
                    using (MySqlCommand myCmd = new MySqlCommand(sql, myConn))
                    {
                        using (MySqlDataReader myDr = myCmd.ExecuteReader())
                        {
                            if (myDr.Read())
                            {
                                iQtySum = myDr.GetInt32(0);
                            }
                        }
                    }
                    myConn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return iQtySum;
        }

        public static bool SaveRestockLevel(int IDStock, int Qty)
        {
            try
            {
                int result = -1;
                using (MySqlConnection myConn = new MySqlConnection(myConnStr.Setting))
                {
                    myConn.Open();
                    string sql = string.Format(@"REPLACE INTO `cars_inhouse_stock_reorder` VALUES({0}, {1}, 'Y');", IDStock, Qty);
                    using (MySqlCommand myCmd = new MySqlCommand(sql, myConn))
                    {
                        result = myCmd.ExecuteNonQuery();
                    }
                    myConn.Close();
                }
                if (result > 0) { return true; } else { return false; }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static int GetRestockLevel(int IDStock)
        {
            int iRestockLevel = 0;
            try
            {
                using (MySqlConnection myConn = new MySqlConnection(myConnStr.Setting))
                {
                    myConn.Open();
                    string sql = string.Format(@"SELECT `ReorderLevel` FROM `cars_inhouse_stock_reorder` WHERE `Status` = 'Y' AND `FKIDStock` = {0};", IDStock);
                    using (MySqlCommand myCmd = new MySqlCommand(sql, myConn))
                    {
                        using (MySqlDataReader myDr = myCmd.ExecuteReader())
                        {
                            if (myDr.Read())
                            {
                                iRestockLevel = myDr.GetInt32(0);
                            }
                            else
                            {
                                iRestockLevel = -1;
                            }
                        }
                    }
                    myConn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return iRestockLevel;
        }

        public static bool UpdateInOut(string InOut, string InOutDate, int IDCentre, int IDStock, int Qty)
        {
            try
            {
                using (MySqlConnection myConn = new MySqlConnection(myConnStr.Setting))
                {
                    myConn.Open();
                    string sql = string.Format(@"INSERT INTO `cars_inhouse_stock_inout` VALUES(NULL, '{0}', '{1}', {2}, {3}, {4}, 'Y');", InOut, InOutDate, IDCentre, IDStock, Qty);
                    using (MySqlCommand myCmd = new MySqlCommand(sql, myConn))
                    {
                        myCmd.ExecuteNonQuery();
                    }
                    myConn.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static List<StorageCentre> ListOfCentre()
        {
            List<StorageCentre> ScList = new List<StorageCentre>();
            try
            {
                using (MySqlConnection myConn = new MySqlConnection(myConnStr.Setting))
                {
                    myConn.Open();
                    string sql = @"SELECT `IDCentre`, `CentreName` FROM `cars_inhouse_centre`;";
                    using (MySqlCommand myCmd = new MySqlCommand(sql, myConn))
                    {
                        using (MySqlDataReader myDr = myCmd.ExecuteReader())
                        {
                            while (myDr.Read())
                            {
                                ScList.Add(new StorageCentre { IDCentre = myDr.GetInt32(0), CentreName = myDr.GetString(1) });
                            }
                        }
                    }
                    myConn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return ScList.OrderBy(x => x.CentreName).ToList();
        }

        public static List<StorageListOfData> ListOfData(string DateFrom, string DateTo, int IDStock)
        {
            List<StorageListOfData> sldList = new List<StorageListOfData>();
            try
            {
                using (MySqlConnection myConn = new MySqlConnection(myConnStr.Setting))
                {
                    myConn.Open();
                    string sql = string.Format(@"SELECT `InOutType`, DATE_FORMAT(`InOutDate`, '%d-%M-%y'), `FKIDCentre`, `Qty`
                        FROM `cars_inhouse_stock_inout`
                        WHERE `Status` = 'Y'
                        AND `InOutDate` BETWEEN '{0}' AND '{1}'
                        AND `FKIDStock` = {2}
                        ORDER BY `InOutDate` ASC;", DateFrom, DateTo, IDStock);
                    using (MySqlCommand myCmd = new MySqlCommand(sql, myConn))
                    {
                        using (MySqlDataReader myDr = myCmd.ExecuteReader())
                        {
                            while (myDr.Read())
                            {
                                sldList.Add(new StorageListOfData { Type = myDr.GetString(0), Date = myDr.GetString(1), IDCentre = myDr.GetInt32(2), Qty = myDr.GetInt32(3)});
                            }
                        }
                    }
                    myConn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return sldList;
        }

        public static bool RemoveRawData(int IDInOut)
        {
            try
            {
                int result = -1;
                using (MySqlConnection myConn = new MySqlConnection(myConnStr.Setting))
                {
                    myConn.Open();
                    string sql = string.Format(@"UPDATE `cars_inhouse_stock_inout` SET `Status` = 'N' WHERE `IDInOut` = {0};", IDInOut);
                    using (MySqlCommand myCmd = new MySqlCommand(sql, myConn))
                    {
                        result = myCmd.ExecuteNonQuery();
                    }
                    myConn.Close();
                }
                if (result > 0) { return true; } else { return false; }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static List<StorageListOfDataDetails> ListOfDataDetails()
        {
            List<StorageListOfDataDetails> sldList = new List<StorageListOfDataDetails>();
            try
            {
                using (MySqlConnection myConn = new MySqlConnection(myConnStr.Setting))
                {
                    myConn.Open();
                    string sql = string.Format(@"SELECT `sio`.`IDInOut` AS `ID`, DATE_FORMAT(`sio`.`InOutDate`, '%d-%M-%y') AS `Date`, `sio`.`InOutType`, IFNULL(`ic`.`CentreName`, '') AS `Centre`, `is`.`StockName` AS `Stock Name`, `sio`.`Qty`
                        FROM `cars_inhouse_stock_inout` AS `sio`
                        INNER JOIN `cars_inhouse_stock` AS `is` ON `is`.`IDStock` = `sio`.`FKIDStock`
                        LEFT JOIN `cars_inhouse_centre` AS `ic` ON `ic`.`IDCentre` = `sio`.`FKIDCentre`
                        WHERE `Status` = 'Y'
                        ORDER BY `InOutDate` ASC;");
                    using (MySqlCommand myCmd = new MySqlCommand(sql, myConn))
                    {
                        using (MySqlDataReader myDr = myCmd.ExecuteReader())
                        {
                            while (myDr.Read())
                            {
                                sldList.Add(new StorageListOfDataDetails {IDInOut = myDr.GetInt32(0), Date = myDr.GetString(1), Type = myDr.GetString(2), CentreName = myDr.GetString(3), StockName = myDr.GetString(4), Qty = myDr.GetInt32(5)});
                            }
                        }
                    }
                    myConn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return sldList;
        }

        public static List<StorageStock> ListOfStock()
        {
            List<StorageStock> SsList = new List<StorageStock>();
            try
            {
                using (MySqlConnection myConn = new MySqlConnection(myConnStr.Setting))
                {
                    myConn.Open();
                    string sql = @"SELECT `IDStock`, `StockName` FROM `cars_inhouse_stock`;";
                    using (MySqlCommand myCmd = new MySqlCommand(sql, myConn))
                    {
                        using (MySqlDataReader myDr = myCmd.ExecuteReader())
                        {
                            while (myDr.Read())
                            {
                                SsList.Add(new StorageStock { IDStock = myDr.GetInt32(0), StockName = myDr.GetString(1) });
                            }
                        }
                    }
                    myConn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return SsList.OrderBy(x => x.StockName).ToList();
        }

        public static string SaveFilename()
        {
            try
            {
                SaveFileDialog sFD = new SaveFileDialog();
                sFD.Title = "Stock In/Out Report";
                sFD.AddExtension = true; // auto add extention .xls
                sFD.DefaultExt = "xls"; // default as excel file
                sFD.Filter = "Excel files |*.xls|All files (*.*)|*.*";
                sFD.FilterIndex = 1;
                sFD.RestoreDirectory = true;
                sFD.FileName = "StockInOutReport-" + DateTime.Now.ToString("yyyy-MM-dd") + ".xls";  // put default file name here
                return (sFD.ShowDialog().Equals(true)) ? sFD.FileName : string.Empty; // Getting if ShowDialog Save is Press, then return the FileName or return empty string
            }
            catch { return string.Empty; } // Dismiss Error
        }
    }
}
