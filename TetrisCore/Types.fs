namespace TetrisCore

module Types =

    open System

    type Position = int * int
    type Shape = I | O | T | S | Z | J | L
    type Color = Gray=0 | LightBlue=1 | Yellow=2 | Purple=3 | Green=4 | Red=5 | Blue=6 | Orange=7

    type TetrisPiece = {
        Shape: Shape
        Position: Position
    }

    type GameState = {
        CurrentPiece: TetrisPiece
        Hold: TetrisPiece Option
        Bag: TetrisPiece list
        Queue: TetrisPiece list
        Board: Color list list
        Score: int
    }

    type Vector2(x: float, y: float) =
        member this.X = x
        member this.Y = y

        static member (-) (a: Vector2, b: Vector2) =
            Vector2(a.X - b.X, a.Y - b.Y)

        member this.DotProduct(other: Vector2) =
            this.X * other.X + this.Y * other.Y

        member this.Magnitude() =
            Math.Sqrt(this.X ** 2.0 + this.Y ** 2.0)

        override this.ToString() =
            sprintf "(%f, %f)" this.X this.Y