using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConversionOptimizer
{
    public class Test : IComparable<Test>
    {
        public bool Convertable;
        public string FitnessePath;
        public string FullPath;
        public int NumLines;
        public string Status;

        public Test(string path, List<string> exceptions, string inputStatus)
        {
            //Get Full Path and add context.txt
            FullPath = @"C:\Projects\FitNesseRoot\" + path.Replace('.', '\\') + @"\content.txt";

            Convertable = true;
            FitnessePath = path;

            if (inputStatus != null)
                Status = inputStatus;

            //Count number of lines
            NumLines = 0;

            TextReader testFile;

            try
            {
                testFile = new StreamReader(FullPath);

                while (testFile.Peek() != -1)
                {
                    NumLines++;

                    string currLine = testFile.ReadLine();

                    MacroDetector(currLine);

                    ExceptionDetector(currLine, exceptions);
                }
            }
            catch (FileNotFoundException e)
            {
                NumLines = -1;
                Status = "Test Not Found";
                Convertable = false;
            }
            catch (DirectoryNotFoundException e)
            {
                NumLines = -1;
                Status = "Test Directory Not Found";
                Convertable = false;
            }


            if (NumLines == -1)
                return;

            //Search for unconvertable syntax

        }

        #region Object Creation Helper Methods

        private void ExceptionDetector(string currLine, List<string> exceptions)
        {
            if (exceptions != null)
            {
                currLine = currLine.ToUpper();

                foreach (string ex in exceptions)
                {
                    if (currLine.Contains(ex))
                    {
                        Convertable = false;

                        if (Status != null && Status.Contains("On Hold:") && !Status.Contains(ex))
                        {
                            Status = Status + " " + ex;
                        }
                        else
                        {
                            Status = "On Hold: " + ex;
                        }
                    }
                }
            }
        }

        private void MacroDetector(string currLine)
        {
            if (currLine.Contains("!include"))
            {
                const string root = @"C:\Projects\FitNesseRoot\";
                /*
                 char[] splitter = { ' ' };
                 string[] macroDetected = currLine.Split(splitter);
                 */

                currLine = Regex.Match(currLine, @"([\w]+[\.]+)+(\w)+").ToString();

                currLine.Trim();


                string macro = root + currLine.Replace('.', '\\');

                if (Program.MacroList.ContainsKey(currLine))
                    return;
                if (Program.TestList.ContainsKey(currLine))
                {
                    Test output;
                    Program.TestList.TryGetValue(currLine, out output);

                    Program.TestList.Remove(currLine);

                    output.Status = "Macro";

                    Program.MacroList.Add(currLine, output);
                    return;
                }

                Test newmacro = new Test(currLine, null, "Macro");

                NumLines += newmacro.NumLines;

                Program.MacroList.Add(currLine, newmacro);
            }
        }

        #endregion


        #region IComparable<Test> Members

        public int CompareTo(Test other)
        {
            if (statusCompare().CompareTo(other.statusCompare()) == 0)
                return NumLines.CompareTo(other.NumLines);
            else
            {
                return statusCompare().CompareTo(other.statusCompare());
            }
        }



        private int statusCompare()
        {
            switch (Status)
            {
                case "Not Started":
                    return 0;
                case "In Progress":
                    return 1;
                case "On Hold":
                    return 2;
                case "Waiting for Review":
                    return 4;
                case "Finished":
                    return 5;
                case "Macro":
                    return 6;
                default:
                    return 3;
            }
        }

        #endregion
    }
}