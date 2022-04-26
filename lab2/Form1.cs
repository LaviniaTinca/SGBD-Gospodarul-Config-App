using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;

namespace lab2
{
    public partial class Form1 : Form
    {
        DataSet dataset = new DataSet();
        SqlDataAdapter parentAdapter = new SqlDataAdapter();
        SqlDataAdapter childAdapter = new SqlDataAdapter();
        
        BindingSource bsParent = new BindingSource();
        BindingSource bsChild = new BindingSource(); //acestea vor fi intermediare intre data si gridView

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["connection"].ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string parentTable = ConfigurationManager.AppSettings["parentTable"];
                    string childTable = ConfigurationManager.AppSettings["childTable"];

                    //dataAdapter deschide si inchide singur conexiunea
                    //setam propr select pt fiecare adapter pt a popula data tables
                    parentAdapter.SelectCommand = new SqlCommand("SELECT * FROM " + parentTable, connection);
                    childAdapter.SelectCommand = new SqlCommand("SELECT * FROM " + childTable, connection);
                    parentAdapter.Fill(dataset, parentTable); //specific numele tabelului pe care il creeaza va fi acelasi cu cel din baza de date
                    childAdapter.Fill(dataset, childTable); //pana aici avem datele dar inca nu le vedem, setam data source ca sa le vedem
                    //ne folosim de binding source - afisare automata a datelor la selectare
                    DataColumn pkColumn = dataset.Tables[parentTable].Columns[0]; //iau prima coloana -id
                    List<string> columns = new List<string>(ConfigurationManager.AppSettings["childTable_allColumns"].Split(','));

                    //DataColumn fkColumn = dataset.Tables[childTable].Columns[columns.Count - 1]; //iau ultima coloana pt foreign key (dar atunci foreign key trebuie sa fie mereu ultima ccol
                    string fk = ConfigurationManager.AppSettings["fk"];
                    DataColumn fkColumn = dataset.Tables[childTable].Columns[fk]; //iau coloana pt foreign key
                    DataRelation relation = new DataRelation("fk_Parent_Child", pkColumn, fkColumn);
                    //acum adaugam in colectia de relatii a dataset ca sa fie functionala
                    dataset.Relations.Add(relation);

                    bsParent.DataSource = dataset.Tables[parentTable];
                    dataGridViewParent.DataSource = bsParent;
                    labelParent.Text = parentTable;//"Parent Table"; //suna mai bine decat sectii
                    //bsChild.DataSource = dataset.Tables["Angajati"]; //asa ar fi nelegat de parent
                    bsChild.DataSource = bsParent;
                    bsChild.DataMember = "fk_Parent_Child";
                    dataGridViewChild.DataSource = bsChild;
                    labelChild.Text = childTable;//"Child Table";
                    string parentName = ConfigurationManager.AppSettings["parentTableName"];
                    //textBox1.DataBindings.Add("Text", bsParent, parentName);
                    //textBoxSectieId.DataBindings.Add("Text", bsParent, "id");
                    List<string> columnsList = new List<string>(ConfigurationManager.AppSettings["childTable_allColumns"].Split(','));
                    textBox1.Visible = false;
                    label1.Visible = false;
                    //fill();
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine(ex.Message); 
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["connection"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);
            
            string childTable = ConfigurationManager.AppSettings["childTable"];
            string columnParams = ConfigurationManager.AppSettings["childTable_columnsParams"];
            List<string> columnsParamsList = new List<string>(ConfigurationManager.AppSettings["childTable_columnsParams"].Split(','));

            childAdapter.InsertCommand = new SqlCommand("INSERT INTO " + childTable + " VALUES (" + columnParams + ")", connection);
            foreach (string columnParam in columnsParamsList)
            {
               //Console.WriteLine(columnParam.Substring(1));
                TextBox textBox = (TextBox)panel.Controls[columnParam.Substring(1)];
               // Console.WriteLine(textBox);
                childAdapter.InsertCommand.Parameters.AddWithValue(columnParam, textBox.Text);
            }
            
            try
            {
            

                connection.Open(); //de ce imi cere sa deschid conexiunea?
                childAdapter.InsertCommand.ExecuteNonQuery();
                MessageBox.Show("Inregistrarea a fost adaugata cu succes!");
                
                populate();
                panel.Controls[ConfigurationManager.AppSettings["childTableId"]].Text = this.getChildTableId(connection).ToString();

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            connection.Close();
        }

        private int getChildId()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["connection"].ConnectionString;
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                childAdapter.SelectCommand = new SqlCommand("SELECT max(id) FROM Angajati", connection);
                int angajat_id = (int)childAdapter.SelectCommand.ExecuteScalar();

