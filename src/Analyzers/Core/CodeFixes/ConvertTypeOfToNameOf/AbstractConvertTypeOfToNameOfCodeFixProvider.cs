// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis.ConvertTypeOfToNameOf
{
    internal abstract class AbstractConvertTypeOfToNameOfCodeFixProvider : SyntaxEditorBasedCodeFixProvider
    {
        internal static string? CodeFixTitle;
        public AbstractConvertTypeOfToNameOfCodeFixProvider()
        {
            CodeFixTitle = GetCodeFixTitle(AnalyzersResources.Convert_gettype_to_nameof, AnalyzersResources.Convert_typeof_to_nameof);
        }

        public sealed override ImmutableArray<string> FixableDiagnosticIds
           => ImmutableArray.Create(IDEDiagnosticIds.ConvertTypeOfToNameOfDiagnosticId);
        internal sealed override CodeFixCategory CodeFixCategory => CodeFixCategory.CodeStyle;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(new MyCodeAction(
                c => FixAsync(context.Document, context.Diagnostics.First(), c)),
               context.Diagnostics);
            return Task.CompletedTask;
        }

        protected override Task FixAllAsync(
            Document document, ImmutableArray<Diagnostic> diagnostics,
            SyntaxEditor editor, CancellationToken cancellationToken)
        {
            foreach (var diagnostic in diagnostics)
            {
                var node = editor.OriginalRoot.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
                ConvertTypeOfToNameOf(editor, node);
            }

            return Task.CompletedTask;
        }

        /// <Summary>
        ///  Method converts typeof(...).Name to nameof(...)
        /// </Summary>
        public void ConvertTypeOfToNameOf(SyntaxEditor editor, SyntaxNode nodeToReplace)
        {
            var typeExpression = GetTypeExpression(nodeToReplace);
            var nameOfSyntax = editor.Generator.NameOfExpression(typeExpression);
            editor.ReplaceNode(nodeToReplace, nameOfSyntax);
        }

        protected abstract SyntaxNode? GetTypeExpression(SyntaxNode node);

        protected abstract string GetCodeFixTitle(string visualbasic, string csharp);

        private class MyCodeAction : CustomCodeActions.DocumentChangeAction
        {
            public MyCodeAction(Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(CodeFixTitle, createChangedDocument, CodeFixTitle)
            {
            }
        }
    }
}
