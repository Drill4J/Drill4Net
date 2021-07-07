Public Class VBLibrary

	Public Sub VB_Try_Catch(cond As Boolean)
		Dim s As String = "none"
		Try
			Throw New Exception()
		Catch
			s = If(cond, "YES", "NO")
		End Try
		Console.WriteLine($"{NameOf(VB_Try_Catch)}: {s}")
	End Sub

	Public Sub VB_Try_Finally(cond As Boolean)
		Dim s As String = Nothing
		Try
			s = "A"
		Finally
			s = If(cond, "YES", "NO") + "/" + s
		End Try
		Console.WriteLine($"{NameOf(VB_Try_Finally)}: {s}")
	End Sub

End Class
