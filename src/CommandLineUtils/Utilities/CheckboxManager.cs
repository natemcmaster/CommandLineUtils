namespace McMaster.Extensions.CommandLineUtils
{
    public static partial class Prompt
    {
        private class CheckboxManagerOptions
        {
            public string CheckedChar { get; set; } = "🔘";
            public string UncheckedChar { get; set; } = "〇";
            public bool IsRadio { get; set; } = false;                
        }
    }
}