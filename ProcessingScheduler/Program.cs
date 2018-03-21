using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessingScheduler
{
    public class Program
    {
        private static int connections;
        private static int delay;

        static void Main(string[] args)
        {
            Console.WriteLine("How many concurrent connections shall we make?");
            connections = int.Parse(Console.ReadLine());

            Console.WriteLine("How many seconds should we delay between connections?");
            delay = int.Parse(Console.ReadLine());

            // Creating multiple connections
            for (int i = 0; i < connections; i++)
            {
                new Thread(StartMonitoring)
                {
                    IsBackground = true
                }.Start();
            }

            Console.ReadLine();
        }

        private async static void StartMonitoring()
        {
            await ProcessData().ContinueWith(task => StartMonitoring());
        }

        private async static Task ProcessData()
        {
            await Task.Delay(delay * 1000);

            try
            {
                using (var dbContext = new AppDbContext())
                {
                    var latestProcessedDate = dbContext.ProcessingRecords.AsNoTracking().OrderByDescending(r => r.ProcessingDate).FirstOrDefault();
                    var shouldProcess = latestProcessedDate == null || latestProcessedDate.ProcessingDate < DateTime.Now;

                    if (shouldProcess)
                    {
                        // Registering the intent to start the processing
                        var now = DateTime.Now;
                        var processingDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
                        var processingRecord = new ProcessingRecord { ProcessingDate = processingDate };
                        await dbContext.ProcessingRecords.AddAsync(processingRecord);
                        await dbContext.SaveChangesAsync();

                        // We start the processing here
                        Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} started the processing");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Logging goes here
                Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId}: skipped starting the processing ({ex.InnerException?.Message ?? ex.Message})");
                return;
            }
        }
    }
}