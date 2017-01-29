﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Linq.Expressions;
using Microsoft.VisualBasic.FileIO;

/*/

get header
latest price
加重平均
_25DMA上昇回数

/*/

namespace ConsoleApplication1
{
    class Program
    {
        struct Record
        {
            public string 日付;
            public string 始値;
            public string 高値;
            public string 安値;
            public string 終値;
            public string 前日比;
            public string 出来高;
            public string 貸株残高;
            public string 融資残高;
            public string 貸借倍率;
            public string 逆日歩;
            public string 特別空売り料;
            public string _5DMA;
            public string _25DMA;
            public string _5DVMA;
            public string _25DVMA;
        }

        struct Score : IComparable
        {
            public string コード;
            public double 直近日当たりの上昇率;
            public double 直近傾きの上昇率;
            public double 最低株価;
            public double 株価;
            public double 最高株価;
            public double 加重平均株価;
            public double 注文買値1;
            public double 注文買値2;
            public double 注文買値3;
            public double minus_min;
            public double minus_ave;
            public double minus_max;
            public double plus_min;
            public double plus_ave;
            public double plus_max;
            public double current_wave;
            public byte _25DMA上昇回数;
            public ulong 取引量;
            public bool is25up;
            public double _25DMA乖離率;
            public double 加重平均乖離率;
            public bool is25RateUP;
            public bool convex;

            public override string ToString()
            {
                return
                    ""  + コード +
                    "," + 直近日当たりの上昇率 +
                    "," + 直近傾きの上昇率 +
                    "," + 最低株価 +
                    "," + 株価 +
                    "," + 最高株価 +
                    "," + 加重平均株価 +
                    "," + 注文買値1 +
                    "," + 注文買値2 +
                    "," + 注文買値3 +
                    "," + minus_min +
                    "," + minus_ave +
                    "," + minus_max +
                    "," + plus_min +
                    "," + plus_ave +
                    "," + plus_max +
                    "," + current_wave +
                    "," + _25DMA上昇回数 +
                    "," + 取引量 +
                    "," + is25up +
                    "," + _25DMA乖離率 +
                    "," + 加重平均乖離率 +
                    "," + is25RateUP +
                    "," + convex +
                    "";
            }

            internal static string GetHeader()
            {
                Score score = new Score();

                return
                    ""  + GetName(() => score.コード) +
                    "," + GetName(() => score.直近日当たりの上昇率) +
                    "," + GetName(() => score.直近傾きの上昇率) +
                    "," + GetName(() => score.最低株価) +
                    "," + GetName(() => score.株価) +
                    "," + GetName(() => score.最高株価) +
                    "," + GetName(() => score.加重平均株価) +
                    "," + GetName(() => score.注文買値1) +
                    "," + GetName(() => score.注文買値2) +
                    "," + GetName(() => score.注文買値3) +
                    "," + GetName(() => score.minus_min) +
                    "," + GetName(() => score.minus_ave) +
                    "," + GetName(() => score.minus_max) +
                    "," + GetName(() => score.plus_min) +
                    "," + GetName(() => score.plus_ave) +
                    "," + GetName(() => score.plus_max) +
                    "," + GetName(() => score.current_wave) +
                    "," + GetName(() => score._25DMA上昇回数) +
                    "," + GetName(() => score.取引量) +
                    "," + GetName(() => score.is25up) +
                    "," + GetName(() => score._25DMA乖離率) +
                    "," + GetName(() => score.加重平均乖離率) +
                    "," + GetName(() => score.is25RateUP) +
                    "," + GetName(() => score.convex) +
                    "";
            }

            private static string GetName<T>(Expression<Func<T>> e)
            {
                var member = (MemberExpression)e.Body;
                return member.Member.Name;
            }

            public int CompareTo(object obj)
            {
                if (((Score)obj).直近日当たりの上昇率 == 直近日当たりの上昇率)
                {
                    return 0;
                }
                return ((Score)obj).直近日当たりの上昇率 > 直近日当たりの上昇率 ? 1 : -1;
            }
        }

        private static bool CheckIsStockCSV(string s)
        {
            return s.EndsWith(".csv") == true && s.Length <= 10;
        }

        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(".");
            CheckDuplicateFileSize(files);
            List<Score> scores = new List<Score>();

