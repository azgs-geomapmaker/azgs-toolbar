using System;
using System.Collections.Generic;
using ncgmpToolbar.Utilities;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ncgmpToolbar.Utilities.DataAccess;
using System.Linq;
using stdole;

namespace ncgmpToolbar
{
    public class drawMapUnits
    {
        private static string FixLegendTextCharacters(string inputText)
        {
            string theOutput = inputText.Replace("&", "&amp;");
            theOutput = inputText.Replace("<", "&lt;");

            return theOutput;
        }

        public static void DrawStratCorDiagram(IEnvelope Envelope)
        {
            double Text2BoxY = 0.2; //Y distance between the bottom of text and the next box
            double Text2BoxX = 0.1; //X distance between a box and the text that describes it
            double ColumnX = 0.1; //Space between columns
            DrawMapUnits(Envelope, false, ColumnX);
        }

        public static void DrawLegend(IEnvelope Envelope)
        {
            double ColumnX = 0.5; //Space between columns
            DrawMapUnits(Envelope, true, ColumnX);
        }

        private static void DrawMapUnits(IEnvelope Envelope, bool showText, double ColumnX) 
        {
            // BOX DIMENSIONS AND UNIFORM SYMBOL ITEMS
            double Text2BoxY = 0.2; //Y distance between the bottom of text and the next box
            double Text2BoxX = 0.1; //X distance between a box and the text that describes it
            double BoxX = 0.4; //Width
            double BoxY = 0.3; //Height

            // Setup a black color object, black outline
            IRgbColor BlackInsides = new ESRI.ArcGIS.Display.RgbColorClass();
            BlackInsides.Blue = 0;
            BlackInsides.Red = 0;
            BlackInsides.Green = 0;

            ILineSymbol BlackOutsides = new SimpleLineSymbolClass();
            BlackOutsides.Width = 1;
            BlackOutsides.Color = BlackInsides;

            // Whole bunch of variables to use while going through the loop below...
            #region Variables Galore!!!
            IMxDocument Doc = ArcMap.Document;
            IPageLayout pageLayout = Doc.ActiveView as IPageLayout;
            IGraphicsContainer GraphicsContainer = pageLayout as IGraphicsContainer;
            double Xcoord, Ycoord;
            Xcoord = Envelope.XMin;
            Ycoord = Envelope.YMax;
            double IndentTerm = 0;
            IPoint Point = null;
            double StringLength = 0;
            string LegendText = "";
            string ItemName = "";
            string ItemDesc = "";
            IElement Ele = null;
            IEnvelope TempEnv = null;
            IRgbColor BoxColr = null;
            ISimpleFillSymbol FillSym = new ESRI.ArcGIS.Display.SimpleFillSymbolClass();
            IFillShapeElement FillEle = null;
            IEnvelope FillEnv = new EnvelopeClass();
            WKSEnvelope Patch = new WKSEnvelope();
            IGeometry Geo = null;
            string LabelText = "";
            ESRI.ArcGIS.Geometry.IPoint LabelPoint = new ESRI.ArcGIS.Geometry.PointClass();

            // Get the transparency of the MapUnitPolys Layer
            double transparency = 100;
            try
            {
                IFeatureLayer polyLayer = commonFunctions.FindFeatureLayer(ArcMap.Editor.EditWorkspace, "MapUnitPolys");
                ILayerEffects layerEffects = polyLayer as ILayerEffects;
                transparency = layerEffects.Transparency;
            }
            catch { }

            #endregion

            // Get a reference to the DescriptionOfMapUnits entries
            var sortedDmuEntries = GetDmuSortedByHierarchy();

            // Loop through legend records
            foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> aDictionaryEntry in sortedDmuEntries)
            {

                // Grab the DMU entry itself
                DescriptionOfMapUnitsAccess.DescriptionOfMapUnit aDescription = aDictionaryEntry.Value;
                bool isHeading = (aDescription.ParagraphStyle.Contains("Heading"));
                

                    // Find out how far to indent the legend item
                    // Strip periods from the HierarchyKey, divide by 4, which is the length of a segment of the key
                    IndentTerm = aDescription.HierarchyKey.Replace(".", "").Length / 4;

                    // Get the coordinates of the text for the legend entry - upper left corner
                    // Xcoord starts at Envelope.XMin, Ycoord is Envelope.YMax: Upper left corner
                    Point = new PointClass();
                    double xAdditions = 0;
                    if (isHeading)
                    {
                        // Xcoord plus (indentation), Ycoord
                        xAdditions = 0.2 * (IndentTerm - 1);
                    }
                    else
                    {
                        //Xcoord plus (indentation) + (Box width and margin), Ycoord
                        xAdditions = 0.2 * (IndentTerm - 1) + BoxX + Text2BoxX;
                    }
                    Point.PutCoords(Xcoord + xAdditions, Ycoord);

                    // StringLength is the width remaining in the envelope in which the text has to fit IN PIXELS.
                    StringLength = 72 * (Envelope.Width - xAdditions);

                    // Fix a couple of special characters in the legend string.
                    // Then amalgamate item name and description
                    ItemName = FixLegendTextCharacters(aDescription.Name);
                    if (!isHeading)
                    {
                        LegendText = ItemName + " - " + FixLegendTextCharacters(aDescription.Description);
                    }
                    else
                    {
                        LegendText = ItemName;
                    }



                    // Format the legend text if it is not a heading. If it is, we're fine.
                    if (!isHeading)
                    {
                        LegendText = GetFormattedString(LegendText, "Arial", 8, StringLength, 8);
                    }

                    // Boldify the ItemName
                    LegendText = LegendText.Replace(ItemName, "<bol>" + ItemName + "</bol>");

                    // If the StratCorDiagram is being drawn
                    if (showText == false)
                    {
                        LegendText = ".";   // placeholder
                        StringLength = 1;
                    }

                    // See if this legend item should be placed on a new column
                    Ele = MakeTextElement(Point, LegendText, "Arial") as IElement;
                
                    TempEnv = new EnvelopeClass();
                    Ele.QueryBounds(Doc.ActiveView.ScreenDisplay, TempEnv);

                    // If the height of the formatted text is larger than the box + space specified
                    if (TempEnv.Height > BoxY + Text2BoxY)  
                    {
                        // If the text will spill out below the envelope drawn by the user
                        if (Ycoord - TempEnv.Height < Envelope.YMin) 
                        {
                            // Move to a new column - the last number is a fudge factor, looks like it is in inches
                            Xcoord = Xcoord + BoxX + Text2BoxX + StringLength / 72 + ColumnX;
                            // Move to the top
                            Ycoord = Envelope.YMax;
                            // Recreate the text element
                            Point.PutCoords(Xcoord + 0.2 * (IndentTerm - 1) + BoxX + Text2BoxX, Ycoord);
                            Ele = MakeTextElement(Point, LegendText, "Arial") as IElement;
                        }
                    }
                    else // The height of the formatted text is not larger than the box + space defined
                    {
                        // If the box itself will spill out below the envelope drawn by the user
                        if (Ycoord - (BoxY + Text2BoxY) < Envelope.YMin) 
                        {
                            // Move to a new column
                            Xcoord = Xcoord + BoxX + Text2BoxX + StringLength / 72 + ColumnX;
                            // Move to the top
                            Ycoord = Envelope.YMax;
                            // Recreate the text element
                            Point.PutCoords(Xcoord + 0.2 * (IndentTerm - 1) + BoxX + Text2BoxX, Ycoord);
                            Ele = MakeTextElement(Point, LegendText, "Arial") as IElement;
                        }
                    }

                    // Only write the legend text if the legend is being drawn (not the StratCorDiagram)
                    if (showText == true)
                        GraphicsContainer.AddElement(Ele, 0);

                if (!isHeading) 
                {
                    FillEnv = new EnvelopeClass();

                    Patch.XMin = Point.X - BoxX - Text2BoxX;
                    Patch.YMax =  Point.Y;
                    Patch.XMax = Point.X - Text2BoxX;
                    Patch.YMin = Point.Y - BoxY;
                    FillEnv.PutCoords(Patch.XMin, Patch.YMin, Patch.XMax, Patch.YMax);
                    Geo = FillEnv as IGeometry;

                    // Get the color of the box
                    BoxColr = new RgbColorClass();
                    if (aDescription.AreaFillRGB == null) 
                    {
                        BoxColr.Red = 255;
                        BoxColr.Green = 0;
                        BoxColr.Blue = 0;
                    }
                    else
                    {
                        BoxColr.Red = int.Parse(aDescription.AreaFillRGB.Split(';')[0]);
                        BoxColr.Green = int.Parse(aDescription.AreaFillRGB.Split(';')[1]);
                        BoxColr.Blue = int.Parse(aDescription.AreaFillRGB.Split(';')[2]);
                    }

                    // Set the transparency for the legend color boxes
                    BoxColr.Red = (int)((255 - BoxColr.Red) * transparency/100 + BoxColr.Red);
                    BoxColr.Green = (int)((255 - BoxColr.Green) * transparency / 100 + BoxColr.Green);
                    BoxColr.Blue = (int)((255 - BoxColr.Blue) * transparency / 100 + BoxColr.Blue);

                    // Draw the fill
                    FillSym = new SimpleFillSymbolClass();
                    FillSym.Color = BoxColr;
                    FillSym.Style = esriSimpleFillStyle.esriSFSSolid;
                    FillSym.Outline = BlackOutsides;

                    FillEle = CreateFillElement(Geo, FillSym) as IFillShapeElement;

                    // Label the box
                    LabelText = aDescription.Label;

                    // Subscripting!!
                    
                    for (int i = 0; i < LabelText.Length; i++)
                    {
                        string thisBit = LabelText.Substring(i, 1);
                        int num;
                        if (int.TryParse(thisBit, out num)) // Checks if the character is numeric
                        {
                            LabelText = LabelText.Replace(thisBit, "<sub>" + thisBit + "</sub>");
                            i = i + 5;
                        }                   
                    }
                    LabelText = LabelText.Replace("ir", "i<sub>r</sub>");
                    LabelText = LabelText.Replace("yc", "y<sub>c</sub>");

                    // Center the label
                    LabelPoint = new PointClass();
                    LabelPoint.X = Point.X - BoxX / 2 - Text2BoxX;
                    LabelPoint.Y = Point.Y - BoxY / 2;

                    //LabelText = GetFormattedString(LabelText, "FGDCGeoAge", 8, StringLength, 0);
                    Ele = MakeTextElement(LabelPoint, LabelText, "FGDCGeoAge", true) as IElement;

                    // Add the box and label
                    IGroupElement3 group = new GroupElementClass();
                    group.AddElement(FillEle as IElement);
                    group.AddElement(Ele);
                    GraphicsContainer.AddElement(group as IElement, 0);
                }

                // Do a partial refresh
                //Doc.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                // Setup the Y coordinates for the next entry
                // if the height of this item's text is bigger than the minimum distance between text and box
                if (TempEnv.Height > Text2BoxY)
                {
                    // Subtract the box height and the text height to get the new ycoord
                    Ycoord = Ycoord - (TempEnv.Height + Text2BoxY);
                }
                else
                {
                    if (isHeading)
                    {
                        Ycoord = Ycoord - (TempEnv.Height + Text2BoxY);
                    }
                    else
                    {
                        Ycoord = Ycoord - (BoxY + Text2BoxY);
                    }
                }
            }

