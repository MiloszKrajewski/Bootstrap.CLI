namespace Bootstrap.CLI

open System
   
module CommandParser = 
    open Tokenizer

    let MissingDefaultHandler () =
        ArgumentException ("The default handler is missing") |> raise

    type Command = { Name: string; HelpText: string option; Handler: string seq -> unit }
    type Config = { DefaultHandler: (string seq -> unit) option; Commands: Command list }

    let internal createCommand name helpText handler =
        { Name = name; HelpText = optionString helpText; Handler = handler }

    let createCommandParser () =
        { DefaultHandler = None; Commands = [] }

    let onDefaultCommand handler configuration =
        { configuration with DefaultHandler = Some handler }

    let onCommand name helpText handler configuration = 
        let head = createCommand name helpText handler
        { configuration with Commands = head :: configuration.Commands }

    let parseCommands args configuration =
        let commandMap = configuration.Commands |> Seq.map (fun c -> (c.Name, c.Handler)) |> Map.ofSeq
        let defaultHandler = configuration.DefaultHandler
        let isCommand name = commandMap |> Map.containsKey name
        let handleAny handler tail =
            match handler with
            | Some f -> f tail
            | _ -> MissingDefaultHandler ()
        let handleCommand name tail = handleAny (commandMap |> Map.tryFind name) tail
        let handleDefault tail = handleAny defaultHandler tail
        let parse args =
            match args with
            | StringToken n :: tail when isCommand n -> handleCommand n tail
            | _ -> handleDefault args
        args |> List.ofSeq |> parse
