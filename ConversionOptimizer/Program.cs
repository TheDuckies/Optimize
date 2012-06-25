using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Collections;

namespace ConversionOptimizer
{
    internal class Program
    {
        public static Dictionary<string, Test> TestList, MacroList;
        public static List<string> exceptionList;

        [STAThread]
        private static void Main(string[] args)
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
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("Total number of") || line.Contains("Path	Lines	Status") || line.Equals(""))
                        continue;

                    char[] toRemove = {'\t'};

                    string[] testline = line.Split(toRemove, StringSplitOptions.RemoveEmptyEntries);

                    Test newTest = new Test(testline[0], exceptionList, null);

                    if (newTest.Status == null)
                        if (testline[1].Equals(newTest.NumLines.ToString()))
                            newTest.Status = testline[2];
                        else
                            newTest.Status = testline[1];

                    newTest.FitnessePath = testline[0];
                    if (newTest.Status.Equals("Macro"))
                        MacroList.Add(newTest.FitnessePath, newTest);
                    else if (!TestList.ContainsKey(newTest.FitnessePath))
                        TestList.Add(newTest.FitnessePath, newTest);

                }
                reader.Close();

                List<Test> sortedTests = new List<Test>(TestList.Values);
                List<Test> sortedMacros = new List<Test>(MacroList.Values);

                Metrics suiteMetrics = new Metrics(TestList.Count, MacroList.Count);

                sortedTests.Sort();

                string fileName = sortedTests[0].FitnessePath.Split('.')[0];

                StreamWriter spreadsheetoutput = new StreamWriter(fileName + ".TSV");

                string[] statii = {"Not Started", "In Progress", "On Hold", "Waiting for Review", "Finished"};

                countStatus(TestList.Values, statii);

                spreadsheetoutput.WriteLine("Total number of Tests: " + Metrics.totalTests);
                spreadsheetoutput.WriteLine("Total number of Macros: " + Metrics.totalMacros);
                spreadsheetoutput.WriteLine("Total number of Tests not started: " + Metrics.notStarted);
                spreadsheetoutput.WriteLine("Total number of Tests in progress: " + Metrics.inProgress);
                spreadsheetoutput.WriteLine("Total number of Tests on hold (generic): " + Metrics.onHold);
                spreadsheetoutput.WriteLine("Total number of Tests on hold (exception list generated): " + Metrics.onHoldException);
                spreadsheetoutput.WriteLine("Total number of Tests waiting for review: " + Metrics.waitingForReview);
                spreadsheetoutput.WriteLine("Total number of Tests finished: " + Metrics.finished);

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

        private static void countStatus (Dictionary<string, Test>.ValueCollection valueCollection, string[] statii)
        {
            foreach (Test test in valueCollection)
            {
                switch(test.Status)
                {
                    case "Not Started":
                        Metrics.notStarted++;
                        break;
                    case "On Hold":
                        Metrics.onHold++;
                        break;
                    case "Waiting for Review":
                        Metrics.waitingForReview++;
                        break;
                    case "Finished":
                        Metrics.finished++;
                        break;
                    default:
                        Metrics.onHoldException++;
                        break;
                }
            }

        }
    }
}