            // Done, refresh, turn off the tool
            Doc.ActiveView.Refresh();
            ArcMap.Application.CurrentTool = null;
        }

        private static string GetFormattedString(string strTextContent, string strFontName, Single sngFontSizeInPoints, double sngMaxWidthInPoints, long hanging)
        {
            System.Array varWordArray;
            IMxApplication pMxApp = ArcMap.Application as IMxApplication;
            IAppDisplay pAppDisplay = pMxApp.Display;
            IDisplayTransformation pTransformation = pAppDisplay.DisplayTransformation;
            IFontDisp pTextFont = new stdole.StdFontClass() as IFontDisp;
            ITextSymbol pTextSymbol = new TextSymbolClass();
            double dblXSize;
            double dblYSize;
            string strGoodWidth = "";
            string strFinalString = "";
            string strTestString = "";
            int i;

            // Split the string into an array of words
            varWordArray = strTextContent.Split(' ') as System.Array;
            
            // Set up the Font
            pTextFont.Name = strFontName;
            pTextFont.Size = decimal.Parse(sngFontSizeInPoints.ToString());

            // Setup the Text Symbol
            pTextSymbol.Font = pTextFont;

            // Setup spacing string for hanging indent
            int pSpaces, i3;
            string hangingIndent;
            pAppDisplay.StartDrawing(pAppDisplay.hDC, 0);
            hangingIndent = ""; //minimum hanging indent
            if (hanging > 0)
            {
                pTextSymbol.GetTextSize(pAppDisplay.hDC, pTransformation, " ", out dblXSize, out dblYSize);
                pSpaces = (int)System.Math.Round(hanging / dblXSize);
                for (i3 = 0; i3 == pSpaces; i3++)
                {
                    hangingIndent = hangingIndent + " ";
                }
            } // do nothing if hanging is 0
            pAppDisplay.FinishDrawing(); // done setting up the hanging indent

            // Add each word into the test string and test for width
            pAppDisplay.StartDrawing(pAppDisplay.hDC, 0);
            long linesAdded = 0;

            for (i = 0; i <= (int)varWordArray.GetUpperBound(0); i++)
            {
                if (strGoodWidth != "")
                {
                    strTestString = strGoodWidth + " " + varWordArray.GetValue(i);
                }
                else
                {
                    strTestString = varWordArray.GetValue(i).ToString();
                }

                // Get the TextSize
                if (linesAdded == 0)
                {
                    pTextSymbol.GetTextSize(pAppDisplay.hDC, pTransformation, strTestString, out dblXSize, out dblYSize);
                }
                else
                {
                    pTextSymbol.GetTextSize(pAppDisplay.hDC, pTransformation, "    " + strTestString, out dblXSize, out dblYSize);
                }

                // If the word added is < max width keep adding to the line, else make a new one
                if (dblXSize < sngMaxWidthInPoints)
                {
                    strGoodWidth = strTestString;
                }
                else
                {
                    if (linesAdded == 0)
                    {
                        strFinalString = hangingIndent + strGoodWidth;
                    }
                    else
                    {
                        strFinalString = strFinalString + Environment.NewLine + hangingIndent + "    " + strGoodWidth;
                    }
                    linesAdded = linesAdded + 1;
                    strGoodWidth = varWordArray.GetValue(i).ToString();
                }
            }

            // Don't indent if there is only one line
            if (strGoodWidth == strTextContent)
                strFinalString = strFinalString + Environment.NewLine + strGoodWidth;
            else
                strFinalString = strFinalString + Environment.NewLine + hangingIndent + "    " + strGoodWidth;
            pAppDisplay.FinishDrawing();

            //return strFinalString.Substring(2);
            return strFinalString;
        }

