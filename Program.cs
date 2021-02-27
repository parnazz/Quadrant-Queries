using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System;
using System.Diagnostics;
using System.Threading;

class Quadrants
{
    public int[] quadrants;
    public int xReflections = 0;
    public int yReflections = 0;

    public Quadrants(int first, int second, int third, int fourth, int x = 0, int y = 0)
    {
        quadrants = new int[] { first, second, third, fourth };
        xReflections = x;
        yReflections = y;
    }

    public static Quadrants operator +(Quadrants a, Quadrants b) =>
        new Quadrants(a.quadrants[0] + b.quadrants[0],
            a.quadrants[1] + b.quadrants[1],
            a.quadrants[2] + b.quadrants[2],
            a.quadrants[3] + b.quadrants[3]);

    public new string ToString()
    {
        return String.Format("{0} {1} {2} {3}", quadrants[0], quadrants[1], quadrants[2], quadrants[3]);
    }
}

class Result
{
    static int n; // Array size
    static int h; // Height of the tree
    static Quadrants[] segTree;
    static StringBuilder stringBuilder = new StringBuilder();
    delegate Quadrants Reflect(int p, ref Quadrants segment);

    static Quadrants firstQuad = new Quadrants(1, 0, 0, 0);
    static Quadrants secondQuad = new Quadrants(0, 1, 0, 0);
    static Quadrants thirdQuad = new Quadrants(0, 0, 1, 0);
    static Quadrants fourthQuad = new Quadrants(0, 0, 0, 1);

    public static void quadrants(List<List<int>> p, List<string> queries)
    {
        n = p.Count;
        h = (int)(Math.Log(n) / Math.Log(2));
        segTree = new Quadrants[2 * n];

        BuildTreeBase(p);
        BuildTree(p);

        foreach (var querie in queries)
        {
            ReadQuerie(querie);
        }

        Console.WriteLine(stringBuilder.ToString());
    }

    private static void Apply(int p, Reflect callback)
    {
        segTree[p] = callback(p, ref segTree[p]);
    }

    private static void Update(int p)
    {
        while (p > 1)
        {
            p >>= 1;

            for (int i = 0; i < 4; i++)
            {
                segTree[p].quadrants[i] = segTree[p * 2].quadrants[i] + segTree[p * 2 + 1].quadrants[i];
            }

            if (segTree[p].xReflections % 2 == 1)
            {
                int first = segTree[p].quadrants[3];
                int second = segTree[p].quadrants[2];
                int third = segTree[p].quadrants[1];
                int fourth = segTree[p].quadrants[0];

                segTree[p].quadrants[0] = first;
                segTree[p].quadrants[1] = second;
                segTree[p].quadrants[2] = third;
                segTree[p].quadrants[3] = fourth;
            }

            if (segTree[p].yReflections % 2 == 1)
            {
                int first = segTree[p].quadrants[1];
                int second = segTree[p].quadrants[0];
                int third = segTree[p].quadrants[3];
                int fourth = segTree[p].quadrants[2];

                segTree[p].quadrants[0] = first;
                segTree[p].quadrants[1] = second;
                segTree[p].quadrants[2] = third;
                segTree[p].quadrants[3] = fourth;
            }
        }
    }

    private static void Push(int p) 
    {
        for (int s = h; s > 0; --s)
        {
            int i = p >> s;
            if (segTree[i].xReflections % 2 == 1)
            {
                Apply(i * 2, ReflectX);
                Apply(i * 2 + 1, ReflectX);
                segTree[i].xReflections = 0;
            }
            if (segTree[i].yReflections % 2 == 1)
            {
                Apply(i * 2, ReflectY);
                Apply(i * 2 + 1, ReflectY);
                segTree[i].yReflections = 0;
            }
        }
    }

    private static void BuildTree(List<List<int>> p)
    {
        // Строим родительские уровни дерева
        for (int i = n - 1; i > 0; --i)
            segTree[i] = segTree[i * 2] + segTree[i * 2 + 1];
    }

