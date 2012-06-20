using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;


namespace ConversionOptimizer
{
    class Program
    {
        private static List<Test> TestList;
        private static List<string> exceptionList;
        private static Hashtable ExistingList;

        [STAThread]
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(@"C:\Projects\Optimize\");

            OpenFileDialog ReadFile = FindFitNesseList();

            BuildExceptionList();
            
            ReadSource(new StreamReader(ReadFile.OpenFile()));

        }

        public static OpenFileDialog FindFitNesseList()
        {
            OpenFileDialog ReadFile = new OpenFileDialog();
            ReadFile.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            ReadFile.InitialDirectory = @"C:\Projects\Optimize\FitNesseList\";
            ReadFile.Title = "Select a tab-delimited source file";

            DialogResult result = ReadFile.ShowDialog();

            while (result != DialogResult.OK)
            {
                if (result == DialogResult.Cancel || result == DialogResult.Abort)
                   Environment.Exit(0);
                if (!ReadFile.CheckFileExists)
                    continue;
                result = ReadFile.ShowDialog();
            }

            return ReadFile;
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

            StreamWriter outputFile = new StreamWriter("OUTPUT.TXT");
            StreamWriter spreadsheetoutput = new StreamWriter("Spreadsheet.TSV");
            spreadsheetoutput.WriteLine("Path" + '\t' + "Lines" + '\t' + "Status");
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
                outputFile.WriteLine("Total number of Tests: " + count);

                try
                {

                    TestList.Sort();
                }
                catch (Exception e)
                {
                }

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
