﻿using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DJ_X100_memory_writer.Service
{
    internal class WriteMemoryService
    {
        DataGridView dataGridView = new DataGridView();
        CsvFileService createCsvFileService = new CsvFileService();

        public void Write(DataGridView dataGridView, string selectedPort)
        {
            createCsvFileService.ExportDataGridViewToX100CmdCsv(dataGridView, ".\\x100cmd_temp.csv");
            X100cmdForm x100CmdForm = new X100cmdForm();
            x100CmdForm.WriteMemoryChannel(selectedPort);
        }
    }
}

