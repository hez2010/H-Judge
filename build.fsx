open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open System.Diagnostics

#load ".fake/build.fsx/intellisense.fsx"

let findMSBuild = 
    async {
        let info = ProcessStartInfo("vswhere.exe", "-property installationPath -latest")
        info.RedirectStandardOutput <- true
        info.UseShellExecute <- false
        use p = Process.Start info
        let vs = p.StandardOutput.ReadToEndAsync() |> Async.AwaitTask
        p.WaitForExit |> ignore
        let! dir = vs
        let msBuild = dir+"\\MSBuild\\Current\\Bin\\MSBuild.exe"
        return msBuild.Replace("\n", "").Replace("\r", "")
    }

Target.create "Clean" (fun _ ->
    !! "**/Cargo.toml"
    |> Seq.iter(fun project -> 
        let dir = Path.getDirectory project
        Shell.Exec ("cargo", "clean", dir) |> ignore
    )
    !! "**/*.csproj"
    |> Seq.iter (fun project -> 
        let dir = Path.getDirectory project
        Shell.Exec ("dotnet", "clean", dir) |> ignore
    )
    if (Environment.isWindows) then
        async {
            let! msbuild = findMSBuild
            let dir = Path.getDirectory msbuild
            !! "**/hjudge*/*.vcxproj"
            |> Seq.iter(fun project ->
                Shell.Exec (msbuild, "/m /v:m /t:Clean "+project, dir) |> ignore
            )
        } |> Async.RunSynchronously
)

Target.create "Build" (fun _ ->
    !! "**/Cargo.toml"
    |> Seq.iter(fun project -> 
        let dir = Path.getDirectory project
        Shell.Exec ("cargo", "build --release", dir) |> ignore
        let outputDir = dir+"/target/release";
        for x in System.IO.Directory.GetFiles outputDir do
            let sb = System.Text.StringBuilder()
            let fileName = System.IO.Path.GetFileName x
            for i in fileName do 
                match i with
                | '_' -> sb.Append '.' |> ignore
                | _ -> sb.Append i |> ignore
            let targetFileName = Path.combine outputDir (sb.ToString())
            System.IO.File.Move(x, targetFileName)
    )
    !! "**/*.csproj"
    |> Seq.iter (DotNet.publish id)
    if (Environment.isWindows) then
        async {
            let! msbuild = findMSBuild
            let dir = Path.getDirectory msbuild
            !! "**/hjudge*/*.vcxproj"
            |> Seq.iter(fun project ->
                Shell.Exec (msbuild, "/m /v:m /p:Configuration=Release /p:Platform=x64 "+project, dir) |> ignore
            )
        } |> Async.RunSynchronously
)

Target.create "Test" (fun _ -> 
    !! "**/*.Test.csproj"
    |> Seq.iter (DotNet.test id)
)

Target.create "All" ignore

"Clean" ==> "Build" ==> "All"

Target.runOrDefault "All"
