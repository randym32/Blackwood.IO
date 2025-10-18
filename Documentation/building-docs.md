# Blackwood.IO Documentation

This directory contains the documentation project for the Blackwood.IO library.

## Prerequisites

- .NET 8.0 SDK or later
- DocFX (will be installed automatically by the build scripts)

## Building the Documentation

### Option 1: Using Visual Studio

To build the documentation using Visual Studio:

1. Open the `Blackwood.IO.sln` solution file in Visual Studio.
2. In the Solution Explorer, locate the `Documentation` project.
3. Right-click the `Documentation` project and select **Build**.
4. Upon successful build, the documentation output will be generated in the `../docs` directory.



### Option 2: Using the Batch Script (Windows)

To build and view the documentation locally:

1. Navigate to the `Documentation` folder
2. Run the build script:
   ```cmd
   .\build-docs.bat
   ```


### Option 3: Using PowerShell Script (Windows)

To build and view the documentation locally:

1. Navigate to the `Documentation` folder
2. Run the build script:

```powershell
.\build-docs.ps1
```

### Option 4: Manual Build
1. Install DocFX globally:
   ```cmd
   dotnet tool install -g docfx
   ```

2. Generate the documentation:
   ```cmd
   docfx docfx.json
   ```

## Output

The generated documentation will be available in the `../docs` directory.
Open `../docs/index.html` in your web browser to view the documentation.

When Visual Studio builds the docs, it will also start a local webserver to allow
browsing the documents at http://localhost:8080

The documentation, when browsed from a local filesystem, may require tweaking
the browser.

### Firefox
Go to `about:config` and set:
- `security.fileuri.strict_origin_policy` to `false`
- `privacy.file_unique_origin` to `false`


### Chrome
Use a local web server extension like "Web Server for Chrome" or serve the files through a development server.

### Brave

When view the local documentation files with the Brave browser, additional steps
are needed. You will have to launch Brave from the command line with specific
flags to relax its restrictions.  This will not full work, but it allows reading.

1. **Open your command prompt or terminal**.
2. Launch Brave with the following flags, replacing `<path-to-your-index.html>` with the full path to your documentation file:

macOS:

   ```bash
   open -n "/Applications/Brave Browser.app" --args --user-data-dir="$HOME/brave-dev-data" --disable-web-security "<path-to-your-index.html>"
   ```

Windows:

   ```bash
   "C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe" --user-data-dir=C:\brave-dev-data --disable-web-security "<path-to-your-index.html>"
   ```

The following flags are important:

   - `--allow-file-access-from-files` enables local file access for the browser.
   - `--disable-web-security` disables certain browser security measures (only use this for trusted, local documentation).
   - Make sure to specify the exact path to your documentation index file.



## Project Structure

- `docfx.json` - DocFX configuration file
- `index.md` - Main documentation page
- `api/` - API documentation files
- `articles/` - Conceptual documentation
- `template/` - Custom DocFX template files
- `contributing/` - Contributing guidelines

## Configuration

The `docfx.json` file contains the configuration for:
- Source code metadata extraction from `../src`
- Content files (markdown, API docs)
- Output destination (`../docs`)
- Template and styling options

`docfx` is fussy, brittle, and its `docfx.json` documentation is often not
helpful for either setting up the project or figuring out why its not working.
(Sure there is documentation, but its thin on substance, little on what the
actual settings do, or how to accomplish anything.)

## Updating Documentation

1. Modify the markdown files in this directory
2. Update API documentation by adding XML documentation comments to the source code
3. Run the build script to regenerate the documentation
4. The updated documentation will be available in `../docs`
