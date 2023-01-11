using System.Data;
using System.Text;

namespace MessengerAnalysis;

public class CsvStream : StreamWriter
{
    public CsvStream(Stream stream) : base(stream)
    {
    }

    public CsvStream(Stream stream, Encoding encoding) : base(stream, encoding)
    {
    }

    public CsvStream(Stream stream, Encoding encoding, int bufferSize) : base(stream, encoding, bufferSize)
    {
    }

    public CsvStream(Stream stream, Encoding? encoding = null, int bufferSize = -1, bool leaveOpen = false) : base(stream, encoding, bufferSize, leaveOpen)
    {
    }

    public CsvStream(string path) : base(path)
    {
    }

    public CsvStream(string path, FileStreamOptions options) : base(path, options)
    {
    }

    public CsvStream(string path, bool append) : base(path, append)
    {
    }

    public CsvStream(string path, bool append, Encoding encoding) : base(path, append, encoding)
    {
    }

    public CsvStream(string path, bool append, Encoding encoding, int bufferSize) : base(path, append, encoding, bufferSize)
    {
    }

    public CsvStream(string path, Encoding encoding, FileStreamOptions options) : base(path, encoding, options)
    {
    }

    public void Field(object value)
    {
        Write($"\"{value?.ToString()?.Replace(";", ".") ?? ""}\";");
    }

    public void Row()
    {
        WriteLine();
    }
}