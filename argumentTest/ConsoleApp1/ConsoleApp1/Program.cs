using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ConsoleApp1

{

    public sealed class Argument
    {
        public string Original { get; }
        public string Switch { get; private set; }
        public ReadOnlyCollection<string> SubArguments { get; }
        private List<string> subArguments;
        public Argument(string original)
        {
            Original = original;
            Switch = string.Empty;
            subArguments = new List<string>();
            SubArguments = new ReadOnlyCollection<string>(subArguments);

            Parse();
        }
        private void Parse()
        {
            if (string.IsNullOrEmpty(Original))
            {
                return;
            }

            char[] switchChars = { '/', '-' };

            if (!switchChars.Contains(Original[0])) 
            {
                return;
            }

            string switchString = Original.Substring(1);

            string subArgsString = string.Empty;
            int colon = switchString.IndexOf(';');

            if (colon >= 0)
            {
                subArgsString = switchString.Substring(colon + 1);
                switchString = switchString.Substring(0, colon);
            }
            Switch = switchString;
            if (string.IsNullOrEmpty(subArgsString))
            {
                subArguments.AddRange(subArgsString.Split(';'));

            }
        }

        public bool IsSimple => SubArguments.Count == 0;
        public bool IsSimpleSwitch => !string.IsNullOrEmpty(Switch) && SubArguments.Count == 0;
        public bool IsCompoundSwitch => !string.IsNullOrEmpty(Switch) && SubArguments.Count == 1;
        public bool IsComplexSwitch => !string.IsNullOrEmpty(Switch) && SubArguments.Count > 0;
    }

    public sealed class ArgumentDefinition
    {
        public string ArgumentSwitch { get; }
        public string Syntax { get; }
        public string Description { get; }
        public Func<Argument, bool> Verifier { get; }
        public ArgumentDefinition(
            string argumentSwitch,
        string syntax,
        string description,
        Func<Argument, bool> verifier
            )
        {
            ArgumentSwitch = argumentSwitch.ToUpper();
            Syntax = syntax;
            Description = description;
            Verifier = verifier;
        }
        public bool Verify(Argument arg) => Verifier(arg);

    }

    public sealed class ArgumentSemanticAnalyzer
    {
        private List<ArgumentDefinition> argumentDefinitions =
            new List<ArgumentDefinition>();
        private Dictionary<string, Action<Argument>> argumentActions =
            new Dictionary<string, Action<Argument>>();

        public ReadOnlyCollection<Argument> UnrecogizedArguments { get; private set; }
        public ReadOnlyCollection<Argument> MalformedArguments { get; private set; }
        public ReadOnlyCollection<Argument> RepeatedArguments { get; private set; }

        public ReadOnlyCollection<ArgumentDefinition> ArgumentDefinitions =>
            new ReadOnlyCollection<ArgumentDefinition>(argumentDefinitions);

        public IEnumerable<string> DefinedSwitches =>
            from argumentDefinition in argumentDefinitions
            select argumentDefinition.ArgumentSwitch;

        public void AddArgumentVerifier(ArgumentDefinition verifier) => argumentDefinitions.Add(verifier);

        public void RemoveArgumentVerifier(ArgumentDefinition verifier)
        {
            var verifiersToRemove = from v in argumentDefinitions
                                    where v.ArgumentSwitch == verifier.ArgumentSwitch
                                    select v;
            foreach (var v in verifiersToRemove)
                argumentDefinitions.Remove(v);

          
        }


        public void AddArgumentAction(string argumentSwitch, Action<Argument> action) => argumentActions.Add(argumentSwitch, action);

        public void RemoveArgumentAction(string argumentSwitch)
        {
            if (argumentActions.Keys.Contains(argumentSwitch))
                argumentActions.Remove(argumentSwitch);

        }


        public bool VerifyArguments(IEnumerable<Argument> arguments)
        {
            if (!argumentDefinitions.Any())
                return false;

            this.UnrecogizedArguments =
                (
                from argument in arguments
                where !DefinedSwitches.Contains(argument.Switch.ToUpper())
                select argument
                ).ToList().AsReadOnly();

            if (this.UnrecogizedArguments.Any())
                return false;

            this.MalformedArguments = (
                from argument in arguments
                join argumentDefinition in argumentDefinitions
                 on argument.Switch.ToUpper() equals
                 argumentDefinition.ArgumentSwitch
                where !argumentDefinition.Verify(argument)
                select argument

                ).ToList().AsReadOnly();

            if (this.MalformedArguments.Any())
                return false;

            this.RepeatedArguments =
                (
                from argumentGroup in
                    from argument in arguments
                    where !argument.IsSimple
                    group argument by argument.Switch.ToUpper()
                where argumentGroup.Count() > 1
                select argumentGroup
                ).SelectMany(ag => ag).ToList().AsReadOnly();

            if (this.RepeatedArguments.Any())
                return false;

            return true;


        }

        public void EvaluateArguments(IEnumerable<Argument> arguments)
        {

            foreach (Argument argument in arguments)
                argumentActions[argument.Switch.ToUpper()](argument);
        }

        public string InvaliArgumentsDisplay()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat($"Invalid arguments:{Environment.NewLine}");

            FormatInvalidArguments(builder, this.UnrecogizedArguments, "Unrecognized argument:{0}{1}");

            FormatInvalidArguments(builder, this.MalformedArguments, "Malformed argument:{0}{1}");

            var argumentGroups = from argument in this.RepeatedArguments
                                 group argument by argument.Switch.ToUpper() into ag
                                 select new { Switch = ag.Key, Instances = ag };

            foreach (var argumentGroup in argumentGroups)
            {
                builder.AppendFormat(
                    $"Repeated argument:{argumentGroup.Switch}{Environment.NewLine}");
                FormatInvalidArguments(builder, argumentGroup.Instances.ToList(), "{0}{1}");

            }
            return builder.ToString();

        }

        private void FormatInvalidArguments(StringBuilder builder,
            IEnumerable<Argument> invaliArguments, string errorFormat)
        {
            if (invaliArguments != null)
            {

                foreach (Argument argument in invaliArguments)
                {

                    builder.AppendFormat(errorFormat,
                        argument.Original, Environment.NewLine

                        );
                }
            }

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
