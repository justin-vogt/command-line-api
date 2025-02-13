﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine.Parsing
{
    public static class CommandResultExtensions
    {
        internal static bool TryGetValueForArgument(
            this CommandResult commandResult,
            IValueDescriptor valueDescriptor,
            out object? value)
        {
            var arguments = commandResult.Command.Arguments;

            for (var i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];

                if (valueDescriptor.ValueName.IsMatch(argument.Name))
                {
                    if (commandResult.FindResultFor(argument) is { } argumentResult)
                    {
                        value = argumentResult.GetValueOrDefault();
                    }
                    else
                    {
                        value = valueDescriptor.GetDefaultValue();
                    }

                    return true;
                }
            }

            value = null;
            return false;
        }

        internal static bool TryGetValueForOption(
            this CommandResult commandResult,
            IValueDescriptor valueDescriptor,
            out object? value)
        {
            var options = commandResult.Command.Options;

            for (var i = 0; i < options.Count; i++)
            {
                if (options[i] is Option { DisallowBinding: false } option)
                {
                    var hasMatchingAlias =
                        HasMatchingAlias(valueDescriptor, option);

                    if (hasMatchingAlias)
                    {
                        var optionResult = commandResult.FindResultFor(option);

                        if (optionResult?.ConvertIfNeeded(valueDescriptor.ValueType) is SuccessfulArgumentConversionResult successful)
                        {
                            value = successful.Value;
                            return true;
                        }
                    }
                }
            }

            value = null;
            return false;

            static bool HasMatchingAlias(
                IValueDescriptor valueDescriptor,
                Option option)
            {
                if (option.HasAlias(valueDescriptor.ValueName))
                {
                    return true;
                }

                foreach (var alias in option.Aliases)
                {
                    if (valueDescriptor.ValueName.IsMatch(alias))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}