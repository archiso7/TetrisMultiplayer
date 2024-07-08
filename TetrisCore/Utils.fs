namespace TetrisCore

module Utils = 

    open System
    open System.Collections.Generic
    open Types

    let crossProduct (v1: Vector2) (v2: Vector2) =
        v1.X * v2.Y - v1.Y * v2.X

    let pointInTriangle a b c p =
        let sign (p1: Vector2) (p2: Vector2) (p3: Vector2) = (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y)

        let d1 = sign p a b
        let d2 = sign p b c
        let d3 = sign p c a

        let has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0)
        let has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0)

        not (has_neg && has_pos)

    let customModulo dividend divisor =
        let result = dividend % divisor
        if result < 0 && divisor > 0 then
            result + divisor
        else if result > 0 && divisor < 0 then
            result + divisor
        else
            result

    let isConvex (polygon: Vector2 list) =
        let rec checkConvexity idx sign =
            if idx >= polygon.Length then true
            else
                let prev = polygon.[customModulo (idx - 1) polygon.Length]
                let curr = polygon.[idx]
                let next = polygon.[customModulo (idx + 1) polygon.Length]
                let cross = crossProduct (curr - prev) (next - curr)
                let newSign = Math.Sign(cross)
                if sign = 0 then checkConvexity (idx + 1) newSign
                else if newSign = 0 || newSign = sign then checkConvexity (idx + 1) sign
                else false
        checkConvexity 0 0

    let sliceConvexPolygon (polygon: Vector2 list) =
        let rec slice idx acc =
            if idx + 2 < polygon.Length then
                let triangle = [polygon.[0]; polygon.[idx + 1]; polygon.[idx + 2]]
                slice (idx + 1) (triangle :: acc)
            else acc
        slice 0 []

    let isEar (polygon: Vector2 list) (i: int) : bool = 
        let prev = customModulo (i - 1) polygon.Length
        let next = customModulo (i + 1) polygon.Length
        let a, b, c = polygon.[prev], polygon.[i], polygon.[next]

        if crossProduct (b - a) (c - b) >= 0 then // Check for convexity
            false
        else
            let mutable counter = 0
            let mutable result = true
            while counter < polygon.Length do
                result <- if pointInTriangle a b c polygon.[counter] && not (counter = prev || counter = i || counter = next) then false else result
                counter <- counter + 1
            result

    let removeAtIndex index list =
        match List.splitAt index list with
        | before, _::after -> List.append before after
        | _ -> list // Handle index out of bounds or empty list case

    let triangulate (polygon: Vector2 list) = 
        if polygon.Length < 3 then 
            []
        elif List.length polygon = 3 then
            [polygon]
        else
            if isConvex polygon then
                sliceConvexPolygon polygon
            else
                let mutable result = []
                let mutable mutablePolygon = polygon

                while mutablePolygon.Length > 3 do
                    let mutable earFound = false
                    let mutable counter = 0
                    while counter < mutablePolygon.Length && not earFound do
                        if isEar mutablePolygon counter then
                            let prev = customModulo (counter - 1) mutablePolygon.Length
                            let next = customModulo (counter + 1) mutablePolygon.Length
                            result <- List.append result [[mutablePolygon.[prev]; mutablePolygon.[counter]; mutablePolygon.[next]]]
                            mutablePolygon <- removeAtIndex counter mutablePolygon
                            earFound <- true
                        else 
                            counter <- counter + 1
                    if not earFound then 
                        failwith "No ear found. Invalid polygon or algorithm error."

                result <- List.append result [mutablePolygon]
                result

    // Shuffle function
    let shuffle<'a> (list: 'a list) =
        let rnd = Random()
        let rec shuffleOne acc remaining =
            match remaining with
            | [] -> acc
            | _ ->
                let index = rnd.Next(0, List.length remaining)
                let (before, rest) = List.splitAt index remaining
                match rest with
                | [] -> shuffleOne acc before
                | element::after -> shuffleOne (element::acc) (before @ after)
        shuffleOne [] list