            foreach (string s in files)
            {
                if (CheckIsStockCSV(s) == true)
                {
                    TextFieldParser parser = new TextFieldParser(s);
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    List<Record> records = new List<Record>();

                    while (parser.EndOfData == false)
                    {
                        Record record = new Record();

                        string[] column = parser.ReadFields();

                        if (column.Length != 16)
                        {
                            throw new Exception(column.Length.ToString());
                        }

                        record.日付 = column[0];
                        record.始値 = column[1];
                        record.高値 = column[2];
                        record.安値 = column[3];
                        record.終値 = column[4];
                        record.前日比 = column[5];
                        record.出来高 = column[6];
                        record.貸株残高 = column[7];
                        record.融資残高 = column[8];
                        record.貸借倍率 = column[9];
                        record.逆日歩 = column[10];
                        record.特別空売り料 = column[11];
                        record._5DMA = column[12];
                        record._25DMA = column[13];
                        record._5DVMA = column[14];
                        record._25DVMA = column[15];

                        records.Add(record);
                    }

                    records.RemoveAt(0);

                    CheckDateSort(records);

                    double today = GetTrend(records.GetRange(1, records.Count - 1));
                    double yesterday = GetTrend(records.GetRange(0, records.Count - 1));

                    double block1 = GetTrend(records.GetRange(00, records.Count - 40));
                    double block2 = GetTrend(records.GetRange(20, records.Count - 40));
                    double block3 = GetTrend(records.GetRange(40, records.Count - 40));

                    //if (0 < block1 && block1 < block2 && block2 < block3)
                    {
                        Score score = new Score();

                        score.コード = s.Substring(2, 4);
                        score.最低株価 = GetLowPrice(records);
                        score.株価 = double.Parse(records[records.Count - 1].終値);
                        score.最高株価 = GetHighPrice(records);
                        score.加重平均株価 = GetWeightAveragePrice(records);
                        score.直近日当たりの上昇率 = (score.株価 + today) / score.株価; // 株価の伸び率＝（今日の株価＋傾き）÷今日の株価
                        score.直近傾きの上昇率 = today / yesterday;
                        score.注文買値1 = GetPrice(records, 1);
                        score.注文買値2 = GetPrice(records, 2);
                        score.注文買値3 = GetPrice(records, 3);
                        score._25DMA上昇回数 = GetUpCount25DMA(records);
                        score.取引量 = (ulong)GetAllAmount(records);
                        score.is25up = double.Parse(records[records.Count - 2]._25DMA) < double.Parse(records[records.Count - 1]._25DMA);
                        score._25DMA乖離率 = (double.Parse(records[records.Count - 1].終値) / double.Parse(records[records.Count - 1]._25DMA) - 1) * 100;
                        score.加重平均乖離率 = score.株価 / score.加重平均株価;
                        score.is25RateUP = GetIs25RateUP(records);
                        score.convex = GetConvex(records);

                        score = Get_minus_min(records, score);

                        scores.Add(score);
                    }
                }
            }

            scores.Sort();

            TextWriter tw = new StreamWriter(DateTime.Now.Ticks + ".csv");
            tw.WriteLine(Score.GetHeader());
            foreach (Score score in scores)
            {
                tw.WriteLine(score.ToString());
            }
            tw.Flush();
        }

        private static double GetLowPrice(List<Record> records)
        {
            double ret = double.MaxValue;

            foreach (Record record in records)
            {
                double price = (double.Parse(record.高値) + double.Parse(record.安値) + double.Parse(record.始値) + double.Parse(record.終値)) / 4;
                ret = price < ret ? price : ret;
            }

            return ret;
        }

        private static double GetHighPrice(List<Record> records)
        {
            double ret = double.MinValue;

            foreach (Record record in records)
            {
                double price = (double.Parse(record.高値) + double.Parse(record.安値) + double.Parse(record.始値) + double.Parse(record.終値)) / 4;
                ret = price > ret ? price : ret;
            }

            return ret;
        }

        private static bool GetConvex(List<Record> records)
        {
            return GetConvex(records[records.Count - 3], records[records.Count - 2], records[records.Count - 1]);
        }

        private static bool GetConvex(Record 二日前, Record 一日前, Record 本日)
        {
            double 二日前_25DMA = double.Parse(二日前._25DMA);
            double 一日前_25DMA = double.Parse(一日前._25DMA);
            double 本日_25DMA = double.Parse(本日._25DMA);
            
            return 本日_25DMA / 一日前_25DMA > 一日前_25DMA / 二日前_25DMA;
        }

        private static bool GetIs25RateUP(List<Record> records)
        {
            double prevday_price = double.Parse(records[records.Count - 2].終値);
            double prevday_25DMA = double.Parse(records[records.Count - 2]._25DMA);
            double prevday_rate = prevday_price / prevday_25DMA;

            double lastday_price = double.Parse(records[records.Count - 1].終値);
            double lastday_25DMA = double.Parse(records[records.Count - 1]._25DMA);
            double lastday_rate = lastday_price / lastday_25DMA;

            return prevday_rate < lastday_rate;
        }

        private static double GetAllAmount(List<Record> records)
        {
            double ret = 0;

            for (int i = 0; i < records.Count - 1; i++)
            {
                double price = (double.Parse(records[i].高値) + double.Parse(records[i].安値) + double.Parse(records[i].始値) + double.Parse(records[i].終値)) / 4;
                ret += price * double.Parse(records[i].出来高);
            }

            return ret;
        }

        private static byte GetUpCount25DMA(List<Record> records)
        {
            byte ret = 0;

            for (int i = 0; i < records.Count - 1; i++)
            {
                if (double.Parse(records[i]._25DMA) < double.Parse(records[i + 1]._25DMA))
                {
                    ret++;
                }
            }

            return ret;
        }

