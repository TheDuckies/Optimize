using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ConversionOptimizer
{
    class Program
    {
        private static List<Test> TestList;
        private static List<string> exceptionList;

        [STAThread]
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(@"C:\Projects\OptimizedSpreadsheets");

            OpenFileDialog readFile = FindFitNesseList();

            BuildExceptionList();
            
            ReadSource(new StreamReader(readFile.OpenFile()));

        }

        public static OpenFileDialog FindFitNesseList()
        {
            OpenFileDialog readFile = new OpenFileDialog();
            readFile.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            readFile.InitialDirectory = @"C:\Projects\Optimize\FitNesseList\";
            readFile.Title = "Select a tab-delimited source file";

            DialogResult result = readFile.ShowDialog();

            while (result != DialogResult.OK)
            {
                if (result == DialogResult.Cancel || result == DialogResult.Abort)
                   Environment.Exit(0);
                if (!readFile.CheckFileExists)
                    continue;
                result = readFile.ShowDialog();
            }

            return readFile;
        }

        public static void BuildExceptionList()
        {
            OpenFileDialog exceptionFile = new OpenFileDialog();
            exceptionFile.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            exceptionFile.InitialDirectory = @"C:\Projects\Optimize\ExceptionsList\";
            exceptionFile.Title = "Select an exception list";
            TextReader exceptionListFile;
            DialogResult result2 = exceptionFile.ShowDialog();

            if (result2 == DialogResult.Cancel || result2 == DialogResult.Abort)
            {
                exceptionListFile = new StreamReader(@"C:\Projects\Optimize\ExceptionsList\blank.txt");
            }
            else
            {
                exceptionListFile = new StreamReader(exceptionFile.OpenFile());
            }

            exceptionList = new List<string>();

            while (exceptionListFile.Peek() != -1)
                exceptionList.Add(exceptionListFile.ReadLine());
        }

        /*
         * TODO
         * 
         */
        public static void ReadSource(StreamReader source)
        {
            
            TestList = new List<Test>();

            using (TextReader reader = source)
            {
                string line;
                int count = 0;
                while( (line = reader.ReadLine()) != null)
                {
                    char[] toRemove = {'\t'};
                    
                    string[] testline = line.Split(toRemove, StringSplitOptions.RemoveEmptyEntries);

                    Test newTest = new Test(testline[0], exceptionList);

                    if(newTest.Status == null)
                        newTest.Status = testline[1];
                    newTest.FitnessePath = testline[0];
                    TestList.Add(newTest);

                    count++;
                }
                
                TestList.Sort();

                string fileName = TestList[0].FitnessePath.Split('.')[0];

                StreamWriter outputFile = new StreamWriter(fileName + ".TXT");
                StreamWriter spreadsheetoutput = new StreamWriter(fileName + ".TSV");

                outputFile.WriteLine("Total number of Tests: " + count);
                spreadsheetoutput.WriteLine("Total number of Tests: " + count);

                outputFile.WriteLine("Path" + '\t' + "Lines" + '\t' + "Status");
                spreadsheetoutput.WriteLine("Path" + '\t' + "Lines" + '\t' + "Status");

                foreach (Test test in TestList)
                {
                   outputFile.WriteLine("Test Path: " + test.FullPath + '\t' + "Line Count: " + test.NumLines + '\t' + "Status: " + test.Status);
                   spreadsheetoutput.WriteLine(test.FitnessePath + '\t' + test.NumLines + '\t' + test.Status);
                }
                outputFile.Close();
                spreadsheetoutput.Close();
            }
        }


    }
}
