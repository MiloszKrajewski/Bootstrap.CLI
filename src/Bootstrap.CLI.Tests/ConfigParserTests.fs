namespace Bootstrap.CLI.Tests

open NUnit.Framework
open Bootstrap.CLI

[<TestFixture>]
type ConfigParserTests() =
    [<Test>]
    member x.ReadConfigLines() =
        let ini = @"""
            [sectionA]
            key1=value1

            [sectionB]
            key2=value2
        """
        let cfg = ini.Split('\n') |> ConfigParser.readConfigLines
        let v1 = cfg |> ConfigParser.getValue "sectionA" "key1"
        let v2 = cfg |> ConfigParser.getValueOrDefault "sectionB" "key3" "xxx"
        let v3 = cfg |> ConfigParser.tryGetValue "sectionB" "key2"
        let v4 = cfg |> ConfigParser.tryGetValue "sectionX" "keyX"
        Assert.AreEqual("value1", v1)
        Assert.AreEqual("xxx", v2)
        Assert.IsTrue((v3 = Some "value2"))
        Assert.IsTrue((v4 = None))