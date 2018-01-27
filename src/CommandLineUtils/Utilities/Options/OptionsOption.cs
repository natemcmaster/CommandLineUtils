using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a check box
    /// </summary>
    /// <seealso cref="T:System.ComponentModel.INotifyPropertyChanged" />
    class OptionsOption : INotifyPropertyChanged
    {
        private bool isSelected;

        public OptionsOption(string title)
        {
            Title = title;
        }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a header.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a header; otherwise, <c>false</c>.
        /// </value>
        public bool IsHeader { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return $"{Title}: {IsSelected}";
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
