using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a check box
    /// </summary>
    /// <seealso cref="T:System.ComponentModel.INotifyPropertyChanged" />
    internal class OptionsOption : INotifyPropertyChanged
    {
        private bool _isSelected;

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
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a header.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a header; otherwise, <c>false</c>.
        /// </value>
        internal bool IsHeader { get; set; }
        
        internal bool IsChildren { get; set; }
        
        internal IEnumerable<OptionsOption> Children { get; set; }

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
