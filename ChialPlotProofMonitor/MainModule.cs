using System;
using System.IO;
using System.Media;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ChialPlotProofMonitor
{
    class MainModule
    {
        //2021-05-12T00:53:47.398 harvester chia.harvester.harvester: INFO     0 plots were eligible for farming be787b4a8e... Found 0 proofs. Time: 0.00199 s. Total 30 plots
        private static void Main(string[] args)
        {
            Console.WriteLine("Begin read process...");
            ReadLogFile();
            string command;
            while (true)
            {
                command = Console.ReadLine();
                if(command == "close")
                {
                    break;
                }

                if(command.Split('/')[0] == "test")
                {
                    Test(command.Split('/')[1]);
                }
            }
        }

        private static async void ReadLogFile()
        {
            Console.WriteLine("Reading File...");
            await Task.Run(()=>ParseLogFile());
        }

        private static async void ParseLogFile()
        {
            StreamReader sr = null;
            string line = null;
            try
            {
                sr = new StreamReader(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.chia\mainnet\log\debug.log"));
                line = sr.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            uint lineCounter = 0;
            uint plotPassed = 0;
            uint missedFilter = 0;
            string[] lineResult;

            while (true)
            {
                try
                {
                    
                    if (AnalizeLine(line, out lineResult))
                    {
                        int cursorLeft = Console.CursorLeft;

                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        if (uint.Parse(lineResult[3]) > 0)
                        {
                            PlaySoundEffect(Properties.Resources.filter_pass_proof);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(lineResult[0] + lineResult[1]);
                            plotPassed += uint.Parse(lineResult[2]);
                        }
                        else if(uint.Parse(lineResult[2]) > 0)
                        {
                            PlaySoundEffect(Properties.Resources.filter_pass_no_proof);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(lineResult[0] + lineResult[1]);
                            plotPassed += uint.Parse(lineResult[2]);
                        }
                        else
                        {
                            missedFilter++;
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(new String(' ',Console.BufferWidth));
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.WriteLine("At line " + lineCounter + "; Filter pass so far: " + plotPassed+ "; Missed attempts "+ missedFilter);

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.SetCursorPosition(cursorLeft, Console.CursorTop);
                    }
                    else if (line == null || sr == null)
                    {
                        sr?.Close();
                        sr = null;
                        await Task.Delay(250);
                        sr = new StreamReader(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.chia\mainnet\log\debug.log"));
                        for (uint i = 0; i < lineCounter - 1; i++)
                        {
                            if(sr.ReadLine() == null)
                            {
                                lineCounter = i;
                                break;
                            }
                        }
                    }

                    line = sr.ReadLine();
                    lineCounter++;
                }
                catch(Exception ex)
                {

                }
            }
        }

        private static bool AnalizeLine(string line,out string[] resultMass)
        {
            resultMass = null;
            if (line == null)
            { 
                return false;
            }

            
            Regex totalLineChek = new Regex(@"harvester chia.harvester.harvester: INFO");
            if(!totalLineChek.IsMatch(line))
            {
                return false;
            }
            resultMass = new string[4];
            Regex timeRegEx = new Regex(@"^([\S]+)");
            resultMass[0] = timeRegEx.Match(line).Value;
            line = timeRegEx.Replace(line, "");

            timeRegEx = new Regex(@"T");
            resultMass[0] = timeRegEx.Replace(resultMass[0], " ");

            Regex infoRegEx = new Regex(@"^([\D]+)");
            line = infoRegEx.Replace(line, " ");

            resultMass[1] = line;

            Regex matchRegEx = new Regex(@"^(\s[\d]+)");
            resultMass[2] = matchRegEx.Match(line).Value;
            line = matchRegEx.Replace(line, "");

            infoRegEx = new Regex(@"^\s([a-zA-z]+\s)+[0-9a-z]+...\s[\D]+");
            line = infoRegEx.Replace(line, "");

            Regex proofRegEx = new Regex(@"^([\d]+)");
            resultMass[3] = proofRegEx.Match(line).Value;
            line = proofRegEx.Replace(line, "");

            return true;
        }

        private static void PlaySoundEffect(Stream stream)
        {
            SoundPlayer soundPlayer = new SoundPlayer(stream);
            soundPlayer.Play();
        }

        private static void Test(string command)
        {
            switch(command)
            {
                case "filter":
                    PlaySoundEffect(Properties.Resources.filter_pass_no_proof);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("2021-05-12 04:54:05.069 1 plots were eligible for farming 86dd32046c... Found 0 proofs. Time: 0.45936 s. Total 30 plots");
                    break;
                case "proof":
                    PlaySoundEffect(Properties.Resources.filter_pass_proof);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("2021-05-12 04:54:05.069 1 plots were eligible for farming 86dd32046c... Found 1 proofs. Time: 0.45936 s. Total 30 plots");
                    break;
            }
        }
    }
}
