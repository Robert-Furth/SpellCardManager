using System.Windows;
using System.Windows.Documents;
using MdTables = Markdig.Extensions.Tables;

namespace SpellCardManager.WPFApp.Markdown.Renderers;

internal class TableRenderer : FlowDocumentObjectRenderer<MdTables.Table> {

    protected override void Write(FlowDocumentRenderer renderer, MdTables.Table table) {
        var resources = (ResourceDictionary)Application.LoadComponent(
            new("Styles/FlowDocument.xaml", UriKind.Relative));
        var tableHeaderStyle = (Style)resources["TableHeaderStyle"];
        var tableEvenRowStyle = (Style)resources["TableEvenRowStyle"];

        var tableElement = new Table();
        var rowGroupElement = new TableRowGroup();
        tableElement.RowGroups.Add(rowGroupElement);
        renderer.Write(tableElement);

        var hasColDefs = table.ColumnDefinitions is not null;
        var headerSeen = false;

        for (var rowIndex = 0; rowIndex < table.Count; rowIndex++) {
            var mdRow = (MdTables.TableRow)table[rowIndex];
            var rowElement = new TableRow();

            if (mdRow.IsHeader && !headerSeen) {
                rowElement.Style = tableHeaderStyle;
                headerSeen = true;
            } else if (rowIndex % 2 == 0) {
                rowElement.Style = tableEvenRowStyle;
            }

            rowGroupElement.Rows.Add(rowElement);
            for (var colIndex = 0; colIndex < mdRow.Count; colIndex++) {
                var mdCell = (MdTables.TableCell)mdRow[colIndex];

                var cellElement = new TableCell();
                rowElement.Cells.Add(cellElement);

                if (hasColDefs) {
                    cellElement.TextAlignment =
                        table.ColumnDefinitions![colIndex].Alignment switch {
                            MdTables.TableColumnAlign.Center => TextAlignment.Center,
                            MdTables.TableColumnAlign.Right => TextAlignment.Right,
                            _ => TextAlignment.Left,
                        };
                }

                renderer.PushTarget(cellElement);
                renderer.WriteChildren(mdCell);
                renderer.PopTarget();
            }
        }
    }
}
