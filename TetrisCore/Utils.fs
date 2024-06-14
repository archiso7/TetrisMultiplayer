namespace TetrisCore

module Utils = 

    open System

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
