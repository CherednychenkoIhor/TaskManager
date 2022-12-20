using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace TaskManager
{
    
    public partial class Form1 : Form
    {
        private List<Process>  processes = null;
        private ListViewItemComparer comparer = null;
 
        // Вставить свой путь к библиотеке DLL, расположенной в прокте
        [DllImport("C:\\Users\\Cherednichenko\\Desktop\\TaskManager\\TaskManager\\for_coursework.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetMemoryInfoFromPid(int pid);
        public Form1()
        {
            InitializeComponent();
        }
        private void GetProcesses() { 
            processes?.Clear();
            processes = Process.GetProcesses().ToList<Process>();
        }

        private void GetHardwareInfo(string key, ListView list)
        {
            list.Items.Clear();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM " + key);
            try
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    ListViewGroup listViewGroup;
                    try
                    {
                        listViewGroup = list.Groups.Add(obj["Name"].ToString(), obj["Name"].ToString());
                    }
                    catch (Exception ex)
                    {
                        listViewGroup = list.Groups.Add(obj.ToString(), obj.ToString());
                    }

                    if (obj.Properties.Count == 0)
                    {
                        MessageBox.Show("Не удалось получить информацию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    foreach (PropertyData data in obj.Properties)
                    {
                        ListViewItem item = new ListViewItem(listViewGroup);
                        if (list.Items.Count % 2 != 0)
                        {
                            item.BackColor = Color.White;
                        }
                        else
                        {
                            item.BackColor = Color.WhiteSmoke;
                        }

                        item.Text = data.Name;

                        if (data.Value != null && !string.IsNullOrEmpty(data.Value.ToString()))
                        {
                            switch (data.Value.GetType().ToString())
                            {
                                case "System.String[]":
                                    string[] stringData = data.Value as string[];
                                    string resStr1 = string.Empty;
                                    foreach (string s in stringData)
                                    {
                                        resStr1 = $"{s} ";
                                    }

                                    item.SubItems.Add(resStr1);

                                    break;
                                case "System.UInt16[]":
                                    ushort[] ushortData = data.Value as ushort[];
                                    string resStr2 = string.Empty;

                                    foreach (ushort u in ushortData)
                                    {
                                        resStr2 += $"{Convert.ToString(u)}";
                                    }
                                    item.SubItems.Add(resStr2);
                                    break;
                                default:
                                    item.SubItems.Add(data.Value.ToString());
                                    break;
                            }
                            list.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
                 
        private void RefreshProcessList() { 
         listView1.Items.Clear();
            double memSize = 0;
            foreach (Process process in processes)
            {
                PerformanceCounter pc = new PerformanceCounter();
                pc.CategoryName = "Process";
                pc.CounterName = "Working Set - Private";
                pc.InstanceName = process.ProcessName;
                memSize = (double)GetMemoryInfoFromPid(pid: process.Id) / (1024 * 1024) ;
                           string[] row = new string[] { process.ProcessName.ToString(), process.Id.ToString(),  Math.Round(memSize, 1).ToString(), process.BasePriority.ToString() };
                listView1.Items.Add(new ListViewItem(row));
                pc.Close();
                pc.Dispose();
            }


            Text = "All processes: " + processes.Count.ToString();
        }

        private void RefreshProcessList(string name)
        {
            listView1.Items.Clear();
            double memSize = 0;
            foreach (Process process in processes)
            {
                PerformanceCounter pc = new PerformanceCounter();
                pc.CategoryName = "Process";
                pc.CounterName = "Working Set - Private";
                pc.InstanceName = process.ProcessName;
                memSize = (double)GetMemoryInfoFromPid(pid: process.Id) / (1024 * 1024);
                string[] row = new string[] { process.ProcessName.ToString(), process.Id.ToString(), Math.Round(memSize, 1).ToString() };
                if (process.ProcessName.Contains(name)) {
                    listView1.Items.Add(new ListViewItem(row));
                }
              
                pc.Close();
                pc.Dispose();
            }


            Text = $"Processes '{name}': " + processes.Count.ToString();
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            comparer.ColumIndex = e.Column;
            comparer.SortDirection = comparer.SortDirection == SortOrder.Ascending ? SortOrder.Descending: SortOrder.Ascending;
            listView1.ListViewItemSorter= comparer;
            listView1.Sort();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comparer = new ListViewItemComparer();
            comparer.ColumIndex= 0;
            toolStripComboBox1.SelectedIndex = 0;
        }

        private void KillProcess(Process process) { 
          process.Kill();
          process.WaitForExit();
        }

        private void KillProcessAndChildren(int pid)
        {
            if (pid == 0) {
                return;
            }

            ManagementObjectSearcher searcher= new ManagementObjectSearcher(
                "Select * Form Win32_Process Where ParentProcessID=" + pid
                );
            ManagementObjectCollection objectCollection = searcher.Get();
            foreach (ManagementObject obj in objectCollection) {
                KillProcessAndChildren(Convert.ToInt32(obj["ProcessId"]));            
            }

            try {
                Process p = Process.GetProcessById(pid);
                p.Kill();
                p.WaitForExit();
            }catch(ArgumentException) { }
        }

        private int GetParrentProcessId(Process p) { 
          int pid = 0;

            try { 
                ManagementObject managementObject = new ManagementObject("win32_process.handle='" + p.Id+"'");
                managementObject.Get();
                pid = Convert.ToInt32(managementObject["ParentProcessId"]);
            }catch(Exception) { }
            return pid;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {

            try {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x)=> x.ProcessName == listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];
                    KillProcess(processToKill);
                    GetProcesses();
                    RefreshProcessList();
                }
            }catch(Exception ) { }
        }

        private void closeTheardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName == listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcessAndChildren(GetParrentProcessId(processToKill));

                    GetProcesses();

                    RefreshProcessList();
                }
            }
            catch (Exception) { }
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            GetProcesses();
            RefreshProcessList(toolStripTextBox1.Text);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void runNewProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Interaction.InputBox("Enter new process name", "Run new process");
            try { 
                Process.Start(path);
            }catch(Exception ) { }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            GetProcesses();
            if (string.IsNullOrEmpty(toolStripTextBox1.Text))
            {
                RefreshProcessList();
            }
            else
            {
                RefreshProcessList(toolStripTextBox1.Text);
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string key = string.Empty;
            
            switch (toolStripComboBox1.SelectedItem.ToString()){
                case "CPU":
                    key = "Win32_Processor";
                    break;
                case "Video card":
                    key = "Win32_VideoController";
                    break;
                case "Chipset":
                    key = "Win32_IDEController";
                    break;
                case "Battery":
                    key = "Win32_Battery";
                    break;
                case "Bios":
                    key = "Win32_BIOS";
                    break;
                case "RAM":
                    key = "Win32_PhysicalMemory";
                    break;
                case "Cache":
                    key = "Win32_CacheMemory";
                    break;
                case "USB":
                    key = "Win32_USBController";
                    break;
                case "Disk":
                    key = "Win32_DiskDrive";
                    break;
                case "Logical drives":
                    key = "Win32_LogicalDisk";
                    break;
                case "Keyboard":
                    key = "Win32_Keyboard";
                    break;
                case "Network":
                    key = "Win32_NetworkAdapter";
                    break;
                case "Users":
                    key = "Win32_Account";
                    break;
                default:
                    key = "Win32_Processor";
                    break;
            }
            GetHardwareInfo(key, listView2);
        }
    }
}