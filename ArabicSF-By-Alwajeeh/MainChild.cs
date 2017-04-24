using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using Excel = Microsoft.Office.Interop.Excel;
using ExportToExcel;

namespace ArabicSF
{
    public class MainChild
    {
        private string path;
        MainForm form;
        public MainChild(string path, MainForm form)
        {
            this.path = path;
            this.form = form;
        }

        public static void SaveDatasetToCSV(string path, MainForm form)
        {
            System.Data.DataTable DatasetAsCSV = new System.Data.DataTable("Dataset");
            DatasetAsCSV.Columns.Add("Filename", typeof(string));
            DatasetAsCSV.Columns.Add("Text", typeof(string));
            DatasetAsCSV.Columns.Add("Class", typeof(string));
            int _NumberOf_Articles = Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories).Count();
            int i = 0;
            foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
            {
                DatasetAsCSV.Rows.Add(file.Substring(file.LastIndexOf('\\') + 1, file.Split('.').First().Length - file.LastIndexOf('\\') - 1),
                    File.ReadAllText(file, Encoding.UTF8), Directory.GetParent(file).Name);
                form.setProgressValue(((i + 1) * 100) / _NumberOf_Articles,i+1,_NumberOf_Articles );
                i++;
            }
            CreateExcelFile.CreateExcelDocument(DatasetAsCSV, @path+"Dataset As Excel.csv");
        }

        public static void DatasetStatisticsToCSV(string path, MainForm form)
        {
            System.Data.DataTable DatasetAsCSV = new System.Data.DataTable("Statistics");
            DatasetAsCSV.Columns.Add("Class", typeof(string));
            DatasetAsCSV.Columns.Add("No. Files", typeof(int));

            int _NumberOf_Directory = Directory.GetDirectories(@path).Count();
            int i = 0;
            foreach (var directory in Directory.GetDirectories(@path,"*.txt", SearchOption.AllDirectories))
            {
                DatasetAsCSV.Rows.Add(directory.Split('\\').Last(), Directory.GetFiles(directory).Count());
                form.setProgressValue(((i + 1) * 100) / _NumberOf_Directory,i+1,_NumberOf_Directory);
                i++;
            }
            
            CreateExcelFile.CreateExcelDocument(DatasetAsCSV, @path+"Dataset Statistics.xlxs");
        }

        
    }
}
