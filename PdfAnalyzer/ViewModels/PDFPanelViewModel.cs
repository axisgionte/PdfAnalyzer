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
using PdfAnalyzer.Properties;

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
            _ = UpdateCSVMessage();
        }

        private void Open()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select PDF",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = true,
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (openFileDialog.ShowDialog() == false)
            {
                return;
            }

            PDFDocuments.Clear();

            foreach (var file in openFileDialog.FileNames)
            {
                PDFDocuments.Add(new PDFDocumentViewModel(file));
            }

            if (csvDocument.Any())
            {
                _ = UpdateCSVMessage();
            }
        }

        // Handle update CSV message by applying text search to each document
        private void HandleUpdateCSVMessage(PDFPanelViewModel recipient, UpdateCSVMessage message)
        {
            csvDocument = message.CSVDocument;
            _ = UpdateCSVMessage();
        }

        private async Task UpdateCSVMessage()
        {
            var completed = 0;
            cancellationTokenSource?.Cancel(); // Cancel any previous operation
            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Iterate through each PDF document and apply the update asynchronously
            if (pdfDocuments.Any() == false)
            {
                return;
            }

            var findAny = Settings.Default.FindAny;
            var lines = new List<string>();

            lines = findAny
                ? csvDocument
                : csvDocument.Select(line => line.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty)).ToList();

            var tasks = new List<Task>();
            int maxConcurrentTasks = Settings.Default.ThreadCount;
            var semaphore = new SemaphoreSlim(maxConcurrentTasks);

            AllTask = pdfDocuments.Count;
            TaskCompleted = 0;

            foreach (var document in pdfDocuments)
            {
                // Run tasks concurrently with a limit, allowing cancellation if needed
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await semaphore.WaitAsync(cancellationToken);
                        try
                        {         
                            await document.UpdateFindStatus(lines, cancellationToken);
                        }
                        finally
                        {
                            semaphore.Release();
                        }

                        Interlocked.Increment(ref completed);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            TaskCompleted = completed;
                        });

                    }
                    catch (OperationCanceledException)
                    {
                        Debug.WriteLine("Operation was canceled.");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating document: {ex.Message}");
                    }
                }, cancellationToken));
            }

            // Await all tasks to complete before returning control
            try
            {
                await Task.WhenAll(tasks);
                Debug.WriteLine($"All {AllTask} tasks completed.");
            }
            catch (Exception ex)
            {            
                Debug.WriteLine($"An error occurred while processing documents: {ex.Message}");
            }
        }
    }
}