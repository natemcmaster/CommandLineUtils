using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class OptionsManager
    {
        private readonly ObservableCollection<OptionsOption> _boxes =
            new ObservableCollection<OptionsOption>();

        private object _model;
        private int _selectorPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsManager"/> class.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="options">The options.</param>
        /// <param name="boxes">The boxes.</param>
        public OptionsManager(string prompt, OptionsManagerOptions options, IEnumerable<string> boxes)
        {
            if (boxes == null)
            {
                throw new ArgumentNullException(nameof(boxes));
            }

            if (boxes.Any())
            {
                this._boxes =
                    new ObservableCollection<OptionsOption>(boxes.Select(i => new OptionsOption(i)));
            }

            Options = options ?? new OptionsManagerOptions();

            if (!string.IsNullOrEmpty(prompt))
            {
                Console.WriteLine(prompt);
            }

            if (Options.DisplayHelpText)
            {
                Console.WriteLine(Options.HelpText);
            }

            StartPosition = Console.CursorTop;
            Draw();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsManager"/> class.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="model">The model.</param>
        /// <param name="options">The options.</param>
        public OptionsManager(string prompt, object model, OptionsManagerOptions options = null) : this(prompt,
            options, null)
        {
            Model = model;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsManager"/> class.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="type">The type.</param>
        /// <param name="options">The options.</param>
        public OptionsManager(string prompt, Type type, OptionsManagerOptions options = null) : this(prompt,
            options, null)
        {
            FillBoxes(type);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsManager"/> class.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="boxes">The boxes.</param>
        public OptionsManager(string prompt, IEnumerable<string> boxes) : this(prompt, null, boxes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsManager"/> class.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="boxes">The boxes.</param>
        public OptionsManager(string prompt, params string[] boxes) : this(prompt, null, boxes?.ToList())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsManager"/> class.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="options">The options.</param>
        /// <param name="boxes">The boxes.</param>
        public OptionsManager(string prompt, OptionsManagerOptions options, params string[] boxes) : this(prompt,
            options,
            boxes?.ToList())
        {
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public OptionsManagerOptions Options { get; }

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
        public ObservableCollection<OptionsOption> Boxes
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
                if (_selectorPosition == value)
                {
                    return;
                }

                _selectorPosition = value;
                Redraw();
            }
        }

        private void FillBoxes(Type type)
        {
            if (!Options.OnlyUseMarked)
            {
                foreach (var property in type.GetTypeInfo()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(i => i.PropertyType == typeof(bool)))
                {
                    Boxes.Add(new OptionsOption(property.Name));
                }
            }
            else
            {
                foreach (var property in type.GetTypeInfo()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(i => i.PropertyType == typeof(bool) &&
                                i.GetCustomAttribute<UseOnOptionsAttribute>() != null))
                {
                    Boxes.Add(new OptionsOption(property.Name));
                }
            }

            Redraw();
        }

        private void BoxesOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            foreach (var item in notifyCollectionChangedEventArgs.NewItems)
            {
                if (item is OptionsOption checkboxSelection)
                {
                    checkboxSelection.PropertyChanged += (o, args) =>
                    {
                        if (args.PropertyName != nameof(OptionsOption.IsSelected))
                        {
                            return;
                        }

                        SetPropertyValue(checkboxSelection.Title, checkboxSelection.IsSelected);
                        Redraw();
                    };
                }
            }
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
            {
                x = Console.WindowWidth;
            }

            for (var i = startY; i < endY; i++)
            {
                ClearConsoleLine(i, x);
            }
        }

        private static void ClearConsoleLine(int line, int length)
        {
            WriteTemporarly(() =>
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', length));
            }, 0, line);
        }

        /// <summary>
        /// Shows this instance.
        /// </summary>
        public void Show()
        {
            Console.CursorVisible = false;

            while (true)
            {
                var key = Console.ReadKey(true);

                if (key.Modifiers != 0)
                    continue;

                if (Options.Keys.Finalize.Contains(key.Key))
                {
                    if (End())
                    {
                        break;
                    }
                }
                else if (Options.Keys.Movedown.Contains(key.Key))
                {
                    GoDown();
                }
                else if (Options.Keys.Moveup.Contains(key.Key))
                {
                    GoUp();
                }
                else if (Options.Keys.Select.Contains(key.Key))
                {
                    Checked();
                }

                Redraw();
            }
        }

        private bool End()
        {
            if (Options.IsSelectionRequired && !Boxes.Any(i => i.IsSelected))
            {
                return false;
            }

            Clear(Options.DisplayHelpText ? StartPosition - 1 : StartPosition);
            if (Options.DisplayHelpText)
            {
                Console.SetCursorPosition(0, StartPosition - 1);
            }

            Console.CursorVisible = true;
            return true;
        }

        private void Clear(int position = -1)
        {
            if (position < 0)
            {
                position = StartPosition;
            }

            ClearConsoleRange(position, Boxes.Count + StartPosition);
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
                {
                    Console.WriteLine(
                        $"  {(box.IsSelected ? Options.CheckedChar : Options.UncheckedChar)} {box.Title}");
                }
            }, 0, StartPosition);

            DrawSelector();
        }

        private void Checked()
        {
            Boxes[SelectorPosition].IsSelected = !Boxes[SelectorPosition].IsSelected;

            if (Options.IsRadio)
            {
                foreach (var checkboxSelection in Boxes.Except(new[] { Boxes[SelectorPosition] }))
                {
                    checkboxSelection.IsSelected = false;
                }
            }

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
            if (SelectorPosition - 1 >= 0)
            {
                SelectorPosition--;
            }
        }

        private void GoDown()
        {
            if (SelectorPosition + 1 < Boxes.Count)
            {
                SelectorPosition++;
            }
        }
    }
}
