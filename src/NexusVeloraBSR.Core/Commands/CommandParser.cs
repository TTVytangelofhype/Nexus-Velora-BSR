namespace NexusVeloraBSR.Core.Commands
{
    public enum CommandType { None, Request, Queue, Undo, Help, Open, Close, Skip, Remove, Clear, Block, SetLimit, SetQueue }

    public sealed class ParsedCommand
    {
        public CommandType Type { get; set; }
        public string Argument { get; set; } = string.Empty;
    }

    public static class CommandParser
    {
        public static ParsedCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message) || message[0] != '!') return new ParsedCommand();
            var parts = message.Trim().Split(new[] { ' ' }, 2);
            var cmd = parts[0].ToLowerInvariant();
            var arg = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            switch (cmd)
            {
                case "!bsr": return new ParsedCommand { Type = CommandType.Request, Argument = arg };
                case "!queue": return new ParsedCommand { Type = CommandType.Queue };
                case "!oops": return new ParsedCommand { Type = CommandType.Undo };
                case "!bsrhelp": return new ParsedCommand { Type = CommandType.Help };
                case "!open": return new ParsedCommand { Type = CommandType.Open };
                case "!close": return new ParsedCommand { Type = CommandType.Close };
                case "!skip": return new ParsedCommand { Type = CommandType.Skip };
                case "!remove": return new ParsedCommand { Type = CommandType.Remove, Argument = arg };
                case "!clearqueue": return new ParsedCommand { Type = CommandType.Clear };
                case "!setlimit": return new ParsedCommand { Type = CommandType.SetLimit, Argument = arg };
                case "!setqueue": return new ParsedCommand { Type = CommandType.SetQueue, Argument = arg };
                case "!block": return new ParsedCommand { Type = CommandType.Block, Argument = arg };
                default: return new ParsedCommand();
            }
        }
    }
}
