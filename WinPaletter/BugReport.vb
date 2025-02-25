﻿Imports System.Text
Imports WinPaletter.XenonCore

Public Class BugReport
    Private Sub BugReport_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ApplyDarkMode(Me)
        Dim c As Color = PictureBox1.Image.AverageColor
        Dim c1 As Color = c.CB(If(GetDarkMode(), -0.35, 0.35))
        Dim c2 As Color = c.CB(If(GetDarkMode(), -0.75, 0.75))
        'XenonTreeView1.BackColor = c2

        XenonAnimatedBox1.BackColor = c1
        'BackColor = c2

        Label2.Font = My.Application.ConsoleFontMedium
        Label3.Font = My.Application.ConsoleFontMedium

        XenonTreeView1.Font = My.Application.ConsoleFontMedium

        Try : BK.Close() : Catch : End Try
        Try : BK.Show() : Catch : End Try

        DrawCustomTitlebar(c1)

        My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Asterisk)
    End Sub

    Sub AddData(str As String, Exception As Exception, [Treeview] As TreeView)

        Try
            With [Treeview].Nodes.Add(str & " Data").Nodes
                If Exception.Data.Keys.Count > 0 Then
                    For Each x As DictionaryEntry In Exception.Data
                        .Add(String.Format("{0} = {1}", x.Key.ToString, x.Value.ToString))
                    Next
                Else
                    .Add("There is no included data in " & str)
                End If

            End With
        Catch
        End Try
    End Sub

    Sub AddException(str As String, Exception As Exception, [Treeview] As TreeView)

        Try
            If Not String.IsNullOrWhiteSpace(Exception.Message) Then

                [Treeview].Nodes.Add(str & " Message").Nodes.Add(Exception.Message)

                Dim n As TreeNode = [Treeview].Nodes.Add(str & " StackTrace")

                For Each x In Exception.StackTrace.CList
                    n.Nodes.Add(x)
                Next

                AddData(str, Exception, [Treeview])

                [Treeview].Nodes.Add(str & " Target Sub\Function").Nodes.Add(Exception.TargetSite.Name & " @ " & Exception.Source)
                [Treeview].Nodes.Add(str & " Assembly").Nodes.Add(Exception.TargetSite.Module.Assembly.FullName)
                [Treeview].Nodes.Add(str & " Assembly's File").Nodes.Add(Exception.TargetSite.Module.Assembly.Location)
                [Treeview].Nodes.Add(str & " HRESULT").Nodes.Add(Exception.HResult)
                If Not String.IsNullOrWhiteSpace(Exception.HelpLink) Then [Treeview].Nodes.Add(str & " Microsoft Help Link").Nodes.Add(Exception.HelpLink)

            End If

        Catch
        End Try
    End Sub

    Public Sub ThrowError(Exception As Exception)

        Dim CV As String = "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion"
        Dim sy As String = "." & Microsoft.Win32.Registry.GetValue(CV, "UBR", 0).ToString
        If sy = ".0" Then sy = ""

        Dim sx As String = System.Runtime.InteropServices.RuntimeInformation.OSDescription.Replace("Microsoft Windows ", "")
        sx = sx.Replace("S", "").Trim

        Label2.Text = My.Computer.Info.OSFullName & " - " & sx & sy & " - " & If(Environment.Is64BitOperatingSystem, "64-bit", "32-bit")

        Label3.Text = My.Application.Info.Version.ToString

        Dim IE As String = ""

        XenonTreeView1.Nodes.Clear()
        AddException("Exception", Exception, XenonTreeView1)
        Dim x As Exception = Exception.InnerException
        AddException("Inner Exception", x, XenonTreeView1)
        'AddException("Base Exception", Exception.GetBaseException, XenonTreeView1)

        XenonTreeView1.ExpandAll()

        If Not IO.Directory.Exists(My.Application.appData & "\Reports") Then IO.Directory.CreateDirectory(My.Application.appData & "\Reports")

        IO.File.WriteAllText(String.Format(My.Application.appData & "\Reports\{0}.{1}.{2} {3}-{4}-{5}.txt", Now.Hour, Now.Minute, Now.Second, Now.Day, Now.Month, Now.Year), GetDetails)

        ShowDialog()

        BK.Close()

        If DialogResult = DialogResult.Abort Then My.Application.ExitAfterException = True Else My.Application.ExitAfterException = False

    End Sub


    Private Sub XenonButton2_Click(sender As Object, e As EventArgs) Handles XenonButton2.Click
        DialogResult = DialogResult.Abort
        Me.Close()
        Process.GetCurrentProcess.Kill()
    End Sub

    Private Sub XenonButton1_Click(sender As Object, e As EventArgs) Handles XenonButton1.Click
        DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub XenonButton5_Click(sender As Object, e As EventArgs) Handles XenonButton5.Click
        Process.Start(My.Resources.Link_Repository & "issues")
        Try : BK.Close() : Catch : End Try
    End Sub

    Private Sub XenonButton3_Click(sender As Object, e As EventArgs) Handles XenonButton3.Click
        Clipboard.SetText(GetDetails)
    End Sub

    Private Sub XenonButton4_Click(sender As Object, e As EventArgs) Handles XenonButton4.Click
        If SaveFileDialog1.ShowDialog = DialogResult.OK Then
            IO.File.WriteAllText(SaveFileDialog1.FileName, GetDetails)
        End If
    End Sub

    Function GetDetails() As String
        Dim SB As New StringBuilder
        SB.Clear()
        SB.AppendLine("```vbnet")
        SB.AppendLine("'General Information")
        SB.AppendLine("'-----------------------------------------------------------")
        SB.AppendLine(String.Format("Report.Date = ""{0}""", Now.ToLongDateString & " " & Now.ToLongTimeString))
        SB.AppendLine(String.Format("OS = ""{0}""", Label2.Text))
        SB.AppendLine(String.Format("WinPaletter.Version = ""{0}""", Label3.Text))
        SB.AppendLine()

        SB.AppendLine("'Error Details")
        SB.AppendLine("'-----------------------------------------------------------")

        For Each x As TreeNode In XenonTreeView1.Nodes
            Dim prop As String = x.Text.Replace(" ", ".").Replace("'s", "").Replace("\", "_")

            If x.Nodes.Count = 1 Then
                SB.AppendLine(prop & " = """ & x.Nodes.Item(0).Text & """")
            Else
                SB.AppendLine(prop & " = {")

                For Each y As TreeNode In x.Nodes
                    SB.AppendLine(y.Text)
                Next

                SB.AppendLine("         }")
            End If

        Next

        SB.AppendLine("```")

        Return SB.ToString
    End Function

    Private Sub XenonButton6_Click(sender As Object, e As EventArgs) Handles XenonButton6.Click

        If IO.Directory.Exists(My.Application.appData & "\Reports") Then
            Process.Start(My.Application.appData & "\Reports")
            Try : BK.Close() : Catch : End Try
        Else
            MsgBox(String.Format(My.Lang.Bug_NoReport, My.Application.appData & "\Reports"), MsgBoxStyle.Critical)
        End If

    End Sub
End Class