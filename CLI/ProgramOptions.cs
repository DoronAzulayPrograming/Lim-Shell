using CommandLine;

namespace LimOnDotNetCore.CLI
{
    public class ProgramOptions
    {
        [Option('p', "path", Required = false, HelpText = "Path to lim script file.")]
        public string Path { get; set; }

        [Option('r', "run", Required = false, HelpText = "Start lim cli interface.")]
        public bool Run { get; set; }

        //[Name("b", "build"), Description("path to lim script file")]
        //public string Build { get; set; }

        //[Name("p", "path"), Description("path to lim script file")]
        //public string Watch { get; set; }
    }
}
