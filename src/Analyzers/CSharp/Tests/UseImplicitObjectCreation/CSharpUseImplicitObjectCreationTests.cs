﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.CSharp.CodeStyle;
using Microsoft.CodeAnalysis.CSharp.UseImplicitObjectCreation;
using Microsoft.CodeAnalysis.Editor.UnitTests.CodeActions;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.UseImplicitObjectCreationTests
{
    using VerifyCS = CSharpCodeFixVerifier<
        CSharpUseImplicitObjectCreationDiagnosticAnalyzer,
        CSharpUseImplicitObjectCreationCodeFixProvider>;

    public partial class UseImplicitObjectCreationTests
    {
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestMissingBeforeCSharp9()
        {
            var source = @"
class C
{
    C c = new C();
}";
            await new VerifyCS.Test
            {
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp8,
                TestCode = source,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestAfterCSharp9()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    C c = new [|C|]();
}",
                FixedCode = @"
class C
{
    C c = new();
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestWithObjectInitializer()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    C c = new [|C|]() { };
}",
                FixedCode = @"
class C
{
    C c = new() { };
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestWithObjectInitializerWithoutArguments()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    C c = new [|C|] { };
}",
                FixedCode = @"
class C
{
    C c = new() { };
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestWithTriviaAfterNew()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    C c = new /*x*/ [|C|]();
}",
                FixedCode = @"
class C
{
    C c = new /*x*/ ();
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestNotWithDifferentTypes()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    object c = new C();
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestNotWithErrorTypes()
        {
            await new VerifyCS.Test
            {
                TestState = {
                    Sources =
                    {
                        @"
class C
{
    E c = new E();
}"
                    },
                    ExpectedDiagnostics =
                    {
                        // /0/Test0.cs(4,5): error CS0246: The type or namespace name 'E' could not be found (are you missing a using directive or an assembly reference?)
                        DiagnosticResult.CompilerError("CS0246").WithSpan(4, 5, 4, 6).WithArguments("E"),
                        // /0/Test0.cs(4,15): error CS0246: The type or namespace name 'E' could not be found (are you missing a using directive or an assembly reference?)
                        DiagnosticResult.CompilerError("CS0246").WithSpan(4, 15, 4, 16).WithArguments("E"),
                    }
                },
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestNotWithDynamic()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    dynamic c = new C();
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestNotWithArrayTypes()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    int[] c = new int[0];
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestNotWithTypeParameter()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C<T> where T : new()
{
    T t = new [|T|]();
}",
                FixedCode = @"
class C<T> where T : new()
{
    T t = new();
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestWithLocalWhenUserDoesNotPreferVar()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    void M()
    {
        C c = new [|C|]();
    }
}",
                FixedCode = @"
class C
{
    void M()
    {
        C c = new();
    }
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                Options =
                {
                    { CSharpCodeStyleOptions.VarWhenTypeIsApparent, CodeStyleOptions2.FalseWithSuggestionEnforcement },
                }
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestNotWithLocalWhenUserDoesPreferVar()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    void M()
    {
        C c = new C();
    }
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                Options =
                {
                    { CSharpCodeStyleOptions.VarWhenTypeIsApparent, CodeStyleOptions2.TrueWithSuggestionEnforcement },
                }
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestWithForVariable()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    void M()
    {
        for (C c = new [|C|]();;)
        {
        }
    }
}",
                FixedCode = @"
class C
{
    void M()
    {
        for (C c = new();;)
        {
        }
    }
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestWithLocalFunctionExpressionBody()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    void M()
    {
        C Func() => new [|C|]();
    }
}",
                FixedCode = @"
class C
{
    void M()
    {
        C Func() => new();
    }
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestWithMethodExpressionBody()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    C Func() => new [|C|]();
}",
                FixedCode = @"
class C
{
    C Func() => new();
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestWithConversionExpressionBody()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    public static implicit operator C(int i) => new [|C|]();
}",
                FixedCode = @"
class C
{
    public static implicit operator C(int i) => new();
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestWithOperatorExpressionBody()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    public static C operator +(C c1, C c2) => new [|C|]();
}",
                FixedCode = @"
class C
{
    public static C operator +(C c1, C c2) => new();
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestWithPropertyExpressionBody()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    C P => new [|C|]();
}",
                FixedCode = @"
class C
{
    C P => new();
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestWithPropertyAccessorExpressionBody()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    C P { get => new [|C|](); }
}",
                FixedCode = @"
class C
{
    C P { get => new(); }
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestNotWithPropertySetAccessorExpressionBody()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    C P { set => new C(); }
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestWithIndexerExpressionBody()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    C this[int i] => new [|C|]();
}",
                FixedCode = @"
class C
{
    C this[int i] => new();
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestWithIndexerAccessorExpressionBody()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    C this[int i] { get => new [|C|](); }
}",
                FixedCode = @"
class C
{
    C this[int i] { get => new(); }
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestNotWithMethodBlockBody()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    C Func() { return new C(); }
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseImplicitObjectCreation)]
        public async Task TestNotInNonApparentCode()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class C
{
    void X() => Bar(new C());
    void Bar(C c) { }
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
            }.RunAsync();
        }
    }
}
