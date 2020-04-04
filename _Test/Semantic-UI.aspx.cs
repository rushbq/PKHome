using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKLib_Method.Methods;
using System.Linq;

public partial class SemanticUI : System.Web.UI.Page
{
    public string ErrMsg;
    public string sDate = DateTime.Today.AddDays(-30).ToString().ToDateString("yyyy/MM/dd");
    public string eDate = DateTime.Today.ToString().ToDateString("yyyy/MM/dd");
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
               


            }
        }
        catch (Exception)
        {

            throw;
        }
    }

    
}