using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;

namespace OcsStore
{
    public class GridUI
    {
        public static void FormatNumberColumn<T>(DataGridColumnBuilder<T> col, bool useDecimal = false, string postfix = null, bool emptyZero = true)
        {
            string numberFormat = (useDecimal ? "#,##0.0" : "#,##0") + (postfix ?? "");
            col.DataType(GridColumnDataType.Number) // Tells the grid to use dxNumberBox
                .Format(numberFormat) // How the number displays outside of edit mode
                .EditorOptions(new { format = numberFormat, min = 0, max = 1000000000, step = 1, showSpinButtons = false });

            if (emptyZero)
                col.CustomizeText("emptyZeroNumberCellText");
        }
    }
}
