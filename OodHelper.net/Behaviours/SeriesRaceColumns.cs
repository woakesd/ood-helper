using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace OodHelper.Behaviors
{
    /// <summary>
    /// Adds one <see cref="DataGridTemplateColumn"/> per series race (R1..Rn) to a DataGrid, each
    /// bound to the row view-model's indexed <c>Cells</c> collection. Replaces the runtime column +
    /// DataTable generation that used to live in <c>SeriesDisplay</c>/<c>SeriesDisplayPage</c>
    /// code-behind; the count comes from the bound <c>SeriesDisplayViewModel.RaceColumnCount</c> so the
    /// views stay declarative.
    /// </summary>
    public static class SeriesRaceColumns
    {
        public static int GetCount(DependencyObject obj) => (int)obj.GetValue(CountProperty);
        public static void SetCount(DependencyObject obj, int value) => obj.SetValue(CountProperty, value);

        public static readonly DependencyProperty CountProperty = DependencyProperty.RegisterAttached(
            "Count", typeof(int), typeof(SeriesRaceColumns), new PropertyMetadata(0, OnCountChanged));

        // Tracks the columns this behaviour added so a re-set replaces them rather than appending.
        private static readonly DependencyProperty GeneratedProperty = DependencyProperty.RegisterAttached(
            "Generated", typeof(List<DataGridColumn>), typeof(SeriesRaceColumns), new PropertyMetadata(null));

        private static void OnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DataGrid grid)) return;

            var previous = (List<DataGridColumn>)grid.GetValue(GeneratedProperty);
            if (previous != null)
                foreach (var c in previous)
                    grid.Columns.Remove(c);

            var generated = new List<DataGridColumn>();
            int count = (int)e.NewValue;
            for (int i = 0; i < count; i++)
            {
                var col = new DataGridTemplateColumn
                {
                    Header = "R" + (i + 1),
                    CellTemplate = BuildCellTemplate(i)
                };
                grid.Columns.Add(col);
                generated.Add(col);
            }

            grid.SetValue(GeneratedProperty, generated);
        }

        private static DataTemplate BuildCellTemplate(int index)
        {
            //
            // Points + (code) for Cells[index]; discarded scores get a grey background, exactly as the
            // legacy per-column DataTemplate did.
            //
            string xaml = @"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
    <StackPanel Orientation=""Horizontal"" Name=""panel"">
        <TextBlock Text=""{Binding Cells[" + index + @"].Points, StringFormat=#.#}"" />
        <TextBlock Text=""{Binding Cells[" + index + @"].CodeDisplay}"" FontSize=""9""/>
    </StackPanel>
    <DataTemplate.Triggers>
        <DataTrigger Binding=""{Binding Cells[" + index + @"].Discard}"" Value=""True"">
            <Setter Property=""Background"" Value=""LightGray"" TargetName=""panel""/>
        </DataTrigger>
    </DataTemplate.Triggers>
</DataTemplate>";
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xaml)))
                return (DataTemplate)XamlReader.Load(ms);
        }
    }
}
