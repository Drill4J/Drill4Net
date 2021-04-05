namespace Drill4Net.Target.Common.FSharp

type FTarget(property1 : int) =
    member val Property1 = property1
    member val Property2 = "" with get, set
    member this.MyProperty1 : int = 5

type RectangleXY(x1 : float, y1: float, x2: float, y2: float) =
    // Field definitions.
    let height = y2 - y1
    let width = x2 - x1
    let area = height * width

    // Private functions.
    static let maxFloat (x: float) (y: float) =
      if x >= y then x else y
    static let minFloat (x: float) (y: float) =
      if x <= y then x else y