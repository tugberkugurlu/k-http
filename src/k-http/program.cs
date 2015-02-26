using System;
using Microsoft.Framework.Runtime;

public class Program
{
    private readonly IApplicationEnvironment _appEnv;
    
    public Program(IApplicationEnvironment appEnv)
    {
        _appEnv = appEnv;
    }
    
    public void Main(string[] args)
    {
        Console.WriteLine("Hello World");
        Console.WriteLine(_appEnv.ApplicationBasePath);
        foreach(var arg in args)
        {
            Console.WriteLine(arg);
        }
    }
}