module Fantomas.Tests.StringTests

open NUnit.Framework
open FsUnit
open Fantomas.Tests.TestHelper

[<Test>]
let ``triple-quoted strings`` () =
    formatSourceString
        false
        "let xmlFragment2 = \"\"\"<book author=\"Milton, John\" title=\"Paradise Lost\">\"\"\""
        { config with MaxValueBindingWidth = 80 }
    |> should
        equal
        "let xmlFragment2 = \"\"\"<book author=\"Milton, John\" title=\"Paradise Lost\">\"\"\"
"

[<Test>]
let ``string literals`` () =
    formatSourceString
        false
        """
let xmlFragment1 = @"<book author=""Milton, John"" title=""Paradise Lost"">"
let str1 = "abc"
    """
        { config with MaxValueBindingWidth = 60 }
    |> prepend newline
    |> should
        equal
        """
let xmlFragment1 =
    @"<book author=""Milton, John"" title=""Paradise Lost"">"

let str1 = "abc"
"""

[<Test>]
let ``multiline strings`` () =
    formatSourceString
        false
        """
let alu =
        "GGCCGGGCGCGGTGGCTCACGCCTGTAATCCCAGCACTTTGG\
        GAGGCCGAGGCGGGCGGATCACCTGAGGTCAGGAGTTCGAGA\
        CCAGCCTGGCCAACATGGTGAAACCCCGTCTCTACTAAAAAT\
        ACAAAAATTAGCCGGGCGTGGTGGCGCGCGCCTGTAATCCCA\
        GCTACTCGGGAGGCTGAGGCAGGAGAATCGCTTGAACCCGGG\
        AGGCGGAGGTTGCAGTGAGCCGAGATCGCGCCACTGCACTCC\
  AGCCTGGGCGACAGAGCGAGACTCCGTCTCAAAAA"B
    """
        config
    |> prepend newline
    |> should
        equal
        """
let alu =
    "GGCCGGGCGCGGTGGCTCACGCCTGTAATCCCAGCACTTTGG\
        GAGGCCGAGGCGGGCGGATCACCTGAGGTCAGGAGTTCGAGA\
        CCAGCCTGGCCAACATGGTGAAACCCCGTCTCTACTAAAAAT\
        ACAAAAATTAGCCGGGCGTGGTGGCGCGCGCCTGTAATCCCA\
        GCTACTCGGGAGGCTGAGGCAGGAGAATCGCTTGAACCCGGG\
        AGGCGGAGGTTGCAGTGAGCCGAGATCGCGCCACTGCACTCC\
  AGCCTGGGCGACAGAGCGAGACTCCGTCTCAAAAA"B
"""

[<Test>]
let ``multiline string piped`` () =
    formatSourceString
        false
        """
let f a b =
    a "
" |> b
    """
        config
    |> prepend newline
    |> should
        equal
        """
let f a b =
    a
        "
"
    |> b
"""

[<Test>]
let ``preserve uncommon literals`` () =
    formatSourceString
        false
        """
let a = 0xFFy
let c = 0b0111101us
let d = 0o0777
let e = 1.40e10f
let f = 23.4M
let g = '\n'
    """
        config
    |> prepend newline
    |> should
        equal
        """
let a = 0xFFy
let c = 0b0111101us
let d = 0o0777
let e = 1.40e10f
let f = 23.4M
let g = '\n'
"""

[<Test>]
let ``uncommon literals strict mode`` () =
    formatSourceString
        false
        """
let a = 0xFFy
let c = 0b0111101us
let d = 0o0777
let e = 1.40e10f
let f = 23.4M
let g = '\n'
    """
        { config with StrictMode = true }
    |> prepend newline
    |> should
        equal
        """
let a = -1y
let c = 61us
let d = 511
let e = 1.4e+10f
let f = 23.4M
let g = '\n'
"""

