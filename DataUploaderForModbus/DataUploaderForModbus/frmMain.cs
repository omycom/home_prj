using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LIB_Common;

namespace DataUploaderForModbus
{
    public partial class frmMain : Form
    {
        private DataTable dtAddressItems = new DataTable();

        public frmMain()
        {
            InitializeComponent();

            SetDatatGridView();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            SetStopStatus();

            LoadProperties();
        }

        private void SetDatatGridView()
        {
            var pkCol = dtAddressItems.Columns.Add("ColumnName", typeof(string));
            dtAddressItems.Columns.Add("StartAddress", typeof(int));
            dtAddressItems.PrimaryKey = new DataColumn[] { pkCol };

            dgvModbusAddressItems.DataSource = dtAddressItems;
        }

        #region SetEnableByStatus

        private void SetRunStatus()
        {
            this.groupBox1.Enabled = false;
            this.groupBox2.Enabled = false;
            this.groupBox3.Enabled = false;
            this.nudInterval.Enabled = false;
            this.btnRun.Enabled = false;
            this.btnStop.Enabled = true;
        }

        private void SetStopStatus()
        {
            this.groupBox1.Enabled = true;
            this.groupBox2.Enabled = true;
            this.groupBox3.Enabled = true;
            this.nudInterval.Enabled = true;
            this.btnRun.Enabled = true;
            this.btnStop.Enabled = false;
        }

        #endregion

        #region Properties
        private void SaveProperties()
        {
            Properties.Settings.Default.modbusIP = this.txtMbIP.Text;
            Properties.Settings.Default.modbusPort = (int)this.nudMbPort.Value;
            Properties.Settings.Default.modbusUnitID = (int)this.nudMbUnitID.Value;
            Properties.Settings.Default.interval = (int)this.nudInterval.Value;
            Properties.Settings.Default.modbusAddressItems = string.Join(";", dtAddressItems.AsEnumerable().Select(r => $"{r["ColumnName"]},{r["StartAddress"]}"));
            Properties.Settings.Default.WebserviceURL = this.txtWebSrvURL.Text;

            Properties.Settings.Default.Save();
        }

        private void LoadProperties()
        {
            this.txtMbIP.Text = Properties.Settings.Default.modbusIP;
            this.nudMbPort.Value = Properties.Settings.Default.modbusPort;
            this.nudMbUnitID.Value = Properties.Settings.Default.modbusUnitID;
            this.nudInterval.Value = Properties.Settings.Default.interval;
            dtAddressItems.Rows.Clear();
            var items = Properties.Settings.Default.modbusAddressItems.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in items)
            {
                var parts = item.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    dtAddressItems.Rows.Add(parts[0], int.Parse(parts[1]));
                }
            }

            this.txtWebSrvURL.Text = Properties.Settings.Default.WebserviceURL;
        }

        #endregion


        private void btnRun_Click(object sender, EventArgs e)
        {
            SetRunStatus();

            SaveProperties();

            timer1.Interval = (int)nudInterval.Value * 1000;
            timer1.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            SetStopStatus();

            timer1.Stop();
        }

        private Dictionary<string, object> ReadModbusData()
        {
            myModbus modbusServer = new myModbus_TCP(this.txtMbIP.Text, (int)this.nudMbPort.Value, (int)this.nudMbUnitID.Value);

            try
            {
                modbusServer.Open();
                Dictionary<string, object> result = new Dictionary<string, object>();

                result["insert_datetime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                foreach (DataRow row in dtAddressItems.Rows)
                {
                    string columnName = row["ColumnName"].ToString();
                    int startAddress = (int)row["StartAddress"];
                    ushort[] values = modbusServer.ReadHoldingRegisters(startAddress, 1);
                    if (values != null && values.Length > 0)
                    {
                        Console.WriteLine($"Read {columnName} at address {startAddress}: {values[0]}");
                        result[columnName] = values[0];
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                modbusServer.Close();
            }
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            var data = ReadModbusData();
            if (data == null)
                return;

            myWebservice webservice = new myWebservice(this.txtWebSrvURL.Text);
            try
            {
                string resultMessage = await webservice.PostAsync(data);
                
                Console.WriteLine($"Webservice Response: {resultMessage}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Webservice Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
