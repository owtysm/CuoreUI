using System;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace CuoreUI.Components
{
    public partial class cuiTaskbarStateController : Component
    {
        private Form privateTargetForm;
        public Form TargetForm
        {
            get => privateTargetForm;
            set
            {
                privateTargetForm = value;
                if (privateTargetForm != null)
                {
                    privateTargetForm.Shown += TargetForm_Shown;
                }
            }
        }

        private void TargetForm_Shown(object sender, EventArgs e)
        {
            UpdateProgressValue();
            UpdateTaskbarState();
        }

        public cuiTaskbarStateController()
        {
            InitializeComponent();
            MaxValue = 100;
        }

        public cuiTaskbarStateController(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            MaxValue = 100;
        }

        public enum TaskbarStates
        {
            Progress = 0,
            Paused = 1,
            Error = 2,
            Default = 3,
        }

        private TaskbarStates privateState = TaskbarStates.Default;
        public TaskbarStates State
        {
            get
            {
                return privateState;
            }
            set
            {
                privateState = value;
                UpdateTaskbarState();
            }
        }

        private int privateProgressValue = 50;
        public int ProgressValue
        {
            get
            {
                return privateProgressValue;
            }
            set
            {
                privateProgressValue = value;
                UpdateProgressValue();
            }
        }

        private int privateMaxValue = 100;
        public int MaxValue
        {
            get
            {
                return privateMaxValue;
            }
            set
            {
                privateMaxValue = value;
                UpdateProgressValue();
            }
        }

        private void UpdateTaskbarState()
        {
            if (privateTargetForm == null || DesignMode || privateTargetForm.Handle == IntPtr.Zero)
            {
                return;
            }

            var taskbar = TaskbarManager.Instance;

            switch (privateState)
            {
                case TaskbarStates.Progress:
                    taskbar.SetProgressState(TaskbarProgressBarState.Normal, privateTargetForm.Handle);
                    break;
                case TaskbarStates.Paused:
                    taskbar.SetProgressState(TaskbarProgressBarState.Paused, privateTargetForm.Handle);
                    break;
                case TaskbarStates.Error:
                    taskbar.SetProgressState(TaskbarProgressBarState.Error, privateTargetForm.Handle);
                    break;
                case TaskbarStates.Default:
                default:
                    taskbar.SetProgressState(TaskbarProgressBarState.NoProgress, privateTargetForm.Handle);
                    break;
            }
        }

        private void UpdateProgressValue()
        {
            if (privateTargetForm == null || DesignMode || privateTargetForm.Handle == IntPtr.Zero)
            {
                return;
            }

            var taskbar = TaskbarManager.Instance;

            taskbar.SetProgressValue(privateProgressValue, privateMaxValue, privateTargetForm.Handle);
        }
    }
}
