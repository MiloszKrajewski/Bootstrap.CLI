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

    [<Test>]
    member x.ReadmeArgumentParser() =
        let framesPerSecond = ref None
        let autoCrop = ref false
        let files = ref []

        ArgumentParser.createArgumentParser ()
        |> ArgumentParser.onOption "fps" "frames per second" (fun s -> 
            framesPerSecond.Value <- s |> int |> Some)
        |> ArgumentParser.onSwitch "auto-crop" "turn on auto crop feature" (fun () ->
            autoCrop.Value <- true)
        |> ArgumentParser.onString (fun s ->
            files.Value <- s :: files.Value)
        |> ArgumentParser.parseArguments [ "--fps"; "25"; "--auto-crop"; "video1.avi"; "video2.avi" ]

        Assert.IsTrue (framesPerSecond.Value = Some 25)
        Assert.IsTrue (autoCrop.Value)
        Assert.IsTrue (files.Value = ["video2.avi"; "video1.avi"])

