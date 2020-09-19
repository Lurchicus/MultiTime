using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MultiTime
{
    /// <summary>
    /// Dynamicly display multiple DST/ST zone adjusted times where the clock 
    /// names and UTC offsets are defined in a json file
    /// </summary>
    /// 
    /// Change history:
    /// 12/07/2018 Dan Rhea - Created the new version using c#
    /// 12/07/2018 Dan Rhea - Added a frame for UTC
    /// 12/07/2018 Dan Rhea - Remapped the background colors in the read only
    ///                       text boxes to be more pleasing to the eye
    /// 12/09/2018 Dan Rhea - Reworked timing offsets to base off of UTC
    ///                       instead of Eastern
    ///                     - Moved the Puerto Rico row from the end of the
    ///                       list to between UTC (top) and Eastern
    ///                     - Made sure we update the offset labels for all 
    ///                       times
    ///                     - Remapped the colors again since I shifted things
    ///                     - Created a global bool for DST so I'm not always
    ///                       calling the DateTime IsDaylightSavingTime method
    ///                     - Wired up a timer event to do screen updates...
    ///                       doesn't work any better just more appropriate
    ///                     - Brute force fixed a memory leak caused by the
    ///                       timer tick event by counting ticks and forcing a
    ///                       garbage collect every 60 seconds
    ///                     - Pull clock information from the MultiTime.json
    ///                       file
    /// 12/11/2018 Dan Rhea - Dynamily create a row for each clock definition 
    ///                       loaded
    ///                     - Moved the colors to a color map class that I
    ///                       can cycle through when creating UI elements
    /// 12/15/2018 Dan Rhea - Moved the color definitions into the json file
    ///                       MultiColor.json that is loaded into the list
    ///                       ColorMap via the RGB class
    /// 04/03/2019 Dan Rhea - Pushed to GitHub
    ///                     - Noticed the DST/ST logic may be reversed ><. So
    ///                       it wasn't reversed, I did add a DT/DST for each
    ///                       clock anyway.
    /// 08/08/202 Dan Rhea - Was poking around in here and discovered what 
    ///                      the DST bug is. I check for DST with a UTC time
    ///                      so it always claims standard time. I need to
    ///                      rethink how I check for DST to it applies or 
    ///                      not to each seperate clock. Noodling this...
    ///                      Fix was simple, check for DST here (Eastern) as
    ///                      all the other offsets will work with this.
    /// 
    /// ToDo:
    /// 
    public partial class MultiTime : Form
    {
        public bool IsDST { get; set; } = false;                    // Daylight savings time in effect or not
        public bool Running { get; set; } = false;                  // The clocks are running or not
        public int Ticks { get; set; } = 0;                         // How many seconds have elapsed
        public bool HellFrozeOver { get; set; } = false;            // Used to indicate we want to stop the clocks and exit
        public int DelayTime { get; set; } = WaitInSeconds * 1000;  // Milliseconds to wait between clock updates
        public static int WaitInSeconds { get; set; } = 1;          // Seconds to wait between clock updates
        public static bool ClockSet { get; set; } = false;          // Has clock been set the first time or not?

        public List<TimeZones> zones;                       // List of clock info, and UTC offsets
        public List<Label> Lab = new List<Label>();         // List of clock row labels
        public List<TextBox> Txt = new List<TextBox>();     // List of clock row text boxes
        public List<Label> LabOffset = new List<Label>();   // List of clock row UTC offset labels
        public List<RGB> ColorMap = new List<RGB>();        // List of colors for text box background

        const int MarginT = 5;          // Top margin for a TextBox
        const int MarginL = 9;          // Top margin for a Label
        const int LabLeft = 19;         // Clock label size and position
        const int LabTop = 15;
        const int LabHeight = 13;
        const int LabWidth = 80;
        const int TxtLeft = 103;        // Clock display size and position
        const int TxtTop = 12;
        const int TxtHeight = 20;
        const int TxtWidth = 262;
        const int LabOffsetLeft = 371;  // UTC offset label size and position
        const int LabOffsetTop = 15;
        const int LabOffsetHeight = 13;
        const int LabOffsetWidth = 30;

        public MultiTime()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Load form event handler
        /// </summary>
        /// <param name="sender">MultiTime form</param>
        /// <param name="e">Load event arguments</param>
        private void MultiTime_Load(object sender, EventArgs e)
        {
            if (DelayTime == 0)
            {
                DelayTime = WaitInSeconds * 1000;
                timUpdateUI.Interval = DelayTime;
            }
            DateTime NowIs = DateTime.Now; //DateTime.UtcNow;
            if (NowIs.IsDaylightSavingTime())
            {
                labMsg.Text = "Daylight time (DST)";
                IsDST = true;
            }
            else
            {
                labMsg.Text = "Standard time (ST)";
                IsDST = false;
            }

            // Load zone information into zones from json file
            using (StreamReader r = new StreamReader("MultiTime.json"))
            {
                string json = r.ReadToEnd();
                zones = JsonConvert.DeserializeObject<List<TimeZones>>(json);
            }

            // Initialize the color map class/list
            LoadColorMap();
            // Create/draw the UI
            DrawUI();
            // Update the UI elements
            ShowClocks();
            btnRun.Focus();
            Running = false;
            Application.DoEvents();
        }

        /// <summary>
        /// Initialize the RGB class with white, grey and pastels and store
        /// into the ColorMap list
        /// </summary>
        void LoadColorMap()
        {
            // Load Color information into ColorMap from json file
            using (StreamReader r = new StreamReader("MultiColor.json"))
            {
                string json = r.ReadToEnd();
                ColorMap = JsonConvert.DeserializeObject<List<RGB>>(json);
            }
            // Set aRGB
            for (int Shade = 0; Shade < ColorMap.Count; Shade++)
            {
                ColorMap[Shade].aRGB = Color.FromArgb(
                    ColorMap[Shade].Alpha,
                    ColorMap[Shade].Red,
                    ColorMap[Shade].Green,
                    ColorMap[Shade].Blue);
            }
        }

        /// <summary>
        /// Dynamicly create a clock display row for each entry in the 
        /// MultiTime.json file
        /// </summary>
        public void DrawUI()
        {
            int Cl = 0;
            int BTop = 0;

            for (int Row = 0; Row < zones.Count; Row++)
            {
                // Set up the clock label 
                string LabName = "Lab" + zones[Row].cname;
                string LabText = zones[Row].name;
                Label Lb = new Label();
                if (Lab.Count == 0)
                {
                    Lb.Top = MarginL;
                }
                else
                {
                    Lb.Top = ((TxtHeight + MarginT) * Lab.Count) + MarginL;
                }
                Lb.Left = LabLeft;
                Lb.Height = TxtHeight;
                Lb.Width = LabWidth;
                Lb.Name = LabName;
                Lb.Text = LabText;
                Lab.Add(Lb);
                Controls.Add(Lb);

                // Set up the clock display
                string TxtName = "Txt" + zones[Row].cname;
                TextBox Tb = new TextBox();
                if (Txt.Count == 0)
                {
                    Tb.Top = MarginT;
                }
                else
                {
                    Tb.Top = ((TxtHeight + MarginT) * Txt.Count) + MarginT;
                }
                Tb.Left = TxtLeft;
                Tb.Height = TxtHeight;
                Tb.Width = TxtWidth;
                Tb.TabStop = false;
                Tb.ReadOnly = true;
                Tb.BackColor = ColorMap[Cl].aRGB;
                Tb.Name = TxtName;
                Txt.Add(Tb);
                Controls.Add(Tb);

                // And finally the label that shows the UTC offset
                string LabOffsetName = "LabOffset" + zones[Row].cname;
                Label Lo = new Label();
                if (LabOffset.Count == 0)
                {
                    Lo.Top = MarginL;
                }
                else
                {
                    Lo.Top = ((TxtHeight + MarginT) * LabOffset.Count) + MarginL;
                }
                Lo.Left = LabOffsetLeft;
                Lo.Height = LabOffsetHeight;
                Lo.Width = LabOffsetWidth;
                Lo.Name = LabOffsetName;
                LabOffset.Add(Lo);
                Controls.Add(Lo);

                BTop = Tb.Top;

                // Cycle through the color map (select next color)
                Cl++;
                if (Cl >= ColorMap.Count) { Cl = 0; }
            }
            // Adjust the ST/DST label and buttons
            labMsg.Top = BTop + MarginL + btnRun.Height;
            btnRun.Top = BTop + MarginT + btnRun.Height;
            btnDone.Top = BTop + MarginT + btnDone.Height;
            // Size the form to the controls
            Height = btnDone.Top + MarginT + btnDone.Height + MarginT;

            Application.DoEvents();
        }

        /// <summary>
        /// Run button click event handler - Start updating clocks every 1000 ms
        /// </summary>
        /// <param name="sender">Run button</param>
        /// <param name="e">Click event arguments</param>
        private void btnRun_Click(object sender, EventArgs e)
        {
            if (Running) { return; }

            btnRun.Enabled = false;
            HellFrozeOver = false;
            timUpdateUI.Start();
            Running = true;
            Application.DoEvents();
        }

        /// <summary>
        /// Timer object tick event handler - used to update the UI display
        /// </summary>
        /// <param name="sender">Timer object</param>
        /// <param name="e">Timer tick event arguments</param>
        private void timUpdateUI_Tick(object sender, EventArgs e)
        {
            // If the clocks aren't running, return
            if (!Running) { return; }
            // Every 60 seconds, fire of a garbage collect
            if (Ticks++ > 60)
            {
                // Fixes the memory leak, but pretty crude IMHO
                Ticks = 0;
                GC.Collect();
            }
            // If clock stop isn't requested, update clocks
            if (!HellFrozeOver)
            {
                ShowClocks();
                // If clock stop requested, stop the timer events
                if (HellFrozeOver)
                {
                    timUpdateUI.Stop();
                }
            }
        }

        /// <summary>
        /// Done button click event handler - Exit
        /// </summary>
        /// <param name="sender">The Done button</param>
        /// <param name="e">Done button click event handler</param>
        private void btnDone_Click(object sender, EventArgs e)
        {
            HellFrozeOver = true;
            Running = false;
            Application.DoEvents();
            GC.Collect();
            Application.Exit();
        }

        /// <summary>
        /// Update the display every 1000 ms (1 sec)
        /// </summary>
        public void ShowClocks()
        {
            DateTime NowIs = DateTime.UtcNow;
            float Offset = 0;

            for (int Clock = 0; Clock < zones.Count; Clock++)
            {
                // Grab the clock name (label for the zone)
                string Name = zones[Clock].name;
                string Id = zones[Clock].id;
                // Grab the appropriate UTC offset (DST biases if needed)
                if (IsDST)
                {
                    Offset = zones[Clock].offsetdst;
                }
                else
                {
                    Offset = zones[Clock].offset;
                }

                // Find our clock controls (I.E. "txt"+"Eastern")
                var LabName = Controls.Find("lab" + zones[Clock].cname, true);
                var TxtName = Controls.Find("txt" + zones[Clock].cname, true);
                var LabOffset = Controls.Find("labOffset" + zones[Clock].cname, true);

                // If we have a control, update them
                if (LabName.Length > 0)
                {
                    // Update the clock name (only need to do once)
                    if (!ClockSet)
                    {
                        LabName[0].Text = Name;
                    }
                    // Update the time/date then time in 24 hr format
                    TxtName[0].Text = NowIs.AddHours(Offset).ToShortDateString() + " - " +
                        NowIs.AddHours(Offset).ToLongTimeString() + " - " +
                        NowIs.AddHours(Offset).Hour.ToString() + ":" +
                        NowIs.AddHours(Offset).Minute.ToString("D2") + ":" +
                        NowIs.AddHours(Offset).Second.ToString("D2") + " " +
                        (NowIs.IsDaylightSavingTime() ? "DST" : "DT");
                    // Update the UTC offset (only need to do once)
                    if (!ClockSet)
                    {
                        LabOffset[0].Text = Offset.ToString();
                    }
                }
            }
            // Give the UI time to draw
            Application.DoEvents();
            ClockSet = true;
        }

        /// <summary>
        /// Defines the clock displays, names, UTC offsets
        /// </summary>
        public class TimeZones
        {
            public string id;           // Zone ID ("UTC", "EST", Etc.)
            public string name;         // Zone Name
            public float offset;        // Standard time UTC offset 
            public float offsetdst;     // DST time UTC offset
            public string cname;        // Control name suffix ( "lab"+cname, "txt"+cname )
            public bool isDST = false;  // DST flag
        }

        /// <summary>
        /// Class to hold color definitions and make up a Color Map List
        /// </summary>
        public class RGB
        {
            public string Id;   // Color id
            public string Name; // Name of color
            public int Alpha;   // Alpha channel value
            public int Red;     // Red channel
            public int Green;   // Green channel
            public int Blue;    // Blue channel
            public Color aRGB;  // ARGB color
        }
    }
}
