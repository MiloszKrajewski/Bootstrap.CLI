namespace Bootstrap.CommandLine.Tests

open NUnit.Framework
open Bootstrap.CommandLine

type SimpleArguments = {
    flag: bool
    value: string
    args: string list }

[<TestFixture>]
type OptionParserConfiguratorTests () = 
    [<Test>]
    member x.SimpleArgumentParser() = 
        let empty = { flag = false; value = ""; args = [] }
        let parser = 
            ArgumentParser.create ()
            |> ArgumentParser.onSwitch "flag" "" (fun c -> { c with flag = true })
            |> ArgumentParser.onOption "v|value" "" (fun c s -> { c with value = s })
            |> ArgumentParser.onString (fun c s -> { c with args = s :: c.args })
        let options = 
            parser |> ArgumentParser.parse empty [ "--flag"; "-v"; "hello"; "arg1"; "arg2"; "arg3" ]

        Assert.AreEqual(true, options.flag)
        Assert.AreEqual("hello", options.value)
        Assert.IsTrue(options.args = [ "arg3"; "arg2"; "arg1" ])

    [<Test>]
    member x.SimpleCommandParser() =
        let result = ref ""
        CommandParser.create ()
        |> CommandParser.onDefaultCommand (fun c a -> c)
        |> CommandParser.onCommand "update" "" (fun c a ->
            result.Value <-
                ArgumentParser.create ()
                |> ArgumentParser.onOption "file" "" (fun c s -> s)
                |> ArgumentParser.parse "" a
            c)
        |> CommandParser.onCommand "delete" null (fun args ->
            failwith "Delete not implemented")
        |> CommandParser.parse () [ "update"; "--file"; "some_filename" ]

        Assert.AreEqual("File 'some_filename' has been updated", result.Value)