                return angajat_id;
            }
        }
        private int getChildTableId(SqlConnection connection)
        {
            string childTableId = ConfigurationManager.AppSettings["childTableId"];
            string childTable = ConfigurationManager.AppSettings["childTable"];
            childAdapter.SelectCommand = new SqlCommand("SELECT max(" + childTableId + ") FROM " + childTable, connection);
            int childTableId_intValue = (int)childAdapter.SelectCommand.ExecuteScalar();

            return childTableId_intValue;
        }

        private void populate()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["connection"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);
            string parentTable = ConfigurationManager.AppSettings["parentTable"];
            string childTable = ConfigurationManager.AppSettings["childTable"];

            try
            {
                parentAdapter.SelectCommand.Connection = connection; //isi deschide si inchide singur conexiunea
                dataset.Tables[childTable].Clear();
                dataset.Tables[parentTable].Clear();
                
                parentAdapter.SelectCommand = new SqlCommand("SELECT * FROM " + parentTable, connection);
                childAdapter.SelectCommand = new SqlCommand("SELECT * FROM " + childTable, connection);
                parentAdapter.Fill(dataset, parentTable);
                childAdapter.Fill(dataset, childTable);
                bsChild.DataSource = bsParent;
                bsChild.DataMember = "fk_Parent_Child";
                dataGridViewChild.DataSource = bsChild;

                fill();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        

        private void fill()
        {
            List<string> columnsList = new List<string>(ConfigurationManager.AppSettings["childTable_columns"].Split(','));
            //List<string> columnsList = new List<string>(ConfigurationManager.AppSettings["childTable_allColumns"].Split(','));
            int startingYCoord = 0;
            int Xcoord = 100;
            panel.Controls.Clear();
            string childTable = ConfigurationManager.AppSettings["childTable"];

            Label label_childId = new Label();
            label_childId.Name = ConfigurationManager.AppSettings["childTableId"] + "_label";
            label_childId.Text = ConfigurationManager.AppSettings["childTableId"];
            label_childId.Location = new Point(0, startingYCoord);
            panel.Controls.Add(label_childId);

            Label label_childIdValue = new Label();
            label_childIdValue.Name = ConfigurationManager.AppSettings["childTableId"];
            label_childIdValue.Text = "";
            label_childIdValue.Location = new Point(Xcoord, startingYCoord);
            panel.Controls.Add(label_childIdValue);
            startingYCoord += 30;


            if (dataset.Tables[childTable].Rows.Count > 0)
            {
                //label_childIdValue.Text = dataset.Tables[childTable].Rows[0][ConfigurationManager.AppSettings["childTableId"]].ToString();
                foreach (string column in columnsList)
                {
                    Label label = new Label();
                    label.Name = column + "_label";
                    label.Location = new Point(0, startingYCoord);
                    label.Text = column;
                    panel.Controls.Add(label);

                    TextBox textBox = new TextBox();
                    textBox.Name = column;
                    textBox.Location = new Point(Xcoord, startingYCoord);
                    textBox.Text = dataset.Tables[childTable].Rows[0][column].ToString();
                    panel.Controls.Add(textBox);

                    startingYCoord += 30;
                }
            }
            else
            {
                foreach (string column in columnsList)
                {
                    Label label = new Label();
                    label.Name = column + "_label";
                    label.Location = new Point(0, startingYCoord);
                    label.Text = column;
                    panel.Controls.Add(label);

                    TextBox textBox = new TextBox();
                    textBox.Name = column;
                    textBox.Location = new Point(Xcoord, startingYCoord);
                    textBox.Text = "";
                    panel.Controls.Add(textBox);

                    startingYCoord += 30;
                }
            }
        }

        
        private void dataGridViewChild_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // adaugat manual in form1.designer           this.dataGridViewChild.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewChild_CellClick);
            string connectionString = ConfigurationManager.ConnectionStrings["connection"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow selectedRow = this.dataGridViewChild.Rows[e.RowIndex];
                    List<string> columnsList = new List<string>(ConfigurationManager.AppSettings["childTable_AllColumns"].Split(','));
                    
                    fill();
                    foreach (string column in columnsList)
                    {
                        panel.Controls[column].Text = selectedRow.Cells[column].Value.ToString();
                    }
                }
                //connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
            connection.Close();
        }



        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                populate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["connection"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);

            string childTable = ConfigurationManager.AppSettings["childTable"];
            string columnParams = ConfigurationManager.AppSettings["childTable_updateParams"];
            List<string> columnsParamsList = new List<string>(ConfigurationManager.AppSettings["childTable_columnsParams"].Split(','));

            childAdapter.UpdateCommand = new SqlCommand("UPDATE " + childTable + " SET " + columnParams, connection);
            foreach (string columnParam in columnsParamsList)
            {
                
               // Console.WriteLine(columnParam.Substring(1));
                TextBox textBox = (TextBox)panel.Controls[columnParam.Substring(1)];
                //Console.WriteLine(textBox.Text);

                childAdapter.UpdateCommand.Parameters.AddWithValue(columnParam, textBox.Text);
            }
            string childTableId = ConfigurationManager.AppSettings["childTableId"];
            childAdapter.UpdateCommand.Parameters.AddWithValue("@" + childTableId, panel.Controls[childTableId].Text);


            try
            {
                //childAdapter.UpdateCommand.Connection = connection; // trebuia cand am uitat sa pun in SqlCommand, altfel dadea: ExecuteNonQuery: Connection property has not been initialized. 
                connection.Open();
                childAdapter.UpdateCommand.ExecuteNonQuery();
                this.populate();
                MessageBox.Show("Inregistrarea a fost modificata cu succes!");
                //connection.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
            connection.Close();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["connection"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);

            string childTable = ConfigurationManager.AppSettings["childTable"];
            string columnParams = ConfigurationManager.AppSettings["childTable_deleteParams"];
            List<string> columnsParamsList = new List<string>(ConfigurationManager.AppSettings["childTable_columnsParams"].Split(','));
            List<string> allColumnsList = new List<string>(ConfigurationManager.AppSettings["childTable_allColumns"].Split(','));

            childAdapter.DeleteCommand = new SqlCommand("DELETE from " + childTable + " WHERE" + columnParams, connection);
            string childTableId = ConfigurationManager.AppSettings["childTableId"];
            childAdapter.DeleteCommand.Parameters.AddWithValue("@" + childTableId, panel.Controls[childTableId].Text);

            try
            {
                connection.Open();
                childAdapter.DeleteCommand.ExecuteNonQuery();
                this.populate();
                foreach (string column in allColumnsList)
                {
                    panel.Controls[column].Text = "";
                }
                MessageBox.Show("Inregistrarea a fost stearsa!");
            }
            catch (Exception exeption) { MessageBox.Show(exeption.Message.ToString()); }
            connection.Close();
        }
    }
}
