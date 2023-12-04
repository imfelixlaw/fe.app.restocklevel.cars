using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InHouseReporting
{
    public class StorageCentre
    {
        public int IDCentre { get; set; }
        public string CentreName { get; set; }
    }

    public class StorageStock
    {
        public int IDStock { get; set; }
        public string StockName { get; set; }
    }

    public class StorageStockReorderLevel
    {
        public int IDStockReorder { get; set; }
        public int IDStock { get; set; }
        public int Qty { get; set; }
    }

    public class StorageBalance
    {
        public int IDStock { get; set; }
        public int Balance { get; set; }
    }

    public class StorageListOfData
    {
        public string Date { get; set; }
        public string Type { get; set; }
        public int IDCentre { get; set; }
        public int Qty { get; set; }
    }
    
    public class StorageListOfDataDetails
    {
        public int IDInOut { get; set; }
        public string Date { get; set; }
        public string Type { get; set; }
        public string CentreName { get; set; }
        public string StockName { get; set; }
        public int Qty { get; set; }
    }
}
