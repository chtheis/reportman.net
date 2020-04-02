//---------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
//KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//PARTICULAR PURPOSE.
//---------------------------------------------------------------------
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms.VisualStyles;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing.Design;
using System.Text;

namespace Reportman.Drawing.Forms
{
    /// <summary>
    /// Summary description for TreeGridView.
    /// </summary>
    [System.ComponentModel.DesignerCategory("code"),
    Designer(typeof(System.Windows.Forms.Design.ControlDesigner)),
    ComplexBindingProperties(),
    Docking(DockingBehavior.Ask)]
    public class TreeGridView : DataGridView
    {
      public SortedList<int, DataGridViewCellStyle> paddings_list;
      public static bool DoubleBufferedPerformance = true;
        public static DataGridViewCellBorderStyle DefaultCellBorderStyle = DataGridViewCellBorderStyle.Single;
        private List<DataGridViewRow> ToDispose;
        private TreeGridNode _root;
        private Graphics grint;
        private TreeGridColumn _expandableColumn;
        internal ImageList _imageList;
        private bool _inExpandCollapse = false;
        internal bool _inExpandCollapseMouseCapture = false;
        private bool _showLines = true;
        private bool _virtualNodes = false;
        private CollectionChangeEventHandler evhan;
        internal VisualStyleRenderer rOpen = null;
        internal VisualStyleRenderer rClosed = null;
        internal int treeboxwidth;
        internal bool themesenabled;

        #region Constructor
        public TreeGridView():base()
        {            
          ToDispose = new List<DataGridViewRow>();
          paddings_list = new SortedList<int,DataGridViewCellStyle>();
          this.DoubleBuffered = DoubleBufferedPerformance;
            CellBorderStyle = DefaultCellBorderStyle;
            // Control when edit occurs because edit mode shouldn't start when expanding/collapsing
            this.EditMode = DataGridViewEditMode.EditProgrammatically;
            this.RowTemplate = new TreeGridNode() as DataGridViewRow;
            // This sample does not support adding or deleting rows by the user.
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this._root = new TreeGridNode(this);
            this._root.IsRoot = true;

            this.themesenabled = true;
            // Ensures that all rows are added unshared by listening to the CollectionChanged event.
            evhan = new CollectionChangeEventHandler(DummyProc);
            //base.Rows.CollectionChanged += delegate(object sender, System.ComponentModel.CollectionChangeEventArgs e) { };
            base.Rows.CollectionChanged += evhan;
        }
        public void ClearAll()
        {
            UnSiteAll();
            Nodes.Clear();
            foreach (DataGridViewRow xrow in ToDispose)
            {
                xrow.Dispose();
            }
            Rows.Clear();
            foreach (DataGridViewColumn ncol in Columns)
                ncol.Dispose();
            Columns.Clear();
        }
        private void DummyProc(object sender, System.ComponentModel.CollectionChangeEventArgs e)
        {

        }
        #endregion

        #region Keyboard F2 to begin edit support
        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Cause edit mode to begin since edit mode is disabled to support 
            // expanding/collapsing 
            base.OnKeyDown(e);
            if (!e.Handled)
            {
                if (e.KeyCode == Keys.F2 && this.CurrentCellAddress.X > -1 && this.CurrentCellAddress.Y > -1)
                {
                    if (!this.CurrentCell.Displayed)
                    {
                        this.FirstDisplayedScrollingRowIndex = this.CurrentCellAddress.Y;
                    }
                    else
                    {
                        // TODO:calculate if the cell is partially offscreen and if so scroll into view
                    }
                    this.SelectionMode = DataGridViewSelectionMode.CellSelect;
                    this.BeginEdit(true);
                }
                else if (e.KeyCode == Keys.Enter && !this.IsCurrentCellInEditMode)
                {
                    this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    this.CurrentCell.OwningRow.Selected = true;
                }
            }
        }
        #endregion

        #region Shadow and hide DGV properties

