using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace ControlsTester
{
    public partial class TesterForm : Form
    {
        System.Collections.Generic.List<Control> availableControls = new System.Collections.Generic.List<Control>();
        int controlsCount = 0;
        int currentControlIndex = 0;

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

            foreach (Type type in types)
            {
                if (type.IsCuoreControl())
                {
                    label2.Text = $"Loading {type.Name} ({type.Namespace})";
                    Control control = Activator.CreateInstance(type) as Control;
                    availableControls.Add(control);
                    controlsCount++;
                }
            }

            AllControlsLoaded();
        }

        private void AllControlsLoaded()
        {
            label2.Text = $"Loaded {controlsCount} controls successfully!";
            panel2.Enabled = true;

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
            label1.Text = $"{control.GetType()} ({currentControlIndex+1}/{controlsCount})";

            control.Anchor = AnchorStyles.None;
            control.Location = new Point(panel1.Width / 2 - control.Width / 2, panel1.Height / 2 - control.Height / 2);

            panel1.Controls.Clear();

            if (control.GetType().Name != "cuiResizeGrip")
            {
                panel1.Controls.Add(control);
            }
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
    }

    public static class TypeExtensions
    {
        public static bool IsCuoreControl(this Type type)
        {
            // check if control is marked with [ToolboxItem(false)] attribute
            object[] toolboxAttributes = type.GetCustomAttributes(typeof(ToolboxItemAttribute), false);
            if (toolboxAttributes.Length > 0)
            {
                return false;
            }

            return type.IsClass
                && type.IsPublic
                && type.IsSubclassOf(typeof(Control))
                && !type.IsSubclassOf(typeof(Form))
                && type.FullName.StartsWith("CuoreUI.Controls.");
        }
    }
}
