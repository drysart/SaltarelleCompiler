﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests {
	[TestFixture]
	public class StatementTests : MethodCompilerTestBase {
		private void AssertCorrect(string csharp, string expected) {
			CompileMethod(csharp);
			string actual = OutputFormatter.Format(CompiledMethod.Body);

			int begin = actual.IndexOf("// BEGIN");
			if (begin > -1) {
				while (begin < (actual.Length - 1) && actual[begin - 1] != '\n')
					begin++;
				actual = actual.Substring(begin);
			}

			int end = actual.IndexOf("// END");
			if (end >= 0) {
				while (end >= 0 && actual[end] != '\n')
					end--;
				actual = actual.Substring(0, end + 1);
			}
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}

		[Test]
		public void CommentsAreCorrectlyTransferred() {
			AssertCorrect(
@"public void M() {
	// Some comment
	/* And some
	   multiline
	   comment
	*/
}",
@"{
	// Some comment
	// And some
	// multiline
	// comment
}
");
		}

		[Test]
		public void InactiveCodeIsNotTransferred() {
			AssertCorrect(
@"public void M() {
#if FALSE
	This is some stuff
	that should not appear in the script
#endif
}",
@"{
}
");
		}

		[Test]
		public void VariableDeclarationsWithoutInitializerWork() {
			AssertCorrect(
@"public void M() {
	int i, j;
	string s;
}",
@"{
	var $i, $j;
	var $s;
}
");
		}

		[Test]
		public void VariableDeclarationsWithInitializerWork() {
			AssertCorrect(
@"public void M() {
	int i = 0, j = 1;
	string s = ""X"";
}",
@"{
	var $i = 0, $j = 1;
	var $s = 'X';
}
");
		}

		[Test]
		public void VariableDeclarationsForVariablesUsedByReferenceWork() {
			AssertCorrect(
@"public void OtherMethod(out int x, out int y) { x = 0; y = 0; }
public void M() {
	// BEGIN
	int i = 0, j;
	// END
	OtherMethod(out i, out j);
}",
@"	var $i = { $: 0 }, $j = { $: null };
");
		}

		[Test]
		public void VariableDeclarationsWhichRequireMultipleStatementsWork() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	int i = (SomeProperty = 1), j = 2, k = 3, l = (SomeProperty = i), m = 4;
}",
@"{
	this.set_SomeProperty(1);
	var $i = 1, $j = 2, $k = 3;
	this.set_SomeProperty($i);
	var $l = $i, $m = 4;
}
");
		}

		[Test]
		public void ForStatementWithVariableDeclarationsWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	for (int i = 0, j = 1; i < 10; i++) {
		int k = i;
	}
	// END
}",
@"	for (var $i = 0, $j = 1; $i < 10; $i++) {
		var $k = $i;
	}
");
		}

		[Test]
		public void ForStatementWithoutVariableDeclarationWorks() {
			AssertCorrect(
@"public void M() {
	int i;
	// BEGIN
	for (i = 0; i < 10; i++) {
		int k = 0;
	}
	// END
}",
@"	for ($i = 0; $i < 10; $i++) {
		var $k = 0;
	}
");
		}

		[Test]
		public void ForStatementWithMultipleInitializersWorks() {
			AssertCorrect(
@"public void M() {
	int i, j;
	// BEGIN
	for (i = 0, j = 1; i < 10; i++) {
		int k = 0;
	}
	// END
}",
@"	for ($i = 0, $j = 1; $i < 10; $i++) {
		var $k = $i;
	}
");
		}

		[Test]
		public void ForStatementWithVariableDeclarationInitializersRequiringMultipleStatementsWork() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	for (int i = (SomeProperty = 1), j = 2, k = 3, l = (SomeProperty = i), m = 4; i < 10; i++) {
		int x = 0;
	}
	// END
}",
@"	for ($i = 0, $j = 1; $i < 10; $i++) {
		var $k = $i;
	}
");
		}

		[Test]
		public void ForStatementWithExpressionInitializersRequiringMultipleStatementsWork() {
			AssertCorrect(
@"public void M() {
	int i, j, k, l, m;
	// BEGIN
	for (i = (SomeProperty = 1), j = 2, k = 3, l = (SomeProperty = i), m = 4; i < 10; i++) {
		int x = 0;
	}
	// END
}",
@"	for ($i = 0, $j = 1; $i < 10; $i++) {
		var $k = $i;
	}
");
		}

		[Test]
		public void ForStatementWithoutInitializerWorks() {
			AssertCorrect(
@"public void M() {
	int i = 0;
	// BEGIN
	for (; i < 10; i++) {
		int k = i;
	}
	// END
}",
@"	for (; $i < 10; $i++) {
		var $k = $i;
	}
");
		}

		[Test]
		public void ForStatementWithoutTestWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	for (int i = 0; ; i++) {
		int k = i;
	}
	// END
}",
@"	for (var $i = 0;; $i++) {
		var $k = $i;
	}
");
		}

		[Test]
		public void ForStatementWithoutIteratorWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	for (int i = 0; i < 10;) {
		int k = i;
	}
	// END
}",
@"	for (var $i = 0; $i < 10;) {
		var $k = $i;
	}
");
		}

		[Test]
		public void ForStatementWithMultipleIteratorsWorks() {
			AssertCorrect(
@"public void M() {
	int i = 0, j = 0;
	// BEGIN
	for (; i < 10; i++, j++) {
		int k = 0;
	}
	// END
}",
@"	for (; $i < 10; $i++, $j++) {
		var $k = 0;
	}
");
		}

		[Test]
		public void ForEverStatementWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	for (;;) {
		int k = 0;
	}
	// END
}",
@"	for (;;) {
		var $k = 0;
	}
");
		}

		[Test]
		public void ForStatementWithNonDeclarationInitializerThatNeedExtraStatementsWorks() {
			Assert.Inconclusive("TODO: Fix and test");
		}

		[Test]
		public void ForStatementWithVariableDeclarationInitializerThatNeedExtraStatementsWorks() {
			Assert.Inconclusive("TODO: Fix and test");
		}

		[Test]
		public void ForStatementWithTestThatNeedExtraStatementsWorks() {
			Assert.Inconclusive("TODO: Fix and test");
		}

		[Test]
		public void ForStatementWithIteratorThatNeedExtraStatementsWorks() {
			Assert.Inconclusive("TODO: Fix and test");
		}
	}
}