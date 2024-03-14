﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace _kege_sft_form
{
    public partial class ch_kege_sft_frm : Form
    {
        static List<RegisteredSoftware> programs = new List<RegisteredSoftware>();
        static List<RegisteredSoftware> selected_programs = new List<RegisteredSoftware>();
        RegisteredSoftware current_programm = new RegisteredSoftware();
        List<string> list_groups = new List<string>();
        List<String> list_group_del = new List<string>();
        string fileText;
        string decode_file;

        string save_path;

        public ch_kege_sft_frm()
        {
            InitializeComponent();

            openFileDialog1.Filter = "SFT files(*.sft)|*.sft";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            programs.Clear();
            selected_programs.Clear();
            list_groups.Clear();
            list_group_del.Clear();
            fileText = "";
            decode_file = "";

            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            // получаем выбранный файл
            string filename = openFileDialog1.FileName;
            // читаем файл
            fileText = File.ReadAllText(filename);
            textBox1.Text = filename;
            save_path = Path.GetDirectoryName(openFileDialog1.FileName);
            save_path = save_path + @"\SOFT_KEGE_NEW.sft";

            // декодируем в биты
            var decode_file_bit = Convert.FromBase64String(fileText);

            // переводим биты в текст
            decode_file = Encoding.UTF8.GetString(decode_file_bit);

            // сохраняем XML по пути корня программы с именем temp.xml
            FileStream Swr = new FileStream("temp.xml", FileMode.Create, FileAccess.Write);
            byte[] arr = Encoding.Unicode.GetBytes(decode_file);
            Swr.Write(arr, 0, arr.Length);
            Swr.Close();
            Swr.Dispose();

            textBox2.Text = save_path;

            Work_XML();
        }

        private void Work_XML()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(Environment.CurrentDirectory + @"\temp.xml");
            // получим корневой элемент
            XmlElement xRoot = xDoc.DocumentElement;
            if (xRoot != null)
            {
                // обход всех узлов в корневом элементе
                foreach (XmlElement xnode in xRoot)
                {
                    current_programm = new RegisteredSoftware();
                    // обходим все дочерние узлы элемента user
                    foreach (XmlNode childnode in xnode.ChildNodes)
                    {
                        // если узел - id
                        if (childnode.Name == "Id")
                        {
                            current_programm.Id = childnode.InnerText;
                        }
                        // если узел RegisterType
                        if (childnode.Name == "RegisterType")
                        {
                            current_programm.RegisterType = childnode.InnerText;
                        }

                        if (childnode.Name == "SoftwareType")
                        {
                            current_programm.SoftwareType = childnode.InnerText;
                        }

                        if (childnode.Name == "Name")
                        {
                            current_programm.Name = childnode.InnerText;
                        }

                        if (childnode.Name == "Version")
                        {
                            current_programm.Version = childnode.InnerText;
                        }

                        if (childnode.Name == "ProgrammingLanguage")
                        {
                            current_programm.ProgrammingLanguage = childnode.InnerText;
                        }
                    }
                    programs.Add(current_programm);
                    list_groups.AddRange(new[] { current_programm.SoftwareType });
                    list_group_del = list_groups.Distinct().ToList();
                }
            }
            UpdateLV();
        }

        private void UpdateLV()
        {
            foreach (string gp in list_group_del)
            {
                listView1.Groups.Add(new ListViewGroup(gp.ToString()));
            }

            for (int j = 0; j < programs.Count; j++)
            {
                listView1.Items.Add(programs[j].Name).Checked = true;
            }

            for (int i = 0; i < programs.Count; i++)
            {
                for(int j = 0; j <  list_group_del.Count; j++)
                {
                    if (list_groups[i] == list_group_del[j])
                    {
                        listView1.Items[i].Group = listView1.Groups[j];
                    }
                }
            }

            saveBTN.Enabled = true;
        }

        private void saveBTN_Click(object sender, EventArgs e)
        {
            var selectedLV = listView1.CheckedItems;

            for (int i = 0; i < programs.Count; i++)
            {
                for (int j = 0; j < selectedLV.Count; j++) 
                {
                    if (programs[i].Name == selectedLV[j].Text)
                    {
                        selected_programs.Add(programs[i]);
                    }
                }
            }

            SaveXML();
        }

        private void SaveXML()
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Encoding = Encoding.GetEncoding("utf-16");
            xmlWriterSettings.Indent = true;

            XmlWriter xW = XmlWriter.Create("new.xml", xmlWriterSettings);

            xW.WriteStartDocument();
            xW.WriteStartElement("ArrayOfRegisteredSoftware");
            xW.WriteAttributeString(@"xmlnsxsd", @"http://www.w3.org/2001/XMLSchema");
            xW.WriteAttributeString(@"xmlnsxsi", @"http://www.w3.org/2001/XMLSchema-instance");

            for (int i = 0; i < selected_programs.Count; i++)
            {
                xW.WriteStartElement("RegisteredSoftware");

                xW.WriteStartElement("Id");
                xW.WriteString(selected_programs[i].Id);
                xW.WriteEndElement();

                xW.WriteStartElement("RegisterType");
                xW.WriteString(selected_programs[i].RegisterType);
                xW.WriteEndElement();

                xW.WriteStartElement("SoftwareType");
                xW.WriteString(selected_programs[i].SoftwareType);
                xW.WriteEndElement();

                xW.WriteStartElement("Name");
                xW.WriteString(selected_programs[i].Name);
                xW.WriteEndElement();

                xW.WriteStartElement("Version");
                xW.WriteString(selected_programs[i].Version);
                xW.WriteEndElement();

                xW.WriteStartElement("ProgrammingLanguage");
                xW.WriteString(selected_programs[i].ProgrammingLanguage);
                xW.WriteEndElement();

                xW.WriteEndElement();
            }
            xW.WriteEndDocument();
            xW.Close();

            toBase64();
        }

        private void toBase64()
        {
            string new_xml_file_text = File.ReadAllText("new.xml");
            byte[] code_data = Encoding.UTF8.GetBytes(new_xml_file_text);
            var code_file_to64 = Convert.ToBase64String(code_data);
            code_data = Encoding.UTF8.GetBytes(code_file_to64);

            FileStream fstream = null;
            try
            {
                if(File.Exists(save_path)) { File.Delete(save_path); }
                fstream = new FileStream(save_path, FileMode.OpenOrCreate);
                fstream.Write(code_data, 0, code_data.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error ", "Write error", MessageBoxButtons.OK);
            }
            finally
            {
                fstream.Close();
                MessageBox.Show("Файл сохранен по пути: " + save_path, "Файл сохранен", MessageBoxButtons.OK);
            }
        }
    }
}
