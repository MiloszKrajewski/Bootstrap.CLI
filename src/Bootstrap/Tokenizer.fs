namespace Bootstrap.CommandLine

module internal Tokenizer =
    let inline splitNames (names: string) = names.Split('|')
    let inline optionString (text: string) = match text with | null -> None | t -> Some t

    module internal Regex =
        open System.Text.RegularExpressions

        let inline create rx = Regex(rx, RegexOptions.ExplicitCapture)

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

    /// Tokienizer active pattern. It is actually quite inefficient to use
    /// it, but it is not used in any hotspot so it is acceptable.
    let (|OptionToken|SwitchToken|StringToken|) arg =
        let m = argRx |> Regex.tryMatch arg
        match m |> Regex.tryGroup "name", m |> Regex.tryGroup "value" with
            | Some n, Some v -> OptionToken (n, v)
            | Some n, None -> SwitchToken n
            | _ -> StringToken arg
