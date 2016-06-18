using System;
using System.IO;
using MagicChunks.Core;

namespace TransformSample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var transformer = new Transformer();

            Console.WriteLine(
                transformer.Transform(File.ReadAllText(@"Web.config"), new TransformationCollection
                {
                    {"configuration/system.web/authentication/@mode", "Forms"},
                    {"configuration/system.web/httpRuntime", "125"},
                    {"configuration/appSettings/add[@key='LoadBundledScripts']/@value", "true"},
                    {"configuration/appSettings/add[@key='SomethingNew']/@value", "NewValue"},
                    {"configuration/newKey", "12345"}
                }));
            Console.Read();

            Console.WriteLine(
                transformer.Transform(File.ReadAllText(@"config.json"), new TransformationCollection
                {
                    {"Data/DefaultConnection/ConnectionString", "mongodb://10.1.25.144/"},
                    {"Data/DefaultConnection/Production", "true"},
                    {"Smtp/Method", "Network"},
                    {"NewKey", "12345"}
                }));
            Console.Read();

            Console.WriteLine(
                transformer.Transform(File.ReadAllText(@"_config.yml"), new TransformationCollection
                {
                    {"baseUrl", "http://production.com/"},
                    {"frontend_version", "3.0.5"},
                    {"new_key", "23"},
                    {"another_key/val/t", "qwerty"}
                }));
            Console.Read();
        }
    }
}