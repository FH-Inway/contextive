#r "nuget: Fun.Build, 1.0.5"

open Fun.Build

let args =
    {| dotnetRuntime =
        CmdArg.Create(
            "-r",
            "--runtime",
            "Dotnet Runtime for Publishing, see https://learn.microsoft.com/en-us/dotnet/core/rid-catalog#known-rids",
            isOptional = true
        )
       dotnetVersion =
        EnvArg.Create(
            "DOTNET_VERSION",
            "Version of the DotNet SDK",
            isOptional = true

        )
       os = EnvArg.Create("RUNNER_OS", "Operating System", isOptional = true)
       event = EnvArg.Create("GITHUB_EVENT_NAME", isOptional = true)
       ref = EnvArg.Create("GITHUB_REF", isOptional = true)
       repo = EnvArg.Create("GITHUB_REPOSITORY", isOptional = true) |}

type Component = { Name: string; Path: string }

let ifTopLevelStage fn (ctx: Internal.StageContext) =
    match ctx.ParentContext with
    | ValueSome(Internal.StageParent.Pipeline _) -> fn ctx
    | _ -> ()

let echoGitHubGroupStart (ctx: Internal.StageContext) = printfn "::group::%s" ctx.Name
let echoGitHubGroupEnd (ctx: Internal.StageContext) = printfn "::endgroup::"

open StageContextExtensions

let gitHubGroupStart = ifTopLevelStage <| echoGitHubGroupStart

let gitHubGroupEnd = ifTopLevelStage <| echoGitHubGroupEnd

let installTool tool =
    stage $"Install {tool}" { run $"dotnet tool install {tool} --global" }

let env (envArg: EnvArg) =
    System.Environment.GetEnvironmentVariable(envArg.Name)

let logEnvironment =
    stage "Log Environment" {
        run "dotnet --version"
        whenEnvVar args.event
        whenEnvVar args.os
        whenEnvVar args.ref
        whenEnvVar args.repo

        echo
            $"""
🎉 The job was automatically triggered by a {env args.event} event.
🐧 This job is now running on a {env args.os} server hosted by GitHub!
🔎 The name of your branch is {env args.ref} and your repository is {env args.repo}."""
    }
