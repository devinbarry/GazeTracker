namespace UnitTestsUI
{
    partial class VisualDisplay
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tablePanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tablePanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tablePanel1
            // 
            this.tablePanel1.ColumnCount = 1;
            this.tablePanel1.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle( System.Windows.Forms.SizeType.Percent, 50F ) );
            this.tablePanel1.Controls.Add( this.flowPanel1, 0, 0 );
            this.tablePanel1.Controls.Add( this.flowPanel2, 0, 1 );
            this.tablePanel1.Controls.Add( this.flowPanel3, 0, 2 );
            this.tablePanel1.Location = new System.Drawing.Point( 12, 12 );
            this.tablePanel1.Name = "tablePanel1";
            this.tablePanel1.RowCount = 3;
            this.tablePanel1.RowStyles.Add( new System.Windows.Forms.RowStyle( System.Windows.Forms.SizeType.Percent, 50F ) );
            this.tablePanel1.RowStyles.Add( new System.Windows.Forms.RowStyle( System.Windows.Forms.SizeType.Percent, 50F ) );
            this.tablePanel1.RowStyles.Add( new System.Windows.Forms.RowStyle( System.Windows.Forms.SizeType.Absolute, 269F ) );
            this.tablePanel1.Size = new System.Drawing.Size( 779, 749 );
            this.tablePanel1.TabIndex = 0;
            // 
            // flowPanel2
            // 
            this.flowPanel2.Location = new System.Drawing.Point( 3, 243 );
            this.flowPanel2.Name = "flowPanel2";
            this.flowPanel2.Size = new System.Drawing.Size( 321, 109 );
            this.flowPanel2.TabIndex = 1;
            // 
            // flowPanel3
            // 
            this.flowPanel3.Location = new System.Drawing.Point( 3, 483 );
            this.flowPanel3.Name = "flowPanel3";
            this.flowPanel3.Size = new System.Drawing.Size( 321, 109 );
            this.flowPanel3.TabIndex = 2;
            // 
            // flowPanel1
            // 
            this.flowPanel1.Location = new System.Drawing.Point( 3, 3 );
            this.flowPanel1.Name = "flowPanel1";
            this.flowPanel1.Size = new System.Drawing.Size( 321, 109 );
            this.flowPanel1.TabIndex = 3;
            // 
            // VisualDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 1174, 1280 );
            this.Controls.Add( this.tablePanel1 );
            this.Name = "VisualDisplay";
            this.Text = "VisualDisplay";
            this.tablePanel1.ResumeLayout( false );
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tablePanel1;
        private System.Windows.Forms.FlowLayoutPanel flowPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowPanel2;
        private System.Windows.Forms.FlowLayoutPanel flowPanel3;

    }
}