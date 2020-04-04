using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Test_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        double dbl_Value = Convert.ToDouble("-10550");

        Response.Write(String.Format("-{0:0.00}", Math.Abs(dbl_Value)));
        //List<OpcsDept> deptRel = new List<OpcsDept>();
        //List<OpcsDept> deptSetRel = new List<OpcsDept>();
        //List<OpcsDept> deptUnRel = new List<OpcsDept>();
        //string deptID = "120";
        ////採購部
        //for (int row = 1; row <= 27; row++)
        //{
        //    deptRel.Add(new OpcsDept(row, deptID));
        //}
        //deptUnRel.Add(new OpcsDept(13, deptID));
        //deptUnRel.Add(new OpcsDept(21, deptID));
        //deptUnRel.Add(new OpcsDept(22, deptID));
        //deptUnRel.Add(new OpcsDept(23, deptID));
        //deptUnRel.Add(new OpcsDept(24, deptID));
        //deptUnRel.Add(new OpcsDept(27, deptID));

        //var query = deptRel
        //    .Where(fld => !deptUnRel.Select(f => f.colID).Contains(fld.colID));
        //deptSetRel.AddRange(query);

        //Response.Write(deptSetRel.Count());
    }

    public class OpcsDept
    {
        public int colID
        {
            get { return this._colID; }
            set { this._colID = value; }
        }
        private int _colID;

        public string deptID
        {
            get { return this._deptID; }
            set { this._deptID = value; }
        }
        private string _deptID;


        public OpcsDept(int colID, string deptID)
        {
            this._colID = colID;
            this._deptID = deptID;
        }
    }
}