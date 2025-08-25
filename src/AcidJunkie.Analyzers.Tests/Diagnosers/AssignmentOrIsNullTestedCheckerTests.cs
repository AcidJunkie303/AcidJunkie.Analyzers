namespace AcidJunkie.Analyzers.Tests.Diagnosers;

public sealed partial class AssignmentOrIsNullTestedCheckerTests
{
    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WhenAssignedDirectly_ThenTrue(string memberDeclaration)
    {
        const string code = """
                            Console.WriteLine();
                            PropertyOrField = "abc";
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: true);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithIf_WhenAssignedInOneBranchOnly_ThenFalse(string memberDeclaration)
    {
        const string code = """
                            if (1 == 2)
                            {
                                PropertyOrField = "abc";
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: false);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithIfElse_WhenAssignedInIfBranchOnly_ThenFalse(string memberDeclaration)
    {
        const string code = """
                            if (1 == 2)
                            {
                                PropertyOrField = "abc";
                            }
                            else
                            {
                                Console.WriteLine();
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: false);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithIfElse_WhenAssignedBothBranches_ThenTrue(string memberDeclaration)
    {
        const string code = """
                            if (1 == 2)
                            {
                                PropertyOrField = "303";
                            }
                            else
                            {
                                PropertyOrField = "909";
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: true);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithNestedIfs_WhenAssignedInAllBranches_ThenTrue(string memberDeclaration)
    {
        const string code = """
                            if (1 == 2)
                            {
                                PropertyOrField = "303";
                            }
                            else
                            {
                                if (3 == 4)
                                {
                                    PropertyOrField = "and";
                                }
                                else
                                {
                                    PropertyOrField = "909";
                                }
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: true);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithNestedIfs_WhennOTAssignedInAllBranches_ThenFalse(string memberDeclaration)
    {
        const string code = """
                            if (1 == 2)
                            {
                                PropertyOrField = "303";
                            }
                            else
                            {
                                if (3 == 4)
                                {
                                    PropertyOrField = "and";
                                }
                                else
                                {
                                    // no assignment here
                                }
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: false);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithForLoop_WhenAssignedInLoopOnly_ThenFalse(string memberDeclaration)
    {
        // for can loop 0 times -> we stay defensive
        const string code = """
                            for(var i=0 ; i< 10 ; i++)
                            {
                                PropertyOrField = "303";
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: false);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithForEachLoop_WhenAssignedInLoopOnly_ThenFalse(string memberDeclaration)
    {
        // foreach can loop 0 times -> we stay defensive
        const string code = """
                            foreach(var c in "")
                            {
                                PropertyOrField = "303";
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: false);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithWhile_WhenAssignedInLoopOnly_ThenFalse(string memberDeclaration)
    {
        // while can loop 0 times -> we stay defensive
        const string code = """
                            while(1 < 2)
                            {
                                PropertyOrField = "303";
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: false);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithDoWhile_WhenAssignedInLoopOnly_ThenTrue(string memberDeclaration)
    {
        // do-while can loop at least 1 time
        const string code = """
                            do
                            {
                                PropertyOrField = "303";
                            } while(1 < 2);
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: true);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithTryCatch_WhenAssignedInTryOnly_ThenFalse(string memberDeclaration)
    {
        const string code = """
                            try
                            {
                                PropertyOrField = "303";
                            }
                            catch
                            {
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: false);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithTryCatch_WhenAssignedInTryAndCatch_ThenTrue(string memberDeclaration)
    {
        const string code = """
                            try
                            {
                                PropertyOrField = "tb";
                            }
                            catch
                            {
                                PropertyOrField = "303";
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: true);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithTryCatch_WhenAssignedInCatchOnly_ThenFalse(string memberDeclaration)
    {
        const string code = """
                            try
                            {
                            }
                            catch
                            {
                                PropertyOrField = "303";
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: false);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithTryFinally_WhenAssignedInTryOnly_ThenFalse(string memberDeclaration)
    {
        const string code = """
                            try
                            {
                                PropertyOrField = "tb";
                            }
                            finally
                            {

                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: false);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithTryFinally_WhenAssignedInFinallyOnly_ThenTrue(string memberDeclaration)
    {
        const string code = """
                            try
                            {
                            }
                            finally
                            {
                                PropertyOrField = "tb";
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: true);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithTryCatchFinally_WhenAssignedInFinallyOnly_ThenTrue(string memberDeclaration)
    {
        const string code = """
                            try
                            {
                            }
                            catch
                            {
                            }
                            finally
                            {
                                PropertyOrField = "tb";
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: true);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithTryCatchFinally_WhenAssignedInTryAndCatch_ThenTrue(string memberDeclaration)
    {
        const string code = """
                            try
                            {
                                PropertyOrField = "tb";
                            }
                            catch
                            {
                                PropertyOrField = "303";
                            }
                            finally
                            {
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: true);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithSwitch_WhenNotAssignedInAllBranches_ThenFalse(string memberDeclaration)
    {
        const string code = """
                            switch(1)
                            {
                                case 1: PropertyOrField = "tb"; break;
                                case 2: PropertyOrField = "303"; break;
                                default: break;
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: false);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithSwitch_WhenAssignedInAllBranches_ThenTrue(string memberDeclaration)
    {
        const string code = """
                            switch(1)
                            {
                                case 1: PropertyOrField = "tb"; break;
                                case 2: PropertyOrField = "-"; break;
                                default: PropertyOrField = "303"; break;
                            }
                            """;

        RunAndValidate(code, memberDeclaration, expectedResult: true);
    }

    [Theory]
    [InlineData("public string PropertyOrField { get; set; } = null!;")]
    [InlineData("private string PropertyOrField = null!;")]
    public void WithNullCheck_WhenCheckedForNull_ThenTrue(string memberDeclaration)
    {
        const string code = "ArgumentNullException.ThrowIfNull(PropertyOrField);";

        RunAndValidate(code, memberDeclaration, expectedResult: true);
    }
}
