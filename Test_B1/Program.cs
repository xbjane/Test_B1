using System.IO;
using System;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace Test_B1
{
    public class Program
    {
        private static readonly string path = "C:/Users/Lenovo/source/repos/Test_B1/Test_B1/bin/Debug/net7.0/note";
        private static ConcurrentQueue<string> conQueue = new ConcurrentQueue<string>();//для хранения данных будем использовать потокобезопасную очередь
        private static ConcurrentQueue<string> lines = new ConcurrentQueue<string>();
        private static ConcurrentQueue<StringData> allStrings = new ConcurrentQueue<StringData>();
        private static int numOfStringsLeft=0; 
        private static int numOfStringsInDb=0;
        static void Main(string[] args)
        {
            Console.WriteLine("Press 1 to generate 100 files \nPress 0 to exit");
            int i = Convert.ToInt32(Console.ReadLine());
            while (i != 0)
            {               
                switch (i)
                {
                    case 1:
                        GenerateFiles();
                        Console.WriteLine("Press 2 to merge files into one \nPress 0 to exit ");
                        i = Convert.ToInt32(Console.ReadLine());
                        switch (i)
                        {
                            case 2:
                                UniteAtFile();
                                Console.WriteLine("Press 3 to load information into database \nPress 2  to merge files into one \nPress 0 to exit ");
                                i = Convert.ToInt32(Console.ReadLine());
                                switch (i)
                                {
                                    case 3:
                                        CreateTableDB();
                                        break;
                                    case 2: UniteAtFile();
                                        break;
                                    default:break;
                                }
                                break;
                            default:break;
                        }
                        break;
                    case 2:

                        break;
                    case 3:
                        CreateTableDB();
                        break;
                    default:break;      
                }
                if (i == 0)
                    break;
                i = Convert.ToInt32(Console.ReadLine());
            }               
        }
        private static void GenerateFiles()
        {//генерация 100 файлов по 100000 строк
            Console.WriteLine("Need to wait a bit...");
            Task[] tasks = new Task[100];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] =  WriteAsync(i);
            }
            Task.WaitAll(tasks);//дожидаемся выполнения всех задач
            Console.WriteLine("Operation was successfully completed!");

            async Task WriteAsync(int i)
            {
                await Task.Run(async () =>// один поток возвращается в место вызова, чтобы раздать остальные задачи                                 // другой продолжает работу здесь
                {
                try
                {
                    using (StreamWriter fS = new StreamWriter(path + (i + 1).ToString() + ".txt", false))
                    {
                        for (int j = 0; j < 100000; j++)
                        { //один поток записывает, следующий идёт следующее записывать 
                            await fS.WriteLineAsync(new StringData(/*5, 10, 10, 1, 100000000, 1, 20*/).ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                 });
            }
        }
        private static void CreateTableDB()
        {
            Console.WriteLine("Need to wait a bit...");
            Task readerTask= ReadAsyncFile();
            Task task = Task.WhenAll(readerTask);
            //пока другие потоки читают из файла, можно начать записывать в БД
            Task writerTask = WriteToDB();
            writerTask.Wait();//ждём выполнения записи
            Console.WriteLine("Operation was successfully completed!");
            async Task ReadAsyncFile()
            {
                await Task.Run(async () => //асинхронное выполнение задачи
                {
                    try
                    {
                        using (StreamReader fS = new StreamReader(path + 0.ToString() + ".txt", false))
                        {
                            string? line;
                            //асинхронно считываем строки из файла
                            while ((line = await fS.ReadLineAsync()) != null)
                            {
                                lines.Enqueue(line);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ReadAsyncFile: " + ex.ToString());
                    }
                });
            }
            async Task WriteToDB()
            {
                try
                {
                    while (task.Status != TaskStatus.RanToCompletion || lines.Count > 0 || allStrings.Count > 0)
                    {
                        await ToDB(new AppContext());

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("WriteToDb: " + ex.ToString());
                }
            }
            async Task ToDB(AppContext appcontext)
            {
                appcontext = new AppContext();
                await Task.Run(async () =>
                {
                    for (int i = 0; i < 100 && lines.Count!=0; i++)
                    {
                        await Task.Run(() =>
                        {
                            if (lines.TryDequeue(out var line))
                                appcontext.StringDatas?.Add(new StringData(line));
                        });
                    }
                    await appcontext.SaveChangesAsync();//сохранем изменения
                    numOfStringsInDb++;
                    Console.WriteLine(numOfStringsLeft-numOfStringsInDb+ "files left");
                    appcontext.Dispose();//освобождаем appContext
                });
            }
        }
       
        private static void UniteAtFile()
        {
            Console.WriteLine("Enter a substring to cut lines: ");
            string substring = Console.ReadLine();
            Console.WriteLine("Need to wait a bit...");
            Task[] tasks = new Task[100];
            
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = ReadAsync(i);

            }
            Task task = Writer();
            task.Wait();
            int delenedRowsNum = 10000000 - numOfStringsLeft;
            Console.WriteLine($"Operation was successfully completed!\nNumber of deleted rows - {delenedRowsNum}");
            async Task ReadAsync(int i)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        using (StreamReader fS = new StreamReader(path + (i + 1).ToString() + ".txt", false))
                        {
                            string? line;
                            while ((line = await fS.ReadLineAsync()) != null)
                            {               
                                    if (substring == null||!line.Contains(substring))
                                    {
                                        numOfStringsLeft++;
                                        conQueue.Enqueue(line);
                                    }
                            }
                        }
                    }                    
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                });
            }
            async Task Writer()
            {
                await Task.Run(async () =>
                {
                    StreamWriter sW = new StreamWriter(path + 0.ToString() + ".txt", false);
                    Task task = Task.WhenAll(tasks);//задача, которая начнёт своё выполнение
                                                    //когда все файлы считаются
                                                    //соответственно все строки взятые из файлов 
                                                    //будут записаны в бд
                                                    //запись в бд закончится когда коллекция будет пуста 
                                                    //и читка из всех файлов закончена

                    while (task.Status!=TaskStatus.RanToCompletion||conQueue.Count>0)
                    {
                       if(conQueue.TryDequeue(out var con))
                        await sW.WriteLineAsync(con);
                    }
                    sW.Close();//закрываем поток чтения
                });
            }
        

        }
    }
}
