using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace McMaster.Extensions.CommandLineUtils
{
    internal class ReflectionCommandLineApplicationBuilder<T>
        where T : class, new()
    {
        private readonly CommandLineApplication _app;
        private readonly List<Action<T>> _binders = new List<Action<T>>();

        public ReflectionCommandLineApplicationBuilder()
        {
            _app = new CommandLineApplication();
            _app.OnExecute(() => 0);

            var type = typeof(T).GetTypeInfo();
            var appAttr = type.GetCustomAttribute<CommandLineApplicationAttribute>();

            if (appAttr != null)
            {
                _app.ThrowOnUnexpectedArgument = appAttr.ThrowOnUnexpectedArgs;
            }

            var props = type.GetProperties();
            if (props != null)
            {
                var argOrder = new SortedDictionary<int, CommandArgument>();

                // TODO extract methods
                foreach (var prop in props)
                {
                    var optionAttr = prop.GetCustomAttribute<OptionAttribute>();
                    var argumentAttr = prop.GetCustomAttribute<ArgumentAttribute>();
                    var setter = prop.GetSetMethod();

                    if (optionAttr != null && argumentAttr != null)
                    {
                        throw new InvalidOperationException(
                            "Cannot specify both " + nameof(OptionAttribute) + " and " + nameof(ArgumentAttribute)
                            + " on a property " + type.Name + "." + prop.Name);
                    }

                    if (argumentAttr != null)
                    {
                        var argument = new CommandArgument
                        {
                            Name = argumentAttr.Name,
                            Description = argumentAttr.Description,
                            ShowInHelpText = argumentAttr.ShowInHelpText,
                            MultipleValues = argumentAttr.MultipleValues,
                        };

                        argOrder.Add(argumentAttr.Order, argument);

                        OnBind(o => setter.Invoke(o, new[] { argument.Value }));
                    }

                    if (optionAttr != null)
                    {
                        var optionType = optionAttr.OptionType ?? GetDefaultOptionType(prop.PropertyType);
                        CommandOption option;
                        if (optionAttr.Template != null)
                        {
                            option = new CommandOption(optionAttr.Template, optionType);
                        }
                        else
                        {
                            var sb = new StringBuilder();
                            sb.Append(char.ToLowerInvariant(prop.Name[0]));
                            for (var i = 1; i < prop.Name.Length; i++)
                            {
                                var ch = prop.Name[i];
                                if (char.IsUpper(ch))
                                {
                                    sb.Append('-').Append(char.ToLowerInvariant(ch));
                                }
                                else
                                {
                                    sb.Append(ch);
                                }
                            }

                            option = new CommandOption(optionType)
                            {
                                ShortName = prop.Name.Substring(0, 1).ToLowerInvariant(),
                                LongName = sb.ToString(),
                                ValueName = prop.Name,
                            };
                        }

                        option.Description = optionAttr.Description;
                        option.ShowInHelpText = optionAttr.ShowInHelpText;
                        option.Inherited = optionAttr.Inherited;

                        _app.Options.Add(option);
                        OnBind(o =>
                        {
                            switch (optionType)
                            {
                                case CommandOptionType.MultipleValue:
                                    setter.Invoke(o, new[] { option.Values });
                                    break;
                                case CommandOptionType.SingleValue:
                                    var value = option.Value();
                                    if (prop.PropertyType == typeof(int))
                                    {
                                        if (!int.TryParse(value, out var intValue))
                                        {
                                            throw new CommandParsingException(_app, $"Invalid option given for --{option.LongName}. Expected a valid number.");
                                        }
                                        setter.Invoke(o, new object[] { intValue });
                                    }
                                    else if (prop.PropertyType == typeof(string))
                                    {
                                        setter.Invoke(o, new object[] { value });
                                    }
                                    else
                                    {
                                        throw new ArgumentException("Error parsing " + prop.DeclaringType.Name + "." + prop.Name + ". Not sure how to bind to a property of type " + prop.PropertyType.Name);
                                    }
                                    break;
                                case CommandOptionType.NoValue:
                                    setter.Invoke(o, new object[] { option.HasValue() });
                                    break;
                            }
                        });
                    }

                    foreach (var arg in argOrder.Values)
                    {
                        if (_app.Arguments.Count > 0)
                        {
                            var lastArg = _app.Arguments[_app.Arguments.Count - 1];
                            if (lastArg.MultipleValues)
                            {
                                throw new InvalidOperationException($"The last argument '{lastArg.Name}' accepts multiple values. No more argument can be added.");
                            }
                        }

                        _app.Arguments.Add(arg);
                    }
                }

            }
        }

        public T Execute(string[] args)
        {
            var options = new T();
            _app.Execute(args);

            foreach (var binder in _binders)
            {
                binder(options);
            }

            return options;
        }

        private void OnBind(Action<T> onBind)
        {
            _binders.Add(onBind);
        }

        private static CommandOptionType GetDefaultOptionType(Type type)
        {
            if (type == typeof(bool))
            {
                return CommandOptionType.NoValue;
            }

            // TODO make this more robust to handle other basic types
            if (type == typeof(string) || type == typeof(int) || type == typeof(long))
            {
                return CommandOptionType.SingleValue;
            }

            // TODO make this more robust to handle types like ICollection, IList, IReadOnlyList
            if (type.IsArray)
            {
                return CommandOptionType.MultipleValue;
            }

            // TODO better errors. Which property?
            throw new InvalidOperationException("Cannot infer the " + nameof(CommandOptionType) + " based on the property's CLR type");
        }
    }
}
