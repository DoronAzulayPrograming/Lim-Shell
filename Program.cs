using System;
using System.Collections.Generic;
using System.IO;
using LimOnDotNetCore.Core;
using LimOnDotNetCore.Core.Values;
using CommandLine;
using LimOnDotNetCore.CLI;
using CommandLine.Text;
using LimOnDotNetCore.Core.Translating;
using LimOnDotNetCore.Core.Errors;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace LimOnDotNetCore
{
    class Program
    {
        public const string DIGITS = "0123456789";
        public const string LETTERS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string LETTERS_DIGITS = LETTERS + DIGITS;


        public static readonly IList<string> KEYWORDS = new List<string>
        {"var","or", "and", "not","if","then","elif","else","for","to","step","while","func",
         "end","return","continue","break"};

        public static SymbolTable GlobalSymbolTable = new SymbolTable();
        public static char CharNone = '\0';

        static void Main(string[] args)
        {
            /*StreamWriter writer = new StreamWriter($"c:/dev/limbuilds/Program.cs", true);
            writer.WriteLine("testtttt");
            writer.Close();*/

            /*using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.WorkingDirectory = @"C:\";
                process.StartInfo.FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe");

                // Redirects the standard input so that commands can be sent to the shell.
                process.StartInfo.RedirectStandardInput = true;
                // Runs the specified command and exits the shell immediately.
                //process.StartInfo.Arguments = @"/c ""dir""";

                process.OutputDataReceived += new DataReceivedEventHandler((sendingProcess, outLine) => Console.WriteLine(outLine.Data));
                process.ErrorDataReceived += new DataReceivedEventHandler((sendingProcess, outLine) => Console.WriteLine(outLine.Data));

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Send a directory command and an exit command to the shell
                process.StandardInput.WriteLine("cd c:\\dev\\limbuilds");
                process.StandardInput.WriteLine("dotnet new console -o helloWorld");
                process.StandardInput.WriteLine("cd helloWorld");

                process.StandardInput.WriteLine($"cd {Directory.GetCurrentDirectory()}");
                process.StandardInput.WriteLine($"copy Values.cs c:\\dev\\limbuilds\\helloWorld");

                process.StandardInput.WriteLine("dotnet build");
                process.StandardInput.WriteLine("dotnet run");
                process.StandardInput.WriteLine("exit");
                process.WaitForExit();
            }*/
            LoadGlobalTable();
            Compiler compiler = new Compiler();
            compiler.Start(args);
        }

        static void LoadGlobalTable()
        {
            GlobalSymbolTable.Set("null", Number.Null);
            GlobalSymbolTable.Set("false", Number.False);
            GlobalSymbolTable.Set("true", Number.True);
            GlobalSymbolTable.Set("math_pi", Number.PI);

            GlobalSymbolTable.Set(BuilInFunction.Print.Name, BuilInFunction.Print);
            GlobalSymbolTable.Set(BuilInFunction.PrintReturn.Name, BuilInFunction.PrintReturn);

            GlobalSymbolTable.Set(BuilInFunction.Input.Name, BuilInFunction.Input);
            GlobalSymbolTable.Set(BuilInFunction.InputInt.Name, BuilInFunction.InputInt);

            GlobalSymbolTable.Set(BuilInFunction.Clear.Name, BuilInFunction.Clear);

            GlobalSymbolTable.Set("cls", BuilInFunction.Clear);

            GlobalSymbolTable.Set(BuilInFunction.IsNumber.Name, BuilInFunction.IsNumber);
            GlobalSymbolTable.Set(BuilInFunction.IsString.Name, BuilInFunction.IsString);
            GlobalSymbolTable.Set(BuilInFunction.IsList.Name, BuilInFunction.IsList);
            GlobalSymbolTable.Set(BuilInFunction.IsFunction.Name, BuilInFunction.IsFunction);
            GlobalSymbolTable.Set("is_func", BuilInFunction.IsFunction);

            GlobalSymbolTable.Set(BuilInFunction.Append.Name, BuilInFunction.Append);
            GlobalSymbolTable.Set(BuilInFunction.Pop.Name, BuilInFunction.Pop);
            GlobalSymbolTable.Set(BuilInFunction.Extend.Name, BuilInFunction.Extend);
            GlobalSymbolTable.Set(BuilInFunction.Length.Name, BuilInFunction.Length);
            GlobalSymbolTable.Set(BuilInFunction.Run.Name, BuilInFunction.Run);
        }
    }


    class Compiler
    {
        void DisplayHelp<T>(ParserResult<T> result, IEnumerable<CommandLine.Error> errs)
        {
            HelpText helpText = null;
            if (errs.IsVersion())  //check if error is version request
                helpText = HelpText.AutoBuild(result);
            else
            {
                helpText = HelpText.AutoBuild(result, h =>
                {
                    //configure help
                    h.AddPostOptionsLine("no command    :   run cli.");
                    h.AddPostOptionsLine(" ");
                    h.AddPostOptionsLine("commands:");
                    h.AddPostOptionsLine(" ");
                    h.AddPostOptionsLine("run       :   pass file name (path to file) to run it");
                    h.AddPostOptionsLine("          :   or leave it empty to run cli.");
                    h.AddPostOptionsLine(" ");
                    h.AddPostOptionsLine("build     :   pass file name (path to file) to build it.");
                    h.AddPostOptionsLine(" ");
                    h.AddPostOptionsLine(" ");
                    return HelpText.DefaultParsingErrorsHandler(result, h);
                }, e => e);
            }
            Console.WriteLine(helpText);
        }
        private void Run(string[] args,ProgramOptions op)
        {
            //args = new string[] { "build", "main.lim", "c:/dev/limbuilds/helloWolrd" };
            if (!string.IsNullOrWhiteSpace(op.Path))
                ExecFile(op.Path, "'path' flag most be path to exists file.");

            else if (op.Run)
                RunInterface();
            else
            {
                if (args.Length > 0)
                {
                    switch (args[0].ToLower())
                    {
                        case "run":
                            if (args.Length > 1)
                                ExecFile(args[1], "After 'run' command Expected to file name (path to file).");
                            else RunInterface();
                            break;
                        case "build":
                            if (args.Length > 2)
                            {
                                if (!File.Exists(args[1]))
                                {
                                    Console.WriteLine("first arg most be path lim file.");
                                    return;
                                }
                                else if (!Directory.Exists(args[2]))
                                {
                                    if (!args[2].Contains(Path.DirectorySeparatorChar) && !args[2].Contains("/"))
                                    {
                                        Console.WriteLine("second arg most be path to build directory.");
                                        return;
                                    }
                                    Directory.CreateDirectory(args[2]);
                                }
                                string projectName;

                                if (args[2].Contains('/')) projectName = args[2].Split('/').Last();
                                else projectName = args[2].Split('\\').Last();
                                Build(args[1], args[2], projectName);
                            }
                            break;
                        default:
                            ExecFile(args[0], "no flag parameter most be file name (path to file).");
                            break;
                    }
                }
                else
                    RunInterface();
            }
        }
        public void Start(string[] args)
        {

            var parser = new CommandLine.Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<ProgramOptions>(args);

            parserResult
              .WithParsed(options => Run(args,options))
              .WithNotParsed(errs => DisplayHelp(parserResult, errs));
        }
        public void RunInterface()
        {
            while (true)
            {

                string text = GetInput();
                if (string.IsNullOrWhiteSpace(text)) continue;

                CompileInnerFile(text);
            }
        }
        public void ExecFile(string path,string error_msg)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine(error_msg);
                return;
            }
            try
            {
                var limScript = ReadFile(path);
                if (string.IsNullOrWhiteSpace(limScript)) return;

                Basic.Run(path, limScript, out var error);
                if (error != null)
                    Console.WriteLine(error.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        string GetInput()
        {
            Console.Write("basic > ");
            return Console.ReadLine();
        }
        string ReadFile(string path)
        {
            StreamReader reader = new StreamReader(path);
            var text = reader.ReadToEnd();
            reader.Close();
            return text;
        }
        void CompileInnerFile(string file_text)
        {
            try
            {
                var retVal = Basic.Run("<stdin>", file_text, out var error);
                if (error != null)
                    Console.WriteLine(error.ToString());
                else
                    Console.WriteLine(retVal.ToRrepresent());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        void Build(string lim_file, string dest_dir, string project_name)
        {
            dest_dir = dest_dir.Replace("/","\\");
            Console.WriteLine($"Compileing lim file: {lim_file}");

            // read lim file
            string limScript;
            try
            {
                limScript = ReadFile(lim_file);
                if (string.IsNullOrWhiteSpace(limScript)) return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            // compile
            IError error;
            // Generate tokens
            Lexer lexer = new Lexer(lim_file, limScript);
            var tokens = lexer.MakeTokens(out error);

            if (error != null)
            {
                Console.WriteLine(error);
                return;
            }

            // Generate AST
            Core.Translating.Parser parser = new Core.Translating.Parser(tokens);
            var ast = parser.Parse();

            if (ast.Error != null)
            {
                error = ast.Error;
                Console.WriteLine(error);
                return;
            }
            Console.WriteLine($"Compile succeeded.");

            Console.WriteLine($"Start Build.");
            var cs_code = ast.Node.ToCSharpCode();
            Processor processor = new Processor();
            CsProject project = processor.Proccess(cs_code, project_name);

            var cmd = OpenCmd();
            cmd.StandardInput.WriteLine($"cd {dest_dir}");
            cmd.StandardInput.WriteLine($"dotnet new console");
            Thread.Sleep(6 * 1000);
            cmd.StandardInput.WriteLine($"del /f program.cs");

            /*cmd.StandardInput.WriteLine($"cd {Directory.GetCurrentDirectory()}");
            cmd.StandardInput.WriteLine($"copy Values.cs {dest_dir}\\{project_name}");*/

            StreamWriter writer = new StreamWriter($"{dest_dir}\\Entry.cs", false);
            writer.WriteLine(project.Entry);
            writer.Close();

            Thread.Sleep(100);
            writer = new StreamWriter($"{dest_dir}\\Values.cs", false);
            writer.WriteLine(project.Values);
            writer.Close();

            Thread.Sleep(100);
            writer = new StreamWriter($"{dest_dir}\\Program.cs", false);
            writer.WriteLine(project.Program);
            writer.Close();

            Thread.Sleep(100);
            cmd.StandardInput.WriteLine($"dotnet build");
            cmd.StandardInput.WriteLine("exit");
            cmd.WaitForExit();
        }
        Process OpenCmd()
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory = @"C:\";
            process.StartInfo.FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe");

            process.StartInfo.RedirectStandardInput = true;

            process.OutputDataReceived += new DataReceivedEventHandler((sendingProcess, outLine) => Console.WriteLine(outLine.Data));
            process.ErrorDataReceived += new DataReceivedEventHandler((sendingProcess, outLine) => Console.WriteLine(outLine.Data));

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }
    }
}































/*
 
 
           if (args.Length == 1)
            {
                try
                {
                    StreamReader reader = new StreamReader(args[0]);
                    var limScript = reader.ReadToEnd();
                    reader.Close();
                    if (string.IsNullOrWhiteSpace(limScript)) return;

                    Basic.Run(args[0], limScript, out var error);
                    if (error != null)
                        Console.WriteLine(error.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                var text = Console.ReadKey();
            }
            else
            {
                while (true)
                {
                    Console.Write("basic > ");
                    var text = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(text)) continue;

                    var retVal = Basic.Run("<stdin>", text, out var error);
                    if (error != null)
                        Console.WriteLine(error.ToString());
                    else if (retVal != null)
                    {
                        if (typeof(ListVal).IsInstanceOfType(retVal))
                        {
                            var list = retVal as ListVal;
                            if (list.Elements.Count == 1)
                                Console.WriteLine(retVal.ToRrepresent());
                            else if (!string.IsNullOrEmpty($"{retVal}"))
                                Console.WriteLine(retVal);
                        }
                        else if (!string.IsNullOrEmpty($"{retVal}"))
                            Console.WriteLine(retVal);
                    }
                }
            }
 
 
 */