        private static ITextElement MakeTextElement(IPoint pPoint, string strText, string sFontName, bool Centered = false, long FontSize = 8)
        {
            // Setup a color
            IRgbColor pRGBColor = new RgbColorClass();
            pRGBColor.Blue = 0;
            pRGBColor.Red = 0;
            pRGBColor.Green = 0;

            // Set text element at the right place
            ITextElement pTextElement = new TextElementClass();
            IElement pElement = pTextElement as IElement;
            pElement.Geometry = pPoint;

            // Setup a Font
            IFontDisp pFontDisp = new stdole.StdFontClass() as IFontDisp;
            pFontDisp.Name = sFontName;

            // Setup a TextSymbol that the TextElement will draw with
            ITextSymbol pTextSymbol = new TextSymbolClass();
            pTextSymbol.Font = pFontDisp;
            pTextSymbol.Color = pRGBColor;
            pTextSymbol.Size = FontSize;

            // Center if appropriate
            if (Centered == true)
            {
                pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                pTextSymbol.VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
            }
            else
            {
                pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHALeft;
                pTextSymbol.VerticalAlignment = esriTextVerticalAlignment.esriTVATop;
            }

            // Give the TextSymbol and string to the TextElement
            pTextElement.Symbol = pTextSymbol;
            pTextElement.Text = strText;

            return pTextElement;
        }

