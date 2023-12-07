using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Bars.TableStyle;

public class Xml
{
    public XDocument Document { get; private set; }
    public IEnumerable<Table> Tables { get; private set; }

    public Xml(string filename)
    {
        Document = XDocument.Load(filename);
        Tables = Parse();
    }
    private List<Table> Parse()
    {
        var header = Document?.Root?.Descendants("Шапка").FirstOrDefault()?.Attribute("Код")?.Value;
        var bookmarks = Document?.Root?.Descendants("Закладка");
        var tables = bookmarks?.Select(x => new Table(x));
        var subTables = Document?.Root?.Descendants("ДинамическаяТаблица")?.Select(x => x?.Attribute("Код")?.Value);

        return tables?.ToList() ?? new List<Table>();
    }
    public void Save(string filename)
    {
        var header = Document?.Root?.Descendants("Шапка").FirstOrDefault()?.Attribute("Код")?.Value;
        var bookmarks = Document?.Root?.Descendants("Закладка");
        foreach( var table in Tables)
        {
            var bookmark = bookmarks?.FirstOrDefault(x => (string?)x.Attribute("Код") == table.Code);
            foreach(var cell in table.Cells)
            {
                var data = bookmark?.Descendants("CellData")
                    .FirstOrDefault(x => (string?)x.Attribute("Row") == cell.Row.Index && (string?)x.Attribute("Col") == cell.Column.Index)?.Element("CellContents");
                data?.Element("HorizontalAlignment")?.SetValue(cell.HorizontalAlignment.ToString());
                data?.Element("VerticalAlignment")?.SetValue(cell.VerticalAlignment.ToString());
            }
            var rows = table.Cells.Select(x => x.Row).Distinct();
            var cols = table.Cells.Select(x => x.Column).Distinct();
            foreach(var col in cols)
            {
                bookmark?.Descendants("ColumnWidths").Elements("GridRowColEntry")
                    .FirstOrDefault(x => (string?)x.Attribute("Index") == col.Index)?.SetAttributeValue("Length", col.Width);
            }
            foreach(var row in rows)
            {
                bookmark?.Descendants("RowHeights").Elements("GridRowColEntry")
                    .FirstOrDefault(x => (string?)x.Attribute("Index") == row.Index)?.SetAttributeValue("Length", row.Height);
            }
        }
        Document?.Save(filename);
    }
}
public class Table
{
    public string Code { get; private set; }
    public List<Cell> Cells { get; private set; }
    public Table(XElement element)
    {
        Code = element?.Attribute("Код")?.Value ?? string.Empty;
        Cells = GetCells(element);
    }
    private List<Cell> GetCells(XElement element)
    {
        var cells = new List<Cell>();
        var columns = element.Descendants("ColumnWidths").Elements("GridRowColEntry")
            .Where(x => x.Attribute("Index") != null && x.Attribute("Length") != null && x?.Attribute("Index")!.Value != "0")
            .Select(x => new Column((string)x.Attribute("Index")!, (string)x.Attribute("Length")!));

        var rows = element.Descendants("RowHeights").Elements("GridRowColEntry")
            .Where(x => x.Attribute("Index") != null && x.Attribute("Length") != null && x?.Attribute("Index")!.Value != "0")
            .Select(x => new Row((string)x.Attribute("Index")!, (string)x.Attribute("Length")!));

        foreach (var row in rows)
        {
            foreach (var col in columns)
            {
                var data = element.Descendants("CellData")
                    .FirstOrDefault(x => (string?)x.Attribute("Row") == row.Index && (string?)x.Attribute("Col") == col.Index)?.Element("CellContents");
                if (data is null)
                    continue;

                Enum.TryParse(data.Element("HorizontalAlignment")?.Value, out HorizontalAlignment horizontal);
                Enum.TryParse(data.Element("VerticalAlignment")?.Value, out VerticalAlignment vertical);

                cells.Add(new(
                    row,
                    col,
                    data.Descendants("anyType")?.FirstOrDefault()?.Value ?? string.Empty,
                    horizontal,
                    vertical
                    ));
            }
        }
        return cells;
    }
}
public class Cell
{
    public Row Row { get; private set; }
    public Column Column { get; private set; }
    public string Value { get; set; } = string.Empty;
    public HorizontalAlignment HorizontalAlignment { get; set; }
    public VerticalAlignment VerticalAlignment { get; set; }
    public Cell(Row row, Column column)
    {
        Row = row;
        Column = column;
    }
    public Cell(Row row, Column column, string value, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment) : this(row, column)
    {
        Value = value;
        HorizontalAlignment = horizontalAlignment;
        VerticalAlignment = verticalAlignment;
    }
}
public class Row
{
    public string Index { get; set; }
    public string Height { get; set; }
    public Row(string index, string height)
    {
        Index = index;
        Height = height;
    }
}
public class Column
{
    public string Index { get; set; }
    public string Width { get; set; }
    public Column(string index, string width)
    {
        Index = index;
        Width = width;
    }
}
public enum HorizontalAlignment
{
    Left, Center, Right, Justify, Stretch
}
public enum VerticalAlignment
{
    Top, Middle, Bottom
}
