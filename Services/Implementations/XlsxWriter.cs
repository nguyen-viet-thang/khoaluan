using System.IO.Compression;
using System.Security;
using System.Text;

namespace PopPop.Services.Implementations;

/// <summary>
/// Minimal Office Open XML (.xlsx) writer: several sheets, text and number cells, bold header row.
/// Enough for report exports without pulling in a third-party Excel library.
/// </summary>
public sealed class XlsxWriter
{
    private readonly List<(string Name, IReadOnlyList<string> Header, List<object?[]> Rows, double[] Widths)> _sheets = [];

    /// <summary>Adds a sheet. Cell values may be string, numeric types, DateTime or null.</summary>
    public XlsxWriter AddSheet(string name, IReadOnlyList<string> header, IEnumerable<object?[]> rows, params double[] columnWidths)
    {
        // Excel sheet names: max 31 chars, no []:*?/\
        var safeName = new string(name.Where(character => !"[]:*?/\\".Contains(character)).ToArray());
        _sheets.Add((safeName.Length > 31 ? safeName[..31] : safeName, header, rows.ToList(), columnWidths));
        return this;
    }

    public byte[] ToBytes()
    {
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            Write(zip, "[Content_Types].xml", ContentTypes());
            Write(zip, "_rels/.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
            Write(zip, "xl/workbook.xml", Workbook());
            Write(zip, "xl/_rels/workbook.xml.rels", WorkbookRels());
            Write(zip, "xl/styles.xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">"
                + "<numFmts count=\"1\"><numFmt numFmtId=\"164\" formatCode=\"dd/mm/yyyy hh:mm\"/></numFmts>"
                + "<fonts count=\"2\"><font><sz val=\"11\"/><name val=\"Calibri\"/></font><font><b/><sz val=\"11\"/><name val=\"Calibri\"/></font></fonts>"
                + "<fills count=\"3\"><fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill><fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFFFE8D6\"/></patternFill></fill></fills>"
                + "<borders count=\"1\"><border/></borders><cellStyleXfs count=\"1\"><xf/></cellStyleXfs>"
                + "<cellXfs count=\"4\"><xf/><xf fontId=\"1\" fillId=\"2\" applyFont=\"1\" applyFill=\"1\"/><xf numFmtId=\"3\" applyNumberFormat=\"1\"/><xf numFmtId=\"164\" applyNumberFormat=\"1\"/></cellXfs>"
                + "</styleSheet>");
            for (var index = 0; index < _sheets.Count; index++)
            {
                Write(zip, $"xl/worksheets/sheet{index + 1}.xml", Sheet(_sheets[index]));
            }
        }

        return stream.ToArray();
    }

    private string ContentTypes()
    {
        var builder = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
            + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/>"
            + "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>"
            + "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>");
        for (var index = 1; index <= _sheets.Count; index++)
        {
            builder.Append($"<Override PartName=\"/xl/worksheets/sheet{index}.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>");
        }

        return builder.Append("</Types>").ToString();
    }

    private string Workbook()
    {
        var builder = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets>");
        for (var index = 0; index < _sheets.Count; index++)
        {
            builder.Append($"<sheet name=\"{Escape(_sheets[index].Name)}\" sheetId=\"{index + 1}\" r:id=\"rId{index + 1}\"/>");
        }

        return builder.Append("</sheets></workbook>").ToString();
    }

    private string WorkbookRels()
    {
        var builder = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
        for (var index = 0; index < _sheets.Count; index++)
        {
            builder.Append($"<Relationship Id=\"rId{index + 1}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet{index + 1}.xml\"/>");
        }

        builder.Append($"<Relationship Id=\"rId{_sheets.Count + 1}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>");
        return builder.Append("</Relationships>").ToString();
    }

    private static string Sheet((string Name, IReadOnlyList<string> Header, List<object?[]> Rows, double[] Widths) sheet)
    {
        var builder = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
        builder.Append("<sheetViews><sheetView workbookViewId=\"0\"><pane ySplit=\"1\" topLeftCell=\"A2\" activePane=\"bottomLeft\" state=\"frozen\"/></sheetView></sheetViews>");
        if (sheet.Widths.Length > 0)
        {
            builder.Append("<cols>");
            for (var index = 0; index < sheet.Widths.Length; index++)
            {
                builder.Append($"<col min=\"{index + 1}\" max=\"{index + 1}\" width=\"{sheet.Widths[index].ToString(System.Globalization.CultureInfo.InvariantCulture)}\" customWidth=\"1\"/>");
            }

            builder.Append("</cols>");
        }

        builder.Append("<sheetData>");
        AppendRow(builder, 1, sheet.Header.Cast<object?>().ToArray(), isHeader: true);
        for (var index = 0; index < sheet.Rows.Count; index++)
        {
            AppendRow(builder, index + 2, sheet.Rows[index], isHeader: false);
        }

        return builder.Append("</sheetData></worksheet>").ToString();
    }

    private static void AppendRow(StringBuilder builder, int rowNumber, object?[] values, bool isHeader)
    {
        builder.Append($"<row r=\"{rowNumber}\">");
        for (var column = 0; column < values.Length; column++)
        {
            var reference = $"{ColumnName(column)}{rowNumber}";
            switch (values[column])
            {
                case null:
                    break;
                case DateTime dateTime:
                    builder.Append($"<c r=\"{reference}\" s=\"3\"><v>{dateTime.ToOADate().ToString(System.Globalization.CultureInfo.InvariantCulture)}</v></c>");
                    break;
                case int or long or decimal or double or float:
                    var number = Convert.ToDecimal(values[column], System.Globalization.CultureInfo.InvariantCulture);
                    builder.Append($"<c r=\"{reference}\" s=\"{(isHeader ? 1 : 2)}\"><v>{number.ToString(System.Globalization.CultureInfo.InvariantCulture)}</v></c>");
                    break;
                default:
                    builder.Append($"<c r=\"{reference}\" t=\"inlineStr\"{(isHeader ? " s=\"1\"" : string.Empty)}><is><t xml:space=\"preserve\">{Escape(values[column]!.ToString()!)}</t></is></c>");
                    break;
            }
        }

        builder.Append("</row>");
    }

    private static string ColumnName(int index)
    {
        var name = string.Empty;
        for (index++; index > 0; index = (index - 1) / 26)
        {
            name = (char)('A' + (index - 1) % 26) + name;
        }

        return name;
    }

    private static string Escape(string value) => SecurityElement.Escape(value) ?? string.Empty;

    private static void Write(ZipArchive zip, string path, string content)
    {
        var entry = zip.CreateEntry(path, CompressionLevel.Optimal);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }
}
