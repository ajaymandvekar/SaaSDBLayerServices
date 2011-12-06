/*
 * Copyright 2011 
 * Ajay Mandvekar(ajaymandvekar@gmail.com),Mugdha Kolhatkar(himugdha@gmail.com),Vishakha Channapattan(vishakha.vc@gmail.com)
 * 
 * This file is part of SaaSDBLayerServices.
 * SaaSDBLayerServices is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * SaaSDBLayerServices is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with SaaSDBLayerServices.  If not, see <http://www.gnu.org/licenses/>.
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]

public class Service : System.Web.Services.WebService
{
    public Service () {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public int CreateObject(int OrgID, string TableName, List<string> fieldname, List<string> datatype, int primaryKey)
    {
        int ObjID = 0;
        int rows = 0;
        int FieldID = 0;
        SqlCommand command_insert = null, command_insert_field = null, command = null;
        SqlParameter refobjid = null, fieldid = null, reffieldid = null;
        SqlParameter Orgid = null, Tname = null, Objid = null, Fieldname = null, Datatype = null, Position = null;
        string sql = null;


        /** Create New Connection **/
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SAASDB"].ToString());
        conn.Open();

        /**Check if table name is already present **/
        string select_command = "Select count(*) from Object where OrgID="+ OrgID +" and ObjName='" + TableName + "'";
        int count = 0;
        command = new SqlCommand(select_command, conn);
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
                count = reader.GetInt32(0);
        }

        if (count > 0)
        {
            return 0;
        }
        reader.Close();
        command = null;
        select_command = null;

        /** Insert into Table **/

        command_insert = new SqlCommand("Insert into Object(OrgID,ObjName) values (@OrgID,@Tname)", conn);
        Orgid = new SqlParameter("@OrgID", OrgID);
        Tname = new SqlParameter("@Tname", TableName);
        command_insert.Parameters.Add(Tname);
        command_insert.Parameters.Add(Orgid);

        rows = command_insert.ExecuteNonQuery();
        if (rows == 0)
        {
            conn.Close();
            return -1;
        }
        else
        {
            command_insert.Parameters.Clear();
            command_insert.CommandText = "SELECT @@IDENTITY";
            ObjID = Convert.ToInt32(command_insert.ExecuteScalar());
            command_insert.Dispose();

            /** Insert into Feilds Table **/
            for (int i = 0; i < fieldname.Count; i++)
            {
                sql = "Insert into Field(OrgID,ObjID,FieldName,DataType,Position) values (@OrgID,@ObjID,@FieldName,@DataType,@Position)";
                command_insert_field = new SqlCommand(sql, conn);
                Orgid = new SqlParameter("@OrgID", OrgID);
                Objid = new SqlParameter("@ObjID", ObjID);
                Fieldname = new SqlParameter("@FieldName", fieldname[i].ToString());
                Datatype = new SqlParameter("@DataType", datatype[i].ToString());
                Position = new SqlParameter("@Position", i);

                command_insert_field.Parameters.Add(Orgid);
                command_insert_field.Parameters.Add(Objid);
                command_insert_field.Parameters.Add(Fieldname);
                command_insert_field.Parameters.Add(Datatype);
                command_insert_field.Parameters.Add(Position);

                command_insert_field.ExecuteNonQuery();
                command_insert_field.Parameters.Clear();
                command_insert_field.Dispose();
                command_insert_field = null;
                sql = null;

                if (i == primaryKey)
                {
                    command_insert.Parameters.Clear();
                    command_insert.CommandText = "SELECT @@IDENTITY";
                    FieldID = Convert.ToInt32(command_insert.ExecuteScalar());
                    command_insert.Dispose();
                }

            }

            if (primaryKey != -1)
            {
                command_insert = new SqlCommand("Insert into Relationship(OrgID,ObjID,RefObjID,FieldID,RefFieldID) values (@OrgID,@ObjID,@RefObjID,@FieldID,@RefFieldID)", conn);
                Orgid = new SqlParameter("@OrgID", OrgID);
                Objid = new SqlParameter("@ObjID", ObjID);
                refobjid = new SqlParameter("@RefObjID", ObjID);
                fieldid = new SqlParameter("@FieldID", FieldID);
                reffieldid = new SqlParameter("@RefFieldID", FieldID);

                command_insert.Parameters.Add(Orgid);
                command_insert.Parameters.Add(Objid);
                command_insert.Parameters.Add(refobjid);
                command_insert.Parameters.Add(fieldid);
                command_insert.Parameters.Add(reffieldid);

                command_insert.ExecuteNonQuery();
                command_insert.Parameters.Clear();
                command_insert.Dispose();
                command_insert = null;
            }
        }
        return ObjID;
    }


    [WebMethod]
    public bool DeleteTable(int OrgID, int ObjID)
    {

        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SAASDB"].ToString());
        conn.Open();

        SqlCommand command_delete = new SqlCommand("Delete from Object where OrgId=@OrgID and ObjID=@ObjID;Delete from Field where OrgId=@OrgID and ObjID=@ObjID;", conn);
        SqlParameter Objid = new SqlParameter("@ObjID", ObjID);
        SqlParameter Orgid = new SqlParameter("@OrgID", OrgID);
        command_delete.Parameters.Add(Objid);
        command_delete.Parameters.Add(Orgid);

        int rows = 0;
        rows = command_delete.ExecuteNonQuery();
        if (rows == 0)
            return false;
        else
            return true;
    }

    [WebMethod]
    public bool DeleteField(int OrgID, int ObjID, int FieldID)
    {
        int flag = 0;
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SAASDB"].ToString());
        conn.Open();

        string select_command = "Select count(*) from Relationship where RefObjID=" + ObjID + " and OrgID=" + OrgID + " and RefFieldID=" + FieldID;
        SqlCommand command = new SqlCommand(select_command, conn);
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            flag = Convert.ToInt32(reader.GetValue(0));
        }
        if (flag > 0)
        {
            reader.Close();
            return false;
        }
        else
        {
            reader.Close();
        }

        SqlCommand command_delete = new SqlCommand("Delete from Field where OrgId=@OrgID and ObjID=@ObjID and FieldID=@FieldID", conn);
        SqlParameter Orgid = new SqlParameter("@OrgID", OrgID);
        SqlParameter Objid = new SqlParameter("@ObjID", ObjID);
        SqlParameter Fieldid = new SqlParameter("@FieldID", FieldID);

        command_delete.Parameters.Add(Orgid);
        command_delete.Parameters.Add(Objid);
        command_delete.Parameters.Add(Fieldid);

        int rows = 0;
        rows = command_delete.ExecuteNonQuery();
        if (rows == 0)
            return false;
        else
            return true;
    }

    [WebMethod]
    public int AddField(int OrgID, int ObjID, string FieldName, string DataType)
    {
        int Pos = 0, rows = 0, newFeildId = 0;
        SqlParameter Orgid = null, Objid = null, Fieldname = null, Datatype = null, Position = null;
        SqlCommand command = null;


        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SAASDB"].ToString());
        conn.Open();

        Orgid = new SqlParameter("@OrgID", OrgID);
        Objid = new SqlParameter("@ObjID", ObjID);
        Fieldname = new SqlParameter("@FieldName", FieldName);
        Datatype = new SqlParameter("@DataType", DataType);


        command = new SqlCommand("Select max(Position) from Field where OrgID=@OrgID and ObjID=@ObjID", conn);
        command.Parameters.Add(Orgid);
        command.Parameters.Add(Objid);
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
                Pos = 1 + reader.GetInt32(0);
        }
        reader.Close();
        command.Parameters.Clear();
        command.Dispose();
        command = null;

        Position = new SqlParameter("@Position", Pos);

        SqlCommand command_addfield = new SqlCommand("Insert into Field(OrgID,ObjID,FieldName,DataType,Position) values (@OrgID,@ObjID,@FieldName,@DataType,@Position)", conn);
        command_addfield.Parameters.Add(Orgid);
        command_addfield.Parameters.Add(Objid);
        command_addfield.Parameters.Add(Fieldname);
        command_addfield.Parameters.Add(Datatype);
        command_addfield.Parameters.Add(Position);
        rows = command_addfield.ExecuteNonQuery();
        if (rows == 0)
        {
            conn.Close();
            return -1;
        }
        else
        {
            command_addfield.Parameters.Clear();
            command_addfield.CommandText = "SELECT @@IDENTITY";
            newFeildId = Convert.ToInt32(command_addfield.ExecuteScalar());
            command_addfield.Dispose();
            return newFeildId;
        }

    }

    [WebMethod]
    public TenantTableInfo ReadData(int OrgID, int ObjID)
    {
        SqlCommand command = null;
        SqlParameter Orgid = null, Objid = null;
        SqlDataReader reader = null;
        int num_fields = 0;
        string position_str = "", data_str = "";
        int j = 0;

        TenantTableInfo tinfo = new TenantTableInfo();
        tinfo.FieldNamesProperty = new List<string>();
        tinfo.FieldValuesProperty = new List<string>();

        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SAASDB"].ToString());
        conn.Open();


        command = new SqlCommand("Select FieldName,Position from Field where OrgID=@OrgID and ObjID=@ObjID", conn);
        Orgid = new SqlParameter("@OrgID", OrgID);
        Objid = new SqlParameter("@ObjID", ObjID);
        command.Parameters.Add(Orgid);
        command.Parameters.Add(Objid);
        reader = command.ExecuteReader();

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                
                if (num_fields == 0)
                {
                    tinfo.FieldNamesProperty.Add("GUID");
                    position_str = position_str + "Value" + reader.GetValue(1).ToString();
                }
                else
                {
                    position_str = position_str + ",Value" + reader.GetValue(1).ToString();
                }
                tinfo.FieldNamesProperty.Add(reader.GetValue(0).ToString());
                num_fields++;
            }
        }

        num_fields++;

        reader.Close();
        command.Parameters.Clear();
        command.Dispose();
        command = null;



        data_str = data_str + "select GUID," + position_str + " from Data where OrgID=@OrgID and ObjID=@ObjID";
        command = new SqlCommand(data_str, conn);
        Orgid = new SqlParameter("@OrgID", OrgID);
        Objid = new SqlParameter("@ObjID", ObjID);
        command.Parameters.Add(Orgid);
        command.Parameters.Add(Objid);
        reader = command.ExecuteReader();
        while (reader.Read())
        {
            j = 0;
            while (j < num_fields)
            {
                if (!reader.IsDBNull(j))
                {
                    tinfo.FieldValuesProperty.Add(reader.GetValue(j).ToString());
                }
                j++;
            }
        }
        reader.Close();
        command.Parameters.Clear();
        command.Dispose();
        command = null;

        return tinfo;
    }

    [WebMethod]
    public TenantTableInfo ReadDataWithGUID(int OrgID, int ObjID,int GUID)
    {
        SqlCommand command = null;
        SqlParameter Orgid = null, Objid = null, guID = null;
        SqlDataReader reader = null;
        int num_fields = 0;
        string position_str = "", data_str = "";
        int j = 0;

        TenantTableInfo tinfo = new TenantTableInfo();
        tinfo.FieldNamesProperty = new List<string>();
        tinfo.FieldValuesProperty = new List<string>();

        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SAASDB"].ToString());
        conn.Open();


        command = new SqlCommand("Select FieldName,Position from Field where OrgID=@OrgID and ObjID=@ObjID", conn);
        Orgid = new SqlParameter("@OrgID", OrgID);
        Objid = new SqlParameter("@ObjID", ObjID);
        command.Parameters.Add(Orgid);
        command.Parameters.Add(Objid);
        reader = command.ExecuteReader();

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {

                if (num_fields == 0)
                {
                    tinfo.FieldNamesProperty.Add("GUID");
                    position_str = position_str + "Value" + reader.GetValue(1).ToString();
                }
                else
                {
                    position_str = position_str + ",Value" + reader.GetValue(1).ToString();
                }
                tinfo.FieldNamesProperty.Add(reader.GetValue(0).ToString());
                num_fields++;
            }
        }

        num_fields++;

        reader.Close();
        command.Parameters.Clear();
        command.Dispose();
        command = null;

        data_str = data_str + "select GUID," + position_str + " from Data where OrgID=@OrgID and ObjID=@ObjID and GUID=@GUID";
        command = new SqlCommand(data_str, conn);
        Orgid = new SqlParameter("@OrgID", OrgID);
        Objid = new SqlParameter("@ObjID", ObjID);
        guID = new SqlParameter("@GUID", GUID);
        command.Parameters.Add(Orgid);
        command.Parameters.Add(Objid);
        command.Parameters.Add(guID);

        reader = command.ExecuteReader();
        while (reader.Read())
        {
            j = 0;
            while (j < num_fields)
            {
                if (!reader.IsDBNull(j))
                {
                    tinfo.FieldValuesProperty.Add(reader.GetValue(j).ToString());
                }
                j++;
            }
        }
        reader.Close();
        command.Parameters.Clear();
        command.Dispose();
        command = null;

        return tinfo;
    }

    [WebMethod]
    public List<Table> GetTables(int OrgID)
    {
        List<Table> tables_list = new List<Table>();
        SqlCommand command = null;
        SqlParameter Orgid = null;
        SqlDataReader reader = null;

        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SAASDB"].ToString());
        conn.Open();

        command = new SqlCommand("Select * from Object where OrgID=@OrgID", conn);
        Orgid = new SqlParameter("@OrgID", OrgID);
        command.Parameters.Add(Orgid);
        reader = command.ExecuteReader();
        while (reader.Read())
        {
            Table table = new Table();
            table.OrgIDProperty = Convert.ToInt32(reader.GetValue(1));
            table.ObjIDProperty = Convert.ToInt32(reader.GetValue(0));
            table.TNameProperty = reader.GetValue(2).ToString();
            tables_list.Add(table);
        }

        return tables_list;
    }
    
    [WebMethod]
    public List<Field> ReadField(int OrgID, int ObjID)
    {
        List<Field> field_list = new List<Field>();
        SqlCommand command = null;
        SqlParameter Orgid = null, Objid = null;
        SqlDataReader reader = null;

        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SAASDB"].ToString());
        conn.Open();

        command = new SqlCommand("Select * from Field where OrgID=@OrgID and ObjID=@ObjID", conn);
        Orgid = new SqlParameter("@OrgID", OrgID);
        Objid = new SqlParameter("@ObjID", ObjID);
        command.Parameters.Add(Orgid);
        command.Parameters.Add(Objid);
        reader = command.ExecuteReader();
        while (reader.Read())
        {
            Field field = new Field();

            field.FieldIDProperty = Convert.ToInt32(reader.GetValue(0));
            field.OrgIDProperty = Convert.ToInt32(reader.GetValue(1));
            field.ObjIDProperty = Convert.ToInt32(reader.GetValue(2));
            field.FieldNameProperty = reader.GetValue(3).ToString();
            field.FieldDataType = reader.GetValue(4).ToString();
            field_list.Add(field);
        }

        return field_list;
    }
    [WebMethod]
    public int Login(string username, string password)
    {
        int orgID = -1;
        try
        {
            bool success = false;
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SAASDB"].ToString());
            conn.Open();
            SqlDataReader reader = null; 
            SqlCommand command = new SqlCommand("Select * from Organization where OrgEmail=@username and OrgPassword=@password", conn);

            SqlParameter OrguserName = new SqlParameter("@username", username);

            SqlParameter Orgpassword = new SqlParameter("@password", password);

            command.Parameters.Add(OrguserName);
            command.Parameters.Add(Orgpassword);

            reader = command.ExecuteReader();
            while (reader.Read())
            {
                orgID = (int)reader.GetValue(0);
            }
            return orgID;
        }
        catch (Exception ex)
        {

            throw;
        }

    }


    [WebMethod]
    public int AccountRegistration(string FName, string LName, string Email, string password)
    {
        try
        {
            int OrgID = -1;
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SAASDB"].ToString());
            conn.Open();
            
            SqlCommand command = new SqlCommand("Insert into Organization(OrgFname,OrgLname,OrgEmail,OrgPassword) values (@Fname,@Lname,@email,@password)", conn);

            SqlParameter OrgFName = new SqlParameter("@Fname", FName);
            SqlParameter OrgLName = new SqlParameter("@Lname", LName);
            SqlParameter OrgEmail = new SqlParameter("@email", Email);
            SqlParameter Orgpassword = new SqlParameter("@password", password);

            command.Parameters.Add(OrgFName);
            command.Parameters.Add(OrgLName);
            command.Parameters.Add(OrgEmail);
            command.Parameters.Add(Orgpassword);
            int success = command.ExecuteNonQuery();
            if (success <= 0)
                return OrgID;
            else
            {
                OrgID = Login(Email, password);
                return OrgID;
            }
        }
        catch (Exception exception_getter)
        {
            throw;
        }
    }
    [WebMethod]
    public Boolean InsertRelationship(int OrgID, int ObjID, int RefObjID, int FieldID)
    {
        SqlParameter Orgid = null, Objid = null, Refobjid = null, Fieldid = null, Reffieldid = null;
        string sql = null;
        SqlCommand command = null;

        /** Create New Connection **/
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SAASDB"].ToString());
        conn.Open();

        Orgid = new SqlParameter("@OrgID", OrgID);
        Objid = new SqlParameter("@ObjID", ObjID);
        Refobjid = new SqlParameter("@RefObjID", RefObjID);
        Fieldid = new SqlParameter("@FieldID", FieldID);

        command = new SqlCommand("Select FieldID from Relationship where OrgID=@OrgID and ObjID=@Refobjid and RefObjID=@Refobjid", conn);
        command.Parameters.Add(Orgid);
        command.Parameters.Add(Refobjid);
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            Reffieldid = new SqlParameter("@RefFieldID", Convert.ToInt32(reader.GetValue(0)));
            break;
        }
        reader.Close();
        command.Parameters.Clear();
        command.Dispose();

        command = new SqlCommand("Insert into Relationship(OrgID,ObjID,RefObjID,FieldID,RefFieldID) values (@OrgID,@ObjID,@RefObjID,@FieldID,@RefFieldID)", conn);
        command.Parameters.Add(Orgid);
        command.Parameters.Add(Objid);
        command.Parameters.Add(Refobjid);
        command.Parameters.Add(Fieldid);
        command.Parameters.Add(Reffieldid);

        command.ExecuteNonQuery();
        command.Parameters.Clear();
        command.Dispose();
        return true;
    }

    [WebMethod]
    public List<Table> getTablesWithPrimaryKey(int OrgID,int ObjID, string Datatype)
    {
        List<Table> tables_list = new List<Table>();
        String select_command = null;
        SqlCommand command = null;
        SqlDataReader reader = null;

        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SAASDB"].ToString());
        conn.Open();
        select_command = "Select * from Object,Relationship,Field where Field.Datatype='" + Datatype.ToString() + "' and Relationship.OrgID=Field.OrgID and Relationship.OrgID=" + OrgID.ToString() + " and Relationship.ObjID=Relationship.RefObjID and Relationship.ObjID=Field.ObjID and Object.orgID=" + OrgID.ToString() + " and Object.ObjID=Field.ObjID and Object.ObjID<>"+ ObjID;
        command = new SqlCommand(select_command, conn);
        reader = command.ExecuteReader();
        while (reader.Read())
        {
            Table table = new Table();
            table.OrgIDProperty = Convert.ToInt32(reader.GetValue(1));
            table.ObjIDProperty = Convert.ToInt32(reader.GetValue(0));
            table.TNameProperty = reader.GetValue(2).ToString();
            tables_list.Add(table);
        }

        return tables_list;
    }

    [WebMethod]
    public int getcurrentindex(int ObjID, string FieldName)
    {
        String select_command = null;
        SqlCommand command = null;
        SqlDataReader reader = null;
        int position = -1;
        int currentindex = 0;
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SAASDB"].ToString());
        conn.Open();
        select_command = "Select Position from Field where ObjID=" + ObjID + " and FieldName='" + FieldName + "'";
        command = new SqlCommand(select_command, conn);
        reader = command.ExecuteReader();
        while (reader.Read())
        {
            position = Convert.ToInt32(reader.GetValue(0));
        }

        reader.Close();
        select_command = "Select count(*),max(Value" + position +") from Data where and ObjID=" + ObjID +"";
        command = new SqlCommand(select_command, conn);
        reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (Convert.ToInt32(reader.GetValue(0)) > 0)
            {
                currentindex = Convert.ToInt32(reader.GetValue(1));
            }
        }
        conn.Close();
        return currentindex;
    }

    [WebMethod]
    public Boolean InsertData(int OrgID, int ObjID, String Name, List<string> Field, List<string> value)
    {
        SqlCommand command = null;
        String select_command = null, insert_str = null;
        SqlDataReader reader = null;
        int fieldid = -1, i = 0, flag = 0;

        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SAASDB"].ToString());
        conn.Open();
        select_command = "Select Field.Position from Field,Relationship where Relationship.ObjID=" + ObjID + " and Relationship.RefObjID=" + ObjID + "and Relationship.OrgID=" + OrgID + " and Field.FieldID=Relationship.FieldID";
        command = new SqlCommand(select_command, conn);
        reader = command.ExecuteReader();
        while (reader.Read())
        {
            fieldid = Convert.ToInt32(reader.GetValue(0));
        }
        reader.Close();

        if (fieldid != -1)
        {
            for (i = 0; i < Field.Count; i++)
            {
                if (i == fieldid)
                {
                    select_command = "Select count(*) from Data where ObjID=" + ObjID + " and OrgID=" + OrgID + " and Value" + i + "='" + value[i].ToString() + "'";
                    command = new SqlCommand(select_command, conn);
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        flag = Convert.ToInt32(reader.GetValue(0));
                    }
                    if (flag > 0)
                    {
                        reader.Close();
                        return false;
                    }
                    else
                    {
                        reader.Close();
                    }
                    break;
                }
            }
        }


        insert_str = "Insert into Data(OrgID,ObjID,Name,";

        for (i = 0; i < Field.Count; i++)
        {
            if (i + 1 == Field.Count)
            {
                insert_str = insert_str + "Value" + Field[i].ToString();
            }
            else
            {
                insert_str = insert_str + "Value" + Field[i].ToString() + ",";
            }
        }

        insert_str = insert_str + ") Values(" + OrgID + "," + ObjID + ",'" + Name + "',";

        for (i = 0; i < value.Count; i++)
        {
            if (i + 1 == value.Count)
            {
                insert_str = insert_str + "'" + value[i].ToString() + "'";
            }
            else
            {
                insert_str = insert_str + "'" + value[i].ToString() + "'" + ",";
            }
        }

        insert_str = insert_str + ")";


        command = new SqlCommand(insert_str, conn);
        int rows = command.ExecuteNonQuery();
        if (rows == 0)
        {
            conn.Close();
            return false;
        }
        else
        {
            return true;
        }

    }

}