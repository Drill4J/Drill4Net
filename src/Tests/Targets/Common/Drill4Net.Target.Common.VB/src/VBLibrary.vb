Public Class VBLibrary

	Public Sub Try_Catch_VB(cond As Boolean)
		Dim s As String = "none"
		Try
			Throw New Exception()
		Catch
			s = If(cond, "YES", "NO")
		End Try
		Console.WriteLine($"{NameOf(Try_Catch_VB)}: {s}")
	End Sub

	Public Sub Try_Finally_VB(cond As Boolean)
		Dim s As String = Nothing
		Try
			s = "A"
		Finally
			s = If(cond, "YES", "NO") + "/" + s
		End Try
		Console.WriteLine($"{NameOf(Try_Finally_VB)}: {s}")
	End Sub

End Class
