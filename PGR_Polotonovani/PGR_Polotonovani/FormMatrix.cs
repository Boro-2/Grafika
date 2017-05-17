using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PGR_Polotonovani
{
    public partial class FormMatrix : Form
    {
        public int[,] matrix;
        public string matrixName;
        Dictionary<string,int[,]> matrices;
        public FormMatrix()
        {
            InitializeComponent();
            matrices = new Dictionary<string, int[,]>();
            try
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                var fi = new System.IO.FileInfo(@"matrix.bin");
                using (var binaryFile = fi.OpenRead())
                {
                    matrices = (Dictionary<string,int[,]>)binaryFormatter.Deserialize(binaryFile);
                }                
            }catch(Exception){
                MessageBox.Show("loading failed");
                matrices.Add("Floyd-Steinberg", new int[2, 3] { { 0, -1, 7 }, { 3, 5, 1 } });
                matrices.Add("False Floyd-Steinberg", new int[2, 2] { { -1, 8 }, { 8, 4 } });
            }
            if (matrices.Count > 0)
                matrix = matrices.First().Value;

            SetDataGridView(matrix);
            matrixName = matrices.First().Key;
            
            listBox1.Items.AddRange(matrices.Keys.ToArray());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void SetDataGridView(int[,] data)
        {
            var rowCount = data.GetLength(0);
            var rowLength = data.GetLength(1);
            dataGridView1.ColumnCount = data.GetLength(1);
            dataGridView1.Rows.Clear();
            for (int rowIndex = 0; rowIndex < rowCount; ++rowIndex)
            {
                var row = new DataGridViewRow();

                for (int columnIndex = 0; columnIndex < rowLength; ++columnIndex)
                {
                    row.Cells.Add(new DataGridViewTextBoxCell()
                    {
                        Value = data[rowIndex, columnIndex]
                    });
                }

                dataGridView1.Rows.Add(row);
            }
        }

        private void listBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
                matrix = matrices[listBox1.SelectedItem.ToString()];
            else
                matrix = matrices.First().Value;

            SetDataGridView(matrix);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            int[,] mt= new int[(int)nR.Value,(int)nC.Value];
            for (int i = 0; i < (int)nR.Value; i++)
            {
                for (int j = 0; j < (int)nC.Value; j++)
                {
                  /*  int number;
                    int.TryParse(dataGridView1[j, i].Value.ToString(), out number);*/
                    
                   if (dataGridView1[j, i].Value==null || !int.TryParse(dataGridView1[j, i].Value.ToString(), out mt[i,j]))
                        mt[i, j] = 0;
                }
            }
            matrices[textBox1.Text]= mt;
            listBox1.Items.Add(textBox1.Text);

            nR.Enabled = false;
            nC.Enabled = false;
            listBox1.Invalidate();
        }

        private void nR_ValueChanged(object sender, EventArgs e)
        {
            dataGridView1.AllowUserToAddRows = true;
            dataGridView1.RowCount = (int)nR.Value+1;
            dataGridView1.AllowUserToAddRows = false;
        }

        private void nC_ValueChanged(object sender, EventArgs e)
        {
            dataGridView1.ColumnCount = (int)nC.Value;
        }

        private void FormMatrix_FormClosing(object sender, FormClosingEventArgs e)
        {
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            var fi = new System.IO.FileInfo(@"matrix.bin");

            using (var binaryFile = fi.Create())
            {
                binaryFormatter.Serialize(binaryFile, matrices);
                binaryFile.Flush();
            }
            matrixName = (listBox1.SelectedItem==null)?matrixName: listBox1.SelectedItem.ToString();
            
        }

        private void btnew_Click(object sender, EventArgs e)
        {
            nR.Enabled = true;
            nR.Value = matrix.GetLength(0);
            nC.Enabled = true;
            nC.Value = matrix.GetLength(1);
            dataGridView1.RowCount = (int)nR.Value;
            dataGridView1.ColumnCount = (int)nC.Value;



        }

        private void btDelete_Click(object sender, EventArgs e)
        {
            matrices.Remove(listBox1.SelectedItem.ToString());
            matrix = matrices.First().Value;
            listBox1.Items.Clear();
            listBox1.Items.AddRange(matrices.Keys.ToArray());
            SetDataGridView(matrix);

        }
    }
}
