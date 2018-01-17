using System;
using System.Collections.Generic;
using System.Linq;

namespace McMaster.Extensions.CommandLineUtils
{
    public static partial class Prompt
    {
        private class CheckboxManager
        {
            private int selectorPosition;
            private int StartPosition { get; set; }
            public List<CheckboxSelection> Boxes { get; private set; }

            public void ClearConsoleRange(int starty, int endy, int x = -1)
            {
                if (x < 0)
                    x = Console.WindowWidth;

                for (var i = starty; i < endy; i++)
                {
                    ClearConsoleLine(i);
                }
            }

            public static void ClearConsoleLine(int line)
            {
                WriteTemporarly(() =>
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new string(' ', Console.WindowWidth));
                }, 0, line);
            }

            private int SelectorPosition
            {
                get => selectorPosition;
                set
                {
                    if (selectorPosition == value)
                    {
                        return;
                    }

                    selectorPosition = value;
                    Redraw();
                }
            }

            private void End()
            {
                Clear();
            }

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

            public CheckboxManager(IEnumerable<string> boxes)
            {
                Boxes = boxes.Select(i => new CheckboxSelection(i)).ToList();
                StartPosition = Console.CursorTop + 1;
                Draw();
            }

            public CheckboxManager(params string[] boxes) : this(boxes.ToList())
            {
            }

            public void Clear()
            {
                ClearConsoleRange(StartPosition, Boxes.Count + StartPosition);
            }

            public void Redraw()
            {
                Clear();
                Draw();
            }

            public void Draw()
            {
                WriteTemporarly(() =>
                {
                    foreach (var box in Boxes)
                    {
                        Console.WriteLine($"  ({(box.IsSelected ? "*" : " ")}) {box.Text}");
                    }
                }, 0, StartPosition);

                DrawSelector();
            }

            public void Checked()
            {
                Boxes[SelectorPosition].IsSelected = !Boxes[SelectorPosition].IsSelected;
                Draw();
            }

            public void DrawSelector()
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

            public void GoUp()
            {
                if (SelectorPosition - 1 >= 0)
                    SelectorPosition--;
            }

            public void GoDown()
            {
                if (SelectorPosition + 1 < Boxes.Count)
                    SelectorPosition++;
            }
        }
    }
}