        private static double GetWeightAveragePrice(List<Record> records)
        {
            double ret = 0.0;
            double vol_sum = 0.0;

            for (int i = 0; i < records.Count; i++)
            {
                ret += double.Parse(records[i].終値) * double.Parse(records[i].出来高);
                vol_sum += double.Parse(records[i].出来高);
            }

            return ret / vol_sum;
        }

        private static void CheckDateSort(List<Record> records)
        {
            CheckDateSort(records[0], records[1]);
        }

        private static void CheckDateSort(Record 一番目, Record 二番目)
        {
            DateTime first_record_date  = DateTime.Parse(一番目.日付.Trim('\"'));
            DateTime second_record_date = DateTime.Parse(二番目.日付.Trim('\"'));
            
            if(first_record_date < second_record_date)
            {
            }
            else
            {
                throw new ApplicationException();
            }
        }

        private static void CheckDuplicateFileSize(string[] files)
        {
            long[] filesizes = new long[files.Length];
            string[] second_records = new string[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                if (CheckIsStockCSV(files[i]) == false)
                {
                    continue;
                }

                FileInfo fi = new FileInfo(files[i]);

                StreamReader sr = fi.OpenText();
                sr.ReadLine();
                string second_record = sr.ReadLine();

                for (int j = 0; j < filesizes.Length; j++)
                {
                    if (filesizes[j] == fi.Length && second_records[j] == second_record)
                    {
                        throw new ApplicationException(files[i]);
                    }
                }

                filesizes[i] = fi.Length;
                second_records[i] = second_record;
            }
        }

        private static Score Get_minus_min(List<Record> records, Score score)
        {
            List<double[]> waves = new List<double[]>();
            List<double> group = new List<double>();
            List<double> group2 = new List<double>();

            for (int i = 0; i < records.Count; i++)
            {
                if (double.Parse(records[i].前日比) < 0)
                {
                    group.Add(double.Parse(records[i].前日比));

                    waves.Add(group2.ToArray());
                    group2.Clear();
                }
                else
                {
                    group2.Add(double.Parse(records[i].前日比));

                    waves.Add(group.ToArray());
                    group.Clear();
                }
            }

            List<double[]> m_waves = new List<double[]>();
            List<double[]> p_waves = new List<double[]>();
            List<double[]> c_waves = new List<double[]>();

            if (group.Count > 0)
            {
                c_waves.Add(group.ToArray());
            }
            else
            {
                c_waves.Add(group2.ToArray());
            }
            
            for (int i = 0; i < waves.Count; i++)
            {
                if (waves[i].Length == 0)
                {
                    continue;
                }

                if (waves[i][0] < 0)
                {
                    m_waves.Add(waves[i]);
                }
                else
                {
                    p_waves.Add(waves[i]);
                }
            }

            List<double> m_waves_sum = new List<double>();
            List<double> p_waves_sum = new List<double>();
            List<double> c_waves_sum = new List<double>();

            for (int i = 0; i < m_waves.Count; i++)
            {
                m_waves_sum.Add(m_waves[i].Sum());
            }
            for (int i = 0; i < p_waves.Count; i++)
            {
                p_waves_sum.Add(p_waves[i].Sum());
            }
            for (int i = 0; i < c_waves.Count; i++)
            {
                c_waves_sum.Add(c_waves[i].Sum());
            }

            score.minus_min = m_waves_sum.Min();
            score.minus_ave = m_waves_sum.Average();
            score.minus_max = m_waves_sum.Max();

            score.plus_min = p_waves_sum.Min();
            score.plus_ave = p_waves_sum.Average();
            score.plus_max = p_waves_sum.Max();

            score.current_wave = c_waves_sum[0];

            return score;
        }

        private static double GetPrice(List<Record> records, byte rate)
        {
            List<double> differ = new List<double>();

            for (int i = 0; i < records.Count - 1; i++)
            {
                if (double.Parse(records[i].終値) > double.Parse(records[i + 1].安値))
                {
                    differ.Add(double.Parse(records[i].終値) - double.Parse(records[i + 1].安値));
                }
            }

            return double.Parse(records[records.Count - 1].終値) - (differ.Average() * rate);
        }

        private static double GetTrend(List<Record> records)
        {
            double[] y = new double[records.Count];

            for (int i = 0; i < records.Count; i++)
            {
                y[i] = (double.Parse(records[i].高値) + double.Parse(records[i].安値) + double.Parse(records[i].始値) + double.Parse(records[i].終値)) / 4;
            }

            return CalculateLinearRegression(y);
        }

        static double CalculateLinearRegression(double[] y)
        {
            double[] x = new double[y.Length];

            for (int i = 0; i < x.Length; i++)
            {
                x[i] = i + 1;
            }

            return CalculateLinearRegression(x, y);
        }

        static double CalculateLinearRegression(double[] x, double[] y)
        {
            Tuple<double, double> trend = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(x, y);
            return trend.Item2;
        }
    }
}
