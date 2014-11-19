#AZGS NCGMP Toolbar

This toolbar was developed by the Arizona Geological Survey (AZGS) for use in geologic map production using the NCGMP09 database schema and ESRI ArcGIS software. It is an ArcGIS AddIn for ArcMap. The AddIn presents the user with a toolbar that the geologist can use to perform various geologic map compilation functions.

###NCGMP Toolbar Functions 
**Opening an existing NCGMP09 database**: The AddIn will only open a geologic map database created with one of the create database tools in the Geologic Mapping Toolset ArcGIS Toolbox. 

**Creating and Managing Data Sources**: The toolbar provides a simple window for creating, managing and selecting data sources for individual or groups of features. 

**Creating and Managing Description Of Map Units**: The Map Unit Legend Editor, accessible from the NCGMP Menu dropdown, provides a window that allows users to define new units, 
edit existing ones, adjust the ordering and hierarchy of the legend, and to indicate which
polygons on the map depict a particular unit. The information entered into this form is written to the NCGMP database’s DescriptionOfMapUnits table.

**Drawing the map unit legend onto an ArcMap layout**: When ArcMap is in layout mode, the 
toolbar allows a user to draw the contents of the DescriptionOfMapUnits table as a set of 
graphical elements on the layout. These graphical elements include a color patch, map unit abbreviation, map unit name, display age and map unit description.

**Provides symbols for common geologic features**: When a user begins editing geologic map
data in ArcMap, the toolbar creates feature templates for many common geologic point and 
line measurements, such as contacts, thrust faults, and bedding measurements. Cartographic Representations are used by default.

**pdating feature identifiers and Data Source identifiers as edits are made**: An editor 
extension automatically manages various primary key fields defined in the NCGMP09 schema. 
As users create new features these fields are automatically populated. Similarly, users can select an "Active Data Source" record, and that Data Source will be automatically attached to each new feature.

**Digitizing structural measurements from scanned geologic maps**: After scanning and georeferencing an existing geologic map, the user can use a tool to digitize measurements from the map. The user clicks three times for each feature to be digitized. First, the user clicks the location of the measurement. Next, the user clicks the two endpoints of the strike line for that measurement. The tool then prompts the user to enter the dip value from the measurement.

**Please also see the AZGS offical [Publication](http://repository.azgs.az.gov/uri_gin/azgs/dlio/1564) about this toolbar for more details about its use.**

### Installation
1. Find the file **ncgmpToolbar.esriAddIn** in the **/bin/Debug** folder. (If using GitHub open the file and click the **Raw** button to download only this file.)
2. Save ncgmpToolbar.esriAddIn to your computer in a location of your choosing.
3. Double-click NCGMPToolbar.esriAddIn
4. Click **Install Add-In**
5. In ArcMap, click **Customize**
6. Select **Customize Mode…**
7. Click **Add from file…**
8. Navigate to and select **NcgmpToolbar.esriAddIn**
9. Click **Open**
10. On Toolbars tab, check the box next to NCGMP Toolbar then close Customize window 

### Development Setup
Prerequisties: **ArcObjects SDK** (from the ArcGIS installation disc) and **Visual Studio**

1. Clone this repository
2. Open the project by double-clicking **ncgmpToolbar.sln**
3. Configure a Debug session to open ArcMap automatically
 - In the **Solution Explorer** panel right click ncgmpToolbar
 - Click **Properties**
 - Under **Debug**, select **Start Action**, **Start external program**
 - Browse to the **ArcMap** executable
4. Run **Build**, **Clean Solution** (This will clear the **/bin/Debug** folder)
5. Click **Debug**, **Start Debugging** (This will build the **NCGMPToolbar.esriAddIn** in the **/bin/Debug** folder)
6. Error logs are written to `C:\Users\<user name>\AppData\Local\Temp\NCGMPToolbarLog.txt`

### Prior Toolbar Version
The original AZGS NCGMP Toolbar used an AZGS-modified version of the NCGMP schema. This toolbar is available by selecting the Tag **v0.9.9.2**.