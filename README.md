# COMP 430 NETTastic

[![Build Status](https://dev.azure.com/thadhouse/COMP430/_apis/build/status/ThadHouse.COMP430?branchName=master)](https://dev.azure.com/thadhouse/COMP430/_build/latest?definitionId=17&branchName=master)


## Compiling the compiler
0. Install .NET Core 3.1 at minimum. https://dotnet.microsoft.com/download/dotnet-core/3.1
1. Clone repo
2. CD into CompilerEXE directory
3. dotnet build

## Running the compiler
0. Follow Compile Steps, stay in CompilerEXE directory
1. dotnet run Program.net
2. The output file will be an executable placed in the CompilerEXE directory.

Additional options can be passed after `run`.
1. Multiple files can be passed to compile. Any extension that is not .dll or .exe is accepted as a file to attempt to compile.
2. By default, the program name will equal the name of the first compilation file passed. This can be changed with `--program-name <program-name>`
3. Libraries can be added to be linked to by passing them as normal files, except they must have a .dll or .exe extension.

## Running the compiled program
1. If on windows, the executable can just be ran from command line by running the compiled exe name.
2. If on Linux or Mac, the latest version of Mono 6 is required to run the executable. .NET Core cannot run the outputted exe. Then, run `mono Program.exe`
