using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using TanglAnalyzer;
using System.Reflection;
using System.IO;

namespace TanglAnalyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        private static string testCore = "";
        private static int testCodeLineNumber;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Assembly assembly= Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            using (Stream rsrcStream = assembly.GetManifestResourceStream("TanglAnalyzer.Test.TestCode.txt"))
            //using (Stream rsrcStream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".Properties." + "TestCode.cs.txt"))
            {
                using (StreamReader sRdr = new StreamReader(rsrcStream))
                {
                    testCore = sRdr.ReadToEnd();
                    testCodeLineNumber = testCore.Split('\n').Length - 3;
                }
            }

        }

        /// <summary>
        /// Make sure a basic Tangl attribute has no diagnostics
        /// </summary>
        [TestMethod]
        public void TanglSimplePassingTest()
        {
            var test = @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.PersonId"")]
        public int PersonId { get; set; }

    }
    }";
            test = InsertTestCode(test);
            VerifyCSharpDiagnostic(test);
        }

        /// <summary>
        /// Make sure a basic Tangl attribute has no diagnostics
        /// </summary>
        [TestMethod]
        public void TanglSimpleStronglyTypesdPassingTest()
        {
            var test = @"
        class TTest
        {   
        [Tangl(typeof(Person), nameof(Person.PersonId))]
        public int PersonId { get; set; }

    }
    }";
            test = InsertTestCode(test);
            VerifyCSharpDiagnostic(test);
        }

        /// <summary>
        /// Make sure a basic Tangl attribute has no diagnostics
        /// </summary>
        [TestMethod]
        public void TanglSimpleNullablePassingTest()
        {
            var test = @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.Birthday"")]
        public DateTime? Birthday { get; set; }

    }
    }";
            test = InsertTestCode(test);
            VerifyCSharpDiagnostic(test);
        }

        /// <summary>
        /// Test of typo in targeted class name
        /// </summary>
        [TestMethod]
        public void TanglMissingTargetTypeTest()
        {
            var test = @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Persons.PersonId"")]
        public long PersonId { get; set; }
    }
    }";
            test = InsertTestCode(test);
            var expected = new DiagnosticResult
            {
                Id = "MissingTanglTargetType",
                Message = String.Format("Type name '{0}' not found.", "ConsoleApplication1.Persons"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", testCodeLineNumber + 4, 21)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        /// <summary>
        /// Test of target name does not exist in targeted type 
        /// </summary>
        [TestMethod]
        public void TanglMissingTargetTest()
        {
            var test = @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.Id"")]
        public long PersonId { get; set; }

    }
    }";
            test = InsertTestCode(test);
            var expected = new DiagnosticResult
            {
                Id = "MissingTanglTarget",
                Message = String.Format("Cannot find Tangl target '{0}'.", "ConsoleApplication1.Person.Id"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", testCodeLineNumber + 4, 21)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        /// <summary>
        /// Test of target name does not exist in targeted type 
        /// </summary>
        [TestMethod]
        public void TanglDifferingValueTypesTest()
        {
            var test = @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.PersonId"")]
        public long PersonId { get; set; }
    }
    }";
            test = InsertTestCode(test);
            var expected = new DiagnosticResult
            {
                Id = "DifferingTanglTypes",
                Message = String.Format("Tangle '{0}' and Tangl target '{1}' have differing types.",
                "ConsoleApplication1.TTest.PersonId", 
                "ConsoleApplication1.Person.PersonId"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", testCodeLineNumber + 4, 21)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
            VerifyCSharpFix(test, test.Replace("public long PersonId", "public int PersonId"));
        }

        /// <summary>
        /// Test of target name does not exist in targeted type 
        /// </summary>
        [TestMethod]
        public void TanglDifferingNullableTypesTest()
        {
            var test = @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.Birthday"")]
        public DateTime Birthday { get; set; }
    }
    }";
            test = InsertTestCode(test);
            var expected = new DiagnosticResult
            {
                Id = "DifferingTanglTypes",
                Message = String.Format("Tangle '{0}' and Tangl target '{1}' have differing types.",
                "ConsoleApplication1.TTest.Birthday",
                "ConsoleApplication1.Person.Birthday"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", testCodeLineNumber + 4, 25)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
            VerifyCSharpFix(test, test.Replace("public DateTime Birthday", "public DateTime? Birthday"));
        }

        /// <summary>
        /// Test of target name does not exist in targeted type 
        /// </summary>
        [TestMethod]
        public void TanglDifferingReferenceTypesTest()
        {
            var test = @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.Spouse"")]
        public Contact Partner { get; set; }
    }
    }";
            test = InsertTestCode(test);
            var expected = new DiagnosticResult
            {
                Id = "DifferingTanglTypes",
                Message = String.Format("Tangle '{0}' and Tangl target '{1}' have differing types.",
                "ConsoleApplication1.TTest.Partner",
                "ConsoleApplication1.Person.Spouse"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", testCodeLineNumber + 4, 24)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
            VerifyCSharpFix(test, test.Replace("public Contact Partner", "public Person Partner"));
        }

        //        var fixtest = @"
        //using System;
        //using System.Collections.Generic;
        //using System.Linq;
        //using System.Text;
        //using System.Threading.Tasks;
        //using System.Diagnostics;

        //namespace ConsoleApplication1
        //{
        //    class TYPENAME
        //    {   
        //    }
        //}";
        //        VerifyCSharpFix(test, fixtest);

        [TestMethod]
        public void TanglMissingAttributesTest()
        {
            var test = @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.LastName"")]
        [MaxLength(50)]
        public string LastName { get; set; }
    }
    }";
            test = InsertTestCode(test);
            var expected = new DiagnosticResult
            {
                Id = TanglCodeAnalyzer.MissingAttributeId,
                Message = String.Format("Missing attribute '{0}'",
                "Required"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", testCodeLineNumber+ 5, 23)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
            //VerifyCSharpFix(test, test.Replace("public long PersonId", "public int PersonId"));
        }

        [TestMethod]
        public void TanglMissingAttributeExceptionTest()
        {
            var test = @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.LastName"", except: ""Required"")]
        [MaxLength(50)]
        public string LastName { get; set; }
    }
    }";
            test = InsertTestCode(test);
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TanglMissingAttributeExceptionTest2()
        {
            var test = @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.LastName"", includeAttributes: true; except: ""Required"")]
        [MaxLength(50)]
        public string LastName { get; set; }
    }
    }";
            test = InsertTestCode(test);
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TanglMissingAttributeExceptionTest3()
        {
            var test = @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.LastName"", includeAttributes: false; except: ""Required"")]
        [MaxLength(50)]
        public string LastName { get; set; }
    }
    }";
            test = InsertTestCode(test);
            var expected = new DiagnosticResult
            {
                Id = TanglCodeAnalyzer.MissingAttributeId,
                Message = String.Format("Missing attribute '{0}'", "Required"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
        new[] {
                            new DiagnosticResultLocation("Test0.cs", testCodeLineNumber+ 5, 23)
            }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TanglDifferingAttributesTest()
        {
            var test = @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.LastName"")]
        [MaxLength(100)]
        [Required]
        public string LastName { get; set; }
    }
    }";
            test = InsertTestCode(test);
            var expected = new DiagnosticResult
            {
                Id = TanglCodeAnalyzer.DifferingAttributeId,
                Message = String.Format("Differing attribute '{0}'",
                "MaxLength"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", testCodeLineNumber + 6, 23)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
            //VerifyCSharpFix(test, test.Replace("public long PersonId", "public int PersonId"));
        }

        //No diagnostics expected to show up
        [TestMethod]
        public void NoCodeTest
            ()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

    //    //Diagnostic and CodeFix both triggered and checked for
    //    [TestMethod]
    //    public void TestMethod2()
    //    {
    //        var test = @"
    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Text;
    //using System.Threading.Tasks;
    //using System.Diagnostics;

    //namespace ConsoleApplication1
    //{
    //    class TypeName
    //    {   
    //    }
    //}";
    //        var expected = new DiagnosticResult
    //        {
    //            Id = "Tangl",
    //            Message = String.Format("Type name '{0}' contains lowercase letters", "TypeName"),
    //            Severity = DiagnosticSeverity.Warning,
    //            Locations =
    //                new[] {
    //                        new DiagnosticResultLocation("Test0.cs", 11, 15)
    //                    }
    //        };

    //        VerifyCSharpDiagnostic(test, expected);

    //        var fixtest = @"
    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Text;
    //using System.Threading.Tasks;
    //using System.Diagnostics;

    //namespace ConsoleApplication1
    //{
    //    class TYPENAME
    //    {   
    //    }
    //}";
    //        VerifyCSharpFix(test, fixtest);
    //    }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new TanglCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TanglCodeAnalyzer();
        }

        private string InsertTestCode(string testCode)
        {
            return testCore.Replace("//{0}", testCode);
        }
    }
}
