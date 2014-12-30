namespace Bootstrap.CLI

/// <summary>
/// Command line tokienizer module.
/// </summary>
module internal Tokenizer =
    let inline splitNames (names: string) = names.Split('|')
    let inline optionString (text: string) = match text with | null -> None | t -> Some t

    module internal Regex =
        open System.Text.RegularExpressions

        let private rxOptions = 
            RegexOptions.ExplicitCapture |||
            RegexOptions.IgnorePatternWhitespace

        let inline create rx = Regex(rx, rxOptions)

        let inline tryMatch tx (rx: Regex) = 
            let m = rx.Match(tx)
            if m.Success then Some m else None

        let inline tryGroup (n: string) (m: Match option) = 
            let getGroup (m: Match) = 
                let g = m.Groups.[n]
                if g.Success then Some g else None
            let getValue (g: Group) = 
                if g.Success then Some g.Value else None
            m |> Option.bind getGroup |> Option.bind getValue

    let private argRx = Regex.create @"^(--|-|/)(?<name>\w[^=]*)(=(?<value>.*))?$"

    /// <summary>
    /// Tokienizer active pattern. It is actually quite inefficient to use
    /// it, but it is not used in any hotspot so it is acceptable.
    /// </summary>
    /// <param name="arg">Argument.</param>
    let (|OptionToken|SwitchToken|StringToken|) arg =
        let m = argRx |> Regex.tryMatch arg
        let name, value = m |> Regex.tryGroup "name", m |> Regex.tryGroup "value"
        match name, value with
        | Some n, Some v -> OptionToken (n, v)
        | Some n, None -> SwitchToken n
        | _ -> StringToken arg

    let private sectionLineRx = Regex.create @"^\s*\[(?<section>\w[^\]]*)\]\s*$"

    let (|SectionLine|_|) line =
        sectionLineRx |> Regex.tryMatch line |> Regex.tryGroup "section"

    let private keyValueLineRx = Regex.create @"^\s*(?<key>\w[^=]*?)\s*\=\s*(?<value>.*?)\s*$"

    let (|KeyValueLine|_|) line =
        let m = keyValueLineRx |> Regex.tryMatch line
        match m |> Regex.tryGroup "key", m |> Regex.tryGroup "value" with
        | Some k, Some v -> Some (k, v)
        | _ -> None

    let private commentLineRx = Regex.create @"^\s*(\#|;)\s*(?<comment>.*?)\s*$"

    let (|CommentLine|_|) line =
        commentLineRx |> Regex.tryMatch line |> Regex.tryGroup "comment"

    let private emptyLineRx = Regex.create @"^(?<line>\s*)$"

    let (|EmptyLine|_|) line =
        emptyLineRx |> Regex.tryMatch line |> Regex.tryGroup "line"