        private static IElement CreateFillElement(IGeometry pGeometry, ISimpleFillSymbol pFillSym)
        {
            IFillShapeElement pFillEle = new RectangleElementClass();
            pFillEle.Symbol = pFillSym;
            IElement pElement = pFillEle as IElement;

            pElement.Geometry = pGeometry;

            IScreenDisplay pScrDisp = ArcMap.Document.ActiveView.ScreenDisplay;
            pElement.Activate(pScrDisp);

            pScrDisp.StartDrawing(0, (System.Int16)esriScreenCache.esriNoScreenCache);
            pElement.Draw(pScrDisp, null);
            pScrDisp.FinishDrawing();

            return pElement;
        }

        private static IOrderedEnumerable<KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit>> GetDmuSortedByHierarchy()
        {
            // Get All DescriptionOfMapUnits.
            DescriptionOfMapUnitsAccess DmuAccess;
            DmuAccess = new DescriptionOfMapUnitsAccess(ArcMap.Editor.EditWorkspace);
            DmuAccess.AddDescriptionOfMapUnits();

            // Sort using Linq syntax
            var sortedDmuEntries = (
                from entry in DmuAccess.DescriptionOfMapUnitsDictionary
                orderby ((DescriptionOfMapUnitsAccess.DescriptionOfMapUnit)entry.Value).HierarchyKey ascending
                select entry);

            return sortedDmuEntries;
        }
    }
}
