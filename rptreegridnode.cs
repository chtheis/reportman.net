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
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing.Design;
using System.Windows.Forms.VisualStyles;
using System.Text;

namespace Reportman.Drawing.Forms
{
    public class TreeGridNodeEventBase
    {
        private TreeGridNode _node;

        public TreeGridNodeEventBase(TreeGridNode node)
        {
            this._node = node;
        }

        public TreeGridNode Node
        {
            get { return _node; }
        }
    }
    public class CollapsingEventArgs : System.ComponentModel.CancelEventArgs
    {
        private TreeGridNode _node;

        private CollapsingEventArgs() { }
        public CollapsingEventArgs(TreeGridNode node)
            : base()
        {
            this._node = node;
        }
        public TreeGridNode Node
        {
            get { return _node; }
        }

    }
    public class CollapsedEventArgs : TreeGridNodeEventBase
    {
        public CollapsedEventArgs(TreeGridNode node)
            : base(node)
        {
        }
    }

    public class ExpandingEventArgs : System.ComponentModel.CancelEventArgs
    {
        private TreeGridNode _node;

        private ExpandingEventArgs() { }
        public ExpandingEventArgs(TreeGridNode node)
            : base()
        {
            this._node = node;
        }
        public TreeGridNode Node
        {
            get { return _node; }
        }

    }
    public class ExpandedEventArgs : TreeGridNodeEventBase
    {
        public ExpandedEventArgs(TreeGridNode node)
            : base(node)
        {
        }
    }

    public delegate void ExpandingEventHandler(object sender, ExpandingEventArgs e);
    public delegate void ExpandedEventHandler(object sender, ExpandedEventArgs e);

    public delegate void CollapsingEventHandler(object sender, CollapsingEventArgs e);
    public delegate void CollapsedEventHandler(object sender, CollapsedEventArgs e);


    
    [
        ToolboxItem(false),
        DesignTimeVisible(false)
    ]
    public class TreeGridNode : DataGridViewRow//, IComponent
    {
      internal TreeGridView _grid;
        internal TreeGridNode _parent;
        internal TreeGridNodeCollection _owner;
      private bool queueddispose;
        public bool IsExpanded;
        internal bool IsRoot;
        internal bool _isSited;
        internal bool _isFirstSibling;
        internal bool _isLastSibling;
        internal Image _image;
        internal int _imageIndex;

        TreeGridCell _treeCell;
        TreeGridNodeCollection childrenNodes;

        private int _index;
        private int _level;
        private bool childCellsCreated = false;

        // needed for IComponent
        private ISite site = null;
        private EventHandler disposed = null;

        internal TreeGridNode(TreeGridView owner)
            : this()
        {
            this._grid = owner;
            this.IsExpanded = true;
            this.CheckQueuedDispose();
        }
      internal void CheckQueuedDispose()
      {
        if (!queueddispose)
        {
          if (this._grid != null)
          {
            this._grid.QueueDispose(this);
            queueddispose = true;
          }
        }
      }

        public TreeGridNode()
        {
            _index = -1;
            _level = -1;
            IsExpanded = false;
            _isSited = false;
            _isFirstSibling = false;
            _isLastSibling = false;
            _imageIndex = -1;
        }

        public override object Clone()
        {
            TreeGridNode r = (TreeGridNode)base.Clone();
            r._level = this._level;
            r._grid = this._grid;
            r.CheckQueuedDispose();
            r._parent = this.Parent;

            r._imageIndex = this._imageIndex;
            if (r._imageIndex == -1)
                r.Image = this.Image;

            r.IsExpanded = this.IsExpanded;
            //r.treeCell = new TreeGridCell();

            return r;
        }

        internal protected virtual void UnSited()
        {
            // This row is being removed from being displayed on the grid.
            TreeGridCell cell;
            foreach (DataGridViewCell DGVcell in this.Cells)
            {
                cell = DGVcell as TreeGridCell;
                if (cell != null)
                {
                    cell.UnSited();
                }
            }
            this._isSited = false;
        }

