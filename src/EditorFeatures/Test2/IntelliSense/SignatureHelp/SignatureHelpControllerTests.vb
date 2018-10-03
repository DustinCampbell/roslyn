' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Immutable
Imports System.Threading
Imports System.Windows.Threading
Imports Microsoft.CodeAnalysis.Editor.Implementation.IntelliSense
Imports Microsoft.CodeAnalysis.Editor.Implementation.IntelliSense.SignatureHelp
Imports Microsoft.CodeAnalysis.Editor.Shared.Utilities
Imports Microsoft.CodeAnalysis.Editor.UnitTests.Utilities
Imports Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces
Imports Microsoft.CodeAnalysis.Shared.TestHooks
Imports Microsoft.CodeAnalysis.SignatureHelp
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.VisualStudio.Commanding
Imports Microsoft.VisualStudio.Language.Intellisense
Imports Microsoft.VisualStudio.Text
Imports Microsoft.VisualStudio.Text.Editor
Imports Microsoft.VisualStudio.Text.Editor.Commanding.Commands
Imports Moq

Namespace Microsoft.CodeAnalysis.Editor.UnitTests.IntelliSense.SignatureHelp

    <[UseExportProvider]>
    Public Class SignatureHelpControllerTests
        <WpfFact, Trait(Traits.Feature, Traits.Features.SignatureHelp)>
        Public Sub InvokeSignatureHelpWithoutDocumentShouldNotStartNewSession()
            Dim emptyDocumentProvider = New Mock(Of IDocumentProvider)
            emptyDocumentProvider _
                .Setup(Function(p) p.GetDocumentAsync(It.IsAny(Of ITextSnapshot), It.IsAny(Of CancellationToken))) _
                .Returns(Task.FromResult(Of Document)(Nothing))

            Dim env = TestEnvironment.Create(documentProvider:=emptyDocumentProvider)

            env.InvokeSignatureHelp()

            Assert.Equal(0, env.GetItemAsyncCallCount)
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.SignatureHelp)>
        Public Sub InvokeSignatureHelpWithDocumentShouldStartNewSession()
            Dim env = TestEnvironment.Create()

            env.InvokeSignatureHelp(waitForController:=False)

            env.PresenterMock.Verify(
                Function(p) p.CreateSession(It.IsAny(Of ITextView), It.IsAny(Of ITextBuffer), It.IsAny(Of ISignatureHelpSession)),
                Times.Once)
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.SignatureHelp)>
        Public Sub EmptyModelShouldStopSession()
            Dim env = TestEnvironment.Create(signatureHelpItems:=ImmutableArray(Of SignatureHelpItem).Empty)

            env.InvokeSignatureHelp()

            env.PresenterSessionMock.Verify(
                Sub(p) p.Dismiss(),
                Times.Once)
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.SignatureHelp)>
        Public Sub UpKeyShouldDismissWhenThereIsOnlyOneItem()
            Dim env = TestEnvironment.Create(signatureHelpItems:=CreateItems(1))

            env.InvokeSignatureHelp()

            Dim handled = env.Controller.TryHandleUpKey()
            Assert.False(handled)

            env.PresenterSessionMock.Verify(
                Sub(p) p.Dismiss(),
                Times.Once)
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.SignatureHelp)>
        Public Sub UpKeyShouldNavigateWhenThereAreMultipleItems()
            Dim env = TestEnvironment.Create(signatureHelpItems:=CreateItems(2))

            env.InvokeSignatureHelp()

            Dim handled = env.Controller.TryHandleUpKey()
            Assert.True(handled)

            env.PresenterSessionMock.Verify(
                Sub(p) p.SelectPreviousItem(),
                Times.Once)
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.SignatureHelp)>
        <WorkItem(985007, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/985007")>
        Public Sub UpKeyShouldNotCrashWhenSessionIsDismissed()
            ' Create a provider that will return an empty state when queried the second time
            Dim slowProvider = New Mock(Of SignatureHelpProvider)
            slowProvider _
                .Setup(Function(p) p.GetItemsAsync(It.IsAny(Of Document), It.IsAny(Of Integer), It.IsAny(Of SignatureHelpTrigger), It.IsAny(Of CancellationToken))) _
                .Returns(Task.FromResult(New SignatureList(slowProvider.Object, CreateItems(2), TextSpan.FromBounds(0, 0), selectedItem:=0, argumentIndex:=0, argumentCount:=0, argumentName:=Nothing)))

            Dim env = TestEnvironment.Create(signatureHelpProvider:=slowProvider.Object)

            env.InvokeSignatureHelp()

            ' Now force an update to the model that will result in stopping the session
            slowProvider _
                .Setup(Function(p) p.GetItemsAsync(It.IsAny(Of Document), It.IsAny(Of Integer), It.IsAny(Of SignatureHelpTrigger), It.IsAny(Of CancellationToken))) _
                .Returns(Task.FromResult(Of SignatureList)(Nothing))

            env.TypeChar(" "c)

            Dim handled = env.Controller.TryHandleUpKey() ' this will block on the model being updated which should dismiss the session

            Assert.False(handled)
            env.PresenterSessionMock.Verify(
                Sub(p) p.Dismiss(),
                Times.Once)
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.SignatureHelp)>
        <WorkItem(179726, "https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workItems?id=179726&_a=edit")>
        Public Sub DownKeyShouldNotBlockOnModelComputation()
            Dim slowProvider = New Mock(Of SignatureHelpProvider)
            Dim resetEvent = New ManualResetEvent(False)

            slowProvider _
                .Setup(Function(p) p.GetItemsAsync(It.IsAny(Of Document), It.IsAny(Of Integer), It.IsAny(Of SignatureHelpTrigger), It.IsAny(Of CancellationToken))) _
                .Returns(Function()
                             resetEvent.WaitOne()
                             Return Task.FromResult(New SignatureList(slowProvider.Object, CreateItems(2), TextSpan.FromBounds(0, 0), selectedItem:=0, argumentIndex:=0, argumentCount:=0, argumentName:=Nothing))
                         End Function)

            Dim env = TestEnvironment.Create(signatureHelpProvider:=slowProvider.Object)

            env.InvokeSignatureHelp(waitForController:=False)

            Dim handled = env.Controller.TryHandleDownKey()
            Assert.False(handled)
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.SignatureHelp)>
        <WorkItem(179726, "https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workItems?id=179726&_a=edit")>
        Public Sub UpKeyShouldNotBlockOnModelComputation()
            Dim slowProvider = New Mock(Of SignatureHelpProvider)
            Dim resetEvent = New ManualResetEvent(False)

            slowProvider _
                .Setup(Function(p) p.GetItemsAsync(It.IsAny(Of Document), It.IsAny(Of Integer), It.IsAny(Of SignatureHelpTrigger), It.IsAny(Of CancellationToken))) _
                .Returns(Function()
                             resetEvent.WaitOne()
                             Return Task.FromResult(New SignatureList(slowProvider.Object, CreateItems(2), TextSpan.FromBounds(0, 0), selectedItem:=0, argumentIndex:=0, argumentCount:=0, argumentName:=Nothing))
                         End Function)

            Dim env = TestEnvironment.Create(signatureHelpProvider:=slowProvider.Object)
            env.InvokeSignatureHelp(waitForController:=False)

            Dim handled = env.Controller.TryHandleUpKey()
            Assert.False(handled)
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.SignatureHelp)>
        <WorkItem(179726, "https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workItems?id=179726&_a=edit")>
        Public Async Function UpKeyShouldBlockOnRecomputationAfterPresentation() As Task
            Dim dispatcher = Windows.Threading.Dispatcher.CurrentDispatcher

            Dim slowProvider = New Mock(Of SignatureHelpProvider)
            slowProvider _
                .Setup(Function(p) p.GetItemsAsync(It.IsAny(Of Document), It.IsAny(Of Integer), It.IsAny(Of SignatureHelpTrigger), It.IsAny(Of CancellationToken))) _
                .Returns(Task.FromResult(New SignatureList(slowProvider.Object, CreateItems(2), TextSpan.FromBounds(0, 0), selectedItem:=0, argumentIndex:=0, argumentCount:=0, argumentName:=Nothing)))

            Dim env = dispatcher.Invoke(
               Function()
                   Dim e = TestEnvironment.Create(signatureHelpProvider:=slowProvider.Object)
                   ' Ensure that signature help is displaying
                   e.InvokeSignatureHelp()
                   Return e
               End Function)

            ' Update session so that providers are requeried.
            ' SlowProvider now blocks on the checkpoint's task.
            Dim checkpoint = New Checkpoint()
            slowProvider _
                .Setup(Function(p) p.GetItemsAsync(It.IsAny(Of Document), It.IsAny(Of Integer), It.IsAny(Of SignatureHelpTrigger), It.IsAny(Of CancellationToken))) _
                .Returns(Function()
                             checkpoint.Task.Wait()
                             Return Task.FromResult(New SignatureList(slowProvider.Object, CreateItems(2), TextSpan.FromBounds(0, 2), selectedItem:=0, argumentIndex:=0, argumentCount:=0, argumentName:=Nothing))
                         End Function)

            dispatcher.Invoke(Sub() env.TypeChar(" "c))

            Dim handled = dispatcher.InvokeAsync(Function() env.Controller.TryHandleUpKey()) ' Send the controller an up key, which should block on the computation
            checkpoint.Release() ' Allow slowprovider to finish
            Await handled.Task.ConfigureAwait(False)

            ' We expect 2 calls to the presenter (because we had an existing presentation session when we started the second computation).
            Assert.True(handled.Result)
            env.PresenterSessionMock.Verify(
               Sub(p) p.PresentItems(It.IsAny(Of ITrackingSpan), It.IsAny(Of IList(Of SignatureHelpItem)), It.IsAny(Of SignatureHelpItem), It.IsAny(Of Integer?)),
               Times.Exactly(2))
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.SignatureHelp)>
        Public Sub DownKeyShouldNavigateWhenThereAreMultipleItems()
            Dim items = CreateItems(2)

            ' Create test environment with two unique signature help items
            Dim env = TestEnvironment.Create(signatureHelpItems:=items)

            ' Ensure that signature help is displaying
            env.InvokeSignatureHelp()

            Dim handled = env.Controller.TryHandleDownKey()
            Assert.True(handled)

            env.PresenterSessionMock.Verify(
                Sub(p) p.SelectNextItem(),
                Times.Once)
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.SignatureHelp)>
        <WorkItem(1181, "https://github.com/dotnet/roslyn/issues/1181")>
        Public Sub UpAndDownKeysShouldStillNavigateWhenDuplicateItemsAreFiltered()
            Dim item = CreateItems(1).Single()
            Dim items = ImmutableArray.Create(item, item)

            ' Create test environment with two duplicate signature help items
            Dim env = TestEnvironment.Create(signatureHelpItems:=items)

            ' Ensure that signature help is displaying
            env.InvokeSignatureHelp()

            Dim handled = env.Controller.TryHandleUpKey()

            Assert.False(handled)
            env.PresenterSessionMock.Verify(
                Sub(p) p.Dismiss(),
                Times.Once)
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.SignatureHelp)>
        Public Sub CaretMoveWithActiveSessionShouldRecomputeModel()
            Dim env = TestEnvironment.Create()

            ' Ensure the buffer has a bit of text in it
            env.TextBuffer.Insert(0, "Text")

            ' Ensure that signature help is displaying
            env.InvokeSignatureHelp()

            ' SignatureHelpProvider.GetItemsAsync should be called once initially
            Assert.Equal(1, env.GetItemAsyncCallCount)

            ' Move the caret back one character
            env.TextView.Caret.MoveTo(env.TextView.Caret.Position.BufferPosition - 1)

            env.Controller.WaitForController()

            ' SignatureHelpProvider.GetItemsAsync should be called again as part of
            ' retriggering when handling the PositionChanged event.
            Assert.Equal(2, env.GetItemAsyncCallCount)
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.SignatureHelp)>
        Public Sub RetriggerActiveSessionOnClosingBrace()
            Dim env = TestEnvironment.Create()

            ' Ensure that signature help is displaying
            env.InvokeSignatureHelp()

            ' SignatureHelpProvider.GetItemsAsync should be called once initially
            Assert.Equal(1, env.GetItemAsyncCallCount)

            ' Type a retrigger character
            env.TypeChar(")"c)

            env.Controller.WaitForController()

            ' SignatureHelpProvider.GetItemsAsync should be called again as part of
            ' retriggering when handling the typed character.
            Assert.Equal(2, env.GetItemAsyncCallCount)
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.SignatureHelp)>
        <WorkItem(959116, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/959116")>
        Public Sub TypingNonTriggerCharacterShouldNotRequestDocument()
            Dim env = TestEnvironment.Create()

            ' Type a character that is not a trigger character
            env.TypeChar("a"c)

            env.DocumentProviderMock.Verify(
                Function(p) p.GetDocumentAsync(It.IsAny(Of ITextSnapshot), It.IsAny(Of CancellationToken)),
                Times.Never)
        End Sub

        Private Shared Function CreateItems(count As Integer) As ImmutableArray(Of SignatureHelpItem)
            Return Enumerable.Range(0, count) _
                .Select(Function(i) New SignatureHelpItem(isVariadic:=False, documentationFactory:=Nothing, prefixParts:=New List(Of TaggedText), separatorParts:={}, suffixParts:={}, parameters:={}, descriptionParts:={})) _
                .ToImmutableArray()
        End Function

        Private Class TestEnvironment
            Public ReadOnly Property TextBuffer As ITextBuffer
            Public ReadOnly Property TextView As ITextView
            Public ReadOnly Property SignatureHelpProvider As SignatureHelpProvider
            Public ReadOnly Property DocumentProviderMock As Mock(Of IDocumentProvider)
            Public ReadOnly Property PresenterMock As Mock(Of IIntelliSensePresenter(Of ISignatureHelpPresenterSession, ISignatureHelpSession))
            Public ReadOnly Property PresenterSessionMock As Mock(Of ISignatureHelpPresenterSession)
            Public ReadOnly Property Controller As Controller

            Public Property GetItemAsyncCallCount As Integer

            Private Sub New(
                document As Document,
                textBuffer As ITextBuffer,
                textView As ITextView,
                signatureHelpItems As ImmutableArray(Of SignatureHelpItem),
                signatureHelpProvider As SignatureHelpProvider,
                documentProviderMock As Mock(Of IDocumentProvider),
                presenterMock As Mock(Of IIntelliSensePresenter(Of ISignatureHelpPresenterSession, ISignatureHelpSession)),
                presenterSessionMock As Mock(Of ISignatureHelpPresenterSession),
                controller As Controller
            )

                Me.TextBuffer = textBuffer
                Me.TextView = textView
                Me.DocumentProviderMock = documentProviderMock
                Me.SignatureHelpProvider = If(signatureHelpProvider, CreateMockSignatureHelpProvider(signatureHelpItems))
                Me.Controller = controller
                Me.PresenterMock = presenterMock
                Me.PresenterSessionMock = presenterSessionMock

                Dim service = DirectCast(SignatureHelpService.GetService(document), SignatureHelpServiceWithProviders)
                service.SetTestProviders({Me.SignatureHelpProvider})
            End Sub

            Private Function CreateMockSignatureHelpProvider(signatureHelpItems As ImmutableArray(Of SignatureHelpItem)) As SignatureHelpProvider
                Dim signatureHelpProvider = New Mock(Of SignatureHelpProvider)

                Dim items = If(signatureHelpItems.IsDefault, CreateItems(1), signatureHelpItems)

                Dim caretPosition As Integer

                signatureHelpProvider _
                    .Setup(Function(p) p.GetItemsAsync(It.IsAny(Of Document), It.IsAny(Of Integer), It.IsAny(Of SignatureHelpTrigger), It.IsAny(Of CancellationToken))) _
                    .Callback(Sub(d As Document, cp As Integer, t As SignatureHelpTrigger, ct As CancellationToken) caretPosition = cp) _
                    .Returns(Function()
                                 GetItemAsyncCallCount += 1

                                 Dim list = If(items.Any(),
                                     New SignatureList(signatureHelpProvider.Object, items, TextSpan.FromBounds(caretPosition, caretPosition), selectedItem:=0, argumentIndex:=0, argumentCount:=0, argumentName:=Nothing),
                                     Nothing)

                                 Return Task.FromResult(list)
                             End Function)

                signatureHelpProvider _
                    .Setup(Function(p) p.IsTriggerCharacter(It.IsAny(Of Char))) _
                    .Returns(False)

                signatureHelpProvider _
                    .Setup(Function(p) p.IsTriggerCharacter("("c)) _
                    .Returns(True)

                signatureHelpProvider _
                    .Setup(Function(p) p.IsRetriggerCharacter(It.IsAny(Of Char))) _
                    .Returns(False)

                signatureHelpProvider _
                    .Setup(Function(p) p.IsRetriggerCharacter(")"c)) _
                    .Returns(True)

                Return signatureHelpProvider.Object
            End Function

            Public Shared Function Create(
                Optional documentProvider As Mock(Of IDocumentProvider) = Nothing,
                Optional signatureHelpItems As ImmutableArray(Of SignatureHelpItem) = Nothing,
                Optional signatureHelpProvider As SignatureHelpProvider = Nothing,
                Optional presenterSession As Mock(Of ISignatureHelpPresenterSession) = Nothing
            ) As TestEnvironment

                Dim workspace = TestWorkspace.CreateWorkspace(
                    <Workspace>
                        <Project Language="C#">
                            <Document>
                            </Document>
                        </Project>
                    </Workspace>)

                Dim documentId = workspace.Documents.Single().Id
                Dim testDocument = workspace.GetTestDocument(documentId)
                Dim document = workspace.CurrentSolution.GetDocument(documentId)

                Dim textBuffer = testDocument.GetTextBuffer()
                Dim textView = CType(testDocument.GetTextView(), ITextView)

                If documentProvider Is Nothing Then
                    documentProvider = New Mock(Of IDocumentProvider)

                    With documentProvider
                        .Setup(Function(p) p.GetDocumentAsync(It.IsAny(Of ITextSnapshot), It.IsAny(Of CancellationToken))) _
                            .Returns(Task.FromResult(document))
                        .Setup(Function(p) p.GetOpenDocumentInCurrentContextWithChanges(It.IsAny(Of ITextSnapshot))) _
                            .Returns(document)
                    End With
                End If

                Dim presenter = New Mock(Of IIntelliSensePresenter(Of ISignatureHelpPresenterSession, ISignatureHelpSession)) With {
                    .DefaultValue = DefaultValue.Mock
                }

                If presenterSession Is Nothing Then
                    presenterSession = New Mock(Of ISignatureHelpPresenterSession) With {
                        .DefaultValue = DefaultValue.Mock
                    }
                End If

                presenter _
                    .Setup(Function(p) p.CreateSession(It.IsAny(Of ITextView), It.IsAny(Of ITextBuffer), It.IsAny(Of ISignatureHelpSession))) _
                    .Returns(presenterSession.Object)

                presenterSession _
                    .Setup(Sub(p) p.PresentItems(It.IsAny(Of ITrackingSpan), It.IsAny(Of IList(Of SignatureHelpItem)), It.IsAny(Of SignatureHelpItem), It.IsAny(Of Integer?))) _
                    .Callback(Sub() presenterSession _
                        .SetupGet(Function(p) p.EditorSessionIsActive) _
                        .Returns(True))

                Dim asyncOperationListener = New Mock(Of IAsynchronousOperationListener)()

                Dim controller = New Controller(
                    workspace.GetService(Of IThreadingContext),
                    textView,
                    textBuffer,
                    presenter.Object,
                    asyncOperationListener.Object,
                    documentProvider.Object)

                Return New TestEnvironment(
                    document, textBuffer, textView, signatureHelpItems, signatureHelpProvider,
                    documentProvider, presenter, presenterSession, controller)
            End Function

            Private Function GetCommandHandler(Of T As VisualStudio.Commanding.CommandArgs)() As IChainedCommandHandler(Of T)
                Return CType(Controller, IChainedCommandHandler(Of T))
            End Function

            Public Sub InvokeSignatureHelp(Optional waitForController As Boolean = True)
                Dim commandHandler = GetCommandHandler(Of InvokeSignatureHelpCommandArgs)()

                commandHandler.ExecuteCommand(
                    New InvokeSignatureHelpCommandArgs(TextView, TextBuffer),
                    nextCommandHandler:=Nothing,
                    TestCommandExecutionContext.Create())

                If waitForController Then
                    Controller.WaitForController()
                End If
            End Sub

            Public Sub TypeChar(ch As Char)
                Dim commandHandler = GetCommandHandler(Of TypeCharCommandArgs)()

                commandHandler.ExecuteCommand(
                    New TypeCharCommandArgs(TextView, TextBuffer, ch),
                    nextCommandHandler:=Sub() TextBuffer.Insert(TextView.Caret.Position.BufferPosition.Position, ch),
                    TestCommandExecutionContext.Create())
            End Sub
        End Class
    End Class
End Namespace
