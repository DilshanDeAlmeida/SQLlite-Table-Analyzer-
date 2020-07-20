using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Guard_Check_Analyzer
{
    public partial class main : Form
    {
        string path;
        SQLiteConnection connection;
        public main()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();

                dialog.DefaultExt = ".db";
                dialog.Filter = "Database files (*.db)|*.db";
                dialog.Title = "Select a SQLlite File";
                dialog.FilterIndex = 2;
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;

                DialogResult dr = dialog.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    path = dialog.FileName;
                    txtFilePath.Text = path;
                    //MessageBox.Show("File Selected \n" + dialog.FileName, "Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "File Dialog Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            progressBar.Value = 0;
            progressBar.Value = 25;
            if (path != "" | path != null)
            {
                DataTable dt = new DataTable();
                try
                {
                    string dbConnection = String.Format("Data Source={0}", path);
                    connection = new SQLiteConnection(dbConnection);
                    connection.Open();
                    SQLiteCommand mycommand = new SQLiteCommand(connection);
                    if (checkBox1.CheckState == CheckState.Checked)
                    {
                        mycommand.CommandText =
                        "SELECT location_name,substr(CAST(created_at as nvarchar(20)), 0, 11)AS date,substr(CAST(created_at as nvarchar(20)), -5, 5)AS time,guard_name,remarks, sysID FROM checkhistory GROUP BY date,location_name ORDER BY sysID ASC;";
                    }
                    else
                    {
                        string datesort = dateTimePicker1.Value.ToString("dd/MM/yyyy");
                        mycommand.CommandText =
                        "SELECT location_name,substr(CAST(created_at as nvarchar(20)), 0, 11)AS date,substr(CAST(created_at as nvarchar(20)), -5, 5)AS time,guard_name,remarks, sysID FROM checkhistory WHERE date = '" + datesort + "' GROUP BY date,location_name ORDER BY sysID ASC;";
                    }
                    SQLiteDataReader reader = mycommand.ExecuteReader();
                    dt.Load(reader);
                    reader.Close();
                    connection.Close();
                    progressBar.Value = 100;
                }
                catch (Exception ex)
                {
                    progressBar.Value = 0;
                    connection.Close();
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                dataGridView1.DataSource = dt;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                createSummary();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void createSummary()
        {
            if (checkBox1.CheckState == CheckState.Unchecked && dataGridView1.Rows.Count > 0)
            {
                string first_tripstartedat = "00:00:00"; string last_tripendedat = "00:00:00";
                int trips_started = 0, trips_completed = 0;
                DataTable datatable = (DataTable)dataGridView1.DataSource;
                for (int i = 0; i < datatable.Rows.Count; i++)
                {
                    if (datatable.Rows[i]["location_name"].ToString() == "Location01")
                    {
                        trips_started += 1;
                        if (first_tripstartedat == "00:00:00")
                        {
                            first_tripstartedat = datatable.Rows[i]["time"].ToString();
                        }
                    }
                    else if (datatable.Rows[i]["location_name"].ToString() == "Location38")
                    {
                        trips_completed += 1;
                        last_tripendedat = datatable.Rows[i]["time"].ToString();
                    }
                    last_tripendedat = datatable.Rows[i]["time"].ToString();
                }
                textBox1.Text = trips_started.ToString();
                textBox2.Text = trips_completed.ToString();
                textBox3.Text = (trips_started - trips_completed).ToString();
                textBox4.Text = first_tripstartedat;
                textBox5.Text = last_tripendedat;
            }
            else
            {
                textBox1.Text = null;
                textBox2.Text = null;
                textBox3.Text = null;
                textBox4.Text = null;
                textBox5.Text = null;
            }
        }
    }
}
