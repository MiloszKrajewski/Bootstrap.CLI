Bootstrap.CLI
===

Very simple command line parser implementation in F#.

Brief History of Bootstrap.CLI
---
I do like to automate my work. For a very long time I was using Python to do it but as I'm trying to learn F# a little I decided to do some of my scripting in F#.
Most of my scripts are console applications with three elements:
 
* command line argument parsing (argparse)
* logging with color console output (logging and colorama)
* file based human readable configuration (configparser or PyYAML)

I know there are multiple libraries providing those features in .NET but in post cases they look to me like `Factory<Factory<Factory<Hammer>>>` and don't have Python's *elegance by simplicity*.

With Bootstrap.CLI I'm just playing with some ideas covering this 3 areas trying to make it as simple and dump as possible.

For now, only command like argument parsing has been implemented.

ArgumentParser
---
Sometimes all you need is some arguments, for example:

	encode --fps 25 --auto-crop video1.avi video2.avi 

We can identify three types of arguments:

* **switches**: they are just boolean flags, for example: `--auto-crop`
* **options**: they do have a value, for example: `--fps 25`
* **arguments**: just some string, for example: `video.avi` 

ArgumentParser configuration is about providing names of switches and options together with handler function which will be called when switch or option is specified. It's not parser's responsibility to store the parsed data, it just fires callbacks.

* `createArgumentParser`: creates parser
* `onSwitch`: declares handler for a switch (on/off)
* `onOption`: declares handler for an option (with value)
* `onString`: declares handler for arguments (just a string, for example file name)
* `parseArguments`: parsers arguments firing callbacks  

So, previous example can be implemented with:  
 
	open Bootstrap.CLI.ArgumentParser 

    let framesPerSecond = ref None
    let autoCrop = ref false
    let files = ref []

    createArgumentParser ()
    |> onOption "fps" "frames per second" (fun s -> 
        framesPerSecond.Value <- s |> int |> Some)
    |> onSwitch "auto-crop" "turn on auto crop feature" (fun () ->
        autoCrop.Value <- true)
    |> onString (fun s ->
        files.Value <- s :: files.Value)
    |> parseArguments [ "--fps"; "25"; "--auto-crop"; "video1.avi"; "video2.avi" ]

    assert (framesPerSecond.Value = Some 25)
    assert (autoCrop.Value)
    assert (files.Value = ["video2.avi"; "video1.avi"])

CommandParser
---
Some applications have many distinct commands (like `git`) and they use command pattern where each command have different set of options, for example:

	mediatool --version
	mediatool encode video.avi --fps 25 --auto-crop
	mediatool offset video.avi --audio 750

CommandParser configuration is about providing names of commands together with handler function which will be called when command is specified. And again, it's not parser's responsibility to store the parsed data, it just fires callbacks.

* `createCommandParser`: creates parser
* `onDefaultCommand`: declares a handler used when no command is specified
* `onCommand`: declares a handler for command

So, previous example can be implemented with:

	open Bootstrap.CLI.ArgumentParser 
	open Bootstrap.CLI.CommandParser 

    createCommandParser ()
	|> onDefaultCommand (fun args ->
		createArgumentParser ()
		|> onSwitch "version" "prints application version" (fun () ->
			printfn "mediatool, version 12.3.4.3555")
		|> parseArguments args)
	|> onCommand "encode" "encodes multiple video files" (fun args ->
	    createArgumentParser ()
	    |> onOption "fps" "frames per second" (fun s -> (/* ... */))
	    |> onSwitch "auto-crop" "turn on auto crop feature" (fun () -> (/* ... */))
	    |> onString (fun s -> (/* ... */))
	    |> parseArguments args)
	|> onCommand "offset" "offsets video or audio stream" (fun args ->
	    createArgumentParser ()
	    |> onOption "audio" "audio offset" (fun s -> (/* ... */))
	    |> onString (fun s -> (/* ... */))
	    |> parseArguments args)
