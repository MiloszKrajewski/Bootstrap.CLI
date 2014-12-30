namespace Bootstrap.CLI

module ConfigParser =
    open Tokenizer
    open System.IO

    let rec private enumerateLines (reader: TextReader) =
        seq {
            let l = reader.ReadLine()
            if l <> null then
                yield l
                yield! enumerateLines reader
        }

    let readConfigLines lines =
        let parse ((section, result) as state) line =
            match line with
            | SectionLine sn -> (sn, result)
            | KeyValueLine (k, v) -> (section, ((section, k), v) :: result)
            | CommentLine c -> state
            | EmptyLine e -> state
            | _ -> state // ignore errors!
        lines |> Seq.fold parse ("default", []) |> snd |> Map.ofSeq

    let readConfigReader (stream: TextReader) =
        stream |> enumerateLines |> readConfigLines

    let readConfigStream (stream: Stream) =
        new StreamReader(stream, true) |> readConfigReader

    let readConfigFile fileName =
        File.OpenRead(fileName) |> readConfigStream

    let tryGetValue sectionName valueName map =
        map |> Map.tryFind (sectionName, valueName)

    let getValue sectionName valueName defaultValue map =
        defaultArg (map |> tryGetValue sectionName valueName) defaultValue
