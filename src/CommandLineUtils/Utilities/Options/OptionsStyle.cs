namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Represents the options style
    /// </summary>
    public class OptionsStyle
    {
        /// <summary>
        /// Gets or sets the checked non radio Unicode.
        /// </summary>
        /// <value>
        /// The checked non radio Unicode.
        /// </value>
        public string CheckedNonRadioUnicode { get; set; } = "☑";

        /// <summary>
        /// Gets or sets the unchecked non radio Unicode.
        /// </summary>
        /// <value>
        /// The unchecked non radio Unicode.
        /// </value>
        public string UncheckedNonRadioUnicode { get; set; } = "▢";

        /// <summary>
        /// Gets or sets the checked radio Unicode.
        /// </summary>
        /// <value>
        /// The checked radio Unicode.
        /// </value>
        public string CheckedRadioUnicode { get; set; } = "🔘";

        /// <summary>
        /// Gets or sets the unchecked radio Unicode.
        /// </summary>
        /// <value>
        /// The unchecked radio Unicode.
        /// </value>
        public string UncheckedRadioUnicode { get; set; } = "〇";

        /// <summary>
        /// Gets or sets the checked non radio.
        /// </summary>
        /// <value>
        /// The checked non radio.
        /// </value>
        public string CheckedNonRadio { get; set; } = "[x]";

        /// <summary>
        /// Gets or sets the unchecked non radio.
        /// </summary>
        /// <value>
        /// The unchecked non radio.
        /// </value>
        public string UncheckedNonRadio { get; set; } = "[ ]";

        /// <summary>
        /// Gets or sets the checked radio.
        /// </summary>
        /// <value>
        /// The checked radio.
        /// </value>
        public string CheckedRadio { get; set; } = "(o)";

        /// <summary>
        /// Gets or sets the unchecked radio.
        /// </summary>
        /// <value>
        /// The unchecked radio.
        /// </value>
        public string UncheckedRadio { get; set; } = "( )";

        /// <summary>
        /// Gets the checked character.
        /// </summary>
        /// <param name="isRadio">if set to <c>true</c> [is radio].</param>
        /// <param name="isUnicode">if set to <c>true</c> [is Unicode].</param>
        /// <returns></returns>
        public string GetCheckedChar(bool isRadio, bool isUnicode)
        {
            if (isRadio)
            {
                return isUnicode ? CheckedRadioUnicode : CheckedRadio;
            }
            return isUnicode ? CheckedNonRadioUnicode : CheckedNonRadio;
        }

        /// <summary>
        /// Gets the unchecked character.
        /// </summary>
        /// <param name="isRadio">if set to <c>true</c> [is radio].</param>
        /// <param name="isUnicode">if set to <c>true</c> [is Unicode].</param>
        /// <returns></returns>
        public string GetUncheckedChar(bool isRadio, bool isUnicode)
        {
            if (isRadio)
            {
                return isUnicode ? UncheckedRadioUnicode : UncheckedRadio;
            }
            return isUnicode ? UncheckedNonRadioUnicode : UncheckedNonRadio;
        }
    }
}