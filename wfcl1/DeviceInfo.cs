using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace CuoreUI
{
    // a lot of code here just comes from my old project
    // thought i'd add this since we already got CuoreUI.Drawing
    public static class DeviceInfo
    {
        private const ulong BytesPerGigabyte = 1073741824;

        public static List<CPU> GetCPUs()
        {
            var cpus = new ConcurrentBag<CPU>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_Processor"))
            {
                var results = searcher.Get();

                Parallel.ForEach(results.Cast<ManagementObject>(), obj =>
                {
                    cpus.Add(new CPU
                    {
                        Name = obj["Name"]?.ToString(),
                        ProcessorId = obj["ProcessorId"]?.ToString(),
                        DeviceID = obj["DeviceID"]?.ToString(),
                        Manufacturer = obj["Manufacturer"]?.ToString(),
                        CurrentClockSpeed = obj["CurrentClockSpeed"]?.ToString(),
                        NumberOfCores = obj["NumberOfCores"]?.ToString(),
                        NumberOfLogicalProcessors = obj["NumberOfLogicalProcessors"]?.ToString(),
                        Architecture = obj["Architecture"]?.ToString()
                    });
                });
            }
            return cpus.ToList();
        }

        public static List<GPU> GetGPUs()
        {
            var gpus = new List<GPU>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    gpus.Add(new GPU
                    {
                        Name = obj["Name"]?.ToString(),
                        DeviceID = obj["DeviceID"]?.ToString(),
                        AdapterRAM = ConvertToGB(obj["AdapterRAM"]),
                        DriverVersion = obj["DriverVersion"]?.ToString(),
                        VideoProcessor = obj["VideoProcessor"]?.ToString(),
                        VideoMemoryType = obj["VideoMemoryType"]?.ToString()
                    });
                }
            }
            return gpus;
        }

        public static List<RAMStick> GetRAMSticks()
        {
            var ramSticks = new List<RAMStick>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PhysicalMemory"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    ramSticks.Add(new RAMStick
                    {
                        CapacityGB = (uint)(ulong.Parse(obj["Capacity"].ToString()) / BytesPerGigabyte),
                        SpeedMHz = uint.Parse(obj["Speed"].ToString()),
                        Manufacturer = obj["Manufacturer"]?.ToString(),
                        PartNumber = obj["PartNumber"]?.ToString(),
                        SerialNumber = obj["SerialNumber"]?.ToString()
                    });
                }
            }
            return ramSticks;
        }

        public static List<Disk> GetDisks()
        {
            var disks = new List<Disk>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_DiskDrive"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    disks.Add(new Disk
                    {
                        Model = obj["Model"]?.ToString(),
                        SizeGB = (ulong.Parse(obj["Size"].ToString()) / BytesPerGigabyte),
                        InterfaceType = obj["InterfaceType"]?.ToString(),
                        MediaType = obj["MediaType"]?.ToString(),
                        Partitions = obj["Partitions"]?.ToString()
                    });
                }
            }
            return disks;
        }

        private static ulong ConvertToGB(object value)
        {
            if (value == null)
                return 0;
            return (ulong.Parse(value.ToString()) / BytesPerGigabyte);
        }

        public class GPU
        {
            public string Name
            {
                get; set;
            }
            public string DeviceID
            {
                get; set;
            }
            public ulong AdapterRAM
            {
                get; set;
            }
            public string DriverVersion
            {
                get; set;
            }
            public string VideoProcessor
            {
                get; set;
            }
            public string VideoMemoryType
            {
                get; set;
            }

            public override string ToString() => $"{Name} - {AdapterRAM} GB RAM";
        }

        public class CPU
        {
            public string Name
            {
                get; set;
            }
            public string ProcessorId
            {
                get; set;
            }
            public string DeviceID
            {
                get; set;
            }
            public string Manufacturer
            {
                get; set;
            }
            public string CurrentClockSpeed
            {
                get; set;
            }
            public string NumberOfCores
            {
                get; set;
            }
            public string NumberOfLogicalProcessors
            {
                get; set;
            }
            public string Architecture
            {
                get; set;
            }

            public override string ToString() => $"{Name} - {NumberOfCores} Cores";
        }

        public class RAMStick
        {
            public uint CapacityGB
            {
                get; set;
            }
            public uint SpeedMHz
            {
                get; set;
            }
            public string Manufacturer
            {
                get; set;
            }
            public string PartNumber
            {
                get; set;
            }
            public string SerialNumber
            {
                get; set;
            }

            public override string ToString() => $"{CapacityGB} GB RAM @ {SpeedMHz} MHz";
        }

        public class Disk
        {
            public string Model
            {
                get; set;
            }
            public ulong SizeGB
            {
                get; set;
            }
            public string InterfaceType
            {
                get; set;
            }
            public string MediaType
            {
                get; set;
            }
            public string Partitions
            {
                get; set;
            }

            public override string ToString() => $"{Model} - {SizeGB} GB";
        }
    }
}
