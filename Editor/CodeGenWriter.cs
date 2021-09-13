using System.Text;

namespace KV.Events.Editor
{
    public class CodeGenWriter
    {
        private const int spacesPerIndentLevel = 4;
        private int _indentLevel = 0;
        
        public StringBuilder StringBuilder { get; }

        public CodeGenWriter(StringBuilder stringBuilder) => StringBuilder = stringBuilder;
        
        public void BeginBlock()
        {
            WriteIndent();
            StringBuilder.Append("{\n");
            ++_indentLevel;
        }

        public void EndBlock()
        {
            --_indentLevel;
            WriteIndent();
            StringBuilder.Append("}\n");
        }
        
        public void WriteLine()
        {
            StringBuilder.Append('\n');
        }

        public void WriteLine(string text)
        {
            WriteIndent();
            StringBuilder.Append(text);
            StringBuilder.Append('\n');
        }
        
        public void Write(string text)
        {
            StringBuilder.Append(text);
        }
        
        public void WriteIndent()
        {
            for (var i = 0; i < _indentLevel; i++)
            {
                for (var n = 0; n < spacesPerIndentLevel; ++n)
                {
                    StringBuilder.Append(' ');
                }
            }
        }

        public void Clear()
        {
            StringBuilder.Clear();
        }

        public override string ToString()
        {
            return StringBuilder?.ToString();
        }
    }
}