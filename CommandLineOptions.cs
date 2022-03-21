using CommandLine;

public class CommandLineOptions
{
    [Value(index: 0, Required = false, HelpText = "Folder path to process.", Default = "")]
    public string FolderPath { get; set; }

    [Option(shortName: 'd', longName: "delete", Required = false, HelpText = "Delete after processing.", Default = false)]
    public bool Delete { get; set; }
}