        // This sample does not support databinding
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)]
        public new object DataSource
        {
            get { return null; }
            set { throw new NotSupportedException("The TreeGridView does not support databinding"); }
        }

        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)]
        public new object DataMember
        {
            get { return null; }
            set { throw new NotSupportedException("The TreeGridView does not support databinding"); }
        }

        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)]
        public new DataGridViewRowCollection Rows
        {
            get { return base.Rows; }
        }

        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)]
        public new bool VirtualMode
        {
            get { return false; }
            set { throw new NotSupportedException("The TreeGridView does not support virtual mode"); }
        }

        // none of the rows/nodes created use the row template, so it is hidden.
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)]
        public new DataGridViewRow RowTemplate
        {
            get { return base.RowTemplate; }
            set { base.RowTemplate = value; }
        }

        #endregion

        #region Public methods
        [Description("Returns the TreeGridNode for the given DataGridViewRow")]
        public TreeGridNode GetNodeForRow(DataGridViewRow row)
        {
            return row as TreeGridNode;
        }

        [Description("Returns the TreeGridNode for the given DataGridViewRow")]
        public TreeGridNode GetNodeForRow(int index)
        {
            return GetNodeForRow(base.Rows[index]);
        }
        public TreeGridCell GetNextNodeCell(TreeGridCell currentcell)
        {            
            int curcol = currentcell.ColumnIndex;
            int currow = currentcell.RowIndex;
            TreeGridCell nextnode = null;


            return nextnode;
        }
        public void ExpandParent(TreeGridNode currnode,List<TreeGridNode> nlist)
        {
            bool doexpand = false;
            if (nlist == null)
            {
                doexpand = true;
                nlist = new List<TreeGridNode>();
            }
            if (currnode.Parent != null)
            {
                if (!currnode.Parent.IsExpanded)
                    nlist.Add(currnode.Parent);
                ExpandParent(currnode.Parent,nlist);
            }
            if (doexpand)
            {
                for (int i = nlist.Count-1;i>=0;i--)
                {
                    if (!nlist[i].IsExpanded)
                        nlist[i].Expand();
                }
            }
        }
        public void ExpandChilds(TreeGridNode currnode,int currentlevel,int maxlevel)
        {
            if ((currnode.HasChildren) && (!currnode.IsExpanded))
                currnode.Expand();
            currentlevel++;
            if (currentlevel > maxlevel)
            {
                foreach (TreeGridNode inode in currnode.Nodes)
                {
                    if (inode.IsExpanded)
                        inode.Collapse();
                    CollapseInvisibleChilds(inode);
                }
            }
            else
            {
                foreach (TreeGridNode inode in currnode.Nodes)
                    ExpandChilds(inode,currentlevel,maxlevel);
            }
        }
        public void ExpandToLevel(int level,ProgresEvent evprogres)
        {
            try
            {
            if (level <= 0)
            {
                CollapseAll(evprogres);
                return;
            }
            else
                CollapseAll(evprogres);
            int rowindex = 0;
            while (rowindex < Rows.Count)
            {
                DataGridViewRow nrow = Rows[rowindex];
                if (nrow is TreeGridNode)
                {
                    DataGridViewRow nextnode = null;
                    if ((rowindex + 1) < Rows.Count)
                        nextnode = Rows[rowindex + 1];
                    TreeGridNode xnode = (TreeGridNode)nrow;
                    ExpandChilds(xnode,1,level);
                    if (evprogres != null)
                    {
                        bool docancel=false;
                        evprogres(rowindex, Rows.Count, ref docancel);
                        if (docancel)
                            break;
                    }
                    if (nextnode != null)
                        rowindex = nextnode.Index;
                    else
                        break;
                }
                else
                    rowindex++;
            }
            }
            finally
            {
                if (evprogres != null)
                {
                    bool docancel = false;
                    evprogres(RowCount, RowCount, ref docancel);
                }
            }

        }
        public void ExpandAll(ProgresEvent evprogres)
        {
            try
            {
            int rowindex = 0;
            SuspendLayout();
            int totalcount = 0;
            while (rowindex < Rows.Count)
            {
                DataGridViewRow nrow = Rows[rowindex];
                if (nrow is TreeGridNode)
                {
                    DataGridViewRow nextnode = null;
                    if ((rowindex+1) < Rows.Count )
                        nextnode = Rows[rowindex + 1];
                    TreeGridNode xnode = (TreeGridNode)nrow;
                    ExpandChilds(xnode,1,999999);
                    
                    if (evprogres != null)
                    {
                        totalcount++;
                        if ((totalcount % 10) == 0)
                        {
                            bool docancel = false;
                            evprogres(rowindex, Rows.Count, ref docancel);
                            if (docancel)
                                break;
                        }
                    }
                    if (nextnode != null)
                        rowindex = nextnode.Index;
                    else
                        break;
                }
                else
                    rowindex++;
            }
            ResumeLayout(true);
            }
            finally
            {
                if (evprogres != null)
                {
                    bool docancel = false;
                    evprogres(RowCount, RowCount, ref docancel);
                }
            }
        }
        public void CollapseInvisibleChilds(TreeGridNode nnode)
        {
            foreach (TreeGridNode childnode in nnode.Nodes)
            {
                childnode.IsExpanded = false;
                CollapseInvisibleChilds(childnode);
            }
        }
        public void CollapseAll(ProgresEvent evprogres)
        {            
            try
            {
            int rowindex = 0;
            while (rowindex < Rows.Count)
            {
                DataGridViewRow nrow = Rows[rowindex];
                if (nrow is TreeGridNode)
                {
                    TreeGridNode xnode = (TreeGridNode)nrow;
                    if (xnode.IsExpanded)
                        xnode.Collapse();
                    CollapseInvisibleChilds(xnode);
                    rowindex++;
                    if (evprogres != null)
                    {
                        bool docancel=false;
                        evprogres(rowindex, Rows.Count, ref docancel);
                        if (docancel)
                            break;
                    }
                }
                else
                    rowindex++;
            }
            }
            finally
            {
                if (evprogres != null)
                {
                    bool docancel = false;
                    evprogres(RowCount, RowCount, ref docancel);
                }
            }
        }
        public bool FindTextNode(TreeGridNode nnode, string busca, int firstcolumn, int lastcolumn, bool searchchild)
        {
            bool found = false;

            for (int i = firstcolumn; i <=lastcolumn; i++)
            {
                if (Columns[i].Visible)
                {
                    string nvalue = "";
                    DataGridViewCell nextcell = nnode.Cells[i];
                    if (nextcell.Value != null)
                        nvalue = nextcell.Value.ToString().ToUpper().Replace(',','.');
                    if (nvalue.IndexOf(busca) >= 0)
                    {
                        found = true;
                        // Expand all parents
                        ExpandParent(nnode,null);
                        CurrentCell = nnode.Cells[i];
                        FirstDisplayedScrollingColumnIndex = CurrentCell.ColumnIndex;
                        break;
                    }
                }
            }
            if ((!found) && (searchchild))
            {
                foreach (TreeGridNode childnode in nnode.Nodes)
                {
                    found = FindTextNode(childnode, busca, 0,Columns.Count-1,true);
                    if (found)
                        break;
                }
            }

            return found;
        }
        public Graphics GetGraphics()
        {
            if (grint == null)
                grint = CreateGraphics();
            return grint;
           
        }
        public void FindText(string ntext)
        {
            if (CurrentCell == null)
                return;
            // Busca desde la celda seleccionada hacia la derecha y abajo
            string busca = ntext.ToUpper().Replace(',','.');

            if (busca.Length == 0)
                return;
            TreeGridNode currnode = (TreeGridNode)CurrentRow;
            int ncolumn = CurrentCell.ColumnIndex+1;
            bool found = false;
            for (int i = currnode.RowIndex; i < Rows.Count; i++)
            {
                found = FindTextNode((TreeGridNode)Rows[i],busca,ncolumn,Columns.Count-1,true);
                if (found)
                    break;
                ncolumn = 0;
            }
            if (!found)
            {
                for (int i = 0; i < currnode.RowIndex; i++)
                {
                    found = FindTextNode((TreeGridNode)Rows[i], busca, 0, Columns.Count - 1, true);
                    if (found)
                        break;
                }
                if (!found)
                {
                    FindTextNode(currnode, busca, 0, CurrentCell.ColumnIndex - 1, false);
                }
            }
        }
        public void CopySelectionToClipBoard()
        {
            DataGridViewSelectedCellCollection collect = SelectedCells;
            SortedList<int, DataGridViewColumn> acolumns = new SortedList<int, DataGridViewColumn>();
            SortedList<int, DataGridViewRow> arows = new SortedList<int, DataGridViewRow>();
            int index;
            foreach (DataGridViewCell ncell in collect)
            {
                if (Columns[ncell.ColumnIndex].Visible)
                {
                    index = acolumns.IndexOfKey(ncell.ColumnIndex);
                    if (index < 0)
                        acolumns.Add(ncell.ColumnIndex, Columns[ncell.ColumnIndex]);
                }
                index = arows.IndexOfKey(ncell.RowIndex);
                if (index < 0)
                    arows.Add(ncell.RowIndex, Rows[ncell.RowIndex]);
            }
            int i = 0;
            StringBuilder nresult = new StringBuilder();
            foreach (KeyValuePair<int, DataGridViewRow> npair in arows)
            {
                if (i > 0)
                    nresult.Append("\n");
                int j = 0;
                foreach (KeyValuePair<int, DataGridViewColumn> colpari in acolumns)
                {
                    if (j > 0)
                        nresult.Append("\t");
                    object nvalor = this[colpari.Key, npair.Key].Value;
                    if (nvalor == null)
                        nresult.Append("");
                    else
                        nresult.Append(nvalor.ToString());
                    j++;
                }
                i++;
            }
            Clipboard.SetData(nresult.ToString(),false);
        }

        #endregion

        #region Public properties
        [Category("Data"),
        Description("The collection of root nodes in the treelist."),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        public TreeGridNodeCollection Nodes
        {
            get
            {
                return this._root.Nodes;
            }
        }

        public new TreeGridNode CurrentRow
        {
            get
            {
                return base.CurrentRow as TreeGridNode;
            }
        }

        [DefaultValue(false),
        Description("Causes nodes to always show as expandable. Use the NodeExpanding event to add nodes.")]
        public bool VirtualNodes
        {
            get { return _virtualNodes; }
            set { _virtualNodes = value; }
        }

        public TreeGridNode CurrentNode
        {
            get
            {
                return this.CurrentRow;
            }
        }

        [DefaultValue(true)]
        public bool ShowLines
        {
            get { return this._showLines; }
            set
            {
                if (value != this._showLines)
                {
                    this._showLines = value;
                    this.Invalidate();
                }
            }
        }

        public ImageList ImageList
        {
            get { return this._imageList; }
            set
            {
                this._imageList = value;
                //TODO: should we invalidate cell styles when setting the image list?

            }
        }

        public new int RowCount
        {
            get { return this.Nodes.Count; }
            set
            {
                for (int i = 0; i < value; i++)
                {
                    TreeGridNode nnode = new TreeGridNode();
                    nnode.Height = RowTemplate.Height;
                    this.Nodes.Add(nnode);
                }

            }
        }
        #endregion

        #region Site nodes and collapse/expand support
        protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
        {
            base.OnRowsAdded(e);
            // Notify the row when it is added to the base grid 
            int count = e.RowCount - 1;
            TreeGridNode row;
            while (count >= 0)
            {
                row = base.Rows[e.RowIndex + count] as TreeGridNode;
                if (row != null)
                {
                    row.Sited();
                }
                count--;
            }
        }

        internal protected void UnSiteAll()
        {
            this.UnSiteNode(this._root);
        }

        internal protected virtual void UnSiteNode(TreeGridNode node)
        {
            if (node.IsSited || node.IsRoot)
            {
                // remove child rows first
                foreach (TreeGridNode childNode in node.Nodes)
                {
                    this.UnSiteNode(childNode);
                }

                // now remove this row except for the root
                if (!node.IsRoot)
                {
                    base.Rows.Remove(node);
                    // Row isn't sited in the grid anymore after remove. Note that we cannot
                    // Use the RowRemoved event since we cannot map from the row index to
                    // the index of the expandable row/node.
                    node.UnSited();
                }
            }
        }

        internal protected virtual bool CollapseNode(TreeGridNode node)
        {
            if (node.IsExpanded)
            {
                CollapsingEventArgs exp = new CollapsingEventArgs(node);
                this.OnNodeCollapsing(exp);

                if (!exp.Cancel)
                {
                    this.LockVerticalScrollBarUpdate(true);
                    this.SuspendLayout();
                    _inExpandCollapse = true;
                    node.IsExpanded = false;

                    List<TreeGridNode> nodescollapse = new List<TreeGridNode>();
                    foreach (TreeGridNode nnode in node.Nodes)
                    {
                        nodescollapse.Add(nnode);
                    }
                    foreach (TreeGridNode childNode in nodescollapse)
                    {
                        Debug.Assert(childNode.RowIndex != -1, "Row is NOT in the grid.");
                        this.UnSiteNode(childNode);
                    }


                    CollapsedEventArgs exped = new CollapsedEventArgs(node);
                    this.OnNodeCollapsed(exped);
                    //TODO: Convert this to a specific NodeCell property
                    _inExpandCollapse = false;
                    this.LockVerticalScrollBarUpdate(false);
                    this.ResumeLayout(true);
                    this.InvalidateCell(node.Cells[0]);

                }

                return !exp.Cancel;
            }
            else
            {
                // row isn't expanded, so we didn't do anything.				
                return false;
            }
        }

        internal protected virtual void SiteNode(TreeGridNode node)
        {
            //TODO: Raise exception if parent node is not the root or is not sited.
            int rowIndex = -1;
            TreeGridNode currentRow;
            node._grid = this;
            node.Height = RowTemplate.Height;

            if (node.Parent != null && node.Parent.IsRoot == false)
            {
                // row is a child
                Debug.Assert(node.Parent != null && node.Parent.IsExpanded == true,"Node.Pareb widhout parent and expanded");

                if (node.Index > 0)
                {
                    currentRow = node.Parent.Nodes[node.Index - 1];
                }
                else
                {
                    currentRow = node.Parent;
                }
            }
            else
            {
                // row is being added to the root
                if (node.Index > 0)
                {
                    currentRow = node.Parent.Nodes[node.Index - 1];
                }
                else
                {
                    currentRow = null;
                }

            }

            if (currentRow != null)
            {
                while (currentRow.Level >= node.Level)
                {
                    if (currentRow.RowIndex < base.Rows.Count - 1)
                    {
                        currentRow = base.Rows[currentRow.RowIndex + 1] as TreeGridNode;
                        Debug.Assert(currentRow != null,"Current row is null");
                    }
                    else
                        // no more rows, site this node at the end.
                        break;

                }
                if (currentRow == node.Parent)
                    rowIndex = currentRow.RowIndex + 1;
                else if (currentRow.Level < node.Level)
                    rowIndex = currentRow.RowIndex;
                else
                    rowIndex = currentRow.RowIndex + 1;
            }
            else
                rowIndex = 0;


            Debug.Assert(rowIndex != -1,"Rowindex not -1");
            this.SiteNode(node, rowIndex);

            Debug.Assert(node.IsSited,"node.IsSited");
            if (node.IsExpanded)
            {
                // check if it has childs
                bool haschilds = true;
/*                foreach (TreeGridNode childNode in node.Nodes)
                {
                    if (childNode.IsExpanded)
                        if (childNode.Nodes.Count > 0)
                        {
                            haschilds = true;
                            break;
                        }
                }*/
                // add all child rows to display
                if (haschilds)
                {
                    foreach (TreeGridNode childNode in node.Nodes)
                    {
                        //TODO: could use the more efficient SiteRow with index.
                        this.SiteNode(childNode);
                    }
                }
                else
                {
                    rowIndex++;
                    DataGridViewRow[] rowarray = new DataGridViewRow[node.Nodes.Count];
                    int idx = 0;
                    foreach (TreeGridNode childNode in node.Nodes)
                    {
                        rowarray[idx] = childNode;
                        childNode._grid = node._grid;
                        childNode.CheckQueuedDispose();
                        idx++;
                    }
                    if (rowIndex < base.Rows.Count)
                    {
                        base.Rows.Insert(rowIndex, rowarray);
                    }
                    else
                    {
                        // for the last item.
                        base.Rows.AddRange(rowarray);
                    }
                }
            }
        }


        internal protected virtual void SiteNode(TreeGridNode node, int index)
        {
            if (index < base.Rows.Count)
            {
                base.Rows.Insert(index, node);
            }
            else
            {
                // for the last item.
                base.Rows.Add(node);
            }
        }

        internal protected virtual bool ExpandNode(TreeGridNode node)
        {
            if (!node.IsExpanded || this._virtualNodes)
            {
                ExpandingEventArgs exp = new ExpandingEventArgs(node);
                this.OnNodeExpanding(exp);

                if (!exp.Cancel)
                {
                    this.LockVerticalScrollBarUpdate(true);
                    this.SuspendLayout();
                    _inExpandCollapse = true;
                    node.IsExpanded = true;


                    bool haschilds = false;
                    foreach (TreeGridNode childNode in node.Nodes)
                    {
                        if (childNode.Nodes.Count > 0)
                            haschilds = true;
                    }
                    if (!haschilds)
                    {
                        DataGridViewRow[] rowarray = new DataGridViewRow[node.Nodes.Count];
                        int idx = 0;
                        foreach (TreeGridNode childNode in node.Nodes)
                        {
                            Debug.Assert(childNode.RowIndex == -1, "Row is already in the grid.");
                            rowarray[idx] = childNode;
                            childNode._grid = node._grid;
                            childNode.CheckQueuedDispose();
                            idx++;
                        }
                        base.Rows.InsertRange(node.RowIndex + 1, rowarray);
                    }
                    else
                    {
                        //TODO Convert this to a InsertRange
                        foreach (TreeGridNode childNode in node.Nodes)
                        {
                            Debug.Assert(childNode.RowIndex == -1, "Row is already in the grid.");

                            this.SiteNode(childNode);
                            //this.BaseRows.Insert(rowIndex + 1, childRow);
                            //TODO : remove -- just a test.
                            //childNode.Cells[0].Value = "child";
                        }
                    }

                    ExpandedEventArgs exped = new ExpandedEventArgs(node);
                    this.OnNodeExpanded(exped);
                    //TODO: Convert this to a specific NodeCell property
                    _inExpandCollapse = false;
                    this.LockVerticalScrollBarUpdate(false);
                    this.ResumeLayout(true);
                    this.InvalidateCell(node.Cells[0]);
                }

                return !exp.Cancel;
            }
            else
            {
                // row is already expanded, so we didn't do anything.
                return false;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            // used to keep extra mouse moves from selecting more rows when collapsing
            base.OnMouseUp(e);
            this._inExpandCollapseMouseCapture = false;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // while we are expanding and collapsing a node mouse moves are
            // supressed to keep selections from being messed up.
            if (!this._inExpandCollapseMouseCapture)
                base.OnMouseMove(e);

        }
        #endregion

        #region Collapse/Expand events
        public event ExpandingEventHandler NodeExpanding;
        public event ExpandedEventHandler NodeExpanded;
        public event CollapsingEventHandler NodeCollapsing;
        public event CollapsedEventHandler NodeCollapsed;

        protected virtual void OnNodeExpanding(ExpandingEventArgs e)
        {
            if (this.NodeExpanding != null)
            {
                NodeExpanding(this, e);
            }
        }
        protected virtual void OnNodeExpanded(ExpandedEventArgs e)
        {
            if (this.NodeExpanded != null)
            {
                NodeExpanded(this, e);
            }
        }
        protected virtual void OnNodeCollapsing(CollapsingEventArgs e)
        {
            if (this.NodeCollapsing != null)
            {
                NodeCollapsing(this, e);
            }

        }
        protected virtual void OnNodeCollapsed(CollapsedEventArgs e)
        {
            if (this.NodeCollapsed != null)
            {
                NodeCollapsed(this, e);
            }
        }
        #endregion

        #region Helper methods
        protected override void Dispose(bool disposing)
        {
            base.Rows.CollectionChanged -= evhan;
            //this.UnSiteAll();
            if (ToDispose != null)
            {
                foreach (DataGridViewRow nitem in ToDispose)
                    nitem.Dispose();
            }
            if (grint != null)
              grint.Dispose();
            //Rows.Clear();
            if (RowTemplate != null)
                RowTemplate.Dispose();
            if (_root != null)
                _root.Dispose();
            base.Rows.CollectionChanged -= evhan;
            base.Dispose(Disposing);
        }
       public void QueueDispose(DataGridViewRow nitem)
       {
        ToDispose.Add(nitem);
      }
        /*
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            
            // this control is used to temporarly hide the vertical scroll bar
            hideScrollBarControl = new Control();
            hideScrollBarControl.Visible = false;
            hideScrollBarControl.Enabled = false;
            hideScrollBarControl.TabStop = false;
            // control is disposed automatically when the grid is disposed
            this.Controls.Add(hideScrollBarControl);
        }*/

        protected override void OnRowEnter(DataGridViewCellEventArgs e)
        {
            // ensure full row select
            base.OnRowEnter(e);
            if (this.SelectionMode == DataGridViewSelectionMode.CellSelect ||
                (this.SelectionMode == DataGridViewSelectionMode.FullRowSelect &&
                base.Rows[e.RowIndex].Selected == false))
            {
                this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                base.Rows[e.RowIndex].Selected = true;
            }
        }

        private void LockVerticalScrollBarUpdate(bool lockUpdate/*, bool delayed*/)
        {
            // Temporarly hide/show the vertical scroll bar by changing its parent
            if (!this._inExpandCollapse)
            {
                if (lockUpdate)
                {
                    this.VerticalScrollBar.Parent = null;
                }
                else
                {
                    this.VerticalScrollBar.Parent = this;
                }
            }
        }

        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            if (typeof(TreeGridColumn).IsAssignableFrom(e.Column.GetType()))
            {
                if (_expandableColumn == null)
                {
                    // identify the expanding column.			
                    _expandableColumn = (TreeGridColumn)e.Column;
                }
                else
                {
                    // this.Columns.Remove(e.Column);
                    //throw new InvalidOperationException("Only one TreeGridColumn per TreeGridView is supported.");
                }
            }

            // Expandable Grid doesn't support sorting. This is just a limitation of the sample.
            e.Column.SortMode = DataGridViewColumnSortMode.NotSortable;

            base.OnColumnAdded(e);
        }

        private static class Win32Helper
        {
            public const int WM_SYSKEYDOWN = 0x0104,
                             WM_KEYDOWN = 0x0100,
                             WM_SETREDRAW = 0x000B;

            [System.Runtime.InteropServices.DllImport("USER32.DLL", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern IntPtr SendMessage(System.Runtime.InteropServices.HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);

            [System.Runtime.InteropServices.DllImport("USER32.DLL", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern IntPtr SendMessage(System.Runtime.InteropServices.HandleRef hWnd, int msg, int wParam, int lParam);

            [System.Runtime.InteropServices.DllImport("USER32.DLL", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern bool PostMessage(System.Runtime.InteropServices.HandleRef hwnd, int msg, IntPtr wparam, IntPtr lparam);

        }
        #endregion


    }

}
