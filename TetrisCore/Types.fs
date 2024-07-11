namespace TetrisCore

module Types =

    open System

    type Position = int * int
    type Shape =
    | I = 1
    | O = 2
    | T = 3
    | S = 4
    | Z = 5
    | J = 6
    | L = 7

    let pieceTable = 
        let dict = 
            dict [
                Shape.I, 
                [| 
                    [|0;0;0;0|]; 
                    [|1;1;1;1|]; 
                    [|0;0;0;0|]; 
                    [|0;0;0;0|] 
                |]
                Shape.O, 
                [| 
                    [|1;1|]; 
                    [|1;1|] 
                |]
                Shape.T, 
                [| 
                    [|0;1;0|]; 
                    [|1;1;1|]; 
                    [|0;0;0|] 
                |]
                Shape.S, 
                [| 
                    [|0;1;1|]; 
                    [|1;1;0|]; 
                    [|0;0;0|] 
                |]
                Shape.Z, 
                [| 
                    [|1;1;0|]; 
                    [|0;1;1|]; 
                    [|0;0;0|] 
                |]
                Shape.J, 
                [| 
                    [|1;0;0|]; 
                    [|1;1;1|]; 
                    [|0;0;0|] 
                |]
                Shape.L, 
                [| 
                    [|0;0;1|]; 
                    [|1;1;1|]; 
                    [|0;0;0|] 
                |]
            ]
        dict

    type TetrisPiece = {
        Shape: Shape
        Table: int[][]
        Rotation: int
        Position: Position
    }

    type GameState = {
        mutable CurrentPiece: TetrisPiece
        Hold: TetrisPiece Option
        Bag: TetrisPiece list
        Queue: TetrisPiece list
        mutable Board: int list list
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

    let wallKick =
        [
            (1, 0)
            (1, -1)
            (0, 2)
            (1, 2)
        ]
    
    let iWallKick=
        [
            (2,0)
            (-1,0)
            (2,1)
            (-1,-2)
        ]