    private static void BuildTreeBase(List<List<int>> p)
    {
        // Строим нижний уровень дерева
        for (int i = n; i < 2 * n; i++)
            segTree[i] = CheckQuadrant(p[i - n]);
    }

    private static Quadrants CheckQuadrant(List<int> p)
    {
        if (Math.Sign(p[0]) * Math.Sign(p[1]) > 0)
        {
            if (p[0] < 0)
                return thirdQuad;
            else
                return firstQuad;
        }
        else
        {
            if (p[0] < 0)
                return secondQuad;
            else
                return fourthQuad;
        }
    }

    private static Quadrants ReflectX(int p, ref Quadrants segment)
    {
        if (p < n)
            segment.xReflections++;

        return new Quadrants(segment.quadrants[3],
            segment.quadrants[2],
            segment.quadrants[1],
            segment.quadrants[0],
            segment.xReflections,
            segment.yReflections);
    }

    private static Quadrants ReflectY(int p, ref Quadrants segment)
    {
        if (p < n)
            segment.yReflections++;

        return new Quadrants(segment.quadrants[1],
            segment.quadrants[0],
            segment.quadrants[3],
            segment.quadrants[2],
            segment.xReflections,
            segment.yReflections);
    }

    private static void Modify(int lowerBound, int upperBound, Reflect callback)
    {
        lowerBound += n - 1;
        upperBound += n;
        int initialLeftBound = lowerBound;
        int initialRightBound = upperBound;

        for (; lowerBound < upperBound; lowerBound >>= 1, upperBound >>= 1)
        {
            if (lowerBound % 2 == 1)
                Apply(lowerBound++, callback);
            if (upperBound % 2 == 1)
                Apply(--upperBound, callback);
        }
        Update(initialLeftBound);
        Update(initialRightBound - 1);
    }

    private static void ReadQuerie(string querie)
    {
        string[] chars = querie.Split(' ');
        int lowerBound = Convert.ToInt32(chars[1]);
        int upperBound = Convert.ToInt32(chars[2]);

        switch (chars[0])
        {
            case "X":
                Modify(lowerBound, upperBound, ReflectX);
                break;
            case "Y":
                Modify(lowerBound, upperBound, ReflectY);
                break;
            case "C":
                PrintResult(lowerBound, upperBound);
                break;
        }
    }

    private static void PrintResult(int lowerBound, int upperBound)
    {
        string str = string.Empty;
        Quadrants res = new Quadrants(0, 0, 0, 0);
        lowerBound += n - 1;
        upperBound += n;
        Push(lowerBound);
        Push(upperBound);

        for (; lowerBound < upperBound; lowerBound >>= 1, upperBound >>= 1)
        {
            if (lowerBound % 2 == 1) 
                res = res + segTree[lowerBound++];
            if (upperBound % 2 == 1) 
                res = res + segTree[--upperBound];
        }

        for (int i = 0; i < 4; i++)
        {
            str += res.quadrants[i] + " ";
        }
        str = str.Trim();

        stringBuilder.AppendLine(str);
    }
}

class Solution
{
    public static void Main(string[] args)
    {
        AutoFillTest();
    }

    private static void AutoFillTest()
    {
        string path = @"D:\Console projects\QuadrantsData4.txt";
        StreamReader reader = new StreamReader(path);

        int n = Convert.ToInt32(reader.ReadLine());
        List<List<int>> p = new List<List<int>>();

        for (int i = 0; i < n; i++)
        {
            p.Add(reader.ReadLine().TrimEnd().Split(' ').ToList().Select(pTemp => Convert.ToInt32(pTemp)).ToList());
        }

        int q = Convert.ToInt32(reader.ReadLine());
        List<string> queries = new List<string>();

        for (int i = 0; i < q; i++)
        {
            string queriesItem = reader.ReadLine();
            queries.Add(queriesItem);
        }

        Stopwatch sw = new Stopwatch();
        sw.Start();
        Result.quadrants(p, queries);
        sw.Stop();
        TimeSpan ts = sw.Elapsed;

        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

        Console.WriteLine("Elapsed " + elapsedTime);
    }
}