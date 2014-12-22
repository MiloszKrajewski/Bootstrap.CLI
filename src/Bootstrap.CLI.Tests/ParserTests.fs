namespace Bootstrap.CLI.Tests

open NUnit.Framework
open Bootstrap.CLI

[<TestFixture>]
type OptionParserConfiguratorTests() = 
    
    [<Test>]
    member x.SimpleArgumentParser() = 
        let flag = ref false
        let value = ref ""
        let args = ref []

        ArgumentParser.createArgumentParser ()
        |> ArgumentParser.onString (fun s -> args.Value <- s :: args.Value)
        |> ArgumentParser.onSwitch "flag" null (fun () -> flag.Value <- true)
        |> ArgumentParser.onOption "v|value" null (fun s -> value.Value <- s)
        |> ArgumentParser.parseArguments [ "--flag"; "-v"; "hello"; "arg1"; "arg2"; "arg3" ]

        Assert.AreEqual(true, flag.Value)
        Assert.AreEqual("hello", value.Value)
        Assert.IsTrue(args.Value = [ "arg3"; "arg2"; "arg1" ])

    [<Test>]
    member x.SimpleCommandParser() =
        let result = ref ""
        let update filename =
            result.Value <- sprintf "File '%s' has been updated" filename

        CommandParser.createCommandParser ()
        |> CommandParser.onDefaultCommand (fun x -> ())
        |> CommandParser.onCommand "update" null (fun args ->
            let fileName = ref ""
            ArgumentParser.createArgumentParser ()
            |> ArgumentParser.onOption "file" null (fun fn -> fileName.Value <- fn)
            |> ArgumentParser.parseArguments args
            update fileName.Value)
        |> CommandParser.onCommand "delete" null (fun args ->
            failwith "Delete not implemented")
        |> CommandParser.parseCommands [ "update"; "--file"; "some_filename" ]

        Assert.AreEqual("File 'some_filename' has been updated", result.Value)
