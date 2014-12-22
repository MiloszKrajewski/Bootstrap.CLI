namespace Bootstrap.CLI

open System

module ArgumentParser = 
    open Tokenizer

    type Switch = { Names: string array; HelpText: string option; Handler: unit -> unit }
    type Option = { Names: string array; HelpText: string option; Handler: string -> unit }
    type Config = { DefaultHandler: (string -> unit) option; Switches: Switch list; Options: Option list }

    let private UnhandledArgument arg = 
        ArgumentException (sprintf "Unhandled argument '%s'" arg) |> raise
    let private UnrecognizedOption name = 
        ArgumentException (sprintf "Unrecognized option '%s'" name) |> raise
    let private UnrecognizedSwitch name = 
        ArgumentException (sprintf "Unrecognized switch '%s'" name) |> raise
    let private MissingOptionValue name = 
        ArgumentException (sprintf "Option '%s' expect value" name) |> raise

    let private createSwitch names helpText handler = 
        { Switch.Names = splitNames names; HelpText = optionString helpText; Handler = handler }

    let private createOption names helpText handler =
        { Option.Names = splitNames names; HelpText = optionString helpText; Handler = handler }

    let createArgumentParser () =
        { DefaultHandler = None; Switches = []; Options = [] }

    let onString handler configuration =
        { configuration with DefaultHandler = Some handler }

    let onSwitch names helpText handler configuration =
        let head = createSwitch names helpText handler
        { configuration with Switches = head :: configuration.Switches }

    let onOption names helpText handler configuration = 
        let head = createOption names helpText handler
        { configuration with Options = head :: configuration.Options }

    let parseArguments args configuration =
        let buildNameMap func list =
            list
            |> Seq.map func
            |> Seq.collect (fun (l, f) -> l |> Seq.map (fun n -> (n, f)))
            |> Map.ofSeq

        let switchMap = configuration.Switches |> buildNameMap (fun s -> (s.Names, s.Handler))
        let optionMap = configuration.Options |> buildNameMap (fun s -> (s.Names, s.Handler))
        let defaultHandler = configuration.DefaultHandler

        let isSwitch name = switchMap |> Map.containsKey name
        let isOption name = optionMap |> Map.containsKey name
        let hasDefaultHandler = defaultHandler.IsSome

        let handleString arg =
            match defaultHandler with | Some f -> f arg | _ -> UnhandledArgument arg
        let handleOption (name, value) =
            match optionMap |> Map.tryFind name with | Some f -> f value | _ -> UnrecognizedOption name
        let handleSwitch name =
            match switchMap |> Map.tryFind name with | Some f -> f () | _ -> UnrecognizedSwitch name

        let rec parse args =
            match args with
            | [] -> ()
            | OptionToken p :: tail -> continueParse (handleOption p) tail
            | SwitchToken n :: tail when isSwitch n -> continueParse (handleSwitch n) tail
            | SwitchToken n :: StringToken v :: tail when isOption n -> continueParse (handleOption (n, v)) tail
            | StringToken v :: tail when hasDefaultHandler -> continueParse (handleString v) tail
            | SwitchToken n :: _ when isOption n -> MissingOptionValue n
            | SwitchToken n :: _ -> UnrecognizedSwitch n
            | StringToken v :: _ -> UnhandledArgument v
        and continueParse _ tail = parse tail

        args |> List.ofSeq |> parse
