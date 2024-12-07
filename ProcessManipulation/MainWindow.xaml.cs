using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProcessManipulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string[] availableProcesses = {
            @"C:\Users\DELL\OneDrive\Documents\Visual Studio 2022\ProcessManipulation\ChildProcess\bin\Debug\net8.0-windows\ChildProcess.exe",
            @"C:\Users\DELL\OneDrive\Documents\Visual Studio 2022\ProcessManipulation\ChildProcess2\bin\Debug\net8.0-windows\ChildProcess2.exe"
        };


        public MainWindow()
        {
            InitializeComponent();
            RefreshProcessList();
            LoadAvailableProcesses();
        }

        private void LoadAvailableProcesses()
        {
            // Locate child processes relative to the main executable
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            StartProcessSelector.ItemsSource = availableProcesses.Select(System.IO.Path.GetFileName);
            StartProcessSelector.SelectedIndex = 0; // Select the first process by default
        }

        private void StartSelectedProcess_Click(object sender, RoutedEventArgs e)
        {
            if (StartProcessSelector.SelectedItem is string selectedProcess)
            {
                try
                {
                    // Define paths for both processes
                    string processPath1 = @"C:\Users\DELL\OneDrive\Documents\Visual Studio 2022\ProcessManipulation\ChildProcess\bin\Debug\net8.0-windows\ChildProcess.exe";
                    string processPath2 = @"C:\Users\DELL\OneDrive\Documents\Visual Studio 2022\ProcessManipulation\ChildProcess2\bin\Debug\net8.0-windows\ChildProcess2.exe";

                    // Determine the correct path based on the selected process
                    string processPath = selectedProcess switch
                    {
                        "ChildProcess.exe" => processPath1,
                        "ChildProcess2.exe" => processPath2,
                    };

                    // Check if the path is valid
                    if (processPath == null || !System.IO.File.Exists(processPath))
                    {
                        MessageBox.Show($"The process '{selectedProcess}' was not found.\nExpected path: {processPath}");
                        return;
                    }

                    // Start the process
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = processPath,
                        UseShellExecute = true // Ensures proper handling of GUI apps
                    });

                    // Add to the ActiveProcess ListBox
                    if (!ActiveProcess.Items.Contains(selectedProcess))
                    {
                        ActiveProcess.Items.Add(selectedProcess);
                    }

                    RefreshProcessList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error starting process '{selectedProcess}': {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please select a process to start.");
            }
        }

        private void StopProcess_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveProcess.SelectedItem is string selectedProcess)
            {
                try
                {
                    // Extract process name without ".exe"
                    string processName = selectedProcess.Replace(".exe", "");

                    // Find all running processes with the specified name
                    var processes = Process.GetProcessesByName(processName);

                    if (processes.Length == 0)
                    {
                        MessageBox.Show($"No running instances of '{selectedProcess}' were found.");
                        return;
                    }

                    // Attempt to kill each process
                    foreach (var process in processes)
                    {
                        process.Kill();
                        process.WaitForExit(); // Ensure the process has fully exited
                    }

                    // Remove the stopped process from the ActiveProcess list
                    ActiveProcess.Items.Remove(selectedProcess);

                    // Refresh the process list in the ProcessList ListBox
                    RefreshProcessList();

                    MessageBox.Show($"Successfully stopped the process: {selectedProcess}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error stopping process '{selectedProcess}': {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please select a process to stop from the Active Process list.");
            }
        }


        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessList.SelectedItem is string processName)
            {
                var processes = Process.GetProcessesByName(processName.Replace(".exe", ""));
                foreach (var process in processes)
                {
                    process.CloseMainWindow();
                }
            }
        }

        private void RefreshProcesses_Click(object sender, RoutedEventArgs e) => RefreshProcessList();

        private void RefreshProcessList()
        {
            var runningProcesses = Process.GetProcesses()
                                          .Where(p => p.ProcessName.StartsWith("ChildProcess"))
                                          .Select(p => p.ProcessName + ".exe")
                                          .Distinct()
                                          .ToList();
            ProcessList.ItemsSource = runningProcesses;
        }

        private void RunCalc_Click(object sender, RoutedEventArgs e) => Process.Start("calc.exe");
    }
}