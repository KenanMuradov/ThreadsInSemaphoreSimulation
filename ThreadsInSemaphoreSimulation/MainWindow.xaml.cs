using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ThreadsInSemaphoreSimulation.UserControls;

namespace ThreadsInSemaphoreSimulation;

public partial class MainWindow : Window
{
    public ObservableCollection<Thread> CreatedThreads { get; set; }
    public ObservableCollection<Thread> WaitingThreads { get; set; }
    public ObservableCollection<Thread> CurrentWorkingThreads { get; set; }
    private readonly Semaphore _semaphore;
    private decimal upDownValue;
    private int workingThreadsCount;
    private int availableThreadsCount;
    private int maxThreadsCount;


    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        CreatedThreads = new();
        WaitingThreads = new();
        CurrentWorkingThreads = new();
        upDownValue = UpDown.Value;
        _semaphore = new(2, 10, "Sema");
        workingThreadsCount = 0;
        availableThreadsCount = 2;
    }

    private void BtnCreate_Click(object sender, RoutedEventArgs e)
    {
        var t = new Thread(MyThreadSimulation);
        t.Name = "Thread number " + t.ManagedThreadId;

        CreatedThreads.Add(t);
    }

    private void MyThreadSimulation(object? semaphore)
    {
        if (semaphore is Semaphore s)
        {
            Thread.Sleep(3000);

            if (s.WaitOne())
            {
                var t = Thread.CurrentThread;
                Dispatcher.Invoke(() => WaitingThreads.Remove(t));
                Dispatcher.Invoke(() => CurrentWorkingThreads.Add(t));
                availableThreadsCount--;
                workingThreadsCount++;
                var workTime = 15;

                t.Name = t.Name + ' ' + workTime;


                while (workTime > 0)
                {
                    Thread.Sleep(1000);
                    workTime--;
                }

                Dispatcher.Invoke(() => CurrentWorkingThreads.Remove(t));
                availableThreadsCount++;
                workingThreadsCount--;
                s.Release();
            }
        }
    }

    private void CreatedThreadsThreadsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (CreatedThreadsThreadsList.SelectedItem is Thread t)
        {
            CreatedThreads.Remove(t);

            WaitingThreads.Add(t);
            t.Start(_semaphore);
        }
    }

    private void UC_NumericUpDown_ValueChanged(object sender, EventArgs e)
    {
        if (sender is UC_NumericUpDown upDown)
        {
            if (upDownValue < upDown.Value)
            {
                _semaphore?.Release();
                availableThreadsCount++;
            }
            else
            {
                if (availableThreadsCount == 0)
                {
                    upDown.Value = upDownValue;
                    return;
                }

                availableThreadsCount--;
                _semaphore?.WaitOne();
            }


            upDownValue = upDown.Value;
        }
    }
}


