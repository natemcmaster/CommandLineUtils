using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;

namespace McMaster.Extensions.CommandLineUtils
{
    public static partial class Prompt
    {
        private class CheckboxManager
        {
            private readonly ObservableCollection<CheckboxSelection> _boxes =
                new ObservableCollection<CheckboxSelection>();

            private object _model;
            private int _selectorPosition;

            /// <summary>
            /// Initializes a new instance of the <see cref="CheckboxManager"/> class.
            /// </summary>
            /// <param name="options">The options.</param>
            /// <param name="boxes">The boxes.</param>
            public CheckboxManager(CheckboxManagerOptions options, IEnumerable<string> boxes)
            {
                if (!Equals(Console.OutputEncoding, Encoding.UTF8))
                    Console.OutputEncoding = Encoding.UTF8;

                if (boxes != null)
                    _boxes = new ObservableCollection<CheckboxSelection>(boxes.Select(i => new CheckboxSelection(i)));

                Options = options ?? new CheckboxManagerOptions();
                StartPosition = Console.CursorTop;
                Draw();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CheckboxManager"/> class.
            /// </summary>
            /// <param name="model">The model.</param>
            /// <param name="options">The options.</param>
            public CheckboxManager(object model, CheckboxManagerOptions options = null) : this(options, null)
            {
                Model = model;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CheckboxManager"/> class.
            /// </summary>
            /// <param name="type">The type.</param>
            /// <param name="options">The options.</param>
            public CheckboxManager(Type type, CheckboxManagerOptions options = null) : this(options, null)
            {
                FillBoxes(type);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CheckboxManager"/> class.
            /// </summary>
            /// <param name="boxes">The boxes.</param>
            public CheckboxManager(IEnumerable<string> boxes) : this(null, boxes)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CheckboxManager"/> class.
            /// </summary>
            /// <param name="boxes">The boxes.</param>
            public CheckboxManager(params string[] boxes) : this(null, boxes?.ToList())
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CheckboxManager"/> class.
            /// </summary>
            /// <param name="options">The options.</param>
            /// <param name="boxes">The boxes.</param>
            public CheckboxManager(CheckboxManagerOptions options, params string[] boxes) : this(options,
                boxes?.ToList())
            {
            }

            /// <summary>
            /// Gets the options.
            /// </summary>
            /// <value>
            /// The options.
            /// </value>
            public CheckboxManagerOptions Options { get; }

            /// <summary>
            /// Gets or sets the model.
            /// </summary>
            /// <value>
            /// The model.
            /// </value>
            public object Model
            {
                get => _model;
                private set
                {
                    _model = value;
                    FillBoxes(value.GetType());
                }
            }

            private int StartPosition { get; }

            /// <summary>
            /// Gets the check boxes.
            /// </summary>
            /// <value>
            /// The boxes.
            /// </value>
            public ObservableCollection<CheckboxSelection> Boxes
            {
                get
                {
                    _boxes.CollectionChanged += BoxesOnCollectionChanged;
                    return _boxes;
                }
            }


            /// <summary>
            /// Gets or sets the selector ('>') position.
            /// </summary>
            /// <value>
            /// The selector position.
            /// </value>
            private int SelectorPosition
            {
                get => _selectorPosition;
                set
                {
                    if (_selectorPosition == value) return;

                    _selectorPosition = value;
                    Redraw();
                }
            }

            private void FillBoxes(Type type)
            {
                foreach (var property in type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(i => i.PropertyType == typeof(bool)))
                    Boxes.Add(new CheckboxSelection(property.Name));
                Redraw();
            }

            private void BoxesOnCollectionChanged(object sender,
                NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
            {
                foreach (var item in notifyCollectionChangedEventArgs.NewItems)
                    if (item is CheckboxSelection checkboxSelection)
                        checkboxSelection.PropertyChanged += (o, args) =>
                        {
                            if (args.PropertyName != nameof(CheckboxSelection.IsSelected)) return;
                            SetPropertyValue(checkboxSelection.Title, checkboxSelection.IsSelected);
                            Redraw();
                        };
            }

            private void SetPropertyValue(string name, object value)
            {
                Model?.GetType().GetTypeInfo().GetProperty(name).SetValue(Model, value);
            }

            private void GetPropertyValue(string name, object value)
            {
                Model?.GetType().GetTypeInfo().GetProperty(name).GetValue(value);
            }

            private static void ClearConsoleRange(int startY, int endY, int x = -1)
            {
                if (x < 0)
                    x = Console.WindowWidth;

                for (var i = startY; i < endY; i++)
                    ClearConsoleLine(i);
            }

            private static void ClearConsoleLine(int line)
            {
                WriteTemporarly(() =>
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new string(' ', Console.WindowWidth));
                }, 0, line);
            }

            private void End()
            {
                Clear();
            }

            /// <summary>
            /// Shows this instance.
            /// </summary>
            public void Show()
            {
                while (true)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Enter)
                    {
                        End();
                        break;
                    }

                    switch (key.Key)
                    {
                        case ConsoleKey.DownArrow:
                            GoDown();
                            break;
                        case ConsoleKey.UpArrow:
                            GoUp();
                            break;
                        case ConsoleKey.Spacebar:
                            Checked();
                            break;
                    }
                }
            }

            private void Clear()
            {
                ClearConsoleRange(StartPosition, Boxes.Count + StartPosition);
            }

            private void Redraw()
            {
                Clear();
                Draw();
            }

            private void Draw()
            {
                WriteTemporarly(() =>
                {
                    foreach (var box in Boxes)
                        Console.WriteLine(
                            $"  {(box.IsSelected ? Options.CheckedChar : Options.UncheckedChar)} {box.Title}");
                }, 0, StartPosition);

                DrawSelector();
            }

            private void Checked()
            {
                Boxes[SelectorPosition].IsSelected = !Boxes[SelectorPosition].IsSelected;

                if (Options.IsRadio)
                    foreach (var checkboxSelection in Boxes.Except(new[] {Boxes[SelectorPosition]}))
                        checkboxSelection.IsSelected = false;

                Draw();
            }

            private void DrawSelector()
            {
                WriteTemporarly(() => Console.Write(">"), 0, StartPosition + SelectorPosition);
            }

            private static void WriteTemporarly(Action action, int x = 0, int y = 0)
            {
                var oldPositionY = Console.CursorTop;
                var oldPositionX = Console.CursorLeft;
                Console.SetCursorPosition(x, y);
                action.Invoke();
                Console.SetCursorPosition(oldPositionX, oldPositionY);
            }

            private void GoUp()
            {
                if (SelectorPosition - 1 >= 0) SelectorPosition--;
            }

            private void GoDown()
            {
                if (SelectorPosition + 1 < Boxes.Count) SelectorPosition++;
            }
        }
    }
}