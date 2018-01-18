using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace McMaster.Extensions.CommandLineUtils
{
    public static partial class Prompt
    {
        /// <summary>
        /// Represents a checkbox
        /// </summary>
        /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
        private sealed class CheckboxSelection : INotifyPropertyChanged
        {
            private bool _isSelected;

            public CheckboxSelection(string title)
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
}