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
                Position = (3, 0)
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
            Board = List.init 10 (fun _ -> List.init 23 (fun _ -> 0))
            Score = 0
        }

    let placePiece (board: int[][]) (piece: TetrisPiece) (remove: bool) (blankCount: int) : int[][] =
        let shape = piece.Table
        let mutable newBoard = Array.copy board
        for y in 0 .. shape.Length - 1 do
            for x in 0 .. shape.[y].Length - 1 do
                if shape.[y].[x] = 1 then
                    let xPos = x + fst piece.Position
                    let yPos = y + snd piece.Position
                    newBoard.[xPos].[yPos] <- 
                        if remove then 
                            if yPos >= blankCount - 1 then 0 else -1
                        else int piece.Shape
        newBoard

    let nextPiece (board: int[][]) (state: GameState) (blankCount: int) = 
        let piece = state.Queue |> List.head
        let newQueue = state.Queue[1..] @ [state.Bag[0]]
        let newBag = if state.Bag.Length < 2 then generateBag() else state.Bag[1..]
        let newBoard = placePiece board piece false blankCount

        // Convert int array array to int list list
        let newBoardList = newBoard |> Array.map Array.toList |> Array.toList

        let newState = 
            { 
                CurrentPiece = piece
                Hold = state.Hold
                Queue = newQueue
                Bag = newBag
                Board = newBoardList
                Score = state.Score
            }
        newState

    let holdPiece (state: GameState) (blankCount: int) =
        match state.Hold with
        | Some tempPiece -> 
            // Swap current piece with the held piece
            state.Hold <- Some {{{ state.CurrentPiece with Position = (1, 1) } with Rotation = 0} with Table = pieceTable[state.CurrentPiece.Shape]}
            let newBoard = placePiece (state.Board |> List.map List.toArray |> List.toArray) state.CurrentPiece true blankCount
            state.CurrentPiece <- {{{ tempPiece with Position = (3, 0) } with Rotation = 0} with Table = pieceTable[tempPiece.Shape]}
            state.Board <- (placePiece newBoard state.CurrentPiece false blankCount) |> Array.map Array.toList |> Array.toList
            state
        | None -> 
            // Move current piece to hold if no piece is held
            state.Hold <- Some {{{ state.CurrentPiece with Position = (1, 1) } with Rotation = 0} with Table = pieceTable[state.CurrentPiece.Shape]}
            let newBoard = placePiece (state.Board |> List.map List.toArray |> List.toArray) state.CurrentPiece true blankCount
            nextPiece newBoard state blankCount

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
                    if (pieceX + x < 0) || (pieceX + x > board.Length - 1) || (pieceY + y < 0) || (pieceY + y > board.[0].Length - 1) then
                        collisionDetected <- true
                    elif board.[pieceX + x].[pieceY + y] > 0 then 
                        collisionDetected <- true

        collisionDetected

    let onGround (board: int[][]) (piece: TetrisPiece) (blankCount: int) = 
        let tempPiece = {piece with Position = (fst piece.Position , snd piece.Position + 1)}
        let cleanBoard = placePiece (Array.copy board) piece true blankCount
        let isOnGround = checkCollision cleanBoard tempPiece
        let b = placePiece cleanBoard piece false blankCount
        isOnGround

    let movePiece (board: int[][]) (piece: TetrisPiece) (direction: Position) (blankCount: int) =
        let newPos = (fst piece.Position + fst direction, snd piece.Position + snd direction)
        let mutable newPiece = {piece with Position = newPos}
        let cleanBoard = placePiece board piece true blankCount
        let canMove = checkCollision cleanBoard newPiece
        let outPiece = 
            if canMove then
                piece
            else
                newPiece
        let newBoard = placePiece cleanBoard outPiece false blankCount

        (not canMove, newBoard, outPiece)

    let rotatePiece (board: int[][]) (piece: TetrisPiece) (direction: int) (blankCount: int) =
        let res = customModulo (piece.Rotation + direction) 4
        let mul = if (direction = 1) || (res = 3) then 1 else -1
        let newTable = rotateMatrix piece.Table direction

        let cleanBoard = placePiece board piece true blankCount

        let newPiece' = { piece with Table = newTable; Rotation = res }
        let mutable newPiece = newPiece'

        let mutable collide = true
        let mutable count = 0
        let kicks = if piece.Shape = Shape.I then iWallKick else wallKick
        let mutable moved = true
        while collide do
            collide <- checkCollision cleanBoard newPiece
            if collide then
                let newPosition =
                    (fst newPiece'.Position + (mul * fst kicks.[count]), snd newPiece'.Position + (mul * snd kicks.[count]))
                newPiece <- { newPiece with Position = newPosition }
                count <- count + 1
                if count > 3 then
                    newPiece <- {{ newPiece' with Table = rotateMatrix newPiece.Table (customModulo (4-direction) 4) } with Rotation = customModulo (newPiece'.Rotation - direction) 4}
                    collide <- false
                    moved <- false

        let newBoard = placePiece cleanBoard newPiece false blankCount

        (moved, newBoard, newPiece)

    let updateGhostPiece (board: int[][]) (piece: TetrisPiece) : TetrisPiece =
        let mutable ghostPiece = createPiece piece.Shape (fst piece.Position, snd piece.Position)
        ghostPiece <- {{ghostPiece with Rotation = piece.Rotation} with Table = rotateMatrix ghostPiece.Table piece.Rotation}
        let mutable canMove = true
        while canMove do
            let newPos = (fst ghostPiece.Position, snd ghostPiece.Position + 1)
            ghostPiece <- { ghostPiece with Position = newPos }
            canMove <- not (checkCollision board ghostPiece)
        { ghostPiece with Position = (fst ghostPiece.Position, snd ghostPiece.Position - 1) }
