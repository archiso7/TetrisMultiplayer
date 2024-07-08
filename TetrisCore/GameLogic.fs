namespace TetrisCore

module GameLogic =

    open Utils
    open System
    open Types

    let createPiece shape position = { Shape = shape; Table = pieceTable[shape]; Rotation = 0; Position = position }

    // let updateGameState (state: GameState) : GameState =

    let generateBag () =
        let random = Random()

        let shapes = [Shape.I; Shape.O; Shape.T; Shape.S; Shape.Z; Shape.J; Shape.L]
        let shuffledShapes = shuffle shapes

        let createTetrisPiece shape =
            {
                Shape = shape
                Table = pieceTable[shape]
                Rotation = 0
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
            Board = List.init 10 (fun _ -> List.init 20 (fun _ -> 0))
            Score = 0
        }

    let placePiece (board: int[][]) (piece: TetrisPiece) (remove: bool) : int[][] =
        let shape = piece.Table
        let mutable newBoard = Array.copy board
        for y in 0 .. shape.Length - 1 do
            for x in 0 .. shape.[y].Length - 1 do
                if shape.[y].[x] = 1 then
                    let xPos = x + fst piece.Position
                    let yPos = y + snd piece.Position
                    newBoard.[xPos].[yPos] <- if remove then 0 else int piece.Shape
        newBoard

    let transposeMatrix (x: 'a array array) : 'a array array =
        match x with
        | [||] -> [||]
        | _ ->
            let width = Array.length x.[0]
            let height = Array.length x
            Array.init width (fun i ->
                Array.init height (fun j ->
                    x.[j].[i]
                )
            )

    let rotateMatrix (x: 'a array array) (direction: int) : 'a array array = 
        if direction = 1 then
                Array.rev (transposeMatrix x)
            elif direction = 2 then
                Array.rev (Array.map Array.rev x)
            elif direction = 3 then
                transposeMatrix x |> Array.map Array.rev // Optionally reverse rows if needed
            else
                x

    let checkCollision (board: int[][]) (piece: TetrisPiece) =
        let shape = piece.Table
        let (pieceX, pieceY) = piece.Position
        let width = shape.[0].Length
        let height = shape.Length

        // Iterate over the piece shape
        let mutable collisionDetected = false
        for x in 0 .. width - 1 do
            for y in 0 .. height - 1 do
                if shape.[y].[x] = 1 then
                    if (pieceX + x < 0) || (pieceX + x > board.Length) || (pieceY + y < 0) || (pieceY + y > board.[0].Length) then
                        collisionDetected <- true
                    elif board.[pieceX + x].[pieceY + y] <> 0 then 
                        collisionDetected <- true

        collisionDetected

    let rotatePiece (board: int[][]) (piece: TetrisPiece) (direction: int) =
        let res = customModulo (piece.Rotation + direction) 4
        let mul = if (direction = 1) || (res = 3) then 1 else -1
        let newTable = rotateMatrix piece.Table direction

        let cleanBoard = placePiece board piece true

        let newPiece' = { piece with Table = newTable; Rotation = res }
        let mutable newPiece = newPiece'

        let mutable collide = true
        let mutable count = 0
        while collide do
            collide <- checkCollision cleanBoard newPiece
            if collide then
                let newPosition =
                    (fst newPiece'.Position + (mul * fst wallKick.[count]), snd newPiece'.Position + (mul * snd wallKick.[count]))
                newPiece <- { newPiece with Position = newPosition }
                count <- count + 1
                if count > 3 then
                    newPiece <- { newPiece' with Table = rotateMatrix newPiece.Table (customModulo (4-direction) 4) }
                    collide <- false

        let newBoard = placePiece cleanBoard newPiece false

        (newBoard, newPiece)

    let movePiece piece direction state =
        // Logic to move a piece
        piece