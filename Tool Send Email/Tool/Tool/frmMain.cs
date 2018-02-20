using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Threading;
using System.Net.Mime;

namespace Tool
{
    public partial class frmMain : Form
    {
        public bool Flag = true;
        public static readonly string[] arrFileDontInclude = { ".ADE", ".ADP", ".BAT", ".CHM", ".CMD", ".COM", ".CPL", ".EXE", ".HTA", ".INS", ".ISP", ".JAR", ".JS", ".JSE", ".LIB", ".LNK", ".MDE", ".MSC", ".MSI", ".MSP", ".MST", ".NSH", "PIF", ".SCR", ".SCT", ".SHB", ".SYS", ".VB", ".VBE", ".VBS", ".VXD", ".WSC", ".WSF", ".WSH" };

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog()==DialogResult.OK)
            {
                lblPath.Text = openFileDialog1.FileName;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtContent.Text))
                txtContent.Text = "Enter text here...";
        }

        private void txtContent_MouseClick(object sender, MouseEventArgs e)
        {
            if (Flag)
            {
                txtContent.Clear();
                Flag = false;
            }
        }

        private List<string> readMailContent(string link)
        {
            List<string> lstResults = new List<string>();
            try
            {
                foreach (string line in File.ReadAllLines(link))
                {
                    lstResults.Add(line);
                }
            }
            catch
            {
                MessageBox.Show("Try again!!! No file exist");
            }
            return lstResults;
        }

        private void WriteLogFile(string content)
        {
            if (!File.Exists("log.txt"))
            {
                File.Create("log.txt");
                TextWriter tw = new StreamWriter("log.txt");
                tw.WriteLine(content);
                tw.Close();
            }
            else
            {
                using (var tw = new StreamWriter("log.txt", true))
                {
                    tw.WriteLine(content);
                    tw.Close();
                }
            }
        }

        private void sendMail(int milisecond, string content, string title, object sender, EventArgs e)
        {
            foreach (var item in readMailContent(openFileDialog1.FileName)) 
            {
                string[] results = item.Split(',');
                foreach(var items in results) // items = email
                {
                    if(IsValid(items))
                    {
                        var fromAddress = new MailAddress(txtUsername.Text, "");
                        var toAddress = new MailAddress(items, "");
                        string fromPassword = txtPassword.Text.ToString();
                        string subject = title;
                        string body = content.ToString(); // txtContent

                        var smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                        };
                        using (var message = new MailMessage(fromAddress, toAddress)
                        {
                            Subject = subject,
                            Body = body
                        })
                        {
                            if(this.cbxListFile.Items.Count > 0)
                            {
                                for (int i = 0; i < this.cbxListFile.Items.Count; i++) // each file
                                {
                                    bool flagCheck = true;
                                    for (int j = 0; j < arrFileDontInclude.Length; j++)
                                    {
                                        if (this.cbxListFile.Items[i].ToString().EndsWith(arrFileDontInclude[j].ToString()))
                                        {
                                            flagCheck = true;
                                            break;
                                        }
                                    }
                                    if (flagCheck)
                                    {
                                        Attachment attachment = new Attachment(this.cbxListFile.Items[i].ToString(), MediaTypeNames.Application.Octet);
                                        ContentDisposition disposition = attachment.ContentDisposition;
                                        disposition.CreationDate = File.GetCreationTime(this.cbxListFile.Items[i].ToString());
                                        disposition.ModificationDate = File.GetLastWriteTime(this.cbxListFile.Items[i].ToString());
                                        disposition.ReadDate = File.GetLastAccessTime(this.cbxListFile.Items[i].ToString());
                                        disposition.FileName = Path.GetFileName(this.cbxListFile.Items[i].ToString());
                                        disposition.Size = new FileInfo(this.cbxListFile.Items[i].ToString()).Length;
                                        disposition.DispositionType = DispositionTypeNames.Attachment;
                                        message.Attachments.Add(attachment);
                                        flagCheck = false;
                                    }
                                }

                            }
                            Thread.Sleep(milisecond);
                            try
                            {
                                smtp.Send(message);
                            }
                            catch
                            {
                                MessageBox.Show("Having a lot of errors while sending email. Can you check email and password?");
                            }
                            WriteLogFile(items.ToString() + "....." + "Success");
                        }
                    }
                    else // email not true
                    {
                        // email not true => continue and write in log file
                        WriteLogFile(items.ToString() + "....." + "Fail");
                        continue;
                    }
                }
            }
        }

        public bool IsValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("AJackTi designed\n" +
                "My linkedin: https://www.linkedin.com/in/ajackti/\n");
        }

        private void btnSendSlow_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            sendMail(500, txtContent.Text, txtTitle.Text, sender,e);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnFileAttach_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                cbxListFile.Items.Add(openFileDialog2.FileName);
            }
        }

        private void btnSendFast_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            sendMail(100, txtContent.Text, txtTitle.Text, sender, e);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Do you want to exit?", "Exit?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.OK)
            {
                e.Cancel = true;
            }
        }
    }
}