        internal protected virtual void Sited()
        {
            // This row is being added to the grid.
            this._isSited = true;
            this.childCellsCreated = true;
            Debug.Assert(this._grid != null);

            TreeGridCell cell;
            foreach (DataGridViewCell DGVcell in this.Cells)
            {
                cell = DGVcell as TreeGridCell;
                if (cell != null)
                {
                    cell.Sited();// Level = this.Level;
                }
            }

        }

        // Represents the index of this row in the Grid
        [System.ComponentModel.Description("Represents the index of this row in the Grid. Advanced usage."),
        System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced),
         Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int RowIndex
        {
            get
            {
                return base.Index;
            }
        }

        // Represents the index of this row based upon its position in the collection.
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int Index
        {
            get
            {
                if (_index == -1)
                {
                    // get the index from the collection if unknown
                    _index = this._owner.IndexOf(this);
                }

                return _index;
            }
            internal set
            {
                _index = value;
            }
        }

        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ImageList ImageList
        {
            get
            {
                if (this._grid != null)
                    return this._grid.ImageList;
                else
                    return null;
            }
        }

        private bool ShouldSerializeImageIndex()
        {
            return (this._imageIndex != -1 && this._image == null);
        }

        [Category("Appearance"),
        Description("..."), DefaultValue(-1),
        TypeConverter(typeof(ImageIndexConverter)),
        Editor("System.Windows.Forms.Design.ImageIndexEditor", typeof(UITypeEditor))]
        public int ImageIndex
        {
            get { return _imageIndex; }
            set
            {
                _imageIndex = value;
                if (_imageIndex != -1)
                {
                    // when a imageIndex is provided we do not store the image.
                    this._image = null;
                }
                if (this._isSited)
                {
                    // when the image changes the cell's style must be updated
                    if (this._treeCell != null)
                    {
                        this._treeCell.UpdateStyle();
                        if (this.Displayed)
                            this._grid.InvalidateRow(this.RowIndex);
                    }
                }
            }
        }

        private bool ShouldSerializeImage()
        {
            return (this._imageIndex == -1 && this._image != null);
        }

        public Image Image
        {
            get
            {
                if (_image == null && _imageIndex != -1)
                {
                    if (this.ImageList != null && this._imageIndex < this.ImageList.Images.Count)
                    {
                        // get image from image index
                        return this.ImageList.Images[this._imageIndex];
                    }
                    else
                        return null;
                }
                else
                {
                    // image from image property
                    return this._image;
                };
            }
            set
            {
                _image = value;
                if (_image != null)
                {
                    // when a image is provided we do not store the imageIndex.
                    this._imageIndex = -1;
                }
                if (this._isSited)
                {
                    // when the image changes the cell's style must be updated
                    this._treeCell.UpdateStyle();
                    if (this.Displayed)
                        this._grid.InvalidateRow(this.RowIndex);
                }
            }
        }

        protected override DataGridViewCellCollection CreateCellsInstance()
        {
            DataGridViewCellCollection cells = base.CreateCellsInstance();
            cells.CollectionChanged += cells_CollectionChanged;
            return cells;
        }

        void cells_CollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
        {
            // Exit if there already is a tree cell for this row
            if (_treeCell != null) return;

            if (e.Action == System.ComponentModel.CollectionChangeAction.Add || e.Action == System.ComponentModel.CollectionChangeAction.Refresh)
            {
                TreeGridCell treeCell = null;

                if (e.Element == null)
                {
                    foreach (DataGridViewCell cell in base.Cells)
                    {
                        //if (cell.GetType().IsAssignableFrom(typeof(TreeGridCell)))
                        if (cell  is TreeGridCell)
                        {
                            treeCell = (TreeGridCell)cell;
                            break;
                        }

                    }
                }
                else
                {
                    treeCell = e.Element as TreeGridCell;
                }

                if (treeCell != null)
                    _treeCell = treeCell;
            }
        }

