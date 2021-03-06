﻿using StratSim.Model;
using StratSim.View.MyFlowLayout;
using StratSim.View.UserControls;
using StratSim.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MyFlowLayout;
using Graphing;

namespace StratSim.View.Panels
{
    /// <summary>
    /// Represents a panel which displays data about the driver's strategies.
    /// Allows for the data to be edited within the panel.
    /// </summary>
    public class StrategyViewer : MyPanel
    {
        const int leftBorder = 10;
        const int topBorder = 25;
        const int horizontalBorder = 10;
        const int defaultHeight = 15;
        const int defaultWidth = 70;
        const int wideWidth = 100;

        Size txtDefault = new Size(defaultWidth, defaultHeight);
        Size lblDefault = new Size(defaultWidth, defaultHeight);

        List<StintPanel> StintPanels = new List<StintPanel>();
        ResultsPanel Results;
        List<GraphLine> Traces = new List<GraphLine>();
        NewGraph graph;
        DriverSelectPanel driverPanel;

        StrategyViewerData strategyViewerData;

        ToolStripDropDownButton thisToolStripDropDown;
        ToolStripButton save, load, resetDropDown, updateDropDown, goToRaceDropDown;

        int raceLaps;
        bool isInitialised;

        public StrategyViewer(MainForm FormToAdd, bool loadFromFile)
            : base(300, 600, "Strategies", FormToAdd, Properties.Resources.Strategies)
        {
            isInitialised = false;
            strategyViewerData = new StrategyViewerData(StrategyViewerData_DataLoaded);
            strategyViewerData.InitiailiseData(loadFromFile);

            PanelClosed += StrategyViewer_PanelClosed;
            OpenedInNewForm += StrategyViewer_OpenedInNewForm;
            PanelOpened += StrategyViewer_PanelOpened;
            MyEvents.StrategyModificationsComplete += MyEvents_StrategyModificationsComplete;

            AutoScroll = true;
            raceLaps = Data.GetRaceLaps();
        }

        protected override void LinkToForm(MainForm Form)
        {
            base.LinkToForm(Form);
            strategyViewerData.LinkToEvents((StratSimPanelControlEvents)PanelControlEvents);
        }

        public void SetGraph(NewGraph Graph)
        {
            graph = Graph;
        }
        public void SetDriverPanel(DriverSelectPanel DriverPanel)
        {
            if (DriverPanel != driverPanel) //Prevents multiple subscribing to events
            {
                driverPanel = DriverPanel;
                driverPanel.NormalisedDriverChanged += driverPanel_NormalisedDriverChanged;
            }
        }

        void StrategyViewer_OpenedInNewForm(MainForm NewForm)
        {
            AddToolStrip();
        }

        public void AddToolStrip()
        {
            PanelControlEvents.OnShowToolStrip(thisToolStripDropDown);
        }

        void driverPanel_NormalisedDriverChanged()
        {
            DisplayStints(Data.DriverIndex);
        }
        /// <summary>
        /// When strategy modifications are completed, re-populates controls
        /// </summary>
        /// <param name="showAllOnGraph">Represents whether all traces should now be shown on the graph</param>
        void MyEvents_StrategyModificationsComplete(bool showAllOnGraph, bool changeNormalisedDriver)
        {
            DrawGraph(showAllOnGraph, changeNormalisedDriver);
            driverPanel.UpdateRadioButtons(Data.DriverIndex);
            DisplayStints(Data.DriverIndex);
        }
        /// <summary>
        /// Once data is loaded, displays and populates the controls on the panel
        /// </summary>
        void StrategyViewerData_DataLoaded()
        {
            MyEvents.OnFinishedLoadingStrategies();
            if (!isInitialised)
            {
                InitialiseControls();
                AddToolStrip();
                isInitialised = true;
            }
            DrawGraph(true, true);
            DisplayStints(Data.DriverIndex);
        }

        void InitialiseControls()
        {
            thisToolStripDropDown = new ToolStripDropDownButton("Strategy Data");

            save = new ToolStripButton("Save", Properties.Resources.Save);
            save.Click += save_Click;
            thisToolStripDropDown.DropDownItems.Add(save);

            load = new ToolStripButton("Load", Properties.Resources.Open);
            load.Click += load_Click;
            thisToolStripDropDown.DropDownItems.Add(load);

            resetDropDown = new ToolStripButton("Reset", Properties.Resources.Reset);
            resetDropDown.Click += resetDropDown_Click;
            thisToolStripDropDown.DropDownItems.Add(resetDropDown);

            updateDropDown = new ToolStripButton("Update", Properties.Resources.Update);
            updateDropDown.Click += updateDropDown_Click;
            thisToolStripDropDown.DropDownItems.Add(updateDropDown);

            goToRaceDropDown = new ToolStripButton("Start Race", Properties.Resources.Race);
            goToRaceDropDown.Click += goToRaceDropDown_Click;
            thisToolStripDropDown.DropDownItems.Add(goToRaceDropDown);

            thisToolStripDropDown.DropDown.Width = 200;

            SetGraph(new NewGraph("Strategy Graph", ParentForm, Properties.Resources.Graph));
            ((StratSimPanelControlEvents)PanelControlEvents).OnStartGraph(graph);
        }

