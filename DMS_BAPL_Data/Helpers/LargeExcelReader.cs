using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DMS_BAPL_Data.Helpers
{
    /// <summary>
    /// Read-only .xlsx reader for large import files (Cogxim sale register / dispatch report).
    /// Reads the sheet row by row with the OpenXML SAX reader, so
    ///  * there is no 32,767-character cell limit (ClosedXML refuses such files),
    ///  * comments, pictures, pivot tables and drawings are simply ignored,
    ///  * memory stays low for sheets with lakhs of rows (only the chosen sheet is loaded).
    /// The API mirrors the small part of ClosedXML the importers use
    /// (Worksheets, Row(n), LastRowUsed(), IsEmpty(), Cell(c).Value.IsNumber ...).
    /// </summary>
    public sealed class LargeExcelWorkbook : IDisposable
    {
        private readonly SpreadsheetDocument _doc;
        private readonly Stream? _ownedStream;
        private readonly List<string> _sharedStrings = new();
        private readonly HashSet<uint> _dateStyles = new();
        private bool _sharedLoaded;

        public IReadOnlyList<LargeExcelSheet> Worksheets { get; }

        public LargeExcelWorkbook(Stream excel)
        {
            // OpenXML needs a seekable stream (an upload stream may not be)
            Stream s = excel;
            if (!excel.CanSeek)
            {
                var ms = new MemoryStream();
                excel.CopyTo(ms);
                ms.Position = 0;
                s = ms;
                _ownedStream = ms;
            }
            _doc = SpreadsheetDocument.Open(s, false);
            var wbPart = _doc.WorkbookPart ?? throw new ArgumentException("The file is not an Excel workbook (.xlsx).");
            LoadDateStyles(wbPart);

            var sheets = new List<LargeExcelSheet>();
            foreach (var sh in wbPart.Workbook.Descendants<Sheet>())
            {
                var id = sh.Id?.Value;
                if (string.IsNullOrEmpty(id)) continue;
                if (wbPart.GetPartById(id) is not WorksheetPart part) continue;   // chart sheets etc.
                sheets.Add(new LargeExcelSheet(this, sh.Name?.Value ?? $"Sheet{sheets.Count + 1}", part));
            }
            if (sheets.Count == 0) throw new ArgumentException("The workbook has no worksheet.");
            Worksheets = sheets;
        }

        internal IReadOnlyList<string> SharedStrings
        {
            get
            {
                if (_sharedLoaded) return _sharedStrings;
                _sharedLoaded = true;
                var part = _doc.WorkbookPart?.SharedStringTablePart;
                if (part == null) return _sharedStrings;
                using var reader = OpenXmlReader.Create(part);
                while (reader.Read())
                {
                    if (reader.ElementType != typeof(SharedStringItem) || !reader.IsStartElement) continue;
                    var item = (SharedStringItem)reader.LoadCurrentElement()!;
                    // plain text + rich-text runs; phonetic hints (rPh) are not part of the value
                    _sharedStrings.Add(string.Concat(item.Descendants<Text>()
                        .Where(t => t.Parent is not PhoneticRun)
                        .Select(t => t.Text)));
                }
                return _sharedStrings;
            }
        }

        internal bool IsDateStyle(uint? styleIndex) => styleIndex.HasValue && _dateStyles.Contains(styleIndex.Value);

        private static readonly Regex QuotedOrBracketed = new("\"[^\"]*\"|\\[[^\\]]*\\]|\\\\.", RegexOptions.Compiled);

        private void LoadDateStyles(WorkbookPart wbPart)
        {
            var stylesheet = wbPart.WorkbookStylesPart?.Stylesheet;
            if (stylesheet?.CellFormats == null) return;

            var custom = new Dictionary<uint, string>();
            if (stylesheet.NumberingFormats != null)
                foreach (var nf in stylesheet.NumberingFormats.Elements<NumberingFormat>())
                    if (nf.NumberFormatId?.Value is uint id) custom[id] = nf.FormatCode?.Value ?? "";

            uint index = 0;
            foreach (var cf in stylesheet.CellFormats.Elements<CellFormat>())
            {
                var fmt = cf.NumberFormatId?.Value ?? 0;
                var isDate = fmt is >= 14 and <= 22 or >= 27 and <= 36 or >= 45 and <= 47 or >= 50 and <= 58;
                if (!isDate && custom.TryGetValue(fmt, out var code))
                {
                    var bare = QuotedOrBracketed.Replace(code, "").ToLowerInvariant();
                    isDate = bare.IndexOfAny(new[] { 'd', 'y', 'h', 's' }) >= 0 && !bare.Contains('0') && !bare.Contains('#');
                }
                if (isDate) _dateStyles.Add(index);
                index++;
            }
        }

        public void Dispose()
        {
            _doc.Dispose();
            _ownedStream?.Dispose();
        }
    }

    public sealed class LargeExcelSheet
    {
        private readonly LargeExcelWorkbook _wb;
        private readonly WorksheetPart _part;
        private Dictionary<int, LargeExcelRow>? _rows;
        private int? _lastRow;

        internal LargeExcelSheet(LargeExcelWorkbook wb, string name, WorksheetPart part)
        {
            _wb = wb;
            Name = name;
            _part = part;
        }

        public string Name { get; }

        /// <summary>Last row number that has a value; reads the sheet on first use.</summary>
        public LargeExcelRowRef? LastRowUsed()
        {
            Load();
            return _lastRow is int n ? new LargeExcelRowRef(n) : null;
        }

        /// <summary>Quick row count for sheet lists: the sheet's own dimension (A1:CI973), else a full read.</summary>
        public int CountRowsFast()
        {
            if (_rows != null) return _lastRow ?? 0;
            try
            {
                using var reader = OpenXmlReader.Create(_part);
                while (reader.Read())
                {
                    if (reader.ElementType == typeof(SheetDimension) && reader.IsStartElement)
                    {
                        var dim = (SheetDimension)reader.LoadCurrentElement()!;
                        var refText = dim.Reference?.Value ?? "";
                        var last = refText.Split(':').Last();
                        var digits = new string(last.SkipWhile(char.IsLetter).ToArray());
                        if (int.TryParse(digits, out var n) && n > 1) return n;
                        break;
                    }
                    if (reader.ElementType == typeof(SheetData)) break;   // no dimension before the data
                }
            }
            catch { /* fall back to a full read */ }
            return LastRowUsed()?.RowNumber() ?? 0;
        }

        public LargeExcelRow Row(int rowNumber)
        {
            Load();
            return _rows!.TryGetValue(rowNumber, out var r) ? r : LargeExcelRow.Empty;
        }

        /// <summary>One row (e.g. the header) without reading the rest of the sheet.</summary>
        public LargeExcelRow PeekRow(int rowNumber)
        {
            if (_rows != null) return Row(rowNumber);
            var shared = _wb.SharedStrings;
            var nextRow = 1;
            using var reader = OpenXmlReader.Create(_part);
            while (reader.Read())
            {
                if (reader.ElementType != typeof(Row) || !reader.IsStartElement) continue;
                var row = (Row)reader.LoadCurrentElement()!;
                var rowNo = row.RowIndex?.Value is uint ri ? (int)ri : nextRow;
                nextRow = rowNo + 1;
                if (rowNo < rowNumber) continue;
                if (rowNo > rowNumber) break;
                return ParseRow(row, rowNo, shared) ?? LargeExcelRow.Empty;
            }
            return LargeExcelRow.Empty;
        }

        private void Load()
        {
            if (_rows != null) return;
            var rows = new Dictionary<int, LargeExcelRow>();
            var shared = _wb.SharedStrings;
            var nextRow = 1;
            using (var reader = OpenXmlReader.Create(_part))
            {
                while (reader.Read())
                {
                    if (reader.ElementType != typeof(Row) || !reader.IsStartElement) continue;
                    var row = (Row)reader.LoadCurrentElement()!;
                    var rowNo = row.RowIndex?.Value is uint ri ? (int)ri : nextRow;
                    nextRow = rowNo + 1;
                    if (ParseRow(row, rowNo, shared) is { } parsed) rows[rowNo] = parsed;
                }
            }
            _rows = rows;
            _lastRow = rows.Count > 0 ? rows.Keys.Max() : null;
        }

        private LargeExcelRow? ParseRow(Row row, int rowNo, IReadOnlyList<string> shared)
        {

            // compact storage: one array per row (index = column - 1)
            var found = new List<(int Col, LargeExcelValue Value)>();
            var nextCol = 1;
            foreach (var c in row.Elements<Cell>())
            {
                var col = ColumnNumber(c.CellReference?.Value) ?? nextCol;
                nextCol = col + 1;
                var v = Convert(c, shared);
                if (!v.IsBlank) found.Add((col, v));
            }
            if (found.Count == 0) return null;
            var cells = new LargeExcelValue[found.Max(f => f.Col)];
            foreach (var (col, v) in found) cells[col - 1] = v;
            return new LargeExcelRow(rowNo, cells);
        }

        private LargeExcelValue Convert(Cell c, IReadOnlyList<string> shared)
        {
            var raw = c.CellValue?.Text;
            var type = c.DataType?.Value;

            if (type == CellValues.SharedString)
                return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) && i >= 0 && i < shared.Count
                    ? LargeExcelValue.FromText(shared[i]) : LargeExcelValue.Blank;
            if (type == CellValues.InlineString)
                return LargeExcelValue.FromText(c.InlineString == null ? raw : string.Concat(c.InlineString.Descendants<Text>().Select(t => t.Text)));
            if (type == CellValues.String)
                return LargeExcelValue.FromText(raw);
            if (type == CellValues.Boolean)
                return raw == null ? LargeExcelValue.Blank : LargeExcelValue.FromBool(raw == "1" || raw.Equals("true", StringComparison.OrdinalIgnoreCase));
            if (type == CellValues.Error)
                return LargeExcelValue.Error;
            if (type == CellValues.Date)
                return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt)
                    ? LargeExcelValue.FromDate(dt) : LargeExcelValue.FromText(raw);

            if (string.IsNullOrEmpty(raw)) return LargeExcelValue.Blank;
            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) return LargeExcelValue.FromText(raw);
            if (_wb.IsDateStyle(c.StyleIndex?.Value) && d > 0 && d < 2958466)
            {
                try { return LargeExcelValue.FromDate(DateTime.FromOADate(d)); } catch { /* keep the number */ }
            }
            return LargeExcelValue.FromNumber(d);
        }

        private static int? ColumnNumber(string? cellRef)
        {
            if (string.IsNullOrEmpty(cellRef)) return null;
            var n = 0;
            foreach (var ch in cellRef)
            {
                if (!char.IsLetter(ch)) break;
                n = n * 26 + (char.ToUpperInvariant(ch) - 'A' + 1);
            }
            return n == 0 ? null : n;
        }
    }

    public readonly struct LargeExcelRowRef
    {
        private readonly int _n;
        public LargeExcelRowRef(int n) { _n = n; }
        public int RowNumber() => _n;
    }

    public sealed class LargeExcelRow
    {
        internal static readonly LargeExcelRow Empty = new(0, Array.Empty<LargeExcelValue>());
        private readonly LargeExcelValue[] _cells;   // index = column - 1 (last item is never blank)
        private readonly int _rowNumber;

        internal LargeExcelRow(int rowNumber, LargeExcelValue[] cells)
        {
            _rowNumber = rowNumber;
            _cells = cells;
        }

        public int RowNumber() => _rowNumber;
        public bool IsEmpty() => _cells.Length == 0;
        public LargeExcelCell Cell(int column) =>
            new(column, column >= 1 && column <= _cells.Length ? _cells[column - 1] : LargeExcelValue.Blank);
        public LargeExcelCell? LastCellUsed() => _cells.Length == 0 ? null : Cell(_cells.Length);
    }

    public sealed class LargeExcelCell
    {
        internal LargeExcelCell(int column, LargeExcelValue value)
        {
            Address = new LargeExcelAddress(column);
            Value = value;
        }
        public LargeExcelValue Value { get; }
        public LargeExcelAddress Address { get; }
    }

    public readonly struct LargeExcelAddress
    {
        public LargeExcelAddress(int column) { ColumnNumber = column; }
        public int ColumnNumber { get; }
    }

    /// <summary>Cell value with the same checks ClosedXML's XLCellValue offers.</summary>
    public readonly struct LargeExcelValue
    {
        private enum Kind : byte { Blank, Text, Number, Date, Bool, Error }
        private readonly Kind _kind;
        private readonly double _number;
        private readonly string? _text;
        private readonly DateTime _date;

        private LargeExcelValue(Kind kind, double number = 0, string? text = null, DateTime date = default)
        {
            _kind = kind; _number = number; _text = text; _date = date;
        }

        public static readonly LargeExcelValue Blank = new(Kind.Blank);
        public static readonly LargeExcelValue Error = new(Kind.Error);
        public static LargeExcelValue FromText(string? s) => string.IsNullOrEmpty(s) ? Blank : new(Kind.Text, text: s);
        public static LargeExcelValue FromNumber(double d) => new(Kind.Number, number: d);
        public static LargeExcelValue FromDate(DateTime d) => new(Kind.Date, date: d);
        public static LargeExcelValue FromBool(bool b) => new(Kind.Bool, number: b ? 1 : 0);

        public bool IsBlank => _kind == Kind.Blank;
        public bool IsText => _kind == Kind.Text;
        public bool IsNumber => _kind == Kind.Number;
        public bool IsDateTime => _kind == Kind.Date;
        public bool IsBoolean => _kind == Kind.Bool;
        public bool IsError => _kind == Kind.Error;

        public double GetNumber() => _number;
        public DateTime GetDateTime() => _date;
        public bool GetBoolean() => _number != 0;

        public string ToString(IFormatProvider provider) => _kind switch
        {
            Kind.Text => _text ?? "",
            Kind.Number => _number.ToString(provider),
            Kind.Date => _date.ToString(provider),
            Kind.Bool => _number != 0 ? "TRUE" : "FALSE",
            _ => ""
        };

        public override string ToString() => ToString(CultureInfo.InvariantCulture);
    }
}
