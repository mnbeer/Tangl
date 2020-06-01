using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using TanglAnalyzer;

namespace Tangl.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        const string testCore = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {

    public class TanglAttribute : Attribute
    {
        private string _target;
        public TanglAttribute(string target)
        {
            _target = target;
        }

        public string Target => _target;
    }

    class Person
    {
        public int PersonId {get; set;}
        public Person Spouse {get; set;}
        public DateTime? Birthday {get; set;}
    }    

    class Contact : Person
    {
        public string Relationship {get; set;}
    } 
";
        /// <summary>
        /// Make sure a basic Tangl attribute has no diagnostics
        /// </summary>
        [TestMethod]
        public void TanglSimplePassingTest()
        {
            var test = testCore + @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.PersonId"")]
        public int PersonId { get; set; }

    }
    }";

            VerifyCSharpDiagnostic(test);
        }

        /// <summary>
        /// Make sure a basic Tangl attribute has no diagnostics
        /// </summary>
        [TestMethod]
        public void TanglSimpleNullablePassingTest()
        {
            var test = testCore + @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.Birthday"")]
        public DateTime? Birthday { get; set; }

    }
    }";

            VerifyCSharpDiagnostic(test);
        }

        /// <summary>
        /// Test of typo in targeted class name
        /// </summary>
        [TestMethod]
        public void TanglMissingTargetTypeTest()
        {
            var test = testCore + @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Persons.PersonId"")]
        public long PersonId { get; set; }
    }
    }";
            var expected = new DiagnosticResult
            {
                Id = "MissingTanglTargetType",
                Message = String.Format("Type name '{0}' not found.", "ConsoleApplication1.Persons"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 38, 21)
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
            var test = testCore + @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.Id"")]
        public long PersonId { get; set; }

    }
    }";
            var expected = new DiagnosticResult
            {
                Id = "MissingTanglTarget",
                Message = String.Format("Cannot find Tangl target '{0}'.", "ConsoleApplication1.Person.Id"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 38, 21)
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
            var test = testCore + @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.PersonId"")]
        public long PersonId { get; set; }
    }
    }";
            var expected = new DiagnosticResult
            {
                Id = "DifferingTanglTypes",
                Message = String.Format("Tangle '{0}' and Tangl target '{1}' have differing types.",
                "ConsoleApplication1.TTest.PersonId", 
                "ConsoleApplication1.Person.PersonId"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 38, 21)
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
            var test = testCore + @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.Birthday"")]
        public DateTime Birthday { get; set; }
    }
    }";
            
            var expected = new DiagnosticResult
            {
                Id = "DifferingTanglTypes",
                Message = String.Format("Tangle '{0}' and Tangl target '{1}' have differing types.",
                "ConsoleApplication1.TTest.Birthday",
                "ConsoleApplication1.Person.Birthday"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 38, 25)
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
            var test = testCore + @"
        class TTest
        {   
        [Tangl(target: ""ConsoleApplication1.Person.Spouse"")]
        public Contact Partner { get; set; }
    }
    }";
            var expected = new DiagnosticResult
            {
                Id = "DifferingTanglTypes",
                Message = String.Format("Tangle '{0}' and Tangl target '{1}' have differing types.",
                "ConsoleApplication1.TTest.Partner",
                "ConsoleApplication1.Person.Spouse"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 38, 24)
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
    }
}