        void goToRaceDropDown_Click(object sender, EventArgs e)
        {
            ((StratSimPanelControlEvents)PanelControlEvents).OnShowRacePanel(base.ParentForm);
        }
        void updateDropDown_Click(object sender, EventArgs e)
        {
            strategyViewerData.SetDriverStrategies();
        }
        void save_Click(object sender, EventArgs e)
        {
            strategyViewerData.SaveData();
        }
        void load_Click(object sender, EventArgs e)
        {
            strategyViewerData.LoadData();
        }
        void resetDropDown_Click(object sender, EventArgs e)
        {
            strategyViewerData.RefreshData();
        }
        void StrategyViewer_PanelClosed(MainForm LeavingForm)
        {
            PanelControlEvents.OnRemoveToolStrip(thisToolStripDropDown);

            MyEvents.StrategyModificationsComplete -= MyEvents_StrategyModificationsComplete;
            strategyViewerData.DataLoaded -= StrategyViewerData_DataLoaded;
            graph.GraphRightClick -= graph_GraphRightClick;
        }
        void StrategyViewer_PanelOpened(MainForm AddedOnForm)
        {
            MyEvents.StrategyModificationsComplete += MyEvents_StrategyModificationsComplete;
            strategyViewerData.DataLoaded += StrategyViewerData_DataLoaded;
            graph.GraphRightClick += graph_GraphRightClick;
        }

        private void graph_GraphRightClick(object sender, DataPoint e)
        {
            AddPitStopToStrategy(e.index, (int)e.X);
        }

        private void AddPitStopToStrategy(int driverIndex, int lapNumber)
        {
            Strategy thisStrategy = strategyViewerData.GetStrategy(driverIndex);
            bool stopWithinRange;
            int nearestStop = thisStrategy.GetNearestPitStop(lapNumber, 1, out stopWithinRange);

            if (stopWithinRange)
            {
                thisStrategy.Stints = thisStrategy.RemovePitStop(nearestStop);
            }
            else
            {
                thisStrategy.Stints = thisStrategy.AddPitStop(lapNumber);
            }

            thisStrategy.UpdateStrategyParameters();
            MyEvents.OnStrategyModified(Data.Drivers[driverIndex], thisStrategy, false);
        }


        /// <summary>
        /// Displays the race stints on the panel after the stints have been modified
        /// </summary>
        /// <param name="driverToDisplay">The driver whose strategy is to be displayed</param>
        void DisplayStints(int driverToDisplay)
        {
            RemoveStintPanels();
            ShowStintPanels(driverToDisplay);
            graph.GraphPanel.SetNormalisationIndex(driverToDisplay);
        }

        /// <summary>
        /// Removes the stint panels from the panel
        /// </summary>
        void RemoveStintPanels()
        {
            foreach (StintPanel stintPanel in StintPanels)
            {
                this.Controls.Remove(stintPanel);
                stintPanel.Dispose();
            }
            StintPanels.Clear();

            this.Controls.Remove(Results);
        }

        /// <summary>
        /// Loads driver strategies and from file
        /// </summary>
        public void LoadDataFromFile()
        {
            strategyViewerData.LoadData();
        }
        /// <summary>
        /// Sets the driver strategies to the currently held data
        /// </summary>
        public void UpdateData()
        {
            strategyViewerData.SetDriverStrategies();
        }

        /// <summary>
        /// Shows the panels and populates with data from the stored strategies
        /// </summary>
        /// <param name="driverToShow">The driver whose data is being displayed</param>
        void ShowStintPanels(int driverToShow)
        {
            int stint = 0;
            StintPanel tempPanel;

            Strategy strategyToShow = strategyViewerData.GetStrategy(driverToShow);

            foreach (Stint s in strategyToShow.Stints)
            {
                tempPanel = new StintPanel(stint, Data.Drivers[driverToShow], strategyToShow);
                tempPanel.Location = new Point(leftBorder, topBorder * (1 + stint) + (80 * stint));
                StintPanels.Add(tempPanel);
                this.Controls.Add(tempPanel);
                stint++;
            }

            Results = new ResultsPanel(driverToShow, strategyToShow);
            Results.Location = new Point(leftBorder, topBorder * (1 + StintPanels.Count) + (80 * StintPanels.Count));
            this.Controls.Add(Results);
        }

        /// <summary>
        /// Creates the graph traces and calls the graph draw method to display the strategies on a graph
        /// </summary>
        /// <param name="showAllOnGraph">Represents whether all traces on the graph will be shown after the update</param>
        /// <param name="changeNormalised">Represents whether the normalised driver should be set to the fastest driver,
        /// or maintained as the current driver</param>
        void DrawGraph(bool showAllOnGraph, bool changeNormalised)
        {
            //create traces.
            DataPoint tempPoint;
            GraphLine pointList;
            Strategy thisStrategy;
            int lapsThroughRace = 0;
            float cumulativeTime = 0;

            Traces.Clear();

            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                pointList = new GraphLine(driverIndex, true, Data.Drivers[driverIndex].LineColour);
                lapsThroughRace = 0;
                cumulativeTime = 0;

                thisStrategy = strategyViewerData.GetStrategy(driverIndex);

                //Add the starting point
                tempPoint.index = driverIndex;
                tempPoint.X = 0;
                tempPoint.Y = 0;
                tempPoint.isCycled = false;
                pointList.DataPoints.Add(tempPoint);

                //The points are now defined as the state at the end of a lap
                foreach (float lap in thisStrategy.LapTimes)
                {
                    cumulativeTime += lap;
                    tempPoint.index = driverIndex;
                    tempPoint.X = ++lapsThroughRace;
                    tempPoint.Y = cumulativeTime;
                    tempPoint.isCycled = false;

                    pointList.DataPoints.Add(tempPoint);
                }
                Traces.Add(pointList);
            }

            graph.DrawGraph(Traces, showAllOnGraph, changeNormalised);
        }

        public NewGraph GetGraph()
        { return this.graph; }

        public bool IsInitialised
        { get { return isInitialised; } }

    }
}
