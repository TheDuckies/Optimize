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
            readFile.Title = "Select a tab- or comma- delimited source file";

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
         * Bug hunt for i/o RE: Author & Notes, Ignore Metrics
         * Convert to CSV format.
         * Verify read-in of CSV
         * Not to be Converted Metric
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
                    if (line.Contains("%") || line.ToUpper().Contains("NUMBER OF") || line.Contains("Path	Lines	Status") || line.Equals("") || line.Contains("Path,"))
                        continue;

                    char[] toRemove = {'\t', ','};

                    string[] testline = line.Split(toRemove, StringSplitOptions.RemoveEmptyEntries);

                    Test newTest = new Test(testline[0], exceptionList, testline[2]);

                    if(testline.Length > 3)
                    newTest.Author = testline[3];

                    if (testline.Length > 4)
                    newTest.Notes = testline[4];

                    if (newTest.Status == null)
                        if (testline[1].Equals(newTest.NumLines.ToString()))
                            newTest.Status = testline[2];
                        else
                            newTest.Status = testline[1];

                    newTest.FitnessePath = testline[0];
                    if (newTest.Status.Equals("Macro"))
                        MacroList.Add(newTest.FitnessePath, newTest);
                    else if (!MacroList.ContainsKey(newTest.FitnessePath))
                        TestList.Add(newTest.FitnessePath, newTest);

                }
                reader.Close();

                List<Test> sortedTests = new List<Test>(TestList.Values);
                List<Test> sortedMacros = new List<Test>(MacroList.Values);

                Metrics suiteMetrics = new Metrics(TestList.Count, MacroList.Count);

                sortedTests.Sort();

                string fileName = sortedTests[0].FitnessePath.Split('.')[0];

                StreamWriter spreadsheetoutput = new StreamWriter(fileName + ".CSV");

                countStatus(TestList.Values);

                spreadsheetoutput.WriteLine("Total number of Tests: " + Metrics.totalTests);
                spreadsheetoutput.WriteLine("Total number of Macros: " + Metrics.totalMacros);
                spreadsheetoutput.WriteLine("Total number of Tests not started: " + Metrics.notStarted);
                spreadsheetoutput.WriteLine("Total number of Tests in progress: " + Metrics.inProgress);
                spreadsheetoutput.WriteLine("Total number of Tests on hold (generic): " + Metrics.onHold);
                spreadsheetoutput.WriteLine("Total number of Tests on hold (exception list generated): " + Metrics.onHoldException);
                spreadsheetoutput.WriteLine("Total number of Tests on hold (inclusive) (as a percentage): " + ((float)(Metrics.onHold+Metrics.onHoldException) * 100 / (float)Metrics.totalTests) + @"%");
                spreadsheetoutput.WriteLine("Total number of Tests waiting for review: " + Metrics.waitingForReview);
                spreadsheetoutput.WriteLine("Total number of Tests finished: " + Metrics.finished);
                spreadsheetoutput.WriteLine("Total number of Tests finished (as a percentage): " + ((float)Metrics.finished * 100 /(float)Metrics.totalTests) + @"%");

                spreadsheetoutput.WriteLine("Path" + ',' + "Lines" + ',' + "Status");

                foreach (Test test in sortedTests)
                    spreadsheetoutput.WriteLine(test.FitnessePath + ',' + test.NumLines + ',' + test.Status + ',' + test.Author + ',' + test.Notes);
                spreadsheetoutput.Close();

                StreamWriter macroutput = new StreamWriter(fileName + "MACROS.CSV", false);

                foreach (Test test in sortedMacros)
                {
                    macroutput.WriteLine(test.FitnessePath + ',' + test.NumLines + ',');
                }

                macroutput.Close();
            }
        }

        private static void countStatus (Dictionary<string, Test>.ValueCollection valueCollection)
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
                    case "Not to be Converted":
                        Metrics.totalTests--;
                        break;
                    case "Test Directory Not Found":
                        Metrics.totalTests--;
                        break;
                    case "In Progress":
                        Metrics.inProgress++;
                        break;
                    default:
                        Metrics.onHoldException++;
                        break;
                }
            }

        }
    }
}
