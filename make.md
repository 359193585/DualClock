
To publish the DualClock application for Linux ARM64, you can use the following command in your terminal:
```bash
dotnet publish -c Release -r linux-arm64 --self-contained true /p:PublishSingleFile=true
```



This command will create a self-contained single file executable for the Linux ARM64 platform. 
The `-c Release` option specifies that you want to publish the release version of your application, 
while the `-r linux-arm64` option specifies the runtime identifier for Linux ARM64. 
The `--self-contained true` option ensures that all necessary dependencies are included in the published output, 
and `/p:PublishSingleFile=true` creates a single executable file for easier distribution.

After running the command, you can find the published output in the `bin/Release/net8.0/linux-arm64/publish` directory of your project.


```bash
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true
```
This command will create a self-contained single file executable for the Linux x64 platform.
