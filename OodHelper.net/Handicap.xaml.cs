using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for Handicap.xaml
    /// </summary>
    public partial class Handicap : Window
    {
        public Guid Id { get; set; }
        HandicapRecord hcap;

        public Handicap(Guid i)
        {
            InitializeComponent();
            Id = i;
            if (Id != Guid.Empty)
            {
                hcap = new HandicapRecord(Id);
            }
            else
            {
                hcap = new HandicapRecord();
            }
            DataContext = hcap;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Db hdb;
                if (Id == Guid.Empty)
                {
                    hdb = new Db(@"INSERT INTO portsmouth_numbers 
                        ([class_name]
                        , [no_of_crew]
                        , [rig]
                        , [spinnaker]
                        , [engine]
                        , [keel]
                        , [number]
                        , [status]
                        , [notes])
                        VALUES (@class_name
                        , @no_of_crew
                        , @rig
                        , @spinnaker
                        , @engine
                        , @keel
                        , @number
                        , @status
                        , @notes)");
                    Id = Guid.NewGuid();
                }
                else
                    hdb = new Db(@"UPDATE portsmouth_numbers 
                        SET [class_name] = @class_name
                        , [no_of_crew] = @no_of_crew
                        , [rig] = @rig
                        , [spinnaker] = @spinnaker
                        , [engine] = @engine
                        , [keel] = @keel
                        , [number] = @number
                        , [status] = @status
                        , [notes] = @notes
                        WHERE id = @id");
                Hashtable p = new Hashtable();
                p["class_name"] = class_name.Text;
                if (no_of_crew.Text == string.Empty)
                    p["no_of_crew"] = DBNull.Value;
                else
                    p["no_of_crew"] = Int32.Parse(no_of_crew.Text);
                p["rig"] = rig.SelectedValue;
                p["spinnaker"] = spinnaker.SelectedValue;
                p["engine"] = engine.SelectedValue;
                p["keel"] = keel.SelectedValue;
                if (number.Text == string.Empty)
                    p["number"] = DBNull.Value;
                else
                    p["number"] = Int32.Parse(number.Text);
                p["status"] = status.SelectedValue;
                p["notes"] = notes.Text;
                p["id"] = Id;

                hdb.ExecuteNonQuery(p);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
