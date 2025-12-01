using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InventoryManagementSystemStaticTest
{
    [TestClass]
    public class StaticRuleTest
    {
        [TestMethod]
        public void CheckMState()
        {
            var controllersPath = Path.Combine(
                AppContext.BaseDirectory,
                @"..\..\..\..\Inventory-Management-System\Controllers"
            );

            controllersPath = Path.GetFullPath(controllersPath);

            var controllerFiles = Directory.GetFiles(
                controllersPath,
                "*Controller.cs",
                SearchOption.TopDirectoryOnly
            );

            var violations = new List<string>();

            foreach (var file in controllerFiles)
            {
                var lines = File.ReadAllLines(file);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("[HttpPost"))
                    {
                        bool modelStateFound = false;

                        for (int j = i + 1; j < Math.Min(i + 40, lines.Length); j++)
                        {
                            if (lines[j].Contains("[HttpGet") ||
                                lines[j].Contains("[HttpPost") ||
                                lines[j].Contains("public IActionResult"))
                                break;

                            if (lines[j].Contains("ModelState.IsValid"))
                            {
                                modelStateFound = true;
                                break;
                            }
                        }

                        if (!modelStateFound)
                        {
                            var info = $"{Path.GetFileName(file)} (around line {i + 1})";
                            violations.Add(info);
                        }
                    }
                }
            }

            if (violations.Any())
            {
                Assert.Fail(
                    "POST methods without ModelState.IsValid check were found:\n" +
                    string.Join(Environment.NewLine, violations)
                );
            }
        }
    }
}
