using DJ_X100_memory_writer.Service;
using System;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

namespace DJ_X100_memory_writer
{
    public partial class Form1 : Form
    {
        CreateCsvFileService csvUtils = new CreateCsvFileService();
        WriteMemoryService writeMemory = new WriteMemoryService();

        private string selectedPort;
        private List<ToolStripMenuItem> portMenuItems;

        public Form1()
        {
            InitializeComponent();

            this.Load += Form1_Load;

            InitComPort();
            treeViewSetup();
            var configurer = new MemoryChannnelSetupService(memoryChDataGridView);
            configurer.SetupDataGridView();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(
                "���̃\�t�g�͌��J�O�̃x�[�^�łł��B�o���N�l�[���o�^�͍쐬���ł��B\n" +
                "DJ-X100�{�̂Ȃǂ̕s��������̐ӔC�ɂ���\n" +
                "��҂͈�؂̐ӔC�𕉂����˂܂��B\n\n" +
                "�A�v���P�[�V�����𗧂��グ��Ɠ��ӂ������̂Ƃ��܂��B\n" +
                "��낵���ł����H",
                "DJ-X100 Memory Writer(�����)",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            if (dialogResult != DialogResult.Yes)
            {
                Application.Exit();
            }
        }


        private void InitComPort()
        {
            portMenuItems = new List<ToolStripMenuItem>();

            ToolStripMenuItem autoSelectItem = new ToolStripMenuItem("�����I��");
            autoSelectItem.Click += PortSelectClick;
            autoSelectItem.CheckOnClick = true;
            cOM�|�[�gCToolStripMenuItem.DropDownItems.Add(autoSelectItem);
            portMenuItems.Add(autoSelectItem);

            foreach (String portName in GetPortLists())
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem(portName);
                menuItem.Click += PortSelectClick;
                menuItem.CheckOnClick = true;
                cOM�|�[�gCToolStripMenuItem.DropDownItems.Add(menuItem);
                portMenuItems.Add(menuItem);
            }

            autoSelectItem.PerformClick();
        }

        private void PortSelectClick(object sender, EventArgs e)
        {
            // �I�����ꂽ���ڂ�ۑ����܂�
            ToolStripMenuItem clickedItem = sender as ToolStripMenuItem;
            selectedPort = clickedItem.Text;

            selectedComportLabel.Text = "�I�𒆂�COM�|�[�g: " + selectedPort;

            // ���̂��ׂĂ̍��ڂ̃`�F�b�N���������܂�
            foreach (var item in portMenuItems)
            {
                if (item != clickedItem)
                {
                    item.Checked = false;
                }
            }
        }

        private static String[] GetPortLists()
        {
            String[] portList = SerialPort.GetPortNames();
            Array.Sort(portList);
            return portList;
        }

        private void treeViewSetup()
        {
            treeView1.ExpandAll();
            string searchText = "�������[�`�����l��";
            SelectNodeByText(treeView1, searchText);
        }

        public void SelectNodeByText(TreeView treeView, string searchText)
        {
            // TreeView���̂��ׂẴm�[�h������
            foreach (TreeNode node in treeView.Nodes)
            {
                if (FindNode(node, searchText))
                {
                    break;
                }
            }
        }

        private bool FindNode(TreeNode treeNode, string searchText)
        {
            // ���݂̃m�[�h�̃e�L�X�g���m�F
            if (treeNode.Text == searchText)
            {
                // �m�[�h��I����Ԃɐݒ�
                treeNode.TreeView.SelectedNode = treeNode;
                // �I�������m�[�h��������悤�ɃX�N���[��
                treeNode.EnsureVisible();
                return true;
            }
            // �q�m�[�h�����݂���ꍇ�́A�����̃m�[�h�𑖍�
            foreach (TreeNode node in treeNode.Nodes)
            {
                if (FindNode(node, searchText))
                {
                    return true;
                }
            }
            // �}�b�`����m�[�h��������Ȃ��ꍇ��false��Ԃ�
            return false;
        }

        private void �V�K�쐬NToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!IsDataGridViewEmpty() &&
                MessageBox.Show("�쐬���̃f�[�^�͔j������܂��B��낵���ł����H", "�x��", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                memoryChDataGridView.Rows.Clear();
                memoryChDataGridView.Columns.Clear();
                var configurer = new MemoryChannnelSetupService(memoryChDataGridView);
                configurer.SetupDataGridView();
            }
        }

        private void �J��NToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!IsDataGridViewEmpty() &&
                MessageBox.Show("�쐬���̃f�[�^�͔j������܂��B��낵���ł����H", "�x��", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "CSV�t�@�C�����J��",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Filter = "CSV�t�@�C��(*.csv)|*csv|���ׂẴt�@�C��(*.*)|*.*",
                    FilterIndex = 0,
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    csvUtils.ImportCsvToDataGridView(memoryChDataGridView, openFileDialog.FileName);
                }
            }
        }

