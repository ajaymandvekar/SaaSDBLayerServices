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

/// <summary>
/// Summary description for Field
/// </summary>
public class Field
{
    

    public Field(int orgid,int objid,int fieldID,string FieldName)
    {
        OrgIDProperty = orgid;
        ObjIDProperty = objid;
        FieldIDProperty = fieldID;
        FieldNameProperty = FieldName;
    }
    public Field()
    {
    }
    public int OrgIDProperty { get; set; }
    public int ObjIDProperty { get; set; }
    public int FieldIDProperty { get; set; }
    public string FieldNameProperty { get; set; }
    public string FieldDataType { get; set; }

}