        [Category("Data"),
         Description("The collection of root nodes in the treelist."),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
         Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        public TreeGridNodeCollection Nodes
        {
            get
            {
                if (childrenNodes == null)
                {
                    childrenNodes = new TreeGridNodeCollection(this);
                }
                return childrenNodes;
            }
            set { ;}
        }

        // Create a new Cell property because by default a row is not in the grid and won't
        // have any cells. We have to fabricate the cell collection ourself.
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DataGridViewCellCollection Cells
        {
            get
            {
                if (!childCellsCreated && this.DataGridView == null)
                {
                    if (this._grid == null) return null;

                    this.CreateCells(this._grid);
                    childCellsCreated = true;
                }
                return base.Cells;
            }
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level
        {
            get
            {
                if (this._level == -1)
                {
                    // calculate level
                    int walk = 0;
                    TreeGridNode walkRow = this.Parent;
                    while (walkRow != null)
                    {
                        walk++;
                        walkRow = walkRow.Parent;
                    }
                    this._level = walk;
                }
                return this._level;
            }
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TreeGridNode Parent
        {
            get
            {
                return this._parent;
            }
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool HasChildren
        {
            get
            {
                return (this.childrenNodes != null && this.Nodes.Count != 0);
            }
        }

        [Browsable(false)]
        public bool IsSited
        {
            get
            {
                return this._isSited;
            }
        }
        [Browsable(false)]
        public bool IsFirstSibling
        {
            get
            {
                return (this.Index == 0);
            }
        }

        [Browsable(false)]
        public bool IsLastSibling
        {
            get
            {
                TreeGridNode parent = this.Parent;
                if (parent != null && parent.HasChildren)
                {
                    return (this.Index == parent.Nodes.Count - 1);
                }
                else
                    return true;
            }
        }

        public virtual bool Collapse()
        {
            return this._grid.CollapseNode(this);
        }

        public virtual bool Expand()
        {
                if (this._grid != null)
                    return this._grid.ExpandNode(this);
                else
                {
                    this.IsExpanded = true;
                    return true;
                }
        }

        internal protected virtual bool InsertChildNode(int index, TreeGridNode node)
        {
            node._parent = this;
            node._grid = this._grid;
            node.CheckQueuedDispose();

            // ensure that all children of this node has their grid set
            if (this._grid != null)
                UpdateChildNodes(node);

            //TODO: do we need to use index parameter?
            if ((this._isSited || this.IsRoot) && this.IsExpanded)
                this._grid.SiteNode(node);
            return true;
        }

        internal protected virtual bool InsertChildNodes(int index, params TreeGridNode[] nodes)
        {
            foreach (TreeGridNode node in nodes)
            {
                this.InsertChildNode(index, node);
            }
            return true;
        }

        internal protected virtual bool AddChildNode(TreeGridNode node)
        {
            node._parent = this;
            node._grid = this._grid;
            node.CheckQueuedDispose();
            // ensure that all children of this node has their grid set
            if (this._grid != null)
            {
              UpdateChildNodes(node);
            }

            if ((this._isSited || this.IsRoot) && this.IsExpanded && !node._isSited)
                this._grid.SiteNode(node);


            return true;
        }
        internal protected virtual bool AddChildNodes(params TreeGridNode[] nodes)
        {
            //TODO: Convert the final call into an SiteNodes??
            foreach (TreeGridNode node in nodes)
            {
                this.AddChildNode(node);
            }
            return true;

        }

        internal protected virtual bool RemoveChildNode(TreeGridNode node)
        {
            if ((this.IsRoot || this._isSited) && this.IsExpanded)
            {
                //We only unsite out child node if we are sited and expanded.
                this._grid.UnSiteNode(node);

            }
            node._grid = null;
            node._parent = null;
            return true;

        }

        internal protected virtual bool ClearNodes()
        {
            if (this.HasChildren)
            {
                for (int i = this.Nodes.Count - 1; i >= 0; i--)
                {
                    this.Nodes.RemoveAt(i);
                }
            }
            return true;
        }

        [
            Browsable(false),
            EditorBrowsable(EditorBrowsableState.Advanced)
        ]
        public event EventHandler Disposed
        {
            add
            {
                this.disposed += value;
            }
            remove
            {
                this.disposed -= value;
            }
        }

        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public ISite Site
        {
            get
            {
                return this.site;
            }
            set
            {
                this.site = value;
            }
        }

        private void UpdateChildNodes(TreeGridNode node)
        {
            if (node.HasChildren)
            {
                foreach (TreeGridNode childNode in node.Nodes)
                {
                    childNode._grid = node._grid;
                    childNode.CheckQueuedDispose();
                    this.UpdateChildNodes(childNode);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(36);
            sb.Append("TreeGridNode { Index=");
            sb.Append(this.RowIndex.ToString(System.Globalization.CultureInfo.CurrentCulture));
            sb.Append(" }");
            return sb.ToString();
        }

        //protected override void Dispose(bool disposing) {
        //    if (disposing)
        //    {
        //        lock(this)
        //        {
        //            if (this.site != null && this.site.Container != null)
        //            {
        //                this.site.Container.Remove(this);
        //            }

        //            if (this.disposed != null)
        //            {
        //                this.disposed(this, EventArgs.Empty);
        //            }
        //        }
        //    }

        //    base.Dispose(disposing);
        //}
      protected override void Dispose(bool disposing)
      {
        base.Dispose(disposing);
      }
    }
    /// <summary>
    /// Summary description for TreeGridCell.
    /// </summary>
    public class TreeGridCell : DataGridViewTextBoxCell
    {
        public const int INDENT_WIDTH = 20;
        public const int INDENT_MARGIN = 5;
        public const int INDENT_FIRST = 8;
        public const int DEF_GLYPHWIDTH = 15;
        private int glyphWidth;
        private int calculatedLeftPadding;
        internal bool IsSited;

        private Padding _previousPadding;
        private int _imageWidth = 0, _imageHeight = 0, _imageHeightOffset = 0;

        public TreeGridCell()
        {
            _previousPadding = new Padding();
            glyphWidth = DEF_GLYPHWIDTH;
            calculatedLeftPadding = 0;
            this.IsSited = false;

        }

        public override object Clone()
        {
            TreeGridCell c = (TreeGridCell)base.Clone();

            c.glyphWidth = this.glyphWidth;
            c.calculatedLeftPadding = this.calculatedLeftPadding;

            return c;
        }

        internal protected virtual void UnSited()
        {
            // The row this cell is in is being removed from the grid.
            this.IsSited = false;
            //this.Style.Padding = this._previousPadding;
        }

        internal protected virtual void Sited()
        {
            // when we are added to the DGV we can realize our style
            this.IsSited = true;

            // remember what the previous padding size is so it can be restored when unsiting
            //this._previousPadding = this.Style.Padding;

            this.UpdateStyle();
        }
        // Performance bottleneck
        internal protected virtual void UpdateStyle()
        {
            // styles shouldn't be modified when we are not sited.
            if (this.IsSited == false) 
                return;

            int level = this.Level;

            Padding p = this._previousPadding;
            Size preferredSize;

            // This line consumes lot of memory
            Graphics g = this.OwningNode._grid.GetGraphics();
            
            preferredSize = this.GetPreferredSize(g, this.InheritedStyle, this.RowIndex, new Size(0, 0));            
            //preferredSize = new Size(20, 21);

            Image image = this.OwningNode.Image;

            if (image != null)
            {
                // calculate image size
                _imageWidth = image.Width + 2;
                _imageHeight = image.Height + 2;

            }
            else
            {
                _imageWidth = glyphWidth;
                _imageHeight = 0;
            }

            // TODO: Make this cleaner
            if (preferredSize.Height < _imageHeight)
            {
                int leftpad = p.Left + (level * INDENT_WIDTH) + _imageWidth + INDENT_MARGIN;
                // Performance bottleneck changing padding takes lot of time
                TreeGridView _grid = (TreeGridView)this.DataGridView;
                if (_grid.paddings_list.IndexOfKey(leftpad) >= 0)
                {
                  this.Style = _grid.paddings_list.Values[leftpad];
                }
                else
                {
                  Padding npad = new Padding(leftpad,
                                                 p.Top + (_imageHeight / 2), p.Right, p.Bottom + (_imageHeight / 2));
                  this.Style.Padding = npad;
                  _grid.paddings_list.Add(leftpad, this.Style);
                }
                _imageHeightOffset = 2;// (_imageHeight - preferredSize.Height) / 2;
            }
            else
            {
                int leftpad = p.Left + (level * INDENT_WIDTH) + _imageWidth + INDENT_MARGIN;

                Padding oldpad = this.Style.Padding;

//                if ((oldpad.Left != npad.Left) || (oldpad.Top != npad.Top) || (oldpad.Right != npad.Right) || (oldpad.Bottom != npad.Bottom))
                if (oldpad.Left != leftpad)
                {
                    // Performance bottleneck changing padding takes lot of time
                    TreeGridView _grid = (TreeGridView)this.DataGridView;

                    if (_grid.paddings_list.IndexOfKey(leftpad) >= 0)
                    {
                      this.Style = _grid.paddings_list[leftpad];
                    }
                    else
                    {
                      Padding npad = new Padding(leftpad,
                                                     p.Top, p.Right, p.Bottom);
                      this.Style.Padding = npad;
                      _grid.paddings_list.Add(leftpad, this.Style);
                    }
                }
            }

            calculatedLeftPadding = ((level - 1) * glyphWidth) + _imageWidth + INDENT_MARGIN;
        }

        public int Level
        {
            get
            {
                TreeGridNode row = this.OwningNode;
                if (row != null)
                {
                    return row.Level;
                }
                else
                    return -1;
            }
        }

        protected virtual int GlyphMargin
        {
            get
            {
                return ((this.Level - 1) * INDENT_WIDTH) + INDENT_MARGIN;
            }
        }

        protected virtual int GlyphOffset
        {
            get
            {
                return ((this.Level - 1) * INDENT_WIDTH);
            }
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {

            TreeGridNode node = this.OwningNode;
            if (node == null) return;

            Image image = node.Image;

            if (this._imageHeight == 0 && image != null)
                 this.UpdateStyle();

            // paint the cell normally
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

            // TODO: Indent width needs to take image size into account
            Rectangle glyphRect = new Rectangle(cellBounds.X + this.GlyphMargin, cellBounds.Y, INDENT_WIDTH, cellBounds.Height - 1);
            int glyphHalf = glyphRect.Width / 2;

            //TODO: This painting code needs to be rehashed to be cleaner
            int level = this.Level;

            //TODO: Rehash this to take different Imagelayouts into account. This will speed up drawing
            //		for images of the same size (ImageLayout.None)
            if (image != null)
            {
                Point pp;
                if (_imageHeight > cellBounds.Height)
                    pp = new Point(glyphRect.X + this.glyphWidth, cellBounds.Y + _imageHeightOffset);
                else
                    pp = new Point(glyphRect.X + this.glyphWidth, (cellBounds.Height / 2 - _imageHeight / 2) + cellBounds.Y);

                // Graphics container to push/pop changes. This enables us to set clipping when painting
                // the cell's image -- keeps it from bleeding outsize of cells.
                System.Drawing.Drawing2D.GraphicsContainer gc = graphics.BeginContainer();
                {
                    graphics.SetClip(cellBounds);
                    graphics.DrawImageUnscaled(image, pp);
                }
                graphics.EndContainer(gc);
            }

            // Paint tree lines			
            if (node._grid.ShowLines)
            {
                using (Pen linePen = new Pen(SystemBrushes.ControlDark, 1.0f))
                {
                    linePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    bool isLastSibling = node.IsLastSibling;
                    bool isFirstSibling = node.IsFirstSibling;
                    if (node.Level == 1)
                    {
                        // the Root nodes display their lines differently
                        if (isFirstSibling && isLastSibling)
                        {
                            // only node, both first and last. Just draw horizontal line
                            graphics.DrawLine(linePen, glyphRect.X + 4, cellBounds.Top + cellBounds.Height / 2, glyphRect.Right, cellBounds.Top + cellBounds.Height / 2);
                        }
                        else if (isLastSibling)
                        {
                            // last sibling doesn't draw the line extended below. Paint horizontal then vertical
                            graphics.DrawLine(linePen, glyphRect.X + 4, cellBounds.Top + cellBounds.Height / 2, glyphRect.Right, cellBounds.Top + cellBounds.Height / 2);
                            graphics.DrawLine(linePen, glyphRect.X + 4, cellBounds.Top, glyphRect.X + 4, cellBounds.Top + cellBounds.Height / 2);
                        }
                        else if (isFirstSibling)
                        {
                            // first sibling doesn't draw the line extended above. Paint horizontal then vertical
                            graphics.DrawLine(linePen, glyphRect.X + 4, cellBounds.Top + cellBounds.Height / 2, glyphRect.Right, cellBounds.Top + cellBounds.Height / 2);
                            graphics.DrawLine(linePen, glyphRect.X + 4, cellBounds.Top + cellBounds.Height / 2, glyphRect.X + 4, cellBounds.Bottom);
                        }
                        else
                        {
                            // normal drawing draws extended from top to bottom. Paint horizontal then vertical
                            graphics.DrawLine(linePen, glyphRect.X + 4, cellBounds.Top + cellBounds.Height / 2, glyphRect.Right, cellBounds.Top + cellBounds.Height / 2);
                            graphics.DrawLine(linePen, glyphRect.X + 4, cellBounds.Top, glyphRect.X + 4, cellBounds.Bottom);
                        }
                    }
                    else
                    {
                        if (isLastSibling)
                        {
                            // last sibling doesn't draw the line extended below. Paint horizontal then vertical
                            graphics.DrawLine(linePen, glyphRect.X + 4, cellBounds.Top + cellBounds.Height / 2, glyphRect.Right, cellBounds.Top + cellBounds.Height / 2);
                            graphics.DrawLine(linePen, glyphRect.X + 4, cellBounds.Top, glyphRect.X + 4, cellBounds.Top + cellBounds.Height / 2);
                        }
                        else
                        {
                            // normal drawing draws extended from top to bottom. Paint horizontal then vertical
                            graphics.DrawLine(linePen, glyphRect.X + 4, cellBounds.Top + cellBounds.Height / 2, glyphRect.Right, cellBounds.Top + cellBounds.Height / 2);
                            graphics.DrawLine(linePen, glyphRect.X + 4, cellBounds.Top, glyphRect.X + 4, cellBounds.Bottom);
                        }

                        // paint lines of previous levels to the root
                        TreeGridNode previousNode = node.Parent;
                        int horizontalStop = (glyphRect.X + 4) - INDENT_WIDTH;

                        while (!previousNode.IsRoot)
                        {
                            if (previousNode.HasChildren && !previousNode.IsLastSibling)
                            {
                                // paint vertical line
                                graphics.DrawLine(linePen, horizontalStop, cellBounds.Top, horizontalStop, cellBounds.Bottom);
                            }
                            previousNode = previousNode.Parent;
                            horizontalStop = horizontalStop - INDENT_WIDTH;
                        }
                    }

                }
            }

            if (node.HasChildren || node._grid.VirtualNodes)
            {
                // Paint node glyphs				
                if (node.IsExpanded)
                {
                    if (node._grid.themesenabled)
                    {
                        try
                        {
                            if (node._grid.rOpen == null)
                            {
                                node._grid.rOpen = new VisualStyleRenderer(VisualStyleElement.TreeView.Glyph.Opened);
                                node._grid.treeboxwidth = System.Convert.ToInt32(WinFormsGraphics.DPIScale * 10);
                            }
                            node._grid.rOpen.DrawBackground(graphics, new Rectangle(glyphRect.X, glyphRect.Y + (glyphRect.Height / 2) - 4, 
                                node._grid.treeboxwidth, node._grid.treeboxwidth));
                            /*
                            Pen npen = Pens.Black;
                            int recwidth = 8;
                            int margin = 2;
                            int leftmargin = -recwidth / 2 - 2;
                            int topmargin = 0;
                            Rectangle nrect = new Rectangle(leftmargin + glyphRect.X + glyphRect.Width / 2 - recwidth / 2,
                                                            topmargin + glyphRect.Y + glyphRect.Height / 2 - recwidth / 2,
                                                            recwidth,
                                                            recwidth);
                            graphics.DrawLine(npen, new Point(nrect.X + margin, nrect.Top + recwidth / 2),
                                                    new Point(nrect.X + recwidth - margin, nrect.Top + recwidth / 2));*/
                        }
                        catch
                        {
                            node._grid.themesenabled = false;
                        }

                    }
                    if (!node._grid.themesenabled)
                    {
                        Pen npen = Pens.Black;
                        int recwidth = 8;
                        int margin = 2;
                        int leftmargin = -recwidth / 2 - 2;
                        int topmargin = 0;
                        Rectangle nrect = new Rectangle(leftmargin + glyphRect.X + glyphRect.Width / 2 - recwidth / 2,
                                                        topmargin + glyphRect.Y + glyphRect.Height / 2 - recwidth / 2,
                                                        recwidth,
                                                        recwidth);
                        using (Brush nbrush = new SolidBrush(cellStyle.BackColor))
                        {
                          graphics.FillRectangle(nbrush, nrect);
                          graphics.DrawRectangle(npen, nrect);
                          graphics.DrawLine(npen, new Point(nrect.X + margin, nrect.Top + recwidth / 2),
                                                  new Point(nrect.X + recwidth - margin, nrect.Top + recwidth / 2));
                        }
                    }
                }
                else
                {
                    if (node._grid.themesenabled)
                    {
                        try
                        {
                            if (node._grid.rClosed == null)
                            {
                                node._grid.rClosed = new VisualStyleRenderer(VisualStyleElement.TreeView.Glyph.Closed);
                                node._grid.treeboxwidth = System.Convert.ToInt32(WinFormsGraphics.DPIScale * 10);
                            }
                            //    node._grid.rClosed = new VisualStyleRenderer("Explorer::TreeView",2,1);
                            node._grid.rClosed.DrawBackground(graphics, new Rectangle(glyphRect.X, glyphRect.Y + (glyphRect.Height / 2) - 4, node._grid.treeboxwidth, node._grid.treeboxwidth));
                        }
                        catch
                        {
                            node._grid.themesenabled = false;
                        }
                    }
                    if (!node._grid.themesenabled)
                    {
                        Pen npen = Pens.Black;
                        int recwidth = 8;
                        int margin = 2;
                        int leftmargin = -recwidth / 2 - 2;
                        int topmargin = 0;
                        using (Brush nbrush = new SolidBrush(cellStyle.BackColor))
                        {
                          Rectangle nrect = new Rectangle(leftmargin + glyphRect.X + glyphRect.Width / 2 - recwidth / 2,
                                                          topmargin + glyphRect.Y + glyphRect.Height / 2 - recwidth / 2,
                                                          recwidth,
                                                          recwidth);
                          graphics.FillRectangle(nbrush, nrect);
                          graphics.DrawRectangle(npen, nrect);
                          graphics.DrawLine(npen, new Point(nrect.X + margin, nrect.Top + recwidth / 2),
                                                  new Point(nrect.X + recwidth - margin, nrect.Top + recwidth / 2));
                          graphics.DrawLine(npen, new Point(nrect.X + recwidth / 2, nrect.Top + margin),
                                                  new Point(nrect.X + recwidth / 2, nrect.Top + recwidth - margin));
                        }
                    }
                }
            }

        }
        protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
        {
            base.OnMouseUp(e);

            TreeGridNode node = this.OwningNode;
            if (node != null)
                node._grid._inExpandCollapseMouseCapture = false;
        }
        protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
        {
            if (e.Location.X > this.InheritedStyle.Padding.Left)
            {
                base.OnMouseDown(e);
            }
            else
            {
                // Expand the node
                //TODO: Calculate more precise location
                TreeGridNode node = this.OwningNode;
                if (node != null)
                {
                    node._grid._inExpandCollapseMouseCapture = true;
                    if (node.IsExpanded)
                        node.Collapse();
                    else
                        node.Expand();
                }
            }
        }
        public TreeGridNode OwningNode
        {
            get { return base.OwningRow as TreeGridNode; }
        }
      protected override void Dispose(bool disposing)
      {
        base.Dispose(disposing);
      }
    }

    public class TreeGridColumn : DataGridViewTextBoxColumn
    {
        internal Image _defaultNodeImage;

        public TreeGridColumn()
        {
            this.CellTemplate = new TreeGridCell();
        }

        // Need to override Clone for design-time support.
        public override object Clone()
        {
            TreeGridColumn c = (TreeGridColumn)base.Clone();
            c._defaultNodeImage = this._defaultNodeImage;
            return c;
        }

        public Image DefaultNodeImage
        {
            get { return _defaultNodeImage; }
            set { _defaultNodeImage = value; }
        }
        protected override void Dispose(bool disposing)
        {
          this.CellTemplate.Dispose();
          base.Dispose(disposing);
        }
    }
    public class TreeGridNodeCollection : System.Collections.Generic.IList<TreeGridNode>, System.Collections.IList
    {
        internal System.Collections.Generic.List<TreeGridNode> _list;
        internal TreeGridNode _owner;
        internal TreeGridNodeCollection(TreeGridNode owner)
        {
            this._owner = owner;
            this._list = new List<TreeGridNode>();
        }

        #region Public Members
        public void Add(TreeGridNode item)
        {
            // The row needs to exist in the child collection before the parent is notified.
            item._grid = this._owner._grid;
            item.CheckQueuedDispose();

            bool hadChildren = this._owner.HasChildren;
            item._owner = this;

            this._list.Add(item);

            this._owner.AddChildNode(item);

            // if the owner didn't have children but now does (asserted) and it is sited update it
            if (!hadChildren && this._owner.IsSited)
            {
                this._owner._grid.InvalidateRow(this._owner.RowIndex);
            }
        }

        public TreeGridNode Add(string text)
        {
            TreeGridNode node = new TreeGridNode();
            this.Add(node);

            node.Cells[0].Value = text;
            return node;
        }

        public TreeGridNode Add(params object[] values)
        {
            TreeGridNode node = new TreeGridNode();
            this.Add(node);

            int cell = 0;

            if (values.Length > node.Cells.Count)
                throw new ArgumentOutOfRangeException("values");

            foreach (object o in values)
            {
                node.Cells[cell].Value = o;
                cell++;
            }
            return node;
        }

        public void Insert(int index, TreeGridNode item)
        {
            // The row needs to exist in the child collection before the parent is notified.
            item._grid = this._owner._grid;
            item.CheckQueuedDispose();
            item._owner = this;

            this._list.Insert(index, item);

            this._owner.InsertChildNode(index, item);
        }

        public bool Remove(TreeGridNode item)
        {
            // The parent is notified first then the row is removed from the child collection.
            this._owner.RemoveChildNode(item);
            item._grid = null;
            return this._list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            TreeGridNode row = this._list[index];

            // The parent is notified first then the row is removed from the child collection.
            this._owner.RemoveChildNode(row);
            row._grid = null;
            this._list.RemoveAt(index);
        }

        public void Clear()
        {
            // The parent is notified first then the row is removed from the child collection.
            this._owner.ClearNodes();
            this._list.Clear();
        }

        public int IndexOf(TreeGridNode item)
        {
            return this._list.IndexOf(item);
        }

        public TreeGridNode this[int index]
        {
            get
            {
                return this._list[index];
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public bool Contains(TreeGridNode item)
        {
            return this._list.Contains(item);
        }

        public void CopyTo(TreeGridNode[] array, int arrayIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count
        {
            get { return this._list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
        #endregion

        #region IList Interface
        void System.Collections.IList.Remove(object value)
        {
            this.Remove(value as TreeGridNode);
        }


        int System.Collections.IList.Add(object value)
        {
            TreeGridNode item = value as TreeGridNode;
            this.Add(item);
            return item.Index;
        }

        void System.Collections.IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }


        void System.Collections.IList.Clear()
        {
            this.Clear();
        }

        bool System.Collections.IList.IsReadOnly
        {
            get { return this.IsReadOnly; }
        }

        bool System.Collections.IList.IsFixedSize
        {
            get { return false; }
        }

        int System.Collections.IList.IndexOf(object item)
        {
            return this.IndexOf(item as TreeGridNode);
        }

        void System.Collections.IList.Insert(int index, object value)
        {
            this.Insert(index, value as TreeGridNode);
        }
        int System.Collections.ICollection.Count
        {
            get { return this.Count; }
        }
        bool System.Collections.IList.Contains(object value)
        {
            return this.Contains(value as TreeGridNode);
        }
        void System.Collections.ICollection.CopyTo(Array array, int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        object System.Collections.IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }



        #region IEnumerable<ExpandableRow> Members

        public IEnumerator<TreeGridNode> GetEnumerator()
        {
            return this._list.GetEnumerator();
        }

        #endregion


        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
        #endregion

        #region ICollection Members

        bool System.Collections.ICollection.IsSynchronized
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        object System.Collections.ICollection.SyncRoot
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion
    }

}