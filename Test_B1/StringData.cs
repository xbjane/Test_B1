using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Test_B1
{
    internal class StringData
    {
        public StringData(int yearsAgo=5, int numCharLatin=10, int numCharRus=10, int numIntStart=1, int NumIntEnd=100000000, int numDoubleStart=1, int numDoubleEnd=20) 
        {//класс описывает строку для записи в текстовый документ 
            try
            {
                Date = new Random().NextDateTime(yearsAgo);
                char[] charArrayLatin=new char[numCharLatin];
                for (int i = 0; i < charArrayLatin.Length; i++)
                {
                    charArrayLatin[i] = new Random().NextRandomChar(RandomExt.latinSymbols);
                }
                StringEng = new string(charArrayLatin);
                char[] charArrayRussian = new char[numCharRus];
                for (int i = 0; i < charArrayRussian.Length; i++)
                {
                    charArrayRussian[i] = new Random().NextRandomChar(RandomExt.russianSymbols);
                }
                StringRus = new string(charArrayRussian);
                NumInt = new Random().Next(numIntStart, NumIntEnd+1);
                NumDouble = new Random().NextDouble(numDoubleStart, numDoubleEnd);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public StringData(string line) 
        {
            try
            {
                var data = line.Split("||");
                Date = DateTime.Parse(data[0]);
                StringEng = data[1];
                StringRus = data[2];
                NumInt = Convert.ToInt32(data[3]);
                var doubleDatas= data[4].Split(",");
                string numDouble = doubleDatas[0] + "." + doubleDatas[1];
                NumDouble = Convert.ToDouble(data[4]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public int Id { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; private set; }
        public string? StringEng { get; private set; } = null!;
        public string? StringRus { get; private set; } = null!;
        public int NumInt { get;private set; }  
        public double NumDouble { get; private set; }

        public override string ToString()//переопределяем метод ToString()для удобного выаода на экран
        {
            return (Date.ToShortDateString() + "||" + StringEng + "||" + StringRus + "||" + NumInt.ToString() + "||" + NumDouble.ToString() + "||");
        }
    }
}