        private void �I��NToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ���O��t���ĕۑ�NToolStrpMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "���O������CSV�t�@�C����ۑ�";
            saveFileDialog.InitialDirectory = @"C:\";
            saveFileDialog.Filter = "CSV�t�@�C��(*.csv)|*csv|���ׂẴt�@�C��(*.*)|*.*";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog.AddExtension = true;
            DialogResult result = saveFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                csvUtils.ExportDataGridViewToCsv(memoryChDataGridView, saveFileDialog.FileName);
            }
        }

        private void ��������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(
                "���̕\�ɓo�^���Ă��Ȃ��������͑S�ď����܂��B\n" +
                "�܂��A�s��ɂ���ă�������������A�������͓��삪�s����ɂȂ�\n" +
                "���ꂪ����܂��̂ŕK�����O�Ƀo�b�N�A�b�v������Ă��������B\n" +
                "x100cmd.exe export --ext backup.csv\n" +
                "�{���ɏ������݂𑱍s���Ă���낵���ł����H",
                "�x��",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning
            );

            if (dialogResult == DialogResult.OK)
            {
                writeMemory.Write(memoryChDataGridView, selectedPort);
            }
        }


        private void x100cmdexe�pCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "���O������x100cmd�pCSV�t�@�C����ۑ�";
            saveFileDialog.InitialDirectory = @"C:\";
            saveFileDialog.Filter = "CSV�t�@�C��(*.csv)|*csv|���ׂẴt�@�C��(*.*)|*.*";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog.AddExtension = true;
            DialogResult result = saveFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                csvUtils.ExportDataGridViewToX100CmdCsv(memoryChDataGridView, saveFileDialog.FileName);
            }
        }

        private bool IsDataGridViewEmpty()
        {
            foreach (DataGridViewRow row in memoryChDataGridView.Rows)
            {
                for (int i = 1; i < row.Cells.Count; i++)
                {
                    if (row.Cells[i].Value != null && !string.IsNullOrWhiteSpace(row.Cells[i].Value.ToString()))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void �o���N�ݒ�BToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2(this);
            form2.Show();
        }

        public void UpdateTreeView(List<string> bankNames)
        {
            TreeNode parentNode = treeView1.Nodes
                .OfType<TreeNode>()
                .FirstOrDefault(n => n.Name == "djx100Node");

            if (parentNode != null)
            {
                TreeNode bankMemoryNode = parentNode.Nodes
                    .OfType<TreeNode>()
                    .FirstOrDefault(n => n.Name == "bankMemoryNode");

                if (bankMemoryNode != null)
                {
                    bankMemoryNode.Nodes.Clear();

                    // �V�����q�m�[�h��ǉ�
                    for (int i = 0; i < bankNames.Count; i++)
                    {
                        // �A���t�@�x�b�g��A���珇�Ɏ擾���A����𖼑O�̐擪�ɒǉ�
                        string bankName = ((char)('A' + i)).ToString() + ": " + bankNames[i];

                        // imageKey���w�肵�ăm�[�h���쐬
                        TreeNode node = new TreeNode(bankName)
                        {
                            ImageKey = "kkrn_icon_folder_1.png",
                            SelectedImageKey = "kkrn_icon_folder_1.png"
                        };
                        bankMemoryNode.Nodes.Add(node);
                    }
                }
            }
        }

        public TreeNode GetBankNode(string bankLabel)
        {
            TreeNode parentNode = treeView1.Nodes
                .OfType<TreeNode>()
                .FirstOrDefault(n => n.Name == "djx100Node");

            if (parentNode != null)
            {
                TreeNode bankMemoryNode = parentNode.Nodes
                    .OfType<TreeNode>()
                    .FirstOrDefault(n => n.Name == "bankMemoryNode");

                if (bankMemoryNode != null)
                {
                    TreeNode bankNode = bankMemoryNode.Nodes
                        .OfType<TreeNode>()
                        .FirstOrDefault(n => n.Text.StartsWith(bankLabel + ":"));

                    return bankNode;
                }
            }

            return null;
        }




    }
}