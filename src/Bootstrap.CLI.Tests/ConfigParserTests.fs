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
        let v1 = ConfigParser.getValue "sectionA" "key1" "xxx" cfg
        Assert.AreEqual("value1", v1)


