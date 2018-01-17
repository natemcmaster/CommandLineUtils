namespace McMaster.Extensions.CommandLineUtils
{
    public static partial class Prompt
    {
        private class CheckboxSelection
        {
            public string Text { get; set; }
            public bool IsSelected { get; set; }

            public CheckboxSelection(string text)
            {
                Text = text;
            }

            public override string ToString()
            {
                return $"{Text}: {IsSelected}";
            }
        }
    }
}