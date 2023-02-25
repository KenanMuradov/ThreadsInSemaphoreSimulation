﻿using System;
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

namespace ThreadsInSemaphoreSimulation;

public partial class MainWindow : Window
{
    public ObservableCollection<Thread> CreatedThreads { get; set; }
    public ObservableCollection<Thread> WaitingThreads { get; set; }
    public ObservableCollection<Thread> CurrentWorkingThreads { get; set; }
    private readonly Semaphore _semaphore;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;


        CreatedThreads = new();
        WaitingThreads = new();
        CurrentWorkingThreads = new();

        _semaphore = new(3, 3, "Sema");
    }

    private void BtnCreate_Click(object sender, RoutedEventArgs e)
    {
        var t = new Thread(MyThreadSimulation);
        t.Name = "Thread number " + t.ManagedThreadId;

        CreatedThreads.Add(t);
    }

    private void MyThreadSimulation(object? semaphore)
    {
        if(semaphore is Semaphore s)
        {
            Thread.Sleep(3000);

            if(s.WaitOne())
            {
                var t = Thread.CurrentThread;
                Dispatcher.Invoke(()=>WaitingThreads.Remove(t));
                Dispatcher.Invoke(()=>CurrentWorkingThreads.Add(t));
                var workTime = Random.Shared.Next(3, 10);

                t.Name = t.Name + ' ' + workTime;


                while (workTime > 0)
                {
                    Thread.Sleep(1000);
                    workTime--;
                }

                Dispatcher.Invoke(() => CurrentWorkingThreads.Remove(t));
                s.Release();
            }
        }
    }

    private void CreatedThreadsThreadsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if(CreatedThreadsThreadsList.SelectedItem is Thread t)
        {
            CreatedThreads.Remove(t);

            WaitingThreads.Add(t);
            t.Start(_semaphore);
        }
    }
}


