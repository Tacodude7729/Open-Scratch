# Open Scratch

A simple program which disassmbles and reassembles scratch projects into a more 'open source friendly' project structure. These files shouldn't be edited manually, but should instread be assembled into an SB3, edited, disassembled than pushed. Executables for Windows, OSX and Linux are avalible from the github releases page. Dotnet is required.


## Example Commands

To disassemble an existing SB3 file, use this simple command:

`openscratch disassemble <SB3 Location> <Project Output Location>`

This will create a folder (if it dosn't already exist) at output location and place the project files in there.
The command to reassemble is just as simple:

`openscratch assemble <Project Input Location> <SB3 Location>`

This will read the project in from `Project Input Location` and create an SB3 file which can be edited, or uploaded to scratch.