[<Test>]
let ``should preserve triple-quote strings`` () =
    formatSourceString
        false
        "
    type GetList() =
        let switchvox_users_voicemail_getList_response = \"\"\"
            </response>\"\"\"

        let switchvox_users_voicemail_getList = \"\"\"
            </request>\"\"\"

        member self.X = switchvox_users_voicemail_getList_response"
        { config with MaxValueBindingWidth = 120 }
    |> prepend newline
    |> should
        equal
        "
type GetList() =
    let switchvox_users_voicemail_getList_response =
        \"\"\"
            </response>\"\"\"

    let switchvox_users_voicemail_getList =
        \"\"\"
            </request>\"\"\"

    member self.X = switchvox_users_voicemail_getList_response
"

[<Test>]
let ``should keep triple-quote strings`` () =
    formatSourceString
        false
        "
[<EntryPoint>]
let main argv =
    use fun1 = R.eval(R.parse(text = \"\"\"
    function(i) {
        x <- rnorm(1000)
        y <- rnorm(1000)
        m <- lm(y~x)
        m$coefficients[[2]]
    }
    \"\"\"))
    0
"
        config
    |> prepend newline
    |> should
        equal
        "
[<EntryPoint>]
let main argv =
    use fun1 =
        R.eval (
            R.parse (
                text =
                    \"\"\"
    function(i) {
        x <- rnorm(1000)
        y <- rnorm(1000)
        m <- lm(y~x)
        m$coefficients[[2]]
    }
    \"\"\"
            )
        )

    0
"

[<Test>]
let ``chars should be properly escaped`` () =
    formatSourceString false """let private peskyChars = [| '"' ; '\t' ; ' ' ; '\\' |]""" config
    |> should
        equal
        """let private peskyChars = [| '"'; '\t'; ' '; '\\' |]
"""

[<Test>]
let ``quotes should be escaped in strict mode`` () =
    formatSourceString
        false
        """
    let formatter =
        // escape commas left in invalid entries
        sprintf "%i,\"%s\""
"""
        { config with StrictMode = true }
    |> should
        equal
        """let formatter = sprintf "%i,\"%s\""
"""

[<Test>]
let ``empty lines in multi-line string should be preserved, 577`` () =
    formatSourceString
        false
        "
let x = \"\"\"some

content

with empty lines\"\"\"
"
        config
    |> prepend newline
    |> should
        equal
        "
let x =
    \"\"\"some

content

with empty lines\"\"\"
"

[<Test>]
let ``string with newline inside union case, 1056`` () =
    formatSourceString
        false
        """
[<Test>]
let ``newline in string`` () =
    let source = "\"
\""

    let triviaNodes =
        tokenize [] source
        |> getTriviaFromTokens
        |> List.filter (fun { Item = item } -> match item with | StringContent("\"\n\"") -> true  | _ -> false)

    List.length triviaNodes == 1
"""
        config
    |> prepend newline
    |> should
        equal
        """
[<Test>]
let ``newline in string`` () =
    let source =
        "\"
\""

    let triviaNodes =
        tokenize [] source
        |> getTriviaFromTokens
        |> List.filter (fun { Item = item } ->
            match item with
            | StringContent ("\"\n\"") -> true
            | _ -> false)

    List.length triviaNodes == 1
"""

[<Test>]
let ``double backslash in triple quote string`` () =
    formatSourceString
        false
        "
let a = \"\"\"\\\\\"\"\"
"
        config
    |> prepend newline
    |> should
        equal
        "
let a = \"\"\"\\\\\"\"\"
"

[<Test>]
let ``single string with compiler define`` () =
    formatSourceString
        false
        """
"#if FOO"
"""
        config
    |> prepend newline
    |> should
        equal
        """
"#if FOO"
"""

[<Test>]
let ``trailing spaces in string should be preserved, 1941`` () =
    formatSourceString
        false
        "
let s = \"\"\"aaaa   
bbb\"\"\"
"
        config
    |> prepend newline
    |> should
        equal
        "
let s =
    \"\"\"aaaa   
bbb\"\"\"
"
