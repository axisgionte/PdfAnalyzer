using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PdfAnalyzer.Messages;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows;

namespace PdfAnalyzer.ViewModels
{
    public class PDFPanelViewModel : ObservableRecipient
    {
        private PDFDocumentViewModel pdfDocument;
        private ObservableCollection<PDFDocumentViewModel> pdfDocuments;
        private CancellationTokenSource cancellationTokenSource;
        private List<string> csvDocument;
        private int allTask;
        private int taskCompleted;

        public ICommand OpenPDFCommand { get; }
        public ICommand SearchCommand { get; }

        public ObservableCollection<PDFDocumentViewModel> PDFDocuments
        {
            get => pdfDocuments;
            set => SetProperty(ref pdfDocuments, value);
        }

        public PDFDocumentViewModel PDFDocument
        {
            get => pdfDocument;
            set => SetProperty(ref pdfDocument, value);
        }

        public int AllTask
        {
            get => allTask;
            set => SetProperty(ref allTask, value);
        }

        public int TaskCompleted
        {
            get => taskCompleted;
            set => SetProperty(ref taskCompleted, value);
        }

        public PDFPanelViewModel()
        {
            Messenger.Register<PDFPanelViewModel, UpdateCSVMessage, int>(this, 1, HandleUpdateCSVMessage);

            pdfDocuments = new ObservableCollection<PDFDocumentViewModel>();
            csvDocument = new List<string>();
            OpenPDFCommand = new RelayCommand(Open);
            SearchCommand = new RelayCommand(Search);
            Application.Current.Exit += Exit;
        }

        private void Exit(object sender, ExitEventArgs e)
        {
           cancellationTokenSource?.Cancel();
        }

        private void Search()
        {
            UpdateCSVMessage();
        }

        // Open file dialog and add selected PDFs to the ObservableCollection
        private void Open()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select PDF",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = true,
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (openFileDialog.ShowDialog() == true)
            {
                PDFDocuments.Clear();
                foreach (var file in openFileDialog.FileNames)
                {
                    PDFDocuments.Add(new PDFDocumentViewModel(file));
                }

                if (csvDocument.Any()) 
                {
                    UpdateCSVMessage();
                }
            }
        }

        // Handle update CSV message by applying text search to each document
        private void HandleUpdateCSVMessage(PDFPanelViewModel recipient, UpdateCSVMessage message)
        {
            csvDocument = message.CSVDocument;
            UpdateCSVMessage();
        }

        private async Task UpdateCSVMessage()
        {    
            cancellationTokenSource?.Cancel(); // Cancel any previous operation
            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Iterate through each PDF document and apply the update asynchronously
            if (pdfDocuments.Any())
            {
                
                var lines = csvDocument.Select(word => word.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty)).ToList();
                var tasks = new List<Task>();
                int maxConcurrentTasks = Properties.Settings.Default.ThreadCount;
                var semaphore = new SemaphoreSlim(maxConcurrentTasks); // Limit number of concurrent tasks

                AllTask = pdfDocuments.Count;
                TaskCompleted = 0;

                foreach (var document in pdfDocuments)
                {
                    // Run tasks concurrently with a limit, allowing cancellation if needed
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await semaphore.WaitAsync();  // Wait until we can acquire the semaphore
                            try
                            {
                                // Pass the cancellation token to handle cancellation
                                await document.UpdateFindStatus(lines, cancellationToken);
                            }
                            finally
                            {
                                semaphore.Release();  // Release the semaphore so other tasks can proceed
                            }

                            // Update task completion progress
                            TaskCompleted++;
                        }
                        catch (OperationCanceledException)
                        {
                            Debug.WriteLine("Operation was canceled.");
                        }
                        catch (Exception ex)
                        {
                            // Handle other errors
                            Debug.WriteLine($"Error updating document: {ex.Message}");
                        }
                    }, cancellationToken));
                }

                // Await all tasks to complete before returning control
                try
                {
                    await Task.WhenAll(tasks);  // Wait until all tasks are completed
                    Debug.WriteLine($"All {AllTask} tasks completed.");
                }
                catch (Exception ex)
                {
                    // Catch and handle any exception if needed
                    Debug.WriteLine($"An error occurred while processing documents: {ex.Message}");
                }
            }
        }

        
    }
}