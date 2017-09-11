using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

    class clsLog
    {
        private TextBox m_oTextBox = new TextBox();
        public string m_sLogPath = "";
        public string m_sComputerName = "";
        private enLogLevel m_nLogLevel = enLogLevel.WriteAlways; //1:絶対書く 2:運用が落ち着いたら必要なし 3:デバッグ用途

        public enum enLogLevel
        {
            WriteAlways = 1,
            WriteUntillStable = 2,
            WriteForDebug = 3
        }

        public void Init(TextBox oTextBox)
        {
            m_oTextBox = oTextBox;
            m_oTextBox.Text = "";
        }

        public void LogIt(string sMessage, enLogLevel nLevel)
        {
            if (m_nLogLevel < nLevel)
            {
                return;
            }
            WriteFile(sMessage);
            if (m_oTextBox != null)
            {
                WriteTextBox(sMessage);
            }
        }

        

        private void WriteTextBox(string sMessage)
        {
            int nMaxLength = 0;

            nMaxLength = 30000;
            if (nMaxLength < m_oTextBox.Text.Length)
            {
                m_oTextBox.Text = m_oTextBox.Text.Substring(m_oTextBox.Text.Length - nMaxLength, nMaxLength);
            }
            m_oTextBox.SelectionStart = 65535;
            m_oTextBox.SelectedText = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") + sMessage + "\r\n";

        }

    }

