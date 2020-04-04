using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


public partial class _Test_AD : System.Web.UI.Page
{
    string ldapPath = "LDAP://OU=MIS_Management,DC=prokits,DC=com,DC=tw";

    protected void Page_Load(object sender, EventArgs e)
    {
      
    }

    protected void btn_showGroup_Click(object sender, EventArgs e)
    {
        List<ADService.LookupLDAP> ListGroup = ADService.ListGroups(ldapPath);

        StringBuilder result = new StringBuilder();
        foreach (var item in ListGroup)
        {
            result.AppendLine(item.AccountName + "_" + item.GUID);
        }

        this.tb_Result.Text = result.ToString();
    }

    protected void btn_showUser_Click(object sender, EventArgs e)
    {
        List<ADService.LookupLDAP> ListUsers = ADService.ListUsers(ldapPath);

        StringBuilder result = new StringBuilder();
        foreach (var item in ListUsers)
        {
            result.AppendLine(item.DisplayName + "_" + item.GUID);
        }

        this.tb_Result.Text = result.ToString();
    }

    protected void btn_showGrpUsers_Click(object sender, EventArgs e)
    {
        string cn = this.tb_Group.Text;
        List<ADService.LookupLDAP> ListUsers = ADService.ListUsers(ldapPath, "packing");
        //int cnt = ADService.ListUsers(ldapPath, cn);

        //Response.Write(cnt);

        StringBuilder result = new StringBuilder();
        foreach (var item in ListUsers)
        {
            result.AppendLine(item.AccountName + "_" + item.GUID);
        }

        //this.tb_Result.Text = result.ToString();
    }

    protected void btn_Clear_Click(object sender, EventArgs e)
    {
        Response.Redirect("AD.aspx");
    }


}