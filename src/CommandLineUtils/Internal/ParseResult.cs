namespace McMaster.Extensions.CommandLineUtils
{
    internal class ParseResult
    {
        public CommandLineApplication SelectedCommand { get; set; }
        public CommandLineApplication InitialCommand { get; set; }
    }
}
