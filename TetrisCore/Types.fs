namespace TetrisCore

module Types =

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