namespace TetrisCore

module GameLogic =

    open Utils
    open System
    open Types

    let createPiece shape position = { Shape = shape; Position = position }

    // let updateGameState (state: GameState) : GameState =

    let generateBag () =
        let random = Random()

        let shapes = [I; O; T; S; Z; J; L]
        let shuffledShapes = shuffle shapes

        let createTetrisPiece shape =
            {
                Shape = shape
                Position = (0, 0)
            }

        shuffledShapes |> List.map createTetrisPiece

    let initialState () =

        let bag = generateBag()
        let piece = bag |> List.head
        let queue = bag[1..5]
        {
            CurrentPiece = piece
            Hold = None
            Queue = queue
            Bag = [bag[6]]
            Board = List.init 20 (fun _ -> List.init 10 (fun _ -> Color.Gray))
            Score = 0
        }

    let checkCollision piece state =
        // Collision detection logic
        false

    let movePiece piece direction state =
        // Logic to move a piece
        piece