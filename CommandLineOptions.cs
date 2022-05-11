using CommandLine;

public class CommandLineOptions
{
    [Value(index: 0, Required = false, HelpText = "Input folder path to process (default: current folder).", Default = "")]
    public string InputFolder { get; set; }

    [Option(shortName: 'd', longName: "delete", Required = false, HelpText = "Delete after processing.", Default = false)]
    public bool Delete { get; set; }


    [Option(shortName: 'o', longName: "output", Required = false, HelpText = "Output folder (default: current folder).", Default = "")]
    public string OutputFolder { get; set; }

}
