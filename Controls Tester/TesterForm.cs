using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace ControlsTester
{
    public partial class TesterForm : Form
    {
        List<Control> availableControls = new List<Control>();
        int controlsCount = 0;
        int componentsCount = 0;
        int currentControlIndex = 0;

        List<string> controlsNames = new List<string>();
        List<string> componentsNames = new List<string>();

        public TesterForm()
        {
            InitializeComponent();
            InitializeTester();
        }

        private void InitializeTester()
        {
            string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // "\CuoreUI\"
            string cuoreSourceFolder = Directory.GetParent(exeDir).Parent.Parent.FullName;

            // "\CuoreUI\wfcl1/bin/Release/net472/CuoreUI.dll"
            string cuoreBinaryPath = Path.Combine(cuoreSourceFolder, "wfcl1", "bin", "Release", "net472", "CuoreUI.dll");

            if (!File.Exists(cuoreBinaryPath))
            {
                MessageBox.Show($"Couldn't find ../../wfcl1/bin/Release/net472/CuoreUI.dll!\nChecked path: \"{cuoreBinaryPath}\"");
                Environment.Exit(0);
                return;
            }

            Assembly assembly = Assembly.LoadFrom(cuoreBinaryPath);

            Type[] types = assembly.GetTypes();

            controlsCount = 0;
            componentsCount = 0;

            foreach (Type type in types)
            {
                if (type.IsCuore())
                {
                    if (type.FullName.StartsWith("CuoreUI.Controls.") && type.IsSubclassOf(typeof(Control)) && !type.IsSubclassOf(typeof(Form)))
                    {
                        label2.Text = $"Loading {type.Name} ({type.Namespace})";
                        controlsNames.Add(type.Name);

                        Control control = Activator.CreateInstance(type) as Control;
                        availableControls.Add(control);
                        controlsCount++;
                    }
                    else if (type.FullName.StartsWith("CuoreUI.Components."))
                    {
                        label3.Text = $"Loading {type.Name} ({type.Namespace})";
                        componentsNames.Add(type.Name);

                        componentsCount++;
                    }
                }
            }

            AllClassesLoaded();
        }

        private void AllClassesLoaded()
        {
            label2.Text = $"Loaded {controlsCount} controls successfully!";
            label3.Text = $"Found {componentsCount} components!";

            panel2.Enabled = true;
            button3.Enabled = true;

            if (controlsCount > 0)
            {
                ShowControl(0);
            }
        }

        void ShowControl()
        {
            Control control = availableControls[currentControlIndex];
            ShowControl(control);
        }

        void ShowControl(int index)
        {
            Control control = availableControls[index];
            ShowControl(control);
        }

        void ShowControl(Control control)
        {
            label1.Text = $"{control.GetType()} ({currentControlIndex + 1}/{controlsCount})";

            control.Anchor = AnchorStyles.None;
            control.Location = new Point(panel1.Width / 2 - control.Width / 2, panel1.Height / 2 - control.Height / 2);

            panel1.Controls.Clear();
            panel1.Controls.Add(control);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            panel1.BackColor = checkBox1.Checked ? Color.Black : SystemColors.Control;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            currentControlIndex = Math.Min(controlsCount - 1, currentControlIndex + 1);
            ShowControl();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            currentControlIndex = Math.Max(0, currentControlIndex - 1);
            ShowControl();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form a = new Form() { Height = 512 + 72 };

            var controlsListBox = new RichTextBox() { ReadOnly = true, Height = 256, Top = 16, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Width = a.Width - 16, ScrollBars = RichTextBoxScrollBars.Vertical };
            var componentsListBox = new RichTextBox() { ReadOnly = true, Height = 256, Top = 16 + 256 + 16, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Width = a.Width - 16, ScrollBars = RichTextBoxScrollBars.Vertical };

            a.Controls.Add(new Label() { Text = "CONTROLS", Height = 16 });
            a.Controls.Add(controlsListBox);

            a.Controls.Add(new Label() { Text = "COMPONENTS", Height = 16, Top = 16 + 256 });
            a.Controls.Add(componentsListBox);

            foreach (string controlName in controlsNames)
            {
                controlsListBox.AppendText($"{controlName}\n");
            }

            foreach (string componentName in componentsNames)
            {
                componentsListBox.AppendText($"{componentName}\n");
            }

            a.Show();
        }
    }

    public static class TypeExtensions
    {
        public static bool IsCuore(this Type type)
        {
            // check if control is marked with [ToolboxItem(false)] attribute
            object[] toolboxAttributes = type.GetCustomAttributes(typeof(ToolboxItemAttribute), false);
            if (toolboxAttributes.Length > 0)
            {
                return false;
            }

            return type.IsClass
                && type.IsPublic
                && type.Name.StartsWith("cui");
        }
    }
}
