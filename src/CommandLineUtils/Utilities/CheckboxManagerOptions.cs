namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Represents checkbox options
    /// </summary>
    public class CheckboxManagerOptions
    {
        /// <summary>
        /// Gets or sets the checked character.
        /// </summary>
        /// <value>
        /// The checked character.
        /// </value>
        public string CheckedChar { get; set; } = "🔘";

        /// <summary>
        /// Gets or sets the unchecked character.
        /// </summary>
        /// <value>
        /// The unchecked character.
        /// </value>
        public string UncheckedChar { get; set; } = "〇";

        /// <summary>
        /// Gets or sets a value indicating whether this instance is radio (button).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is radio; otherwise, <c>false</c>.
        /// </value>
        public bool IsRadio { get; set; } = false;
    }
}