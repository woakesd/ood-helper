using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for TestPage.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class SeriesDisplayPage : Page
    {
        DataTable rd;

        public SeriesDisplayPage(SeriesDisplay sd)
        {
            InitializeComponent();

            rd = sd.data;

            int i = 1;
            foreach (SeriesEvent se in sd.events)
            {
                //rd.Columns.Add("r" + i.ToString(), typeof(SeriesEntry));

                int k = rd.Columns["R" + i].Ordinal;

                DataGridTemplateColumn x = new DataGridTemplateColumn();
                UTF8Encoding ue = new UTF8Encoding();

                x.Header = "R" + i;
                string dataTemplate = @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                            <StackPanel Orientation=""Horizontal"" Name=""panel"">
                                <TextBlock Text=""{Binding r" + i + @".Points, StringFormat=#.#}"" />
                                <TextBlock Text=""{Binding r" + i + @".CodeDisplay}"" FontSize=""6""/>
                            </StackPanel>
                            <DataTemplate.Triggers>
                                <DataTrigger Binding=""{Binding r" + i + @".Discard}"" Value=""True"">
                                    <DataTrigger.Setters>
                                        <Setter Property=""Background"" Value=""LightGray"" TargetName=""panel""/>
                                    </DataTrigger.Setters>
                                </DataTrigger>
                            </DataTemplate.Triggers>
                        </DataTemplate>";
                x.CellTemplate = (DataTemplate)System.Windows.Markup.XamlReader.Load(new System.IO.MemoryStream(ue.GetBytes(dataTemplate)));
                Results.Columns.Add(x);

                i++;
            }

            Results.ItemsSource = rd.DefaultView;
        }
    }
}
