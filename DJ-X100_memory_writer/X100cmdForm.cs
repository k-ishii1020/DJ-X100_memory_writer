﻿using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace DJ_X100_memory_writer
{
    public partial class X100cmdForm : Form
    {
        private Process process = null;

        public X100cmdForm()
        {
            InitializeComponent();
        }

        private bool CheckX100cmdVersion()
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c .\\x100cmd.exe -v",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            process.StartInfo = startInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var versionMatch = Regex.Match(output, @"version (\d+\.\d+\.\d+)");
            if (versionMatch.Success)
            {
                var versionString = versionMatch.Groups[1].Value;
                var version = new Version(versionString);

                var requiredVersion = new Version("1.3.10");
                if (version < requiredVersion)
                {
                    MessageBox.Show("x100cmdのバージョンが要求バージョン以下です。" + requiredVersion + "以上のバージョンを使用してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    return false;
                }
            }
            else
            {
                MessageBox.Show("x100cmdのバージョン情報を取得できませんでした。\nx100cmdはこのプログラムと同じディレクトリに配置が必要です。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();

                return false;
            }
            return true;
        }

        public bool ReadMemoryChannel(string selectedPort)
        {
            if (!CheckX100cmdVersion()) return false;

            string port;

            if (selectedPort == "自動選択")
            {
                port = "auto";
            }
            else
            {
                port = selectedPort;
            }

            string command = $"/K x100cmd.exe -p {port} export -y -a --ext x100cmd_temp_export.csv && pause && exit";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = command,
                UseShellExecute = false
            };

            var process = new Process { StartInfo = processStartInfo };
            process.Start();
            process.WaitForExit();
            MessageBox.Show("メモリチャンネルの読み込みが完了しました", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return true;
        }

        public void WriteMemoryChannel(string selectedPort)
        {
            if (!CheckX100cmdVersion()) return;

            string port;

            if (selectedPort == "自動選択")
            {
                port = "auto";
            }
            else
            {
                port = selectedPort;
            }

            string command = $"/k .\\x100cmd.exe -r -p" + port + " import x100cmd_temp.csv && pause && exit";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = command,
                UseShellExecute = false
            };

            var process = new Process { StartInfo = processStartInfo };
            process.Start();
            process.WaitForExit();
            MessageBox.Show("メモリチャンネルの書き込みが完了しました", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



        public async Task ReadBankName(Action<char, string> updateBankName)
        {
            if (!CheckX100cmdVersion()) return;

            for (char c = 'A'; c <= 'Z'; c++)
            {
                string command = $".\\x100cmd.exe bank read {c}";
                string output = await ExecuteBankNameWrite(command);

                var match = Regex.Match(output, $"\"{c}\",\"(.+)\"");
                string bankName = match.Success ? match.Groups[1].Value.Trim() : "";

                updateBankName(c, bankName);

                await Task.Delay(350);

                if (c == 'Z')
                {
                    this.Invoke((Action)delegate
                    {
                        textBox1.AppendText("メモリバンクの読込が完了しました。" + Environment.NewLine);
                        progressBar1.Style = ProgressBarStyle.Continuous;
                        progressBar1.Value = progressBar1.Maximum;
                        MessageBox.Show("メモリバンクの読込が完了しました。実行結果を確認してください。", "読み込み完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        okButton.Enabled = true;
                    });
                }
            }
        }

        private async Task<string> ExecuteBankNameWrite(string command)
        {
            var output = new StringBuilder();

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c chcp 65001 > NUL && {command}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
            };

            process = new Process { StartInfo = processStartInfo };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                            this.Invoke((Action)delegate
                            {
                                textBox1.AppendText(e.Data + Environment.NewLine);
                            });
                    output.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();

            this.Invoke((Action)delegate
            {
                textBox1.AppendText(Environment.NewLine);
            });
            output.AppendLine();

            return output.ToString();
        }

        public async Task WriteBankName(string selectedPort, Dictionary<char, string> bankNames)
        {
            if (!CheckX100cmdVersion()) return;

            string port = selectedPort == "自動選択" ? "auto" : selectedPort;

            for (char c = 'A'; c <= 'Z'; c++)
            {
                string bankName = bankNames.ContainsKey(c) ? bankNames[c] : "";

                string command = $".\\x100cmd.exe bank write -y {c} \"{bankName}\"";
                string output = await ExecuteBankNameWrite(command);

                if (output.Contains("OK"))
                {
                    this.Invoke((Action)delegate
                    {
                        textBox1.AppendText($"メモリバンク {c} ({bankName}) への書き込みが成功しました。" + Environment.NewLine);
                    });
                }
                else
                {
                    this.Invoke((Action)delegate
                    {
                        textBox1.AppendText($"メモリバンク {c} ({bankName}) への書き込みに失敗しました。エラー: {output}" + Environment.NewLine);
                        MessageBox.Show($"メモリバンク {c} ({bankName}) への書き込みに失敗しました。\nエラー: {output}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    });
                    return;
                }

                await Task.Delay(350);

                if (c == 'Z')
                {
                    this.Invoke((Action)delegate
                    {
                        textBox1.AppendText("\n\nメモリバンクへの書き込みが完了しました。" + Environment.NewLine);
                        progressBar1.Style = ProgressBarStyle.Continuous;
                        progressBar1.Value = progressBar1.Maximum;
                        MessageBox.Show("メモリバンクへの書き込みが完了しました。実行結果を確認してください。", "書き込み完了", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        okButton.Enabled = true;
                    });
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            process?.Kill();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
