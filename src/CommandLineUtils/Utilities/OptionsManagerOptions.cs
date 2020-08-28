namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Represents check box options
    /// </summary>
    public class OptionsManagerOptions
    {
        /// <summary>
        /// Gets or sets the binding keys.
        /// </summary>
        /// <value>
        /// The keys.
        /// </value>
        public OptionsKeys Keys { get; set; } = new OptionsKeys();

        /// <summary>
        /// Gets or sets a value indicating whether [only use marked properties].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [only use marked properties]; otherwise, <c>false</c>.
        /// </value>
        public bool OnlyUseMarked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance uses Unicode.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance uses Unicode; otherwise, <c>false</c>.
        /// </value>
        public bool IsUnicode { get; set; } = true;

        /// <summary>
        /// Gets or sets the style.
        /// </summary>
        /// <value>
        /// The style.
        /// </value>
        public OptionsStyle Style { get; set; } = new OptionsStyle();

        /// <summary>
        /// Gets or sets a value indicating whether [display help text].
        /// </summary>
        /// <value>
        /// <c>true</c> if [display help text]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayHelpText { get; set; } = true;

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        public string HelpText { get; set; } = "Use the arrow keys to move, Space to select and Enter to exit";

        internal string CheckedChar => Style.GetCheckedChar(IsRadio, IsUnicode);

        internal string UncheckedChar => Style.GetUncheckedChar(IsRadio, IsUnicode);

        /// <summary>
        /// Gets or sets a value indicating whether this instance should not stop until at least one check box is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance requires at least one check box marked; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelectionRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is radio (button).
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is radio; otherwise, <c>false</c>.
        /// </value>
        public bool IsRadio { get; set; } = false;
    }
}
