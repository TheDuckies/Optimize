using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Collections;

namespace ConversionOptimizer
{
    class Program
    {
        public static Dictionary<string, Test> TestList, MacroList;
        public static List<string> exceptionList;

        [STAThread]
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(@"C:\Projects\Optimize\OptimizedSpreadsheets\");

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
            
            TestList = new Dictionary<string, Test>();
            MacroList = new Dictionary<string, Test>();

            using (TextReader reader = source)
            {
                string line;
                while( (line = reader.ReadLine()) != null)
                {
                    if (line.Contains("Total number of Tests:") || line.Contains("Path	Lines	Status") || line.Equals(""))
                        continue;

                    char[] toRemove = {'\t'};
                    
                    string[] testline = line.Split(toRemove, StringSplitOptions.RemoveEmptyEntries);

                    Test newTest = new Test(testline[0], exceptionList, null);

                    if (newTest.Status == null)
                        if(testline[1].Equals(newTest.NumLines.ToString()))
                            newTest.Status = testline[2];
                        else
                            newTest.Status = testline[1];

                    if(newTest.Status.Equals("Macro"))
                        MacroList.Add(newTest.FitnessePath, newTest);
                    else if(!TestList.ContainsKey(newTest.FitnessePath))
                        TestList.Add(newTest.FitnessePath, newTest);
                }
                reader.Close();

               List<Test> sortedTests = new List<Test>(TestList.Values);
                List<Test> sortedMacros = new List<Test>(MacroList.Values);

                sortedTests.Sort();

                string fileName = sortedTests[0].FitnessePath.Split('.')[0];

                StreamWriter spreadsheetoutput = new StreamWriter(fileName + ".TSV");

                spreadsheetoutput.WriteLine("Total number of Tests: " + TestList.Count);

                spreadsheetoutput.WriteLine("Path" + '\t' + "Lines" + '\t' + "Status");

                foreach (Test test in sortedTests)
                    spreadsheetoutput.WriteLine(test.FitnessePath + '\t' + test.NumLines + '\t' + test.Status);
                spreadsheetoutput.Close();

                StreamWriter macroutput = new StreamWriter(fileName + "MACROS.TSV");

                foreach (Test test in sortedMacros)
                {
                    macroutput.WriteLine(test.FitnessePath + '\t' + test.NumLines + '\t');
                }

                macroutput.Close();
            }
        }
